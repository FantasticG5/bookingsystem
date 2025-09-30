using Data;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ===== EF Core =====
builder.Services.AddDbContext<BookingDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== DataProtection (delad nyckelring mellan tjänster) =====
var keysPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "CoreGym", "dp-keys"
);
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("CoreGym"); // MÅSTE matcha AuthSystem

// ===== Repos/Services =====
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IEmailSender, AzureEmailSender>();
builder.Services.AddScoped<IBookingService, BookingService>();

// ===== HttpClient mot EventSystem =====
builder.Services.AddHttpClient("TrainingClasses", client =>
{
    var baseUrl = builder.Configuration["TrainingClasses:BaseUrl"]
        ?? throw new InvalidOperationException("TrainingClasses:BaseUrl saknas i appsettings.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ===== Cookie-auth =====
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme) // <-- samma som Identity
    .AddCookie(IdentityConstants.ApplicationScheme, o =>
    {
        o.Cookie.Name = ".myapp.id";               // <-- samma namn
        o.Cookie.SameSite = SameSiteMode.None;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; },
            OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; }
        };
    });

builder.Services.AddAuthorization();

// ===== CORS =====
const string SpaCors = "spa";
string[] allowedOrigins = { "https://fantasticg5-dmdbeshvcmfxe6ey.northeurope-01.azurewebsites.net", "http://localhost:5173", "https://localhost:5173"};

builder.Services.AddCors(opt =>
{
    opt.AddPolicy(SpaCors, p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ===== CSRF/Antiforgery (för att säkra POST/DELETE från browsern) =====
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-XSRF-TOKEN";
    o.Cookie.Name = ".CoreGym.Anti";
    o.Cookie.SameSite = SameSiteMode.None;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.HttpOnly = false;
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking API"));

app.UseHttpsRedirection();
app.UseCors(SpaCors);
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (ctx, next) =>
{
    if (HttpMethods.IsPost(ctx.Request.Method) ||
        HttpMethods.IsPut(ctx.Request.Method)  ||
        HttpMethods.IsPatch(ctx.Request.Method)||
        HttpMethods.IsDelete(ctx.Request.Method))
    {
        var antiforgery = ctx.RequestServices.GetRequiredService<IAntiforgery>();
        await antiforgery.ValidateRequestAsync(ctx);
    }
    await next();
});

// ===== CSRF helper endpoint =====
app.MapGet("/csrf", (IAntiforgery af, HttpContext ctx) =>
{
    var tokens = af.GetAndStoreTokens(ctx); // skapar/lagrar cookie-token
    // Lägg även ut REQUEST-token i en läsbar cookie så frontend kan hämta den:
    ctx.Response.Cookies.Append("XSRF-TOKEN-BOOKING", tokens.RequestToken!, new CookieOptions
    {
        HttpOnly = false,
        Secure = true,
        SameSite = SameSiteMode.None
    });
    return Results.NoContent();
});

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

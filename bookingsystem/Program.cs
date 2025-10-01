// Program.cs (Booking)
using Data;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Infrastructure.Repositories;
using Infrastructure.option;

var builder = WebApplication.CreateBuilder(args);

// EF (som du hade)
builder.Services.AddDbContext<BookingDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// Repos/Services (som du hade)
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IEmailSender, AzureEmailSender>();
builder.Services.AddScoped<IBookingService, BookingService>();

// HttpClient till EventSystem (som du hade)
builder.Services.AddHttpClient("TrainingClasses", client =>
{
    var baseUrl = builder.Configuration["TrainingClasses:BaseUrl"]
        ?? throw new InvalidOperationException("TrainingClasses:BaseUrl saknas i appsettings.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

// JWT Bearer
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
var accessKey = new SymmetricSecurityKey(Convert.FromBase64String(jwt.AccessKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = accessKey,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// CORS (utan credentials)
const string SpaCors = "spa";
string[] allowedOrigins = {
    "https://fantasticg5-dmdbeshvcmfxe6ey.westeurope-01.azurewebsites.net",
    "http://localhost:5173",
    "https://localhost:5173"
};
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(SpaCors, p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Booking API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,
    RequireHeaderSymmetry = false,
    ForwardLimit = null
});

app.UseCors(SpaCors);

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking API"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

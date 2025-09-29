using Data;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repos & Services
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IEmailSender, AzureEmailSender>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Namngiven HttpClient för mikroservicen
builder.Services.AddHttpClient("TrainingClasses", client =>
{
    var baseUrl = builder.Configuration["TrainingClasses:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl ?? throw new InvalidOperationException("TrainingClasses:BaseUrl saknas i konfigurationen."));
    client.Timeout = TimeSpan.FromSeconds(15);
    // client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// >>> alltid på
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

// app.UseHttpsRedirection();
app.MapControllers();

// Redirecta root -> swagger (nu funkar även i prod)
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

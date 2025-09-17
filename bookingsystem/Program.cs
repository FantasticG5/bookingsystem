using Data;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registrera EF Core med InMemory-databas (enkelt för MVP/test)
builder.Services.AddDbContext<BookingDbContext>(opt => opt.UseInMemoryDatabase("BookingDb"));

// Registrera våra tjänster (DI)
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Standard MVC/Swagger setup
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// (Valfritt) Lägg till en testbokning i minnet vid uppstart
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Bookings.Add(new Data.Entities.Booking { Id = 1, ClassId = 101, UserId = 42 });
    db.SaveChanges();
}

app.Run();

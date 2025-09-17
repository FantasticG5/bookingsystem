using Data;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core InMemory (MVP/test) ---
builder.Services.AddDbContext<BookingDbContext>(opt =>
    opt.UseInMemoryDatabase("BookingDb"));

// --- DI f�r tj�nster ---
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// --- Controllers + Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CORS: till�t din Vite-frontend ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", p =>
        p.WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    
    if (!db.Bookings.Any())
    {
        db.Bookings.AddRange(new[]
        {
            new Data.Entities.Booking { Id = 1, ClassId = 101, UserId = 1, CreatedAt = DateTime.UtcNow.AddDays(-1), IsCancelled = false },
            new Data.Entities.Booking { Id = 2, ClassId = 102, UserId = 2, CreatedAt = DateTime.UtcNow.AddDays(-2), IsCancelled = false },
            new Data.Entities.Booking { Id = 3, ClassId = 103, UserId = 3, CreatedAt = DateTime.UtcNow.AddDays(-3), IsCancelled = true }
        });
        db.SaveChanges();
    }
}

// --- Swagger i dev ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");


app.UseHttpsRedirection();

app.MapControllers();




app.Run();

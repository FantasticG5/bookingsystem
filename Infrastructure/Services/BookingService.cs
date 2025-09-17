using Data;
using Infrastructure.DTOs;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

// BookingService innehåller logik för att hantera bokningar
public class BookingService : IBookingService
{
    private readonly BookingDbContext _db;   // Databas-kopplingen
    private readonly IEmailService _email;   // E-posttjänsten

    public BookingService(BookingDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    // CancelAsync = markerar en bokning som avbokad + skickar mail
    public async Task<bool> CancelAsync(CancelBookingRequest request)
    {
        // Leta upp bokningen i databasen
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId);
        if (booking is null) return false;           // Hittades ej
        if (booking.IsCancelled) return true;        // Redan avbokad = OK (idempotent)

        // Markera som avbokad
        booking.IsCancelled = true;
        await _db.SaveChangesAsync();

        // Skapa mailinnehåll
        var subject = "Bekräftelse: Din avbokning är registrerad";
        var body =
$@"Hej {request.MemberName ?? "medlem"},

Din avbokning är nu registrerad.
Boknings-id: {booking.Id}
Pass-id: {booking.ClassId}
Tidpunkt (skapad): {booking.CreatedAt:yyyy-MM-dd HH:mm} UTC

Vänliga hälsningar,
Core Gym Club AB";

        // Skicka mail (mock eller SMTP)
        await _email.SendCancellationAsync(request.MemberEmail, subject, body);
        return true;
    }
}

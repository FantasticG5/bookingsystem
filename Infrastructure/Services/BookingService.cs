
using Data.Entities;
using Infrastructure.DTOs;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

public class BookingService(IBookingRepository repository, IEmailSender email) : IBookingService
{
    private readonly IBookingRepository _repository = repository;
    private readonly IEmailSender _email = email;

    public async Task<Booking> BookClassAsync(BookingDto dto)
    {
        // Simple rule: one booking per user/class
        var existing = await _repository.GetBookingAsync(dto.ClassId, dto.UserId);
        if (existing != null)
        {
            throw new InvalidOperationException("User has already booked this class.");
        }

        var booking = new Booking
        {
            ClassId = dto.ClassId,
            UserId = dto.UserId,
            CreatedAt = DateTime.UtcNow,
            IsCancelled = false
        };

        return await _repository.AddBookingAsync(booking);
    }

    public async Task<bool> CancelBookingAsync(CancelBookingDto dto)
    {
        var ok = await _repository.CancelBookingAsync(dto.ClassId, dto.UserId);
        if (!ok) return false;

        var subject = "Bekräftelse på avbokning";
        var html = $@"
          <div style=""font-family:Arial,sans-serif"">
            <h2>Din avbokning är registrerad</h2>
            <p>Hej!</p>
            <p>Vi har avbokat din plats för klass <b>#{dto.ClassId}</b>.</p>
            <ul>
              <li>Användare: {dto.UserId}</li>
              <li>Status: Avbokad</li>
            </ul>
            <p>Hör av dig om något verkar fel.</p>
            <p>/Teamet</p>
          </div>";

        await _email.SendAsync(dto.Email, subject, html);
        return true;
    }
}

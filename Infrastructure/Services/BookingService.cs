
using Data.Entities;
using Infrastructure.DTOs;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

public class BookingService(IBookingRepository repository, IEmailSender email) : IBookingService
{
    private readonly IBookingRepository _repository = repository;
    private readonly IEmailSender _email = email;

    public async Task<Booking> BookClassAsync(string userId, int classId, CancellationToken ct = default)
    {
        // Simple rule: one booking per user/class
        // en bokning per user/klass
        var existing = await _repository.GetBookingAsync(classId, userId);
        if (existing is not null)
            throw new InvalidOperationException("User has already booked this class.");

        var booking = new Booking
        {
            ClassId = classId,
            UserId  = userId,
            CreatedAt = DateTime.UtcNow,
            IsCancelled = false
        };

        return await _repository.AddBookingAsync(booking);
    }

    public async Task<IReadOnlyList<BookingReadDto>> GetByUserAsync(string userId, CancellationToken ct = default)
    {
        var items = await _repository.GetByUserAsync(userId, ct);
        return items.Select(b => new BookingReadDto(b.Id, b.ClassId, b.CreatedAt, b.IsCancelled)).ToList();
    }

    public async Task<bool> CancelBookingAsync(string userId, int classId, string? email = null, CancellationToken ct = default)
    {
       var changed = await _repository.CancelBookingAsync(classId, userId, ct);
    if (!changed) return false;

        if (!string.IsNullOrWhiteSpace(email))
        {
            var subject = "Bekräftelse på avbokning";
            var html = $@"
              <div style=""font-family:Arial,sans-serif"">
                <h2>Din avbokning är registrerad</h2>
                <p>Hej!</p>
                <p>Vi har avbokat din plats för klass <b>#{classId}</b>.</p>
                <p>/Teamet</p>
              </div>";
            await _email.SendAsync(email, subject, html);
        }
        return true;
    }
}

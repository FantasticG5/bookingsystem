using Data.Entities;
using Data.Interfaces;
using Infrastructure.Interfaces;

namespace Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;

    public BookingService (IBookingRepository repository)
    {
        _repository = repository;
    }
    public async Task<(CreateBookingResult Result, int? BookingId)> CreateBookingAsync(int classId, int userId, CancellationToken ct = default)
    {
        if (classId <= 0 || userId <= 0)
            return (CreateBookingResult.ClassNotFound, null);

        var existing = await _repository.GetUserAndClass(userId, classId, ct);
        if (existing is not null)
            return (CreateBookingResult.AlreadyBooked, null);

        var booking = new Booking
        {
            ClassId = classId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsCancelled = false
        };


        var saved = await _repository.AddBookingAsync(booking, ct);
        return (CreateBookingResult.Created, saved.Id);
    }
}

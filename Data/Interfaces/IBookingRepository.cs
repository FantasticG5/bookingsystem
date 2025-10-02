using Data.Entities;

namespace Infrastructure.Interfaces;

public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Booking booking);
    Task<Booking?> GetBookingAsync(int classId, string userId);
    Task<bool> CancelBookingAsync(int classId, string userId, CancellationToken ct = default);

    Task<IReadOnlyList<Booking>> GetByUserAsync(string userId, CancellationToken ct = default);

}

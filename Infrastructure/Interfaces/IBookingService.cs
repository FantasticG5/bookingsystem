
using Data.Entities;
using Infrastructure.DTOs;

namespace Infrastructure.Interfaces;

public interface IBookingService
{
    Task<Booking> BookClassAsync(string userId, int classId, string? email, CancellationToken ct = default);
    Task<IReadOnlyList<BookingReadDto>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<bool> CancelBookingAsync(string userId, int classId, string? email = null, CancellationToken ct = default);

}

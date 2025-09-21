using Data.Entities;

namespace Data.Interfaces;

public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Booking booking, CancellationToken ct = default);
    Task<Booking> GetUserAndClass(int userId, int classId, CancellationToken ct = default);

}

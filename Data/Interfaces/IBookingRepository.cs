using Data.Entities;

namespace Infrastructure.Interfaces;

public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Booking booking);
    Task<Booking?> GetBookingAsync(int classId, int userId);
}

namespace Infrastructure.Interfaces;


public enum CreateBookingResult { Created, AlreadyBooked, ClassNotFound}

public interface IBookingService
{
    Task<(CreateBookingResult Result, int? BookingId)> CreateBookingAsync(
        int classId, int userId, CancellationToken ct = default);
}

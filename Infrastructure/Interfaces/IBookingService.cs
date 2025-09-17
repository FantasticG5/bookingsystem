using Infrastructure.DTOs;

namespace Infrastructure.Interfaces;

// Interface för logik kring bokningar (Cancel, Create, etc.)
public interface IBookingService
{
    Task<bool> CancelAsync(CancelBookingRequest request);
}


using Data.Entities;
using Infrastructure.DTOs;

namespace Infrastructure.Interfaces;

public interface IBookingService
{
    Task<Booking> BookClassAsync(BookingDto dto);
     Task<bool> CancelBookingAsync(CancelBookingDto dto);
}

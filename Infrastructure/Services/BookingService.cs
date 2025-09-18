
using Data.Entities;
using Infrastructure.DTOs;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;

    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
    }

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
}

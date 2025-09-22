using Data;
using Data.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<Booking> AddBookingAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> GetBookingAsync(int classId, int userId)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.ClassId == classId && b.UserId == userId && !b.IsCancelled);
    }
}

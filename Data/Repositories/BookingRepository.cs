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

    public async Task<Booking?> GetBookingAsync(int classId, string userId)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.ClassId == classId && b.UserId == userId && !b.IsCancelled);
    }

    public async Task<IReadOnlyList<Booking>> GetByUserAsync(string userId, CancellationToken ct = default)
{
    if (string.IsNullOrWhiteSpace(userId))
        return Array.Empty<Booking>();

    return await _context.Bookings
        .AsNoTracking()
        .Where(b => b.UserId == userId && !b.IsCancelled)
        .OrderByDescending(b => b.CreatedAt)
        .ToListAsync(ct);
}

    public async Task<bool> CancelBookingAsync(int classId, string userId, CancellationToken ct = default)
    {
        var booking = await _context.Bookings
        .Where(b => b.ClassId == classId && b.UserId == userId && !b.IsCancelled)
        .OrderByDescending(b => b.CreatedAt) // ta senaste om flera
        .FirstOrDefaultAsync(ct);

    if (booking is null)
        return false;

    booking.IsCancelled = true;
    await _context.SaveChangesAsync(ct);
    return true; // state change skedde
    }
}

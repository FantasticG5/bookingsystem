using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Data;

// DbContext = databas-koppling för våra entiteter (Bookings)
public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Prefix "bs_" for bookingsystem
        modelBuilder.Entity<Booking>().ToTable("bs_Bookings");
    }
}
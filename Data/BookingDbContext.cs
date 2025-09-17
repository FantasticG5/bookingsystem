using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Data;

// DbContext = databas-koppling för våra entiteter (Bookings)
public class BookingDbContext : DbContext
{
    // ctor tar emot konfiguration från Program.cs
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    // En "DbSet" motsvarar en tabell i databasen
    public DbSet<Booking> Bookings => Set<Booking>();
}

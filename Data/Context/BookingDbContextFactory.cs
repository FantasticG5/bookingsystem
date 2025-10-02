
﻿// Data/Contexts/DataContextFactory.cs
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Data.Contexts;

public class DataContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        // Försök hitta appsettings.json smart: först CWD, sen ../eventsystem, sen parent
        var basePath = TryResolveBasePath();

        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "ASPNETCORE_") // plockar t.ex. ASPNETCORE_ConnectionStrings__DefaultConnection
            .Build();

        var cs =
            cfg.GetConnectionString("DefaultConnection") ??
            cfg.GetConnectionString("Default") ??
            throw new InvalidOperationException(
                "Ingen connection string hittades. Ange ConnectionStrings:DefaultConnection (eller Default) i appsettings.json " +
                "eller sätt env-variabeln ASPNETCORE_ConnectionStrings__DefaultConnection.");

        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new BookingDbContext(options);
    }

    private static string TryResolveBasePath()
    {
        var cwd = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(cwd, "appsettings.json"))) return cwd;

        var candidate = Path.GetFullPath(Path.Combine(cwd, "..", "bookingsystem"));
        if (File.Exists(Path.Combine(candidate, "appsettings.json"))) return candidate;

        var parent = Directory.GetParent(cwd)?.FullName;
        if (parent is not null && File.Exists(Path.Combine(parent, "appsettings.json"))) return parent;

        return cwd; // sista utvägen
    }
}

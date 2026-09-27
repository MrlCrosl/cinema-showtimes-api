using Cinema.Application.Abstractions;
using Cinema.Domain.Entities;
using Cinema.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Auditorium> Auditoriums => Set<Auditorium>();

    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<Showtime> Showtimes => Set<Showtime>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<ShowtimeSeat> ShowtimeSeats => Set<ShowtimeSeat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // All timestamps are stored as UTC; SQLite drops the Kind, so re-apply it when reading.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }
}

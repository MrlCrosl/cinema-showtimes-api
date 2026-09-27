using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Abstractions;

/// <summary>Unit of work over the cinema database. Implemented by the Infrastructure DbContext.</summary>
public interface IAppDbContext
{
    DbSet<Movie> Movies { get; }

    DbSet<Auditorium> Auditoriums { get; }

    DbSet<Seat> Seats { get; }

    DbSet<Showtime> Showtimes { get; }

    DbSet<Reservation> Reservations { get; }

    DbSet<ShowtimeSeat> ShowtimeSeats { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

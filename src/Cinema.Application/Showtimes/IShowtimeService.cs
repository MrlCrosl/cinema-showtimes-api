namespace Cinema.Application.Showtimes;

public interface IShowtimeService
{
    /// <exception cref="Cinema.Domain.Exceptions.NotFoundException">The movie or auditorium does not exist.</exception>
    /// <exception cref="Cinema.Domain.Exceptions.ConflictException">The showtime overlaps another one in the same auditorium.</exception>
    Task<ShowtimeResponse> CreateAsync(CreateShowtimeRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShowtimeResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <exception cref="Cinema.Domain.Exceptions.NotFoundException">The showtime does not exist.</exception>
    Task<ShowtimeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>All seats of the showtime's auditorium with their current availability, ordered by row and number.</summary>
    /// <exception cref="Cinema.Domain.Exceptions.NotFoundException">The showtime does not exist.</exception>
    Task<IReadOnlyList<SeatAvailabilityResponse>> GetSeatsAsync(Guid showtimeId, CancellationToken cancellationToken = default);
}

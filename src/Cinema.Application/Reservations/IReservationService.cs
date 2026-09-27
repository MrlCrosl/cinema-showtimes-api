namespace Cinema.Application.Reservations;

public interface IReservationService
{
    /// <exception cref="Cinema.Domain.Exceptions.NotFoundException">The showtime does not exist.</exception>
    /// <exception cref="Cinema.Domain.Exceptions.DomainValidationException">A requested seat does not belong to the showtime.</exception>
    /// <exception cref="Cinema.Domain.Exceptions.ConflictException">The showtime has started, a seat is taken, or a concurrent request won.</exception>
    Task<ReservationResponse> ReserveAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);

    /// <exception cref="Cinema.Domain.Exceptions.NotFoundException">The reservation does not exist.</exception>
    /// <exception cref="Cinema.Domain.Exceptions.ConflictException">The reservation is expired, already confirmed, or a concurrent request won.</exception>
    Task<ReservationResponse> ConfirmAsync(Guid reference, CancellationToken cancellationToken = default);

    /// <exception cref="Cinema.Domain.Exceptions.NotFoundException">The reservation does not exist.</exception>
    Task<ReservationResponse> GetAsync(Guid reference, CancellationToken cancellationToken = default);
}

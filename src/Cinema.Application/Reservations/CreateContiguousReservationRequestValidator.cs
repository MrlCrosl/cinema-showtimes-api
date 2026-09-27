using FluentValidation;

namespace Cinema.Application.Reservations;

public sealed class CreateContiguousReservationRequestValidator : AbstractValidator<CreateContiguousReservationRequest>
{
    public CreateContiguousReservationRequestValidator()
    {
        RuleFor(r => r.ShowtimeId).NotEmpty();

        RuleFor(r => r.Count).InclusiveBetween(1, CreateReservationRequestValidator.MaxSeatsPerReservation);
    }
}

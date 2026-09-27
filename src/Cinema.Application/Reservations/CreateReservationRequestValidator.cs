using FluentValidation;

namespace Cinema.Application.Reservations;

public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public const int MaxSeatsPerReservation = 10;

    public CreateReservationRequestValidator()
    {
        RuleFor(r => r.ShowtimeId).NotEmpty();

        RuleFor(r => r.SeatIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(ids => ids.Count <= MaxSeatsPerReservation)
            .WithMessage($"At most {MaxSeatsPerReservation} seats can be reserved at once.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("'Seat Ids' must not contain duplicates.");

        RuleForEach(r => r.SeatIds)
            .NotEmpty()
            .WithMessage("'Seat Ids' must not contain an empty id.");
    }
}

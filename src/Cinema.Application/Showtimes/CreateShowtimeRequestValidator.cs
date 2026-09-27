using FluentValidation;

namespace Cinema.Application.Showtimes;

public sealed class CreateShowtimeRequestValidator : AbstractValidator<CreateShowtimeRequest>
{
    public CreateShowtimeRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(r => r.MovieId).NotEmpty();

        RuleFor(r => r.AuditoriumId).NotEmpty();

        RuleFor(r => r.StartTime)
            .GreaterThan(_ => timeProvider.GetUtcNow())
            .WithMessage("'Start Time' must be in the future.");
    }
}

using Cinema.Domain.Entities;
using FluentValidation;

namespace Cinema.Application.Movies;

public sealed class CreateMovieRequestValidator : AbstractValidator<CreateMovieRequest>
{
    /// <summary>How many years ahead of the current UTC year a movie may be dated.</summary>
    public const int MaxYearsAhead = 5;

    public CreateMovieRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(r => r.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(r => r.Category)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(r => r.Year)
            .Must(year => year >= Movie.MinYear && year <= MaxYear(timeProvider))
            .WithMessage(_ => $"'Year' must be between {Movie.MinYear} and {MaxYear(timeProvider)}.");

        RuleFor(r => r.DurationMinutes)
            .InclusiveBetween(1, Movie.MaxDurationMinutes);
    }

    private static int MaxYear(TimeProvider timeProvider) => timeProvider.GetUtcNow().Year + MaxYearsAhead;
}

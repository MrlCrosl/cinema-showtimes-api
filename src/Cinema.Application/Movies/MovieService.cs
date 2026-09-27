using Cinema.Application.Abstractions;
using Cinema.Domain.Entities;
using Cinema.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Application.Movies;

public sealed class MovieService(IAppDbContext dbContext, IValidator<CreateMovieRequest> validator) : IMovieService
{
    public async Task<MovieResponse> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var movie = Movie.Create(request.Title, request.Category, request.Year, request.DurationMinutes);

        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(movie);
    }

    public async Task<IReadOnlyList<MovieResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movies = await dbContext.Movies
            .AsNoTracking()
            .OrderBy(m => m.Title)
            .ToListAsync(cancellationToken);

        return movies.Select(ToResponse).ToList();
    }

    public async Task<MovieResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await dbContext.Movies
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Movie), id);

        return ToResponse(movie);
    }

    private static MovieResponse ToResponse(Movie movie) =>
        new(movie.Id, movie.Title, movie.Category, movie.Year, movie.DurationMinutes);
}

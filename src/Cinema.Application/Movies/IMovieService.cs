namespace Cinema.Application.Movies;

public interface IMovieService
{
    Task<MovieResponse> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MovieResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <exception cref="Cinema.Domain.Exceptions.NotFoundException">The movie does not exist.</exception>
    Task<MovieResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

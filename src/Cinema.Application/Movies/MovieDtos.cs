namespace Cinema.Application.Movies;

public sealed record CreateMovieRequest(string Title, string Category, int Year, int DurationMinutes);

public sealed record MovieResponse(Guid Id, string Title, string Category, int Year, int DurationMinutes);

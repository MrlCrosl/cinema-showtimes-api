using Cinema.Application.Movies;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/movies")]
[Produces("application/json")]
public sealed class MoviesController(IMovieService movieService) : ControllerBase
{
    [EndpointSummary("Create a movie")]
    [HttpPost]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieResponse>> Create(CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = await movieService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [EndpointSummary("List movies, ordered by title")]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MovieResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MovieResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var movies = await movieService.GetAllAsync(cancellationToken);
        return Ok(movies);
    }

    [EndpointSummary("Get a movie by id")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var movie = await movieService.GetByIdAsync(id, cancellationToken);
        return Ok(movie);
    }
}

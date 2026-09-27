using Cinema.Application.Showtimes;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/showtimes")]
[Produces("application/json")]
public sealed class ShowtimesController(IShowtimeService showtimeService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ShowtimeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ShowtimeResponse>> Create(CreateShowtimeRequest request, CancellationToken cancellationToken)
    {
        var showtime = await showtimeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = showtime.Id }, showtime);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ShowtimeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ShowtimeResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var showtimes = await showtimeService.GetAllAsync(cancellationToken);
        return Ok(showtimes);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ShowtimeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShowtimeResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var showtime = await showtimeService.GetByIdAsync(id, cancellationToken);
        return Ok(showtime);
    }
}

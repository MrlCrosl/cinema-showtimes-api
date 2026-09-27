using Cinema.Application.Showtimes;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/showtimes")]
[Produces("application/json")]
public sealed class ShowtimesController(IShowtimeService showtimeService) : ControllerBase
{
    [EndpointSummary("Create a showtime")]
    [EndpointDescription("Rejects a showtime that overlaps another one in the same auditorium (409). The example uses the seeded movie and auditorium ids.")]
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

    [EndpointSummary("List showtimes, ordered by start time")]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ShowtimeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ShowtimeResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var showtimes = await showtimeService.GetAllAsync(cancellationToken);
        return Ok(showtimes);
    }

    [EndpointSummary("Get a showtime by id")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ShowtimeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShowtimeResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var showtime = await showtimeService.GetByIdAsync(id, cancellationToken);
        return Ok(showtime);
    }

    [EndpointSummary("Seat map of a showtime")]
    [EndpointDescription("Every seat of the auditorium with its status: Free, Reserved or Sold. A reserved seat whose hold has expired is reported as Free. Seat ids from this response are used to reserve.")]
    [HttpGet("{id:guid}/seats")]
    [ProducesResponseType<IReadOnlyList<SeatAvailabilityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SeatAvailabilityResponse>>> GetSeats(Guid id, CancellationToken cancellationToken)
    {
        var seats = await showtimeService.GetSeatsAsync(id, cancellationToken);
        return Ok(seats);
    }
}

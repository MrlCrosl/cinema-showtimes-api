using Cinema.Application.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
public sealed class ReservationsController(IReservationService reservationService) : ControllerBase
{
    [EndpointSummary("Reserve specific seats")]
    [EndpointDescription("Holds the seats for 10 minutes; confirm within that time. showtimeId comes from POST /api/showtimes or GET /api/showtimes; seatIds come from GET /api/showtimes/{id}/seats. The example seat ids are the seeded Hall 1 row 1 seats 1 and 2.")]
    [HttpPost]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = await reservationService.ReserveAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByReference), new { reference = reservation.Reference }, reservation);
    }

    /// <summary>Reserves the first available block of adjacent seats in one row.</summary>
    [EndpointSummary("Reserve a block of adjacent seats")]
    [EndpointDescription("The system picks the first available block of count adjacent seats in one row (lowest row, leftmost). showtimeId comes from POST /api/showtimes or GET /api/showtimes. No block available returns 409.")]
    [HttpPost("contiguous")]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> CreateContiguous(CreateContiguousReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = await reservationService.ReserveContiguousAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByReference), new { reference = reservation.Reference }, reservation);
    }

    [EndpointSummary("Get a reservation by reference")]
    [EndpointDescription("The reference is the value returned when the reservation was created. State is Pending, Confirmed or Expired.")]
    [HttpGet("{reference:guid}")]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetByReference(Guid reference, CancellationToken cancellationToken)
    {
        var reservation = await reservationService.GetAsync(reference, cancellationToken);
        return Ok(reservation);
    }

    [EndpointSummary("Confirm a reservation")]
    [EndpointDescription("Marks the reserved seats as sold. Fails with 409 if the reservation has expired or is already confirmed.")]
    [HttpPost("{reference:guid}/confirm")]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Confirm(Guid reference, CancellationToken cancellationToken)
    {
        var reservation = await reservationService.ConfirmAsync(reference, cancellationToken);
        return Ok(reservation);
    }
}

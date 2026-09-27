using Cinema.Application.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
public sealed class ReservationsController(IReservationService reservationService) : ControllerBase
{
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

    [HttpGet("{reference:guid}")]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetByReference(Guid reference, CancellationToken cancellationToken)
    {
        var reservation = await reservationService.GetAsync(reference, cancellationToken);
        return Ok(reservation);
    }

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

using System.Text.Json;
using System.Text.Json.Nodes;
using Cinema.Api.Controllers;
using Cinema.Application.Movies;
using Cinema.Application.Reservations;
using Cinema.Application.Showtimes;
using Cinema.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Cinema.Api.OpenApi;

/// <summary>
/// Adds a request body example to each POST endpoint. Examples use seeded ids so they work on a fresh database;
/// values that only exist at runtime (showtime id) are called out in the operation description.
/// </summary>
public sealed class RequestExamplesTransformer(IOptions<JsonOptions> jsonOptions) : IOpenApiOperationTransformer
{
    /// <summary>Placeholder for an id that is created at runtime; the description says where to get the real one.</summary>
    private static readonly Guid ShowtimePlaceholder = new("11111111-1111-7111-8111-111111111111");

    private static readonly IReadOnlyDictionary<(string Controller, string Action), object> Examples =
        new Dictionary<(string, string), object>
        {
            [("Movies", nameof(MoviesController.Create))] = new CreateMovieRequest("Dune: Part Two", "Sci-Fi", 2024, 166),
            [("Showtimes", nameof(ShowtimesController.Create))] = new CreateShowtimeRequest(
                SeedData.InceptionId,
                SeedData.Hall1Id,
                new DateTimeOffset(2030, 1, 1, 18, 0, 0, TimeSpan.Zero)),
            [("Reservations", nameof(ReservationsController.Create))] = new CreateReservationRequest(
                ShowtimePlaceholder,
                [SeedData.SeatId(SeedData.Hall1Id, 1, 1), SeedData.SeatId(SeedData.Hall1Id, 1, 2)]),
            [("Reservations", nameof(ReservationsController.CreateContiguous))] = new CreateContiguousReservationRequest(ShowtimePlaceholder, 3)
        };

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor action
            || !Examples.TryGetValue((action.ControllerName, action.ActionName), out var example)
            || operation.RequestBody?.Content is not { } content)
        {
            return Task.CompletedTask;
        }

        var exampleNode = JsonSerializer.SerializeToNode(example, example.GetType(), jsonOptions.Value.SerializerOptions);

        foreach (var mediaType in content.Values)
        {
            mediaType.Example = exampleNode?.DeepClone();
        }

        return Task.CompletedTask;
    }
}

using System.Diagnostics;
using Cinema.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.ErrorHandling;

/// <summary>Maps application and domain exceptions to RFC 9457 ProblemDetails responses.</summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            // The client went away; there is nobody to answer and nothing went wrong on our side.
            return true;
        }

        var problemDetails = Map(exception);

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        problemDetails.Instance = httpContext.Request.Path;
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        if (await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            }))
        {
            return true;
        }

        // The client did not accept a ProblemDetails content type; write JSON anyway so the status is not lost.
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static ProblemDetails Map(Exception exception) => exception switch
    {
        ValidationException validation => new ValidationProblemDetails(GroupErrors(validation))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        },
        DomainValidationException domainValidation => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed.",
            Detail = domainValidation.Message
        },
        NotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found.",
            Detail = notFound.Message
        },
        ConflictException conflict => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict.",
            Detail = conflict.Message
        },
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred."
        }
    };

    private static Dictionary<string, string[]> GroupErrors(ValidationException exception) =>
        exception.Errors
            .GroupBy(e => e.PropertyName, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.Ordinal);
}

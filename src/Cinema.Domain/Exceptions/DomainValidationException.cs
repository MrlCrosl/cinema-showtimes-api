namespace Cinema.Domain.Exceptions;

/// <summary>Raised when a domain invariant is violated by otherwise well-formed input. Maps to HTTP 400.</summary>
public sealed class DomainValidationException(string message) : Exception(message);

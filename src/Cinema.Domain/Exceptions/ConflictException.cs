namespace Cinema.Domain.Exceptions;

/// <summary>Raised when an operation conflicts with the current state (overlaps, taken seats, expired reservations). Maps to HTTP 409.</summary>
public sealed class ConflictException(string message) : Exception(message);

namespace Cinema.Domain.Exceptions;

/// <summary>Raised when an entity cannot be found by its key. Maps to HTTP 404.</summary>
public sealed class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with key '{key}' was not found.")
{
    public string EntityName { get; } = entityName;

    public object Key { get; } = key;
}

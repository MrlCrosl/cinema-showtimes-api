namespace Cinema.Domain.Entities;

public sealed class Seat
{
    private Seat()
    {
    }

    public Guid Id { get; private set; }

    public Guid AuditoriumId { get; private set; }

    public int Row { get; private set; }

    public int Number { get; private set; }

    public static Seat Create(Guid auditoriumId, int row, int number)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(auditoriumId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(row);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);

        return new Seat
        {
            Id = Guid.CreateVersion7(),
            AuditoriumId = auditoriumId,
            Row = row,
            Number = number
        };
    }
}

namespace Cinema.Domain.Entities;

public sealed class Auditorium
{
    private readonly List<Seat> _seats = [];

    private Auditorium()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<Seat> Seats => _seats;

    public static Auditorium Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Auditorium
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim()
        };
    }

    public Seat AddSeat(int row, int number)
    {
        if (_seats.Any(s => s.Row == row && s.Number == number))
        {
            throw new ArgumentException($"Seat {row}/{number} already exists in auditorium '{Name}'.");
        }

        var seat = Seat.Create(Id, row, number);
        _seats.Add(seat);
        return seat;
    }
}

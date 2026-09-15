namespace BattleShip.Models.Domain;

public sealed class Ship
{
    public string Name { get; }
    public int Size { get; }
    public Orientation Orientation { get; }
    public IReadOnlyList<Coordinate> Cells { get; }

    public Ship(string name, int size, Orientation orientation, IReadOnlyList<Coordinate> cells)
    {
        if (cells.Count != size)
            throw new ArgumentException($"Ship '{name}' expects {size} cells but received {cells.Count}.", nameof(cells));

        ValidateStraightLine(name, orientation, cells);

        Name = name;
        Size = size;
        Orientation = orientation;
        Cells = cells.ToList();
    }

    public bool Occupies(Coordinate coordinate) => Cells.Contains(coordinate);

    private static void ValidateStraightLine(string name, Orientation orientation, IReadOnlyList<Coordinate> cells)
    {
        for (var i = 0; i < cells.Count; i++)
        {
            var expected = orientation == Orientation.Horizontal
                ? new Coordinate(cells[0].Row, cells[0].Col + i)
                : new Coordinate(cells[0].Row + i, cells[0].Col);

            if (cells[i] != expected)
                throw new ArgumentException(
                    $"Ship '{name}' cells must form a contiguous {orientation.ToString().ToLowerInvariant()} line.",
                    nameof(cells));
        }
    }
}

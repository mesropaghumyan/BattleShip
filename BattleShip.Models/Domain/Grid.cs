namespace BattleShip.Models.Domain;

public sealed class Grid
{
    public const int Size = 10;

    private readonly List<Ship> _ships = [];

    public IReadOnlyList<Ship> Ships => _ships;

    public void PlaceShip(Ship ship)
    {
        foreach (var cell in ship.Cells)
        {
            if (!cell.IsWithinBounds(Size))
                throw new ShipOutOfBoundsException($"Ship '{ship.Name}' has a cell {cell} outside the {Size}x{Size} grid.");

            if (_ships.Any(existing => existing.Occupies(cell)))
                throw new ShipOverlapException($"Ship '{ship.Name}' overlaps an existing ship at {cell}.");
        }

        _ships.Add(ship);
    }

    public bool IsOccupied(Coordinate coordinate) => _ships.Any(ship => ship.Occupies(coordinate));
}

namespace BattleShip.Models.Domain;

public sealed class Grid
{
    public const int Size = 10;

    private readonly List<Ship> _ships = [];
    private readonly HashSet<Coordinate> _shotsPlayed = [];

    public IReadOnlyList<Ship> Ships => _ships;
    public IReadOnlyCollection<Coordinate> ShotsPlayed => _shotsPlayed;
    public bool IsFleetSunk => _ships.Count > 0 && _ships.All(ship => ship.Cells.All(_shotsPlayed.Contains));

    public IEnumerable<Coordinate> RemainingCells()
    {
        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                var cell = new Coordinate(row, col);
                if (!_shotsPlayed.Contains(cell))
                    yield return cell;
            }
        }
    }

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

    public ShotOutcome ResolveShot(Coordinate target)
    {
        if (!target.IsWithinBounds(Size))
            throw new ArgumentOutOfRangeException(nameof(target), target, $"Coordinate {target} is outside the {Size}x{Size} grid.");

        if (!_shotsPlayed.Add(target))
            throw new CellAlreadyPlayedException($"Cell {target} has already been played on this grid.");

        var hitShip = _ships.FirstOrDefault(ship => ship.Occupies(target));
        if (hitShip is null)
            return ShotOutcome.Miss;

        return hitShip.Cells.All(_shotsPlayed.Contains) ? ShotOutcome.Sunk : ShotOutcome.Hit;
    }

    /// <summary>
    /// Returns the outcome of a shot played at <paramref name="coordinate"/>, or <c>null</c> if that coordinate has not been played yet.
    /// Does not leak unhit ship coordinates.
    /// </summary>
    public ShotOutcome? GetShotOutcome(Coordinate coordinate)
    {
        if (!_shotsPlayed.Contains(coordinate))
            return null;

        var hitShip = _ships.FirstOrDefault(ship => ship.Occupies(coordinate));
        if (hitShip is null)
            return ShotOutcome.Miss;

        return hitShip.Cells.All(_shotsPlayed.Contains) ? ShotOutcome.Sunk : ShotOutcome.Hit;
    }

    /// <summary>
    /// Returns all coordinates played on this grid that hit a ship which has not yet been fully sunk.
    /// </summary>
    public IReadOnlyCollection<Coordinate> UnsunkHits =>
        _shotsPlayed
            .Where(shot =>
            {
                var ship = _ships.FirstOrDefault(s => s.Occupies(shot));
                return ship is not null && !ship.Cells.All(_shotsPlayed.Contains);
            })
            .ToList();
}

namespace BattleShip.Models.Domain;

public sealed class GameEngine(Random? random = null)
{
    private readonly Random _random = random ?? Random.Shared;

    public Grid CreateGridWithRandomFleet()
    {
        var grid = new Grid();

        foreach (var (name, size) in FleetBlueprint.Standard)
        {
            grid.PlaceShip(GenerateNonOverlappingShip(name, size, grid));
        }

        return grid;
    }

    public (Grid PlayerGrid, Grid ComputerGrid) CreateGrids() =>
        (CreateGridWithRandomFleet(), CreateGridWithRandomFleet());

    private const int MaxPlacementAttempts = 10_000;

    private Ship GenerateNonOverlappingShip(string name, int size, Grid grid)
    {
        for (var attempt = 0; attempt < MaxPlacementAttempts; attempt++)
        {
            var orientation = _random.Next(2) == 0 ? Orientation.Horizontal : Orientation.Vertical;
            var startRow = _random.Next(Grid.Size);
            var startCol = _random.Next(Grid.Size);
            var cells = BuildCells(startRow, startCol, size, orientation);

            if (cells.All(cell => cell.IsWithinBounds(Grid.Size)) && cells.All(cell => !grid.IsOccupied(cell)))
            {
                return new Ship(name, size, orientation, cells);
            }
        }

        throw new InvalidOperationException(
            $"Could not find a valid placement for ship '{name}' (size {size}) after {MaxPlacementAttempts} attempts.");
    }

    private static IReadOnlyList<Coordinate> BuildCells(int startRow, int startCol, int size, Orientation orientation)
    {
        var cells = new List<Coordinate>(size);

        for (var i = 0; i < size; i++)
        {
            cells.Add(orientation == Orientation.Horizontal
                ? new Coordinate(startRow, startCol + i)
                : new Coordinate(startRow + i, startCol));
        }

        return cells;
    }
}

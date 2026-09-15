using BattleShip.Models.Domain;

namespace BattleShip.Tests.Unit;

public class GameEngineTests
{
    [Fact]
    public void CreateGridWithRandomFleet_PlacesFleetWithoutOverlapOrOutOfBounds()
    {
        var engine = new GameEngine(new Random(42));

        var grid = engine.CreateGridWithRandomFleet();

        Assert.Equal(FleetBlueprint.Standard.Count, grid.Ships.Count);

        var allCells = grid.Ships.SelectMany(ship => ship.Cells).ToList();
        Assert.Equal(allCells.Count, allCells.Distinct().Count());
        Assert.All(allCells, cell => Assert.True(cell.IsWithinBounds(Grid.Size)));
    }

    [Fact]
    public void CreateGridWithRandomFleet_RespectsStandardFleetComposition()
    {
        var engine = new GameEngine(new Random(7));

        var grid = engine.CreateGridWithRandomFleet();

        var actual = grid.Ships
            .Select(ship => (ship.Name, ship.Size))
            .OrderBy(ship => ship.Name)
            .ToList();
        var expected = FleetBlueprint.Standard
            .OrderBy(ship => ship.Name)
            .ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreateGridWithRandomFleet_ShipsAreAlignedHorizontallyOrVertically()
    {
        var engine = new GameEngine(new Random(123));

        var grid = engine.CreateGridWithRandomFleet();

        foreach (var ship in grid.Ships)
        {
            var distinctRows = ship.Cells.Select(c => c.Row).Distinct().Count();
            var distinctCols = ship.Cells.Select(c => c.Col).Distinct().Count();

            var isHorizontalLine = distinctRows == 1 && distinctCols == ship.Size;
            var isVerticalLine = distinctCols == 1 && distinctRows == ship.Size;

            Assert.True(isHorizontalLine || isVerticalLine,
                $"Ship '{ship.Name}' is not aligned on a single row or column.");
        }
    }

    [Fact]
    public void CreateGrids_ReturnsTwoIndependentlyGeneratedGrids()
    {
        var engine = new GameEngine(new Random(99));

        var (playerGrid, computerGrid) = engine.CreateGrids();

        Assert.NotSame(playerGrid, computerGrid);
        Assert.Equal(FleetBlueprint.Standard.Count, playerGrid.Ships.Count);
        Assert.Equal(FleetBlueprint.Standard.Count, computerGrid.Ships.Count);

        var playerCells = playerGrid.Ships.SelectMany(s => s.Cells).OrderBy(c => c.Row).ThenBy(c => c.Col).ToList();
        var computerCells = computerGrid.Ships.SelectMany(s => s.Cells).OrderBy(c => c.Row).ThenBy(c => c.Col).ToList();
        Assert.NotEqual(playerCells, computerCells);
    }
}

using BattleShip.Models.Domain;

namespace BattleShip.Tests.Unit;

public class GridTests
{
    [Fact]
    public void PlaceShip_Throws_WhenShipOverlapsAnExistingShip()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        var overlapping = new Ship("Sous-marin", 3, Orientation.Vertical,
            [new Coordinate(0, 1), new Coordinate(1, 1), new Coordinate(2, 1)]);

        Assert.Throws<ShipOverlapException>(() => grid.PlaceShip(overlapping));
    }

    [Fact]
    public void PlaceShip_Throws_WhenShipCellIsOutOfBounds()
    {
        var grid = new Grid();
        var outOfBounds = new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 9), new Coordinate(0, 10)]);

        Assert.Throws<ShipOutOfBoundsException>(() => grid.PlaceShip(outOfBounds));
    }

    [Fact]
    public void PlaceShip_Succeeds_WhenShipsAreAdjacentButNotOverlapping()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        var adjacent = new Ship("Sous-marin", 3, Orientation.Vertical,
            [new Coordinate(1, 0), new Coordinate(2, 0), new Coordinate(3, 0)]);

        grid.PlaceShip(adjacent);

        Assert.Equal(2, grid.Ships.Count);
    }

    [Fact]
    public void IsOccupied_ReturnsTrueOnlyForCellsCoveredByAShip()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        Assert.True(grid.IsOccupied(new Coordinate(0, 0)));
        Assert.False(grid.IsOccupied(new Coordinate(0, 2)));
    }
}

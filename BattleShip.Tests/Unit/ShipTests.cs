using BattleShip.Models.Domain;

namespace BattleShip.Tests.Unit;

public class ShipTests
{
    [Fact]
    public void Constructor_Throws_WhenCellCountDoesNotMatchSize()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ship("Torpilleur", 2, Orientation.Horizontal, [new Coordinate(0, 0)]));
    }

    [Fact]
    public void Constructor_Throws_WhenCellsAreNotContiguous()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ship("Torpilleur", 2, Orientation.Horizontal, [new Coordinate(0, 0), new Coordinate(0, 2)]));
    }

    [Fact]
    public void Constructor_Throws_WhenCellsDoNotMatchDeclaredOrientation()
    {
        // cells form a vertical line but the ship claims to be horizontal
        Assert.Throws<ArgumentException>(() =>
            new Ship("Torpilleur", 2, Orientation.Horizontal, [new Coordinate(0, 0), new Coordinate(1, 0)]));
    }

    [Fact]
    public void Constructor_Succeeds_ForAContiguousHorizontalLine()
    {
        var ship = new Ship("Torpilleur", 2, Orientation.Horizontal, [new Coordinate(3, 3), new Coordinate(3, 4)]);

        Assert.True(ship.Occupies(new Coordinate(3, 3)));
        Assert.True(ship.Occupies(new Coordinate(3, 4)));
        Assert.False(ship.Occupies(new Coordinate(3, 5)));
    }

    [Fact]
    public void Constructor_Succeeds_ForAContiguousVerticalLine()
    {
        var ship = new Ship("Torpilleur", 2, Orientation.Vertical, [new Coordinate(3, 3), new Coordinate(4, 3)]);

        Assert.True(ship.Occupies(new Coordinate(3, 3)));
        Assert.True(ship.Occupies(new Coordinate(4, 3)));
    }
}

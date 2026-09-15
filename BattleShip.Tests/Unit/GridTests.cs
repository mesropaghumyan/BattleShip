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

    [Fact]
    public void ResolveShot_ReturnsMiss_WhenNoShipOccupiesTheCell()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        Assert.Equal(ShotOutcome.Miss, grid.ResolveShot(new Coordinate(5, 5)));
    }

    [Fact]
    public void ResolveShot_ReturnsHit_WhenShipIsTouchedButNotFullySunk()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        Assert.Equal(ShotOutcome.Hit, grid.ResolveShot(new Coordinate(0, 0)));
    }

    [Fact]
    public void ResolveShot_ReturnsSunk_WhenEveryCellOfAShipHasBeenHit()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        grid.ResolveShot(new Coordinate(0, 0));
        Assert.Equal(ShotOutcome.Sunk, grid.ResolveShot(new Coordinate(0, 1)));
    }

    [Fact]
    public void ResolveShot_Throws_AndDoesNotCountAsANewMove_WhenCellAlreadyPlayed()
    {
        var grid = new Grid();
        grid.ResolveShot(new Coordinate(3, 3));

        Assert.Throws<CellAlreadyPlayedException>(() => grid.ResolveShot(new Coordinate(3, 3)));
        Assert.Single(grid.ShotsPlayed);
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public void ResolveShot_Throws_WhenTargetIsOutOfBounds(int row, int col)
    {
        var grid = new Grid();

        Assert.Throws<ArgumentOutOfRangeException>(() => grid.ResolveShot(new Coordinate(row, col)));
    }

    [Fact]
    public void IsFleetSunk_IsFalse_WhenNoShipsArePlaced()
    {
        var grid = new Grid();

        Assert.False(grid.IsFleetSunk);
    }

    [Fact]
    public void IsFleetSunk_IsTrue_OnlyAfterEveryShipCellIsHit()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        grid.ResolveShot(new Coordinate(0, 0));
        Assert.False(grid.IsFleetSunk);

        grid.ResolveShot(new Coordinate(0, 1));
        Assert.True(grid.IsFleetSunk);
    }

    [Fact]
    public void IsFleetSunk_RequiresEveryShip_NotJustOne()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));
        grid.PlaceShip(new Ship("Sous-marin", 3, Orientation.Vertical,
            [new Coordinate(5, 5), new Coordinate(6, 5), new Coordinate(7, 5)]));

        grid.ResolveShot(new Coordinate(0, 0));
        grid.ResolveShot(new Coordinate(0, 1));
        Assert.False(grid.IsFleetSunk);

        grid.ResolveShot(new Coordinate(5, 5));
        grid.ResolveShot(new Coordinate(6, 5));
        grid.ResolveShot(new Coordinate(7, 5));
        Assert.True(grid.IsFleetSunk);
    }

    [Fact]
    public void GetShotOutcome_ReturnsNull_WhenCellHasNotBeenPlayed()
    {
        var grid = new Grid();

        Assert.Null(grid.GetShotOutcome(new Coordinate(3, 3)));
    }

    [Fact]
    public void GetShotOutcome_ReturnsMiss_WhenShotMisses()
    {
        var grid = new Grid();
        grid.ResolveShot(new Coordinate(3, 3));

        Assert.Equal(ShotOutcome.Miss, grid.GetShotOutcome(new Coordinate(3, 3)));
    }

    [Fact]
    public void GetShotOutcome_ReturnsHit_WhenShipIsTouchedButNotSunk()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(1, 1), new Coordinate(1, 2)]));

        grid.ResolveShot(new Coordinate(1, 1));

        Assert.Equal(ShotOutcome.Hit, grid.GetShotOutcome(new Coordinate(1, 1)));
    }

    [Fact]
    public void GetShotOutcome_ReturnsSunk_WhenShipIsFullySunk()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(1, 1), new Coordinate(1, 2)]));

        grid.ResolveShot(new Coordinate(1, 1));
        grid.ResolveShot(new Coordinate(1, 2));

        Assert.Equal(ShotOutcome.Sunk, grid.GetShotOutcome(new Coordinate(1, 1)));
        Assert.Equal(ShotOutcome.Sunk, grid.GetShotOutcome(new Coordinate(1, 2)));
    }

    [Fact]
    public void UnsunkHits_ContainsOnlyHitsOnShipsNotYetSunk()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(1, 1), new Coordinate(1, 2)]));
        grid.PlaceShip(new Ship("Sous-marin", 3, Orientation.Vertical,
            [new Coordinate(5, 5), new Coordinate(6, 5), new Coordinate(7, 5)]));

        // Un raté
        grid.ResolveShot(new Coordinate(0, 0));
        Assert.Empty(grid.UnsunkHits);

        // Une touche sur le torpilleur (non coulé)
        grid.ResolveShot(new Coordinate(1, 1));
        Assert.Equal([new Coordinate(1, 1)], grid.UnsunkHits);

        // Une touche sur le sous-marin (non coulé)
        grid.ResolveShot(new Coordinate(5, 5));
        Assert.Contains(new Coordinate(1, 1), grid.UnsunkHits);
        Assert.Contains(new Coordinate(5, 5), grid.UnsunkHits);
        Assert.Equal(2, grid.UnsunkHits.Count);

        // On coule le torpilleur : ses cases ne doivent plus être dans UnsunkHits
        grid.ResolveShot(new Coordinate(1, 2));
        Assert.Equal([new Coordinate(5, 5)], grid.UnsunkHits);
    }
}


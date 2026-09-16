using BattleShip.Models.Domain;
using BattleShip.Models.Domain.Opponent;

namespace BattleShip.Tests.Unit;

public class GameTests
{
    private static Grid SingleShipGrid(Coordinate cell) =>
        BuildGrid(new Ship("Vedette", 1, Orientation.Horizontal, [cell]));

    private static Grid BuildGrid(Ship ship)
    {
        var grid = new Grid();
        grid.PlaceShip(ship);
        return grid;
    }

    [Fact]
    public void ApplyShot_HumanShooter_ResolvesAgainstComputerGrid()
    {
        var computerGrid = SingleShipGrid(new Coordinate(0, 0));
        var game = new Game(new Grid(), computerGrid);

        var outcome = game.ApplyShot(Side.Human, new Coordinate(0, 0));

        Assert.Equal(ShotOutcome.Sunk, outcome);
        Assert.Single(computerGrid.ShotsPlayed);
    }

    [Fact]
    public void ApplyShot_ComputerShooter_ResolvesAgainstHumanGrid()
    {
        var humanGrid = SingleShipGrid(new Coordinate(0, 0));
        var game = new Game(humanGrid, new Grid());

        var outcome = game.ApplyShot(Side.Computer, new Coordinate(0, 0));

        Assert.Equal(ShotOutcome.Sunk, outcome);
        Assert.Single(humanGrid.ShotsPlayed);
    }

    [Fact]
    public void ApplyShot_FinishesGameAndSetsWinner_WhenTargetFleetIsFullySunk()
    {
        var computerGrid = SingleShipGrid(new Coordinate(0, 0));
        var game = new Game(new Grid(), computerGrid);

        game.ApplyShot(Side.Human, new Coordinate(0, 0));

        Assert.Equal(GameStatus.Finished, game.Status);
        Assert.Equal(Side.Human, game.Winner);
    }

    [Fact]
    public void ApplyShot_DoesNotFinishGame_WhenFleetIsNotFullySunk()
    {
        var computerGrid = BuildGrid(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));
        var game = new Game(new Grid(), computerGrid);

        game.ApplyShot(Side.Human, new Coordinate(0, 0));

        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.Winner);
    }

    [Fact]
    public void ApplyShot_Throws_WhenGameAlreadyFinished()
    {
        var computerGrid = SingleShipGrid(new Coordinate(0, 0));
        var game = new Game(new Grid(), computerGrid);
        game.ApplyShot(Side.Human, new Coordinate(0, 0));

        Assert.Throws<GameAlreadyFinishedException>(() => game.ApplyShot(Side.Human, new Coordinate(1, 1)));
    }

    [Fact]
    public void ApplyShot_RejectedReplay_DoesNotChangeGameStatusOrWinner()
    {
        var computerGrid = BuildGrid(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));
        var game = new Game(new Grid(), computerGrid);
        game.ApplyShot(Side.Human, new Coordinate(0, 0));

        Assert.Throws<CellAlreadyPlayedException>(() => game.ApplyShot(Side.Human, new Coordinate(0, 0)));
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.Winner);
        Assert.Single(computerGrid.ShotsPlayed);
    }

    [Fact]
    public void ApplyShot_DoesNotFinishGame_WhenOnlyOneOfTwoShipsIsSunk()
    {
        var computerGrid = new Grid();
        computerGrid.PlaceShip(new Ship("Vedette", 1, Orientation.Horizontal, [new Coordinate(0, 0)]));
        computerGrid.PlaceShip(new Ship("Sous-marin", 3, Orientation.Vertical,
            [new Coordinate(5, 5), new Coordinate(6, 5), new Coordinate(7, 5)]));
        var game = new Game(new Grid(), computerGrid);

        game.ApplyShot(Side.Human, new Coordinate(0, 0));

        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Null(game.Winner);
    }

    [Fact]
    public void ApplyShot_Throws_ForAnUndefinedSideValue()
    {
        var game = new Game(new Grid(), new Grid());
        var undefinedSide = (Side)99;

        Assert.Throws<ArgumentOutOfRangeException>(() => game.ApplyShot(undefinedSide, new Coordinate(0, 0)));
    }

    [Fact]
    public void Constructor_Throws_WhenAGridIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new Game(null!, new Grid()));
        Assert.Throws<ArgumentNullException>(() => new Game(new Grid(), null!));
    }

    [Fact]
    public void Constructor_Throws_WhenBothGridsAreTheSameInstance()
    {
        var sharedGrid = new Grid();

        Assert.Throws<ArgumentException>(() => new Game(sharedGrid, sharedGrid));
    }

    [Theory]
    [InlineData(Difficulty.Easy, typeof(EasyOpponentStrategy))]
    [InlineData(Difficulty.Hard, typeof(HardOpponentStrategy))]
    public void ResolveStrategy_SelectsStrategyForDifficulty(Difficulty difficulty, Type expectedType)
    {
        var game = new Game(new Grid(), new Grid(), difficulty);

        Assert.Equal(difficulty, game.Difficulty);
        Assert.IsType(expectedType, game.OpponentStrategy);
    }

    [Fact]
    public void Constructor_Throws_ForUnknownDifficulty()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Game(new Grid(), new Grid(), (Difficulty)99));
    }
}

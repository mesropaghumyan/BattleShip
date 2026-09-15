using BattleShip.Models.Domain;
using BattleShip.Models.Domain.Opponent;

namespace BattleShip.Tests.Unit;

public class EasyOpponentStrategyTests
{
    [Fact]
    public void ChooseShot_NeverReturnsAnAlreadyPlayedCell()
    {
        var grid = new Grid();
        IOpponentStrategy strategy = new EasyOpponentStrategy(new Random(1));

        for (var i = 0; i < 50; i++)
        {
            var shot = strategy.ChooseShot(grid);
            Assert.DoesNotContain(shot, grid.ShotsPlayed);
            grid.ResolveShot(shot);
        }
    }

    [Fact]
    public void ChooseShot_ReturnsTheLastRemainingCell_WhenOnlyOneIsUnplayed()
    {
        var grid = new Grid();
        var lastCell = new Coordinate(9, 9);

        for (var row = 0; row < Grid.Size; row++)
        {
            for (var col = 0; col < Grid.Size; col++)
            {
                var cell = new Coordinate(row, col);
                if (cell != lastCell)
                    grid.ResolveShot(cell);
            }
        }

        var strategy = new EasyOpponentStrategy();

        Assert.Equal(lastCell, strategy.ChooseShot(grid));
    }

    [Fact]
    public void ChooseShot_Throws_WhenGridIsFullyPlayed()
    {
        var grid = new Grid();
        for (var row = 0; row < Grid.Size; row++)
        {
            for (var col = 0; col < Grid.Size; col++)
            {
                grid.ResolveShot(new Coordinate(row, col));
            }
        }

        var strategy = new EasyOpponentStrategy();

        Assert.Throws<InvalidOperationException>(() => strategy.ChooseShot(grid));
    }

    [Fact]
    public void ChooseShot_Throws_WhenGridIsNull()
    {
        var strategy = new EasyOpponentStrategy();

        Assert.Throws<ArgumentNullException>(() => strategy.ChooseShot(null!));
    }

    [Fact]
    public void ChooseShot_IsDeterministic_ForTheSameInjectedSeed()
    {
        var strategyA = new EasyOpponentStrategy(new Random(2024));
        var strategyB = new EasyOpponentStrategy(new Random(2024));
        var gridA = new Grid();
        var gridB = new Grid();

        var sequenceA = new List<Coordinate>();
        var sequenceB = new List<Coordinate>();

        for (var i = 0; i < 20; i++)
        {
            var shotA = strategyA.ChooseShot(gridA);
            var shotB = strategyB.ChooseShot(gridB);
            sequenceA.Add(shotA);
            sequenceB.Add(shotB);
            gridA.ResolveShot(shotA);
            gridB.ResolveShot(shotB);
        }

        Assert.Equal(sequenceA, sequenceB);
    }
}

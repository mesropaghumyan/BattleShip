using BattleShip.Models.Domain;

namespace BattleShip.Tests.Unit;

public class CoordinateTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(10, 0, false)]
    [InlineData(0, 10, false)]
    public void IsWithinBounds_MatchesGridEdges(int row, int col, bool expected)
    {
        var coordinate = new Coordinate(row, col);

        Assert.Equal(expected, coordinate.IsWithinBounds(10));
    }
}

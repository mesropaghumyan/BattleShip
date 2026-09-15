namespace BattleShip.Models.Domain;

public readonly record struct Coordinate(int Row, int Col)
{
    public bool IsWithinBounds(int gridSize) => Row >= 0 && Row < gridSize && Col >= 0 && Col < gridSize;
}

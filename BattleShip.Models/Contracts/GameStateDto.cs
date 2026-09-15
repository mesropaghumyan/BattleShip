using BattleShip.Models.Domain;

namespace BattleShip.Models.Contracts;

/// <summary>
/// Composite, two-sided view of a game's state, produced by <c>GameViewMapper</c>.
/// <c>OwnGrid</c> is the human player's full grid (own ships always visible).
/// <c>OpponentGrid</c> is the computer's grid as seen by the human player: only shots taken
/// and ships that have been fully sunk are exposed; unsunk enemy ships are never leaked.
/// </summary>
public sealed class GameStateDto
{
    public required Guid GameId { get; init; }
    public required GameOutcome Status { get; init; }
    public required GridViewDto OwnGrid { get; init; }
    public required GridViewDto OpponentGrid { get; init; }
}

/// <summary>
/// Game status relative to the human player (as opposed to the domain's shooter-agnostic <see cref="GameStatus"/>).
/// </summary>
public enum GameOutcome
{
    InProgress,
    Won,
    Lost
}

public sealed class GridViewDto
{
    public required IReadOnlyList<ShipViewDto> Ships { get; init; }
    public required IReadOnlyList<ShotResultDto> Shots { get; init; }
}

public sealed class ShipViewDto
{
    public required string Name { get; init; }
    public required int Size { get; init; }
    public required Orientation Orientation { get; init; }
    public required IReadOnlyList<CoordinateDto> Cells { get; init; }
    public required bool IsSunk { get; init; }
}

public sealed class ShotResultDto
{
    public required CoordinateDto Coordinate { get; init; }
    public required ShotOutcome Outcome { get; init; }
}

public readonly record struct CoordinateDto(int Row, int Col);

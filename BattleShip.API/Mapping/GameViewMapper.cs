using BattleShip.Models.Contracts;
using BattleShip.Models.Domain;

namespace BattleShip.API.Mapping;

/// <summary>
/// Sole conversion point from the domain model (<see cref="Game"/>/<see cref="Grid"/>/<see cref="Ship"/>) to the
/// HTTP contract (<see cref="GameStateDto"/>). No endpoint serializes <c>Grid</c>/<c>Ship</c> directly (AD-2).
/// This is also the single place responsible for never leaking the coordinates of an opponent ship that has not
/// been fully sunk yet.
/// </summary>
public static class GameViewMapper
{
    public static GameStateDto ToDto(Guid gameId, Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        return new GameStateDto
        {
            GameId = gameId,
            Status = ToOutcome(game),
            OwnGrid = ToOwnGridView(game.HumanGrid),
            OpponentGrid = ToOpponentGridView(game.ComputerGrid)
        };
    }

    private static GameOutcome ToOutcome(Game game)
    {
        if (game.Status == GameStatus.InProgress)
            return GameOutcome.InProgress;

        if (game.Winner is null)
            throw new InvalidOperationException("A finished game must have a winner.");

        return game.Winner == Side.Human ? GameOutcome.Won : GameOutcome.Lost;
    }

    /// <summary>The human player's own grid: every own ship is always visible, whatever its state.</summary>
    private static GridViewDto ToOwnGridView(Grid grid) => new()
    {
        Ships = grid.Ships.Select(ship => ToShipView(ship, grid)).ToList(),
        Shots = grid.ShotsPlayed.Select(shot => ToShotResult(shot, grid)).ToList()
    };

    /// <summary>
    /// The computer's grid as seen by the human player: only ships that are fully sunk are exposed; ships that
    /// have not been fully sunk are never included, even if partially hit (AD-2).
    /// </summary>
    private static GridViewDto ToOpponentGridView(Grid grid) => new()
    {
        Ships = grid.Ships
            .Where(ship => IsSunk(ship, grid))
            .Select(ship => ToShipView(ship, grid))
            .ToList(),
        Shots = grid.ShotsPlayed.Select(shot => ToShotResult(shot, grid)).ToList()
    };

    private static ShipViewDto ToShipView(Ship ship, Grid grid) => new()
    {
        Name = ship.Name,
        Size = ship.Size,
        Orientation = ship.Orientation,
        Cells = ship.Cells.Select(cell => new CoordinateDto(cell.Row, cell.Col)).ToList(),
        IsSunk = IsSunk(ship, grid)
    };

    private static ShotResultDto ToShotResult(Coordinate coordinate, Grid grid) => new()
    {
        Coordinate = new CoordinateDto(coordinate.Row, coordinate.Col),
        Outcome = DetermineOutcome(coordinate, grid)
    };

    private static ShotOutcome DetermineOutcome(Coordinate coordinate, Grid grid)
    {
        var hitShip = grid.Ships.FirstOrDefault(ship => ship.Occupies(coordinate));
        if (hitShip is null)
            return ShotOutcome.Miss;

        return IsSunk(hitShip, grid) ? ShotOutcome.Sunk : ShotOutcome.Hit;
    }

    private static bool IsSunk(Ship ship, Grid grid) => ship.Cells.All(grid.ShotsPlayed.Contains);
}

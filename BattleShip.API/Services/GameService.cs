using BattleShip.API.Mapping;
using BattleShip.API.State;
using BattleShip.Models.Contracts;
using BattleShip.Models.Domain;

namespace BattleShip.API.Services;

/// <summary>
/// Orchestrates game creation and state reads. The only component allowed to call <see cref="IGameStore"/> (AD-3).
/// </summary>
public sealed class GameService(IGameStore gameStore, GameEngine gameEngine)
{
    /// <summary>
    /// Creates a new game with two randomly-populated fleets and registers it in the store.
    /// <paramref name="difficulty"/> is expected to already have been validated by FluentValidation (AD-5);
    /// this epic only ever creates the human vs. Easy-computer matchup regardless of the value passed in, since
    /// <c>Hard</c> is rejected upstream.
    /// </summary>
    /// <returns>The id of the newly created game.</returns>
    public Guid CreateGame(Difficulty difficulty)
    {
        var (playerGrid, computerGrid) = gameEngine.CreateGrids();
        var game = new Game(playerGrid, computerGrid);

        var gameId = Guid.NewGuid();
        gameStore.Add(gameId, game);

        return gameId;
    }

    /// <summary>Returns the two-sided state view for <paramref name="gameId"/>, or <c>null</c> if unknown.</summary>
    public GameStateDto? GetGameState(Guid gameId) =>
        gameStore.WithGame(gameId, game => GameViewMapper.ToDto(gameId, game));
}

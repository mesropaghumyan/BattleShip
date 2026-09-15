using BattleShip.Models.Domain;

namespace BattleShip.API.State;

/// <summary>
/// In-memory registry of active games. Registered as a singleton (AD-3).
/// <c>GameService</c> is the only component allowed to call this store (AD-3).
/// </summary>
public interface IGameStore
{
    /// <summary>
    /// Registers a newly created game under <paramref name="gameId"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">A game already exists for <paramref name="gameId"/>.</exception>
    void Add(Guid gameId, Game game);

    /// <summary>
    /// Runs <paramref name="action"/> against the game stored under <paramref name="gameId"/>, holding a
    /// per-game lock for the duration (AD-3), and returns its result. Returns <c>null</c> when no game is
    /// registered for <paramref name="gameId"/>; <paramref name="action"/> is not invoked in that case.
    /// </summary>
    TResult? WithGame<TResult>(Guid gameId, Func<Game, TResult> action) where TResult : class;
}

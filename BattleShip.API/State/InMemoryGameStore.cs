using System.Collections.Concurrent;
using BattleShip.Models.Domain;

namespace BattleShip.API.State;

/// <summary>
/// Singleton, in-memory implementation of <see cref="IGameStore"/>. Games are kept in a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>; access to a given game is additionally serialized through a
/// per-game lock so that a concurrent read (e.g. state polling) and a future write (shot resolution, Story 1.5)
/// on the same game never interleave (AD-3).
/// </summary>
public sealed class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();
    private readonly ConcurrentDictionary<Guid, object> _locks = new();

    public void Add(Guid gameId, Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var gameLock = _locks.GetOrAdd(gameId, static _ => new object());
        lock (gameLock)
        {
            if (!_games.TryAdd(gameId, game))
                throw new InvalidOperationException($"A game with id '{gameId}' already exists.");
        }
    }

    public TResult? WithGame<TResult>(Guid gameId, Func<Game, TResult> action) where TResult : class
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!_games.TryGetValue(gameId, out var game))
            return null;

        var gameLock = _locks.GetOrAdd(gameId, static _ => new object());
        lock (gameLock)
        {
            return action(game);
        }
    }
}

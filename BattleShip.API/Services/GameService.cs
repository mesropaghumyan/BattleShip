using BattleShip.API.Mapping;
using BattleShip.API.State;
using BattleShip.Models.Contracts;
using BattleShip.Models.Domain;
using BattleShip.Models.Domain.Opponent;

namespace BattleShip.API.Services;

/// <summary>
/// Orchestrates game creation, state reads and shot resolution. The only component allowed to call
/// <see cref="IGameStore"/> (AD-3).
/// </summary>
public sealed class GameService(
    IGameStore gameStore,
    GameEngine gameEngine,
    Func<Difficulty, IOpponentStrategy>? strategyFactory = null)
{
    public GameService(IGameStore gameStore, GameEngine gameEngine, IOpponentStrategy opponentStrategy)
        : this(gameStore, gameEngine, _ => opponentStrategy)
    {
    }

    /// <summary>
    /// Creates a new game with two randomly-populated fleets and registers it in the store
    /// with the appropriate opponent strategy according to <paramref name="difficulty"/>.
    /// <paramref name="difficulty"/> is expected to already have been validated by FluentValidation (AD-5).
    /// </summary>
    /// <returns>The id of the newly created game.</returns>
    public Guid CreateGame(Difficulty difficulty)
    {
        var (playerGrid, computerGrid) = gameEngine.CreateGrids();
        var strategy = strategyFactory?.Invoke(difficulty);
        var game = new Game(playerGrid, computerGrid, difficulty, strategy);

        var gameId = Guid.NewGuid();
        gameStore.Add(gameId, game);

        return gameId;
    }

    /// <summary>Returns the two-sided state view for <paramref name="gameId"/>, or <c>null</c> if unknown.</summary>
    public GameStateDto? GetGameState(Guid gameId) =>
        gameStore.WithGame(gameId, game => GameViewMapper.ToDto(gameId, game));

    /// <summary>
    /// Plays a single turn for <paramref name="gameId"/>: applies the player's shot at <paramref name="coordinate"/>
    /// then, if the game is not over as a result, resolves the computer's immediate riposte
    /// (using the game's configured <see cref="IOpponentStrategy"/>) in the same atomic <see cref="IGameStore.WithGame{TResult}"/> access
    /// (AD-3, AD-4). <paramref name="coordinate"/> is expected to already have been validated by FluentValidation
    /// (AD-5); the domain's own guards still apply (already-played cell, already-finished game).
    /// </summary>
    /// <returns>The outcome of the turn, or <c>null</c> if no game is registered for <paramref name="gameId"/>.</returns>
    /// <exception cref="Models.Domain.CellAlreadyPlayedException">The targeted cell was already played.</exception>
    /// <exception cref="Models.Domain.GameAlreadyFinishedException">The game had already finished.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="coordinate"/> is outside the grid.</exception>
    public PlayShotResult? PlayShot(Guid gameId, Coordinate coordinate) =>
        gameStore.WithGame(gameId, game =>
        {
            var playerOutcome = game.ApplyShot(Side.Human, coordinate);
            var playerShot = new ShotAttempt(coordinate, playerOutcome);

            ShotAttempt? computerShot = null;
            if (game.Status == GameStatus.InProgress)
            {
                var computerCoordinate = game.OpponentStrategy.ChooseShot(game.HumanGrid);
                var computerOutcome = game.ApplyShot(Side.Computer, computerCoordinate);
                computerShot = new ShotAttempt(computerCoordinate, computerOutcome);
            }

            return new PlayShotResult(playerShot, computerShot, game.Status, game.Winner);
        });
}

/// <summary>A single resolved shot: where it landed and what it did.</summary>
public sealed record ShotAttempt(Coordinate Coordinate, ShotOutcome Outcome);

/// <summary>
/// Result of <see cref="GameService.PlayShot"/>: the player's shot, the computer's riposte (absent if the
/// player's shot ended the game), and the resulting game status. Internal plumbing between <c>GameService</c>
/// and <c>BattlefieldGrpcService</c> — not a wire contract; the wire contract is <c>ShotTurnReply</c> (AD-9).
/// </summary>
public sealed record PlayShotResult(ShotAttempt PlayerShot, ShotAttempt? ComputerShot, GameStatus GameStatus, Side? Winner);

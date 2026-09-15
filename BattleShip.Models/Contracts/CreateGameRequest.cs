using BattleShip.Models.Domain;

namespace BattleShip.Models.Contracts;

/// <summary>
/// HTTP request body for <c>POST /games</c>.
/// </summary>
public sealed class CreateGameRequest
{
    /// <summary>
    /// Nullable so a missing field is distinguishable from a valid value by FluentValidation
    /// (a non-nullable enum would silently default to <see cref="Difficulty.Easy"/> when absent).
    /// </summary>
    public Difficulty? Difficulty { get; set; }
}

/// <summary>
/// HTTP response body for a successful <c>POST /games</c>.
/// </summary>
public sealed record CreateGameResponse(Guid GameId);

using BattleShip.Grpc;
using BattleShip.Models.Domain;
using FluentValidation;

namespace BattleShip.API.Validation;

/// <summary>
/// Validates the protobuf <see cref="FireShotRequest"/> before any call reaches <c>GameService</c> (AD-5).
/// <c>GameId</c> must be syntactically a GUID (existence in the store is checked later, by <c>GameService</c>,
/// and mapped to <c>NotFound</c>); <c>Row</c>/<c>Col</c> must fall within the 10x10 grid.
/// </summary>
public sealed class FireShotRequestValidator : AbstractValidator<FireShotRequest>
{
    public FireShotRequestValidator()
    {
        RuleFor(request => request.GameId)
            .Must(gameId => Guid.TryParse(gameId, out _))
            .WithMessage("GameId must be a valid GUID.");

        RuleFor(request => request.Row)
            .InclusiveBetween(0, Grid.Size - 1)
            .WithMessage($"Row must be between 0 and {Grid.Size - 1}.");

        RuleFor(request => request.Col)
            .InclusiveBetween(0, Grid.Size - 1)
            .WithMessage($"Col must be between 0 and {Grid.Size - 1}.");
    }
}

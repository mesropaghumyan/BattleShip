using BattleShip.Models.Contracts;
using BattleShip.Models.Domain;
using FluentValidation;

namespace BattleShip.API.Validation;

/// <summary>
/// Validates <see cref="CreateGameRequest"/> before any call reaches <c>GameService</c> (AD-5).
/// Only <see cref="Difficulty.Easy"/> is accepted this epic: <see cref="Difficulty.Hard"/> is rejected because
/// <c>HardOpponentStrategy</c> does not exist yet (Epic 2).
/// </summary>
public sealed class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(request => request.Difficulty)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithMessage("Difficulty is required.")
            .Equal(Difficulty.Easy)
                .WithMessage("Only the 'Easy' difficulty is supported at this time.");
    }
}

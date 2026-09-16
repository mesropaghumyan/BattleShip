using BattleShip.Models.Contracts;
using BattleShip.Models.Domain;
using FluentValidation;

namespace BattleShip.API.Validation;

/// <summary>
/// Validates <see cref="CreateGameRequest"/> before any call reaches <c>GameService</c> (AD-5).
/// Accepts either <see cref="Difficulty.Easy"/> or <see cref="Difficulty.Hard"/> (Story 2.2).
/// </summary>
public sealed class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(request => request.Difficulty)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithMessage("Difficulty is required.")
            .IsInEnum()
                .WithMessage("Difficulty must be either 'Easy' or 'Hard'.");
    }
}

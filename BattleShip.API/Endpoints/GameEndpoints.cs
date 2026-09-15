using BattleShip.API.Services;
using BattleShip.Models.Contracts;
using FluentValidation;

namespace BattleShip.API.Endpoints;

/// <summary>
/// <c>POST /games</c> (create) and <c>GET /games/{id}</c> (read state). No HTTP endpoint accepts a shot;
/// that is reserved for gRPC (Story 1.5, AD-4).
/// </summary>
public static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games").WithTags("Games");

        group.MapPost("", CreateGame);
        group.MapGet("/{id:guid}", GetGameState);

        return app;
    }

    private static async Task<IResult> CreateGame(
        CreateGameRequest request,
        IValidator<CreateGameRequest> validator,
        GameService gameService,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }

        var gameId = gameService.CreateGame(request.Difficulty!.Value);

        return Results.Created($"/games/{gameId}", new CreateGameResponse(gameId));
    }

    private static IResult GetGameState(Guid id, GameService gameService)
    {
        var state = gameService.GetGameState(id);

        return state is null
            ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Game not found")
            : Results.Ok(state);
    }
}

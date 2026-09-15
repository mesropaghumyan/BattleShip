using BattleShip.API.Services;
using BattleShip.Models.Domain;
using FluentValidation;
using Grpc.Core;
using Fb = BattleShip.Grpc;

namespace BattleShip.API.Grpc;

/// <summary>
/// Implements the <c>Battlefield.FireShot</c> gRPC contract (<c>Protos/battlefield.proto</c>). The sole transport
/// through which a shot can be played (AD-4); delegates to <c>GameService</c>, which is the only component
/// allowed to call <c>IGameStore</c> (AD-3). Maps domain outcomes/exceptions to the gRPC codes fixed by AD-11.
/// </summary>
/// <remarks>
/// Generated protobuf types are referenced through the <c>Fb</c> alias (<see cref="Fb"/> = <c>BattleShip.Grpc</c>)
/// because several of their names (<c>Coordinate</c>, <c>ShotOutcome</c>, <c>GameOutcome</c>) collide with
/// unrelated types already used elsewhere in the domain/contracts (AD-9: the two contracts share no type).
/// </remarks>
public sealed class BattlefieldGrpcService(
    GameService gameService,
    IValidator<Fb.FireShotRequest> validator) : Fb.Battlefield.BattlefieldBase
{
    public override async Task<Fb.ShotTurnReply> FireShot(Fb.FireShotRequest request, ServerCallContext context)
    {
        var validationResult = await validator.ValidateAsync(request, context.CancellationToken);
        if (!validationResult.IsValid)
        {
            var message = string.Join(" ", validationResult.Errors.Select(failure => failure.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, message));
        }

        // Syntactic validity of GameId was just confirmed by FireShotRequestValidator (AD-5); parsing here
        // cannot fail.
        var gameId = Guid.Parse(request.GameId);
        var coordinate = new Coordinate(request.Row, request.Col);

        PlayShotResult? result;
        try
        {
            result = gameService.PlayShot(gameId, coordinate);
        }
        catch (CellAlreadyPlayedException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (GameAlreadyFinishedException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            // Defense in depth: FireShotRequestValidator already rejects out-of-grid coordinates before this
            // point, but Grid.ResolveShot guards the same invariant independently (AD-5).
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No game found for id '{gameId}'."));

        return ToReply(result);
    }

    private static Fb.ShotTurnReply ToReply(PlayShotResult result)
    {
        var reply = new Fb.ShotTurnReply
        {
            PlayerShot = ToShotResult(result.PlayerShot),
            Status = ToGameOutcome(result.GameStatus, result.Winner)
        };

        if (result.ComputerShot is not null)
            reply.ComputerShot = ToShotResult(result.ComputerShot);

        return reply;
    }

    private static Fb.ShotResult ToShotResult(ShotAttempt shot) => new()
    {
        Coordinate = new Fb.Coordinate { Row = shot.Coordinate.Row, Col = shot.Coordinate.Col },
        Outcome = ToShotOutcome(shot.Outcome)
    };

    private static Fb.ShotOutcome ToShotOutcome(ShotOutcome outcome) => outcome switch
    {
        ShotOutcome.Miss => Fb.ShotOutcome.Miss,
        ShotOutcome.Hit => Fb.ShotOutcome.Hit,
        ShotOutcome.Sunk => Fb.ShotOutcome.Sunk,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown shot outcome.")
    };

    private static Fb.GameOutcome ToGameOutcome(GameStatus status, Side? winner)
    {
        if (status == GameStatus.InProgress)
            return Fb.GameOutcome.InProgress;

        if (winner is null)
            throw new InvalidOperationException("A finished game must have a winner.");

        return winner == Side.Human ? Fb.GameOutcome.Won : Fb.GameOutcome.Lost;
    }
}

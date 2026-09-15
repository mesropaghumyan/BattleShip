using Fb = BattleShip.Grpc;

namespace BattleShip.App.Services;

/// <summary>
/// Wraps the gRPC-Web <c>Battlefield.FireShot</c> call — the sole transport through which a shot can be
/// played (AD-4). Thin envelope over the generated <see cref="Fb.Battlefield.BattlefieldClient"/>.
/// </summary>
public sealed class ShotGrpcClient(Fb.Battlefield.BattlefieldClient client)
{
    public async Task<Fb.ShotTurnReply> FireShotAsync(
        Guid gameId,
        int row,
        int col,
        CancellationToken cancellationToken = default)
    {
        var request = new Fb.FireShotRequest
        {
            GameId = gameId.ToString(),
            Row = row,
            Col = col
        };

        return await client.FireShotAsync(request, cancellationToken: cancellationToken);
    }
}

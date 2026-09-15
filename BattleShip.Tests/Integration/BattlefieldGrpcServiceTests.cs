using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleShip.API.State;
using BattleShip.Models.Contracts;
using BattleShip.Models.Domain;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Fb = BattleShip.Grpc;

namespace BattleShip.Tests.Integration;

/// <summary>
/// Covers the <c>Battlefield.FireShot</c> I/O matrix (spec-1-5) end to end, through an in-process gRPC channel
/// bound to <see cref="WebApplicationFactory{TEntryPoint}"/>'s <c>TestServer</c> handler. Generated protobuf
/// types are referenced through the <c>Fb</c> alias (= <c>BattleShip.Grpc</c>) since several of their names
/// (<c>Coordinate</c>, <c>ShotOutcome</c>, <c>GameOutcome</c>) collide with unrelated domain/contract types
/// also used in this file.
/// </summary>
public sealed class BattlefieldGrpcServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program> _factory;

    public BattlefieldGrpcServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FireShot_ValidShotThatDoesNotEndTheGame_ReturnsPlayerAndComputerShotsWithInProgressStatus()
    {
        var gameId = await CreateGameAsync();
        var client = CreateClient();

        // The very first shot of a fresh game can never sink an entire 17-cell fleet, so the game is
        // guaranteed to still be in progress and the computer is guaranteed to riposte.
        var reply = await client.FireShotAsync(new Fb.FireShotRequest { GameId = gameId.ToString(), Row = 0, Col = 0 });

        Assert.NotNull(reply.PlayerShot);
        Assert.Equal(0, reply.PlayerShot.Coordinate.Row);
        Assert.Equal(0, reply.PlayerShot.Coordinate.Col);
        Assert.NotNull(reply.ComputerShot);
        Assert.Equal(Fb.GameOutcome.InProgress, reply.Status);
    }

    [Fact]
    public async Task FireShot_ThatSinksAShipButNotTheWholeFleet_ComputerShotReflectsARealShotOnHumanGrid()
    {
        var gameId = await CreateGameAsync();
        var client = CreateClient();

        var smallestShip = GetSmallestComputerShip(gameId);

        // Sink the smallest ship (2 or 3 cells) but leave the rest of the fleet (12+ more cells) untouched,
        // so the game is still in progress after the final hit and the computer must riposte.
        Fb.ShotTurnReply? lastReply = null;
        foreach (var cell in smallestShip.Cells)
        {
            lastReply = await client.FireShotAsync(new Fb.FireShotRequest
            {
                GameId = gameId.ToString(),
                Row = cell.Row,
                Col = cell.Col
            });
        }

        Assert.NotNull(lastReply);
        Assert.Equal(Fb.ShotOutcome.Sunk, lastReply!.PlayerShot.Outcome);
        Assert.Equal(Fb.GameOutcome.InProgress, lastReply.Status);
        Assert.NotNull(lastReply.ComputerShot);

        // The computer's shot is a real shot against HumanGrid: it shows up in the state's OwnGrid shot history.
        var state = await GetGameStateAsync(gameId);
        var computerShot = lastReply.ComputerShot;
        Assert.Contains(state.OwnGrid.Shots, shot =>
            shot.Coordinate.Row == computerShot.Coordinate.Row &&
            shot.Coordinate.Col == computerShot.Coordinate.Col);
    }

    [Fact]
    public async Task FireShot_ThatSinksTheLastShip_ReturnsWonWithNoComputerShotAndRejectsFurtherShots()
    {
        var gameId = await CreateGameAsync();
        var client = CreateClient();
        var lastCell = SinkAllButOneCellOfComputerFleet(gameId);

        var reply = await client.FireShotAsync(new Fb.FireShotRequest
        {
            GameId = gameId.ToString(),
            Row = lastCell.Row,
            Col = lastCell.Col
        });

        Assert.Equal(Fb.ShotOutcome.Sunk, reply.PlayerShot.Outcome);
        Assert.Null(reply.ComputerShot);
        Assert.Equal(Fb.GameOutcome.Won, reply.Status);

        // No further shot is accepted once the game is won.
        var exception = await Assert.ThrowsAsync<RpcException>(() => client.FireShotAsync(new Fb.FireShotRequest
        {
            GameId = gameId.ToString(),
            Row = 0,
            Col = 0
        }).ResponseAsync);
        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
    }

    [Fact]
    public async Task FireShot_WhereComputerRiposteWinsTheGame_ReturnsLost()
    {
        var gameId = await CreateGameAsync();
        var client = CreateClient();
        LeaveOnlyOneUnplayedCellOnHumanGrid(gameId);

        // A single shot on a fresh ComputerGrid can never sink its whole fleet, so the game only ends
        // via the computer's forced riposte on HumanGrid's one remaining (ship) cell.
        var reply = await client.FireShotAsync(new Fb.FireShotRequest { GameId = gameId.ToString(), Row = 0, Col = 0 });

        Assert.NotNull(reply.ComputerShot);
        Assert.Equal(Fb.ShotOutcome.Sunk, reply.ComputerShot.Outcome);
        Assert.Equal(Fb.GameOutcome.Lost, reply.Status);
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(-1, 0)]
    [InlineData(0, 10)]
    [InlineData(0, -1)]
    public async Task FireShot_WithCoordinateOutsideGrid_FailsWithInvalidArgument(int row, int col)
    {
        var gameId = await CreateGameAsync();
        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(() => client.FireShotAsync(
            new Fb.FireShotRequest { GameId = gameId.ToString(), Row = row, Col = col }).ResponseAsync);

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task FireShot_WithMalformedGameId_FailsWithInvalidArgument()
    {
        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(() => client.FireShotAsync(
            new Fb.FireShotRequest { GameId = "not-a-guid", Row = 0, Col = 0 }).ResponseAsync);

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task FireShot_OnACellAlreadyPlayed_FailsWithFailedPreconditionAndLeavesStateUnchanged()
    {
        var gameId = await CreateGameAsync();
        var client = CreateClient();

        await client.FireShotAsync(new Fb.FireShotRequest { GameId = gameId.ToString(), Row = 5, Col = 5 });
        var stateAfterFirstShot = await GetGameStateAsync(gameId);

        var exception = await Assert.ThrowsAsync<RpcException>(() => client.FireShotAsync(
            new Fb.FireShotRequest { GameId = gameId.ToString(), Row = 5, Col = 5 }).ResponseAsync);

        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);

        var stateAfterFailedShot = await GetGameStateAsync(gameId);
        Assert.Equal(stateAfterFirstShot.OpponentGrid.Shots.Count, stateAfterFailedShot.OpponentGrid.Shots.Count);
        Assert.Equal(stateAfterFirstShot.OwnGrid.Shots.Count, stateAfterFailedShot.OwnGrid.Shots.Count);
    }

    [Fact]
    public async Task FireShot_OnAGameThatHasAlreadyFinished_FailsWithFailedPreconditionAndLeavesStateUnchanged()
    {
        var gameId = await CreateGameAsync();
        var client = CreateClient();
        var lastCell = SinkAllButOneCellOfComputerFleet(gameId);
        await client.FireShotAsync(new Fb.FireShotRequest { GameId = gameId.ToString(), Row = lastCell.Row, Col = lastCell.Col });

        var stateAfterWin = await GetGameStateAsync(gameId);
        Assert.Equal(GameOutcome.Won, stateAfterWin.Status);

        var exception = await Assert.ThrowsAsync<RpcException>(() => client.FireShotAsync(
            new Fb.FireShotRequest { GameId = gameId.ToString(), Row = 1, Col = 1 }).ResponseAsync);

        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);

        var stateAfterRejectedShot = await GetGameStateAsync(gameId);
        Assert.Equal(stateAfterWin.OpponentGrid.Shots.Count, stateAfterRejectedShot.OpponentGrid.Shots.Count);
    }

    [Fact]
    public async Task FireShot_WithUnknownGameId_FailsWithNotFound()
    {
        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(() => client.FireShotAsync(
            new Fb.FireShotRequest { GameId = Guid.NewGuid().ToString(), Row = 0, Col = 0 }).ResponseAsync);

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public void ModelsProject_StillReferencesNoAspNetGrpcOrFluentValidationPackage()
    {
        var modelsAssembly = typeof(Game).Assembly;

        var referencedAssemblyNames = modelsAssembly.GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(referencedAssemblyNames, name =>
            name.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("FluentValidation", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Grpc", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Plays every cell of HumanGrid except one ship cell, directly through the store (bypassing gRPC, purely
    /// for test setup), so EasyOpponentStrategy has no choice but to target that cell next. Unlike
    /// <see cref="SinkAllButOneCellOfComputerFleet"/>, every non-reserved cell is played (not just ship cells):
    /// the strategy picks uniformly among ALL remaining cells, not just unsunk ship ones.
    /// </summary>
    private void LeaveOnlyOneUnplayedCellOnHumanGrid(Guid gameId)
    {
        var store = _factory.Services.GetRequiredService<IGameStore>();

        store.WithGame(gameId, game =>
        {
            var reservedCell = game.HumanGrid.Ships[0].Cells[0];

            for (var row = 0; row < Grid.Size; row++)
            {
                for (var col = 0; col < Grid.Size; col++)
                {
                    var cell = new Coordinate(row, col);
                    if (cell != reservedCell)
                        game.ApplyShot(Side.Computer, cell);
                }
            }

            return game;
        });
    }

    /// <summary>
    /// Sinks every ship of the computer's fleet except for a single last cell, directly through the store
    /// (bypassing gRPC, purely for test setup). Returns the one remaining unplayed ship cell.
    /// </summary>
    private Coordinate SinkAllButOneCellOfComputerFleet(Guid gameId)
    {
        var store = _factory.Services.GetRequiredService<IGameStore>();

        // WithGame requires a reference-typed result, hence the single-element array wrapping the value-typed
        // Coordinate the caller actually wants.
        var lastCellHolder = store.WithGame(gameId, game =>
        {
            var allCells = game.ComputerGrid.Ships.SelectMany(ship => ship.Cells).ToList();
            var lastCell = allCells[^1];

            foreach (var cell in allCells.Take(allCells.Count - 1))
                game.ApplyShot(Side.Human, cell);

            return new[] { lastCell };
        }) ?? throw new InvalidOperationException("Game not found while seeding test state.");

        return lastCellHolder[0];
    }

    private Ship GetSmallestComputerShip(Guid gameId)
    {
        var store = _factory.Services.GetRequiredService<IGameStore>();

        return store.WithGame(gameId, game => game.ComputerGrid.Ships.OrderBy(ship => ship.Size).First())
            ?? throw new InvalidOperationException("Game not found while reading test state.");
    }

    private async Task<Guid> CreateGameAsync()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/games", new CreateGameRequest { Difficulty = Difficulty.Easy }, JsonOptions);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreateGameResponse>(JsonOptions);
        return body!.GameId;
    }

    private async Task<GameStateDto> GetGameStateAsync(Guid gameId)
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/games/{gameId}");
        response.EnsureSuccessStatusCode();

        var state = await response.Content.ReadFromJsonAsync<GameStateDto>(JsonOptions);
        return state!;
    }

    private Fb.Battlefield.BattlefieldClient CreateClient()
    {
        var handler = new ResponseVersionHandler { InnerHandler = _factory.Server.CreateHandler() };
        var channel = GrpcChannel.ForAddress(_factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        return new Fb.Battlefield.BattlefieldClient(channel);
    }

    /// <summary>
    /// <see cref="WebApplicationFactory{TEntryPoint}"/>'s in-process <c>TestServer</c> handler always reports
    /// HTTP/1.1 responses, which <see cref="GrpcChannel"/> refuses. This is the standard workaround (per
    /// Microsoft's gRPC testing docs): echo back the request's HTTP version on the response.
    /// </summary>
    private sealed class ResponseVersionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Version = request.Version;
            return response;
        }
    }
}

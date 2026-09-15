using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleShip.API.State;
using BattleShip.Models.Contracts;
using BattleShip.Models.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BattleShip.Tests.Integration;

public sealed class GameEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GameEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGame_WithEasyDifficulty_Returns201WithGameId()
    {
        var response = await PostCreateGameAsync(Difficulty.Easy);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateGameResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.GameId);
    }

    [Fact]
    public async Task CreateGame_WithoutDifficultyField_Returns400ValidationProblem()
    {
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/games", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task CreateGame_WithHardDifficulty_Returns400ValidationProblem()
    {
        var response = await PostCreateGameAsync(Difficulty.Hard);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task CreateGame_WithHardDifficulty_IsRejectedWithBadRequestBeforeReachingGameService()
    {
        // FluentValidation (AD-5) rejects Hard before the request ever reaches GameService.CreateGame; no game
        // id is returned, so there is nothing for IGameStore to have registered.
        var response = await PostCreateGameAsync(Difficulty.Hard);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetGameState_JustAfterCreation_OwnGridHasFiveShipsAndOpponentGridHasNoShots()
    {
        var gameId = await CreateGameAsync();

        var response = await _client.GetAsync($"/games/{gameId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var state = await response.Content.ReadFromJsonAsync<GameStateDto>(JsonOptions);
        Assert.NotNull(state);
        Assert.Equal(gameId, state!.GameId);
        Assert.Equal(GameOutcome.InProgress, state.Status);
        Assert.Equal(5, state.OwnGrid.Ships.Count);
        Assert.Empty(state.OpponentGrid.Shots);
    }

    [Fact]
    public async Task GetGameState_ForUnknownGameId_Returns404WithProblemBody()
    {
        var response = await _client.GetAsync($"/games/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("title", out var title));
        Assert.Equal("Game not found", title.GetString());
        Assert.True(problem.TryGetProperty("status", out var status));
        Assert.Equal((int)HttpStatusCode.NotFound, status.GetInt32());
    }

    [Fact]
    public async Task GetGameState_WithNonGuidIdSegment_Returns404()
    {
        var response = await _client.GetAsync("/games/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateGame_TwiceProducesDistinctIdsWithIndependentState()
    {
        var firstGameId = await CreateGameAsync();
        var secondGameId = await CreateGameAsync();

        Assert.NotEqual(firstGameId, secondGameId);

        var store = _factory.Services.GetRequiredService<IGameStore>();
        store.WithGame(firstGameId, game =>
        {
            game.ApplyShot(Side.Human, game.ComputerGrid.Ships[0].Cells[0]);
            return game;
        });

        var firstState = await GetGameStateAsync(firstGameId);
        var secondState = await GetGameStateAsync(secondGameId);

        Assert.Single(firstState.OpponentGrid.Shots);
        Assert.Empty(secondState.OpponentGrid.Shots);
        Assert.Equal(5, firstState.OwnGrid.Ships.Count);
        Assert.Equal(5, secondState.OwnGrid.Ships.Count);
    }

    [Fact]
    public async Task GetGameState_OpponentGrid_NeverLeaksCoordinatesOfAShipThatIsNotFullySunk()
    {
        var gameId = await CreateGameAsync();
        var store = _factory.Services.GetRequiredService<IGameStore>();

        var partiallyHitShipCells = store.WithGame(gameId, game =>
        {
            var ships = game.ComputerGrid.Ships.OrderBy(ship => ship.Size).ToList();
            var shipToSink = ships[0];
            var shipToPartiallyHit = ships[^1];

            foreach (var cell in shipToSink.Cells)
                game.ApplyShot(Side.Human, cell);

            game.ApplyShot(Side.Human, shipToPartiallyHit.Cells[0]);

            return shipToPartiallyHit.Cells;
        });
        Assert.NotNull(partiallyHitShipCells);

        var state = await GetGameStateAsync(gameId);

        // Exactly one ship (the fully sunk one) is revealed on the opponent grid.
        var revealedShip = Assert.Single(state.OpponentGrid.Ships);
        Assert.True(revealedShip.IsSunk);

        var revealedCoordinates = state.OpponentGrid.Ships
            .SelectMany(ship => ship.Cells)
            .Select(cell => new Coordinate(cell.Row, cell.Col))
            .ToHashSet();

        foreach (var cell in partiallyHitShipCells!)
            Assert.DoesNotContain(cell, revealedCoordinates);

        // The partial hit is still reported as a shot result, just without exposing the ship's shape.
        Assert.Contains(state.OpponentGrid.Shots, shot =>
            shot.Coordinate.Row == partiallyHitShipCells[0].Row &&
            shot.Coordinate.Col == partiallyHitShipCells[0].Col &&
            shot.Outcome == ShotOutcome.Hit);
    }

    [Fact]
    public async Task GetGameState_SerializesEnumsAsStringNamesOnTheWire()
    {
        var gameId = await CreateGameAsync();

        var response = await _client.GetAsync($"/games/{gameId}");
        response.EnsureSuccessStatusCode();

        // Read the raw JSON text directly, bypassing JsonOptions above (which tolerates integers via its
        // JsonStringEnumConverter and would not catch a regression to numeric enum serialization).
        var rawJson = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"status\":\"InProgress\"", rawJson);
    }

    [Fact]
    public async Task CreateGame_WithUnparseableDifficultyValue_Returns400()
    {
        using var content = new StringContent("{\"Difficulty\": 99}", Encoding.UTF8, "application/json");

        // An out-of-range enum value fails JSON model binding before CreateGameRequestValidator ever runs, so
        // this produces ASP.NET's default 400 rather than the ValidationProblem shape used elsewhere.
        var response = await _client.PostAsync("/games", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void ModelsProject_StillReferencesNoAspNetOrFluentValidationPackage()
    {
        var modelsAssembly = typeof(Game).Assembly;

        var referencedAssemblyNames = modelsAssembly.GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(referencedAssemblyNames, name =>
            name.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("FluentValidation", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<HttpResponseMessage> PostCreateGameAsync(Difficulty difficulty) =>
        await _client.PostAsJsonAsync("/games", new CreateGameRequest { Difficulty = difficulty }, JsonOptions);

    private async Task<Guid> CreateGameAsync()
    {
        var response = await PostCreateGameAsync(Difficulty.Easy);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreateGameResponse>(JsonOptions);
        return body!.GameId;
    }

    private async Task<GameStateDto> GetGameStateAsync(Guid gameId)
    {
        var response = await _client.GetAsync($"/games/{gameId}");
        response.EnsureSuccessStatusCode();

        var state = await response.Content.ReadFromJsonAsync<GameStateDto>(JsonOptions);
        return state!;
    }
}

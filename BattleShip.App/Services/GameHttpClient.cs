using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleShip.Models.Contracts;
using BattleShip.Models.Domain;

namespace BattleShip.App.Services;

/// <summary>
/// Wraps the HTTP calls to BattleShip.API's <c>/games</c> endpoints: game creation and state reads
/// stay on HTTP (AD-4). No <c>ProjectReference</c> to <c>BattleShip.API</c> — network calls only (AD-8).
/// </summary>
public sealed class GameHttpClient(HttpClient httpClient)
{
    // Mirrors BattleShip.API's own JSON options: ASP.NET Core's minimal API defaults serialize with
    // camelCase property names and case-insensitive binding, and enums travel as their names
    // ("Easy", "Hit", ...) rather than raw integers over the wire (ConfigureHttpJsonOptions).
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<Guid> CreateGameAsync(
        Difficulty difficulty = Difficulty.Easy,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateGameRequest { Difficulty = difficulty };
        var response = await httpClient.PostAsJsonAsync("games", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreateGameResponse>(JsonOptions, cancellationToken);
        return body?.GameId ?? throw new InvalidOperationException("POST /games returned an empty response body.");
    }

    public async Task<GameStateDto> GetGameStateAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"games/{gameId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var state = await response.Content.ReadFromJsonAsync<GameStateDto>(JsonOptions, cancellationToken);
        return state ?? throw new InvalidOperationException($"GET /games/{gameId} returned an empty response body.");
    }
}

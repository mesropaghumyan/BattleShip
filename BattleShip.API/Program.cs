using System.Text.Json.Serialization;
using BattleShip.API.Endpoints;
using BattleShip.API.Grpc;
using BattleShip.API.Services;
using BattleShip.API.State;
using BattleShip.API.Validation;
using BattleShip.Models.Domain;
using BattleShip.Models.Domain.Opponent;
using FluentValidation;

const string AppCorsPolicy = "AppCors";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Limited to BattleShip.App's own origins (AD-8: no wildcard). Grpc-Status/Grpc-Message/Grpc-Encoding are
// exposed so the gRPC-Web client (Grpc.Net.Client.Web) can read them from the browser.
builder.Services.AddCors(options =>
{
    options.AddPolicy(AppCorsPolicy, policy => policy
        .WithOrigins("https://localhost:7297", "http://localhost:5277")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding"));
});

// Enums are exchanged as their names (e.g. "Easy", "Hit") rather than raw integers over the wire.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();
builder.Services.AddSingleton<GameEngine>();
// Easy and Hard opponent strategies are selected based on the game's Difficulty (Story 2.2, AD-6).
builder.Services.AddSingleton<Func<Difficulty, IOpponentStrategy>>(_ => Game.ResolveStrategy);
builder.Services.AddSingleton<GameService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateGameRequestValidator>();

// The sole transport for playing a shot (AD-4); gRPC-Web lets the future Blazor WASM client call it
// (Story 1.6) without a plain gRPC-capable browser stack.
builder.Services.AddGrpc();

// Any unhandled exception (e.g. a Guid collision in IGameStore.Add, or GameEngine exhausting placement
// attempts) is turned into a structured ProblemDetails response instead of a bare 500.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

// No HTTPS redirection: local dev runs API and App on plain HTTP (default "http" launch profile) so the
// demo works with no dev-certs trust step. Re-add if this project is ever hosted beyond localhost.
app.UseCors(AppCorsPolicy);

app.UseGrpcWeb();

app.MapGameEndpoints();
app.MapGrpcService<BattlefieldGrpcService>().EnableGrpcWeb();

app.Run();

// Exposed so BattleShip.Tests can bootstrap the app via WebApplicationFactory<Program>.
public partial class Program
{
}

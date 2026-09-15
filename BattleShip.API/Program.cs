using System.Text.Json.Serialization;
using BattleShip.API.Endpoints;
using BattleShip.API.Services;
using BattleShip.API.State;
using BattleShip.API.Validation;
using BattleShip.Models.Domain;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Enums are exchanged as their names (e.g. "Easy", "Hit") rather than raw integers over the wire.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();
builder.Services.AddSingleton<GameEngine>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateGameRequestValidator>();

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

app.UseHttpsRedirection();

app.MapGameEndpoints();

app.Run();

// Exposed so BattleShip.Tests can bootstrap the app via WebApplicationFactory<Program>.
public partial class Program
{
}

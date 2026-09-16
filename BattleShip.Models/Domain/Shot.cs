namespace BattleShip.Models.Domain;

public sealed record Shot(
    Side Side,
    Coordinate Coordinate,
    ShotOutcome Outcome,
    DateTimeOffset PlayedAt);
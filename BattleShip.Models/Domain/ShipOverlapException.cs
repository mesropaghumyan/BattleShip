namespace BattleShip.Models.Domain;

public sealed class ShipOverlapException(string message) : InvalidOperationException(message);

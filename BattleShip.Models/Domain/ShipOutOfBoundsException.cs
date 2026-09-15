namespace BattleShip.Models.Domain;

public sealed class ShipOutOfBoundsException(string message) : InvalidOperationException(message);

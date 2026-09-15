namespace BattleShip.Models.Domain;

public sealed class CellAlreadyPlayedException(string message) : InvalidOperationException(message);

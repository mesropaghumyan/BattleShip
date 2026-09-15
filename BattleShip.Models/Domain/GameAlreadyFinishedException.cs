namespace BattleShip.Models.Domain;

public sealed class GameAlreadyFinishedException(string message) : InvalidOperationException(message);

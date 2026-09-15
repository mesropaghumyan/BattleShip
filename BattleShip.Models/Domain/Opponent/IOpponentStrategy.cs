using BattleShip.Models.Domain;

namespace BattleShip.Models.Domain.Opponent;

public interface IOpponentStrategy
{
    Coordinate ChooseShot(Grid targetGrid);
}

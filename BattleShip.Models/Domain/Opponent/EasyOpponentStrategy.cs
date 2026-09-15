using BattleShip.Models.Domain;

namespace BattleShip.Models.Domain.Opponent;

public sealed class EasyOpponentStrategy(Random? random = null) : IOpponentStrategy
{
    private readonly Random _random = random ?? Random.Shared;

    public Coordinate ChooseShot(Grid targetGrid)
    {
        ArgumentNullException.ThrowIfNull(targetGrid);

        var available = targetGrid.RemainingCells().ToList();

        if (available.Count == 0)
            throw new InvalidOperationException("No remaining cells to target; the grid has been fully played.");

        return available[_random.Next(available.Count)];
    }
}

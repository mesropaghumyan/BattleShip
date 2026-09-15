namespace BattleShip.Models.Domain;

public sealed class Game
{
    public Grid HumanGrid { get; }
    public Grid ComputerGrid { get; }
    public GameStatus Status { get; private set; } = GameStatus.InProgress;
    public Side? Winner { get; private set; }

    public Game(Grid humanGrid, Grid computerGrid)
    {
        ArgumentNullException.ThrowIfNull(humanGrid);
        ArgumentNullException.ThrowIfNull(computerGrid);
        if (ReferenceEquals(humanGrid, computerGrid))
            throw new ArgumentException("Human and computer grids must be distinct instances.", nameof(computerGrid));

        HumanGrid = humanGrid;
        ComputerGrid = computerGrid;
    }

    public ShotOutcome ApplyShot(Side shooter, Coordinate target)
    {
        if (Status == GameStatus.Finished)
            throw new GameAlreadyFinishedException("This game has already finished; no further shots can be played.");

        var targetGrid = shooter switch
        {
            Side.Human => ComputerGrid,
            Side.Computer => HumanGrid,
            _ => throw new ArgumentOutOfRangeException(nameof(shooter), shooter, "Unknown shooter side.")
        };
        var outcome = targetGrid.ResolveShot(target);

        if (targetGrid.IsFleetSunk)
        {
            Status = GameStatus.Finished;
            Winner = shooter;
        }

        return outcome;
    }
}

using BattleShip.Models.Domain.Opponent;

namespace BattleShip.Models.Domain;

public sealed class Game
{
    public Grid HumanGrid { get; }
    public Grid ComputerGrid { get; }
    public GameStatus Status { get; private set; } = GameStatus.InProgress;
    public Side? Winner { get; private set; }
    public Difficulty Difficulty { get; }
    public IOpponentStrategy OpponentStrategy { get; }

    public Game(
        Grid humanGrid,
        Grid computerGrid,
        Difficulty difficulty = Difficulty.Easy,
        IOpponentStrategy? opponentStrategy = null)
    {
        ArgumentNullException.ThrowIfNull(humanGrid);
        ArgumentNullException.ThrowIfNull(computerGrid);
        if (ReferenceEquals(humanGrid, computerGrid))
            throw new ArgumentException("Human and computer grids must be distinct instances.", nameof(computerGrid));

        HumanGrid = humanGrid;
        ComputerGrid = computerGrid;
        Difficulty = difficulty;
        OpponentStrategy = opponentStrategy ?? ResolveStrategy(difficulty);
    }

    public static IOpponentStrategy ResolveStrategy(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => new EasyOpponentStrategy(),
        Difficulty.Hard => new HardOpponentStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, $"Unknown difficulty '{difficulty}'.")
    };

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

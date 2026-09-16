namespace BattleShip.Models.Domain;

public sealed record SideStatistics(
    int ShotCount,
    int HitCount,
    TimeSpan Duration)
{
    public decimal HitRate => ShotCount == 0 ? 0m : (decimal)HitCount / ShotCount;
}

public sealed record GameStatistics(
    SideStatistics Human,
    SideStatistics Computer,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt)
{
    public TimeSpan Duration => FinishedAt - StartedAt;
}

public static class GameStatisticsCalculator
{
    public static GameStatistics Calculate(
        IReadOnlyList<Shot> shots,
        DateTimeOffset startedAt,
        DateTimeOffset finishedAt)
    {
        ArgumentNullException.ThrowIfNull(shots);
        if (finishedAt < startedAt)
            throw new ArgumentException("The finish time cannot precede the start time.", nameof(finishedAt));

        var duration = finishedAt - startedAt;
        return new GameStatistics(
            CalculateFor(Side.Human, shots, duration),
            CalculateFor(Side.Computer, shots, duration),
            startedAt,
            finishedAt);
    }

    private static SideStatistics CalculateFor(Side side, IReadOnlyList<Shot> shots, TimeSpan duration)
    {
        var sideShots = shots.Where(shot => shot.Side == side).ToList();
        return new SideStatistics(
            sideShots.Count,
            sideShots.Count(shot => shot.Outcome is ShotOutcome.Hit or ShotOutcome.Sunk),
            duration);
    }
}
using BattleShip.Models.Domain;

namespace BattleShip.Models.Domain.Opponent;

public sealed class HardOpponentStrategy(Random? random = null) : IOpponentStrategy
{
    private readonly Random _random = random ?? Random.Shared;

    public Coordinate ChooseShot(Grid targetGrid)
    {
        ArgumentNullException.ThrowIfNull(targetGrid);

        var remaining = targetGrid.RemainingCells().ToList();
        if (remaining.Count == 0)
            throw new InvalidOperationException("No remaining cells to target; the grid has been fully played.");

        var unsunkHits = targetGrid.UnsunkHits;

        if (unsunkHits.Count >= 2)
        {
            var hasAlignedHits = HasAlignedHits(unsunkHits, targetGrid, out var axisCandidates);
            if (hasAlignedHits)
            {
                if (axisCandidates.Count > 0)
                {
                    return axisCandidates[_random.Next(axisCandidates.Count)];
                }

                // L'axe identifié a essuyé un échec (tous les candidats de l'axe sont déjà joués ou hors grille).
                // Selon l'AC : "poursuit dans l'axe identifié jusqu'à couler le navire ou essuyer un échec, puis revient en phase de recherche".
                return ChooseHuntShot(remaining);
            }
        }

        if (unsunkHits.Count > 0)
        {
            var targetCandidates = GetAdjacentCandidates(unsunkHits, targetGrid);
            if (targetCandidates.Count > 0)
            {
                return targetCandidates[_random.Next(targetCandidates.Count)];
            }
        }

        return ChooseHuntShot(remaining);
    }

    private Coordinate ChooseHuntShot(List<Coordinate> remaining)
    {
        var checkerboardCandidates = remaining
            .Where(cell => (cell.Row + cell.Col) % 2 == 0)
            .ToList();

        if (checkerboardCandidates.Count > 0)
        {
            return checkerboardCandidates[_random.Next(checkerboardCandidates.Count)];
        }

        return remaining[_random.Next(remaining.Count)];
    }

    private static bool HasAlignedHits(
        IReadOnlyCollection<Coordinate> unsunkHits,
        Grid targetGrid,
        out List<Coordinate> axisCandidates)
    {
        axisCandidates = [];
        var candidateSet = new HashSet<Coordinate>();
        var foundAnyAlignedPair = false;

        // 1. Alignement horizontal (même ligne)
        var rowGroups = unsunkHits.GroupBy(h => h.Row).Where(g => g.Count() >= 2);
        foreach (var group in rowGroups)
        {
            var row = group.Key;
            var cols = group.Select(h => h.Col).OrderBy(c => c).ToList();

            for (var i = 0; i < cols.Count - 1; i++)
            {
                var c1 = cols[i];
                var c2 = cols[i + 1];

                if (c2 - c1 <= 4 && HasNoObstacleBetweenHorizontal(row, c1, c2, targetGrid, unsunkHits))
                {
                    foundAnyAlignedPair = true;

                    // Cases intérieures non encore jouées
                    for (var c = c1 + 1; c < c2; c++)
                    {
                        var interior = new Coordinate(row, c);
                        if (!targetGrid.ShotsPlayed.Contains(interior))
                            candidateSet.Add(interior);
                    }

                    // Extrémité gauche
                    var left = new Coordinate(row, c1 - 1);
                    if (left.IsWithinBounds(Grid.Size) && !targetGrid.ShotsPlayed.Contains(left))
                        candidateSet.Add(left);

                    // Extrémité droite
                    var right = new Coordinate(row, c2 + 1);
                    if (right.IsWithinBounds(Grid.Size) && !targetGrid.ShotsPlayed.Contains(right))
                        candidateSet.Add(right);
                }
            }
        }

        // 2. Alignement vertical (même colonne)
        var colGroups = unsunkHits.GroupBy(h => h.Col).Where(g => g.Count() >= 2);
        foreach (var group in colGroups)
        {
            var col = group.Key;
            var rows = group.Select(h => h.Row).OrderBy(r => r).ToList();

            for (var i = 0; i < rows.Count - 1; i++)
            {
                var r1 = rows[i];
                var r2 = rows[i + 1];

                if (r2 - r1 <= 4 && HasNoObstacleBetweenVertical(col, r1, r2, targetGrid, unsunkHits))
                {
                    foundAnyAlignedPair = true;

                    // Cases intérieures non encore jouées
                    for (var r = r1 + 1; r < r2; r++)
                    {
                        var interior = new Coordinate(r, col);
                        if (!targetGrid.ShotsPlayed.Contains(interior))
                            candidateSet.Add(interior);
                    }

                    // Extrémité haut
                    var up = new Coordinate(r1 - 1, col);
                    if (up.IsWithinBounds(Grid.Size) && !targetGrid.ShotsPlayed.Contains(up))
                        candidateSet.Add(up);

                    // Extrémité bas
                    var down = new Coordinate(r2 + 1, col);
                    if (down.IsWithinBounds(Grid.Size) && !targetGrid.ShotsPlayed.Contains(down))
                        candidateSet.Add(down);
                }
            }
        }

        axisCandidates = candidateSet.ToList();
        return foundAnyAlignedPair;
    }

    private static bool HasNoObstacleBetweenHorizontal(
        int row,
        int c1,
        int c2,
        Grid targetGrid,
        IReadOnlyCollection<Coordinate> unsunkHits)
    {
        for (var c = c1 + 1; c < c2; c++)
        {
            var coord = new Coordinate(row, c);
            if (targetGrid.ShotsPlayed.Contains(coord) && !unsunkHits.Contains(coord))
                return false;
        }
        return true;
    }

    private static bool HasNoObstacleBetweenVertical(
        int col,
        int r1,
        int r2,
        Grid targetGrid,
        IReadOnlyCollection<Coordinate> unsunkHits)
    {
        for (var r = r1 + 1; r < r2; r++)
        {
            var coord = new Coordinate(r, col);
            if (targetGrid.ShotsPlayed.Contains(coord) && !unsunkHits.Contains(coord))
                return false;
        }
        return true;
    }

    private static List<Coordinate> GetAdjacentCandidates(
        IReadOnlyCollection<Coordinate> unsunkHits,
        Grid targetGrid)
    {
        var candidates = new HashSet<Coordinate>();

        foreach (var hit in unsunkHits)
        {
            Coordinate[] neighbors =
            [
                new(hit.Row - 1, hit.Col),
                new(hit.Row + 1, hit.Col),
                new(hit.Row, hit.Col - 1),
                new(hit.Row, hit.Col + 1)
            ];

            foreach (var neighbor in neighbors)
            {
                if (neighbor.IsWithinBounds(Grid.Size) && !targetGrid.ShotsPlayed.Contains(neighbor))
                {
                    candidates.Add(neighbor);
                }
            }
        }

        return candidates.ToList();
    }
}

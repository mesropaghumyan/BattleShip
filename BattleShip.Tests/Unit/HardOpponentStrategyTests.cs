using BattleShip.Models.Domain;
using BattleShip.Models.Domain.Opponent;

namespace BattleShip.Tests.Unit;

public class HardOpponentStrategyTests
{
    [Fact]
    public void HardOpponentStrategy_ImplementsIOpponentStrategy()
    {
        IOpponentStrategy strategy = new HardOpponentStrategy();
        var grid = new Grid();

        var shot = strategy.ChooseShot(grid);

        Assert.True(shot.IsWithinBounds(Grid.Size));
    }

    [Fact]
    public void ChooseShot_Throws_WhenGridIsNull()
    {
        var strategy = new HardOpponentStrategy();

        Assert.Throws<ArgumentNullException>(() => strategy.ChooseShot(null!));
    }

    [Fact]
    public void ChooseShot_Throws_WhenGridIsFullyPlayed()
    {
        var grid = new Grid();
        for (var row = 0; row < Grid.Size; row++)
        {
            for (var col = 0; col < Grid.Size; col++)
            {
                grid.ResolveShot(new Coordinate(row, col));
            }
        }

        var strategy = new HardOpponentStrategy();

        Assert.Throws<InvalidOperationException>(() => strategy.ChooseShot(grid));
    }

    #region Phase 1 — Recherche (Hunt)

    [Fact]
    public void ChooseShot_InHuntPhase_PicksOnlyCheckerboardParityCells_WhenNoHitsExist()
    {
        var strategy = new HardOpponentStrategy(new Random(1234));
        var grid = new Grid();

        var shot = strategy.ChooseShot(grid);

        Assert.Equal(0, (shot.Row + shot.Col) % 2);
    }

    [Fact]
    public void ChooseShot_InHuntPhase_ContinuouslyPicksCheckerboardParityCells_WhenOnlyMissesOccur()
    {
        var strategy = new HardOpponentStrategy(new Random(42));
        var grid = new Grid();

        for (var i = 0; i < 25; i++)
        {
            var shot = strategy.ChooseShot(grid);

            // Vérifie la parité damier (x + y) % 2 == 0
            Assert.Equal(0, (shot.Row + shot.Col) % 2);

            // Ne rejoue jamais une case déjà jouée
            Assert.DoesNotContain(shot, grid.ShotsPlayed);

            grid.ResolveShot(shot);
        }
    }

    [Fact]
    public void ChooseShot_InHuntPhase_FallsBackToOddCells_WhenAllCheckerboardCellsArePlayed()
    {
        var grid = new Grid();

        // Jouer toutes les 50 cases paires (damier)
        for (var row = 0; row < Grid.Size; row++)
        {
            for (var col = 0; col < Grid.Size; col++)
            {
                if ((row + col) % 2 == 0)
                {
                    grid.ResolveShot(new Coordinate(row, col));
                }
            }
        }

        var strategy = new HardOpponentStrategy();
        var shot = strategy.ChooseShot(grid);

        // La case choisie doit être une case restante (donc impaire)
        Assert.Equal(1, (shot.Row + shot.Col) % 2);
        Assert.DoesNotContain(shot, grid.ShotsPlayed);
    }

    #endregion

    #region Phase 2 — Ciblage (Target)

    [Fact]
    public void ChooseShot_InTargetPhase_PicksAdjacentUnplayedCell_WhenSingleHitNotSunk()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Croiseur", 4, Orientation.Horizontal,
            [new Coordinate(5, 5), new Coordinate(5, 6), new Coordinate(5, 7), new Coordinate(5, 8)]));

        // Tir touché à (5, 5)
        grid.ResolveShot(new Coordinate(5, 5));

        var strategy = new HardOpponentStrategy(new Random(10));
        var shot = strategy.ChooseShot(grid);

        List<Coordinate> expectedAdjacent =
        [
            new Coordinate(4, 5), // Haut
            new Coordinate(6, 5), // Bas
            new Coordinate(5, 4), // Gauche
            new Coordinate(5, 6)  // Droite
        ];

        Assert.Contains(shot, expectedAdjacent);
        Assert.DoesNotContain(shot, grid.ShotsPlayed);
    }

    [Fact]
    public void ChooseShot_InTargetPhase_RespectsGridBounds_WhenHitIsOnCorner()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        grid.ResolveShot(new Coordinate(0, 0));

        var strategy = new HardOpponentStrategy(new Random(7));
        var shot = strategy.ChooseShot(grid);

        List<Coordinate> expectedInBounds =
        [
            new Coordinate(0, 1),
            new Coordinate(1, 0)
        ];

        Assert.Contains(shot, expectedInBounds);
        Assert.True(shot.IsWithinBounds(Grid.Size));
    }

    [Fact]
    public void ChooseShot_InTargetPhase_SkipsAlreadyPlayedAdjacentCells()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Croiseur", 4, Orientation.Horizontal,
            [new Coordinate(5, 5), new Coordinate(5, 6), new Coordinate(5, 7), new Coordinate(5, 8)]));

        grid.ResolveShot(new Coordinate(5, 5)); // Touché

        // Jouer 3 des 4 cases adjacentes (qui sont des ratés)
        grid.ResolveShot(new Coordinate(4, 5)); // Haut - Raté
        grid.ResolveShot(new Coordinate(6, 5)); // Bas - Raté
        grid.ResolveShot(new Coordinate(5, 4)); // Gauche - Raté

        var strategy = new HardOpponentStrategy();
        var shot = strategy.ChooseShot(grid);

        // Seule la 4ème case adjacente (5, 6) est encore disponible
        Assert.Equal(new Coordinate(5, 6), shot);
    }

    #endregion

    #region Phase 3 — Alignement (Alignment)

    [Fact]
    public void ChooseShot_InAlignmentPhase_PursuesHorizontalAxis_WhenTwoHitsAreAlignedHorizontally()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Croiseur", 4, Orientation.Horizontal,
            [new Coordinate(3, 2), new Coordinate(3, 3), new Coordinate(3, 4), new Coordinate(3, 5)]));

        grid.ResolveShot(new Coordinate(3, 3)); // Touché
        grid.ResolveShot(new Coordinate(3, 4)); // Touché

        var strategy = new HardOpponentStrategy(new Random(99));
        var shot = strategy.ChooseShot(grid);

        // Doit choisir l'une des extrémités horizontales : (3, 2) ou (3, 5)
        List<Coordinate> expectedHorizontalExtremities =
        [
            new Coordinate(3, 2),
            new Coordinate(3, 5)
        ];

        Assert.Contains(shot, expectedHorizontalExtremities);
    }

    [Fact]
    public void ChooseShot_InAlignmentPhase_PursuesVerticalAxis_WhenTwoHitsAreAlignedVertically()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Sous-marin", 3, Orientation.Vertical,
            [new Coordinate(4, 2), new Coordinate(5, 2), new Coordinate(6, 2)]));

        grid.ResolveShot(new Coordinate(4, 2)); // Touché
        grid.ResolveShot(new Coordinate(5, 2)); // Touché

        var strategy = new HardOpponentStrategy(new Random(99));
        var shot = strategy.ChooseShot(grid);

        // Doit choisir l'une des extrémités verticales : (3, 2) ou (6, 2)
        List<Coordinate> expectedVerticalExtremities =
        [
            new Coordinate(3, 2),
            new Coordinate(6, 2)
        ];

        Assert.Contains(shot, expectedVerticalExtremities);
    }

    [Fact]
    public void ChooseShot_InAlignmentPhase_PicksOppositeExtremity_WhenOneExtremityHasMissed()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Croiseur", 4, Orientation.Horizontal,
            [new Coordinate(3, 2), new Coordinate(3, 3), new Coordinate(3, 4), new Coordinate(3, 5)]));

        grid.ResolveShot(new Coordinate(3, 3)); // Touché
        grid.ResolveShot(new Coordinate(3, 4)); // Touché
        grid.ResolveShot(new Coordinate(3, 5)); // Touché (ou raté)

        // Jouer l'extrémité droite (3, 6) comme un raté
        grid.ResolveShot(new Coordinate(3, 6)); // Raté

        var strategy = new HardOpponentStrategy();
        var shot = strategy.ChooseShot(grid);

        // L'extrémité droite étant bloquée, la stratégie doit choisir l'extrémité gauche (3, 2)
        Assert.Equal(new Coordinate(3, 2), shot);
    }

    [Fact]
    public void ChooseShot_InAlignmentPhase_FillsInteriorGap_WhenHuntHitsCreatedAGap()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Porte-avions", 5, Orientation.Horizontal,
            [new Coordinate(4, 1), new Coordinate(4, 2), new Coordinate(4, 3), new Coordinate(4, 4), new Coordinate(4, 5)]));

        // Deux touches sur cases paires du damier avec un trou impair au milieu
        grid.ResolveShot(new Coordinate(4, 2)); // 4+2=6 (Pair - Touché)
        grid.ResolveShot(new Coordinate(4, 4)); // 4+4=8 (Pair - Touché)

        var strategy = new HardOpponentStrategy(new Random(1));
        var shot = strategy.ChooseShot(grid);

        // Le trou intérieur (4, 3) ou les extrémités (4, 1), (4, 5) doivent être ciblés
        List<Coordinate> expectedAxisCandidates =
        [
            new Coordinate(4, 1),
            new Coordinate(4, 3),
            new Coordinate(4, 5)
        ];

        Assert.Contains(shot, expectedAxisCandidates);
    }

    [Fact]
    public void ChooseShot_ReturnsToHuntPhase_WhenShipIsSunk()
    {
        var grid = new Grid();
        grid.PlaceShip(new Ship("Torpilleur", 2, Orientation.Horizontal,
            [new Coordinate(2, 2), new Coordinate(2, 3)]));

        grid.ResolveShot(new Coordinate(2, 2)); // Touché
        grid.ResolveShot(new Coordinate(2, 3)); // Coulé !

        var strategy = new HardOpponentStrategy(new Random(55));
        var shot = strategy.ChooseShot(grid);

        // Après coulé, retour immédiat en phase de recherche (damier (x + y) % 2 == 0)
        Assert.Equal(0, (shot.Row + shot.Col) % 2);
        Assert.DoesNotContain(shot, grid.ShotsPlayed);
    }

    [Fact]
    public void ChooseShot_ReturnsToHuntPhase_WhenAlignedAxisFails()
    {
        var grid = new Grid();

        // Plaçons deux torpilleurs verticaux côte à côte pour simuler un faux alignement horizontal
        grid.PlaceShip(new Ship("Torpilleur1", 2, Orientation.Vertical,
            [new Coordinate(5, 3), new Coordinate(6, 3)]));
        grid.PlaceShip(new Ship("Torpilleur2", 2, Orientation.Vertical,
            [new Coordinate(5, 4), new Coordinate(6, 4)]));

        grid.ResolveShot(new Coordinate(5, 3)); // Touché
        grid.ResolveShot(new Coordinate(5, 4)); // Touché

        // Les deux extrémités horizontales sont des ratés
        grid.ResolveShot(new Coordinate(5, 2)); // Raté à gauche
        grid.ResolveShot(new Coordinate(5, 5)); // Raté à droite

        var strategy = new HardOpponentStrategy(new Random(88));
        var shot = strategy.ChooseShot(grid);

        // L'axe horizontal a essuyé un échec complet -> retour en recherche (damier)
        Assert.Equal(0, (shot.Row + shot.Col) % 2);
        Assert.DoesNotContain(shot, grid.ShotsPlayed);
    }

    #endregion

    #region Invariants, Déterminisme & Règle violée (FR8)

    [Fact]
    public void ChooseShot_IsDeterministic_ForTheSameInjectedSeed()
    {
        var strategyA = new HardOpponentStrategy(new Random(2026));
        var strategyB = new HardOpponentStrategy(new Random(2026));
        var gridA = new Grid();
        var gridB = new Grid();

        for (var i = 0; i < 20; i++)
        {
            var shotA = strategyA.ChooseShot(gridA);
            var shotB = strategyB.ChooseShot(gridB);

            Assert.Equal(shotA, shotB);

            gridA.ResolveShot(shotA);
            gridB.ResolveShot(shotB);
        }
    }

    [Fact]
    public void ChooseShot_NeverReturnsAlreadyPlayedCell_AcrossFullGameSimulation()
    {
        var engine = new GameEngine(new Random(12345));
        var (humanGrid, _) = engine.CreateGrids();

        var strategy = new HardOpponentStrategy(new Random(6789));

        while (!humanGrid.IsFleetSunk)
        {
            var shot = strategy.ChooseShot(humanGrid);

            // Invariant capital : jamais de case déjà jouée
            Assert.DoesNotContain(shot, humanGrid.ShotsPlayed);
            Assert.True(shot.IsWithinBounds(Grid.Size));

            humanGrid.ResolveShot(shot);
        }

        // La flotte a bien été coulée en moins de 100 coups
        Assert.True(humanGrid.IsFleetSunk);
        Assert.True(humanGrid.ShotsPlayed.Count <= 100);
    }

    [Fact]
    public void ChooseShot_ViolatedRuleDetection_FailsIfReplayingPlayedCell()
    {
        // Ce test démontre explicitement qu'un coup invalide (rejouer une case) est détecté
        var grid = new Grid();
        var playedCell = new Coordinate(4, 4);
        grid.ResolveShot(playedCell);

        var strategy = new HardOpponentStrategy();
        var nextShot = strategy.ChooseShot(grid);

        // Détection de violation : le tir ne peut JAMAIS être la case déjà jouée
        Assert.NotEqual(playedCell, nextShot);
        Assert.Throws<CellAlreadyPlayedException>(() => grid.ResolveShot(playedCell));
    }

    #endregion
}

---
title: 'Moteur de jeu — résolution des tirs et fin de partie'
type: 'feature'
created: '2026-09-15'
status: 'done'
route: 'oneshot'
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le jeu ne peut pas encore résoudre un tir ni détecter sa propre fin : aucune structure ne relie les deux grilles d'une partie, ne rejette un coup déjà joué ou tiré après la fin de partie, ni ne détecte la victoire.

**Approach:** Ajouter dans `BattleShip.Models/Domain` un agrégat `Game` (deux `Grid` — joueur et ordinateur, statut `InProgress`/`Finished`, vainqueur) et enrichir `Grid` avec le suivi des tirs joués (`ResolveShot` : `Miss`/`Hit`/`Sunk`, rejet d'un tir sur une case déjà jouée sans changer l'état) et la détection de flotte coulée. Introduire `enum Side { Human, Computer }` (AD-10) pour identifier l'auteur d'un tir et le camp vainqueur. `Game.ApplyShot` rejette tout tir une fois la partie terminée. Testé unitairement dans `BattleShip.Tests/Unit`, avec au moins un test démontrant la détection d'une règle violée (ex. un tir rejoué ne doit pas compter comme un nouveau coup).

</frozen-after-approval>

## Implementation Notes

- Nouveaux fichiers dans `BattleShip.Models/Domain/` : `ShotOutcome.cs` (Miss/Hit/Sunk), `Side.cs` (Human/Computer, AD-10, introduit un peu plus tôt que prévu car `Game` en a besoin pour identifier tireur/vainqueur), `GameStatus.cs` (InProgress/Finished), `CellAlreadyPlayedException.cs`, `GameAlreadyFinishedException.cs`, `Game.cs` (agrégat : deux `Grid`, statut, vainqueur, `ApplyShot`).
- `Grid` enrichi : `_shotsPlayed` (HashSet), `ResolveShot(Coordinate)` (Miss/Hit/Sunk, rejette via `CellAlreadyPlayedException` si case déjà jouée — le `HashSet.Add` qui échoue garantit que rien n'est modifié avant le rejet — et via `ArgumentOutOfRangeException` si hors grille), `IsFleetSunk`.
- `Game.ApplyShot(Side shooter, Coordinate target)` route vers la grille adverse (Human tire sur ComputerGrid, Computer sur HumanGrid), rejette tout tir une fois `Status == Finished` (`GameAlreadyFinishedException`), et bascule `Status`/`Winner` dès que la grille ciblée a toute sa flotte coulée.
- Tests ajoutés : `GridTests.cs` (Miss/Hit/Sunk, rejet du rejoué sans effet de bord, rejet hors grille, `IsFleetSunk`) et `GameTests.cs` (délégation par camp, fin de partie + vainqueur, tir refusé après fin, tir rejoué sans changement d'état). 32/32 tests passent.
- Aucune surprise ; scope resté conforme à l'intent, aucune dépendance API/gRPC/opponent introduite (Stories 1.3-1.5).
- Suite à la revue Blind Hunter : `Game.ApplyShot` route désormais via un `switch` exhaustif (au lieu d'un ternaire) qui rejette toute valeur `Side` non définie (`ArgumentOutOfRangeException`) — se prépare à recevoir des valeurs venant d'une frontière externe (API/gRPC, Stories 1.4/1.5). Le constructeur de `Game` valide `humanGrid`/`computerGrid` non nuls et refuse la même instance pour les deux camps (`ArgumentException`). Ajout d'un test à deux navires démontrant que `IsFleetSunk`/`Game.Status` exigent que toute la flotte soit coulée, pas un seul navire (le test précédent ne distinguait pas `All` de `Any`). `ResolveShot_Throws_WhenTargetIsOutOfBounds` couvre maintenant les 4 bornes (ligne/colonne, négatif/≥Size). 40/40 tests passent après correctifs.
- Non retenu (hors scope de cette story) : historique chronologique des tirs — déjà prévu Epic 2 / Story 2.3 (CAP-10). Alternance stricte des tours non appliquée dans `Game` — délibéré : `GameService` (Story 1.5, AD-4) appelle `ApplyShot` pour le joueur puis immédiatement pour l'ordinateur dans une même requête ; `Game` reste une primitive d'état, pas l'orchestrateur du tour.

## Review Triage Log

- **`IsFleetSunk`/`Game.ApplyShot` jamais testés avec deux navires (le test existant ne distinguait pas `All` de `Any`)** — medium, réel, rattaché directement à l'AC "tous les navires d'un camp sont coulés". Corrigé : test à deux navires ajouté (`GridTests` et `GameTests`).
- **`ResolveShot_Throws_WhenTargetIsOutOfBounds` ne couvrait qu'une seule borne** — low, réel. Corrigé : `Theory` couvrant les 4 bornes.
- **`Game.ApplyShot` accepte silencieusement toute valeur `Side` non définie comme `Computer`** — medium, réel, rattaché à AD-5 (le domaine garde ses propres gardes) et pertinent pour la frontière API/gRPC à venir. Corrigé : `switch` exhaustif avec rejet explicite.
- **Constructeur `Game` sans garde sur grilles nulles ou identiques** — medium, réel (corruption silencieuse d'état si même instance passée deux fois). Corrigé : `ArgumentNullException`/`ArgumentException`.
- **Absence de contexte (quelle grille/quel camp) dans les messages d'exception de `Grid`** — low, rejeté. `Grid` est volontairement agnostique du camp (c'est `Game`/la future couche API qui connaît le `Side` et le `GameId`) ; ajouter ce contexte à `Grid` brouillerait sa frontière de responsabilité pour un gain de débogage marginal.
- **`ShotsPlayed` n'a pas de structure chronologique pour un futur historique** — non applicable à cette story : c'est exactement le périmètre de la Story 2.3 (CAP-10, historique des coups), déjà planifiée.
- **Pas d'alternance de tour appliquée dans `Game`** — non applicable à cette story par conception : l'orchestration du tour (joueur puis riposte IA) revient à `GameService` en Story 1.5 (AD-4) ; `Game.ApplyShot` reste une primitive appelable par camp, pas l'arbitre de l'ordre.
- **`targetGrid` recalculé par ternaire/switch à chaque appel plutôt qu'extrait dans une méthode nommée** — rejeté. Un seul point d'usage aujourd'hui ; extraire une méthode maintenant serait une abstraction prématurée (à revoir si la Story 1.5 en a réellement besoin ailleurs).
- **Constructeurs des nouvelles exceptions limités à `(string message)`** — non un défaut, cohérent avec `ShipOutOfBoundsException`/`ShipOverlapException` de la Story 1.1 ; pas d'action.

---
title: 'Adversaire ordinateur — stratégie aléatoire (facile)'
type: 'feature'
created: '2026-09-15'
status: 'done'
route: 'oneshot'
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Il n'existe encore aucun adversaire ordinateur capable de choisir un coup ; la boucle de jeu (Story 1.5) ne peut pas faire riposter l'ordinateur.

**Approach:** Introduire `IOpponentStrategy` (`BattleShip.Models/Domain/Opponent/`) avec une implémentation `EasyOpponentStrategy` qui choisit uniformément au hasard une coordonnée non encore jouée sur la grille ciblée (`Grid.ShotsPlayed`), et qui retourne systématiquement la dernière case restante quand une seule est encore disponible. Testé unitairement : jamais de case déjà jouée, comportement correct sur une grille presque entièrement jouée.

</frozen-after-approval>

## Implementation Notes

- Nouveaux fichiers dans `BattleShip.Models/Domain/Opponent/` : `IOpponentStrategy.cs` (contrat `ChooseShot(Grid targetGrid)`), `EasyOpponentStrategy.cs` (énumère les 100 cases, filtre `Grid.ShotsPlayed`, tire uniformément parmi les cases restantes).
- Sous-namespace `BattleShip.Models.Domain.Opponent`, cohérent avec le dossier `Opponent/` du structural seed de l'architecture ; référence `BattleShip.Models.Domain` pour `Grid`/`Coordinate`.
- `EasyOpponentStrategy` accepte un `Random` optionnel (même pattern que `GameEngine`, Story 1.1) pour des tests déterministes.
- Tests ajoutés dans `EasyOpponentStrategyTests.cs` : jamais de case déjà jouée (50 tirs consécutifs sur la même grille), retourne bien la dernière case restante quand une seule reste, lève une exception explicite si la grille est entièrement jouée (cas non couvert par l'AC mais cohérent avec le principe "le domaine garde ses propres gardes", AD-5).
- 43/43 tests passent, build sans avertissement.
- Suite à la revue Blind Hunter : `EasyOpponentStrategy.ChooseShot` valide désormais `targetGrid` non nul (`ArgumentNullException`). La logique "cases restantes" est extraite en `Grid.RemainingCells()` (réutilisable par les futures stratégies, notamment la Story 2.1) au lieu d'être dupliquée dans la stratégie. Un test type par `IOpponentStrategy` (pas seulement `EasyOpponentStrategy`) et un test de déterminisme du `Random` injecté (même graine → même séquence) ont été ajoutés. 45/45 tests passent après correctifs.
- Reporté (voir `deferred-work.md`) : `IOpponentStrategy.ChooseShot(Grid)` expose toute la grille, y compris les positions réelles des navires — pas un problème pour la stratégie facile qui ne lit que `ShotsPlayed`, mais la forme définitive de l'interface (et si elle doit exposer hit/miss par case) sera à trancher à l'ouverture de la Story 2.1 (adversaire difficile), pas devinée maintenant.

## Review Triage Log

- **`ChooseShot` ne valide pas `targetGrid` non nul, contrairement à la convention établie (`Game`, Story 1.2)** — low, réel, fix trivial. Corrigé.
- **`IOpponentStrategy` expose la grille complète (navires inclus), pas seulement l'historique des tirs** — medium, réel, mais la correction dépend des besoins non encore connus de la Story 2.1 (hit/miss par case). Reporté dans `deferred-work.md` plutôt que deviné maintenant.
- **Les tests instancient `EasyOpponentStrategy` directement plutôt que de typer par `IOpponentStrategy`** — low, réel. Corrigé (un test re-typé).
- **Aucun test ne verrouille le déterminisme du `Random` injecté (un bug pourrait ignorer la graine sans faire échouer les tests existants)** — medium, réel. Corrigé : test de séquence identique pour une même graine.
- **Logique "cases restantes" dupliquée en dur dans `EasyOpponentStrategy` plutôt que sur `Grid`** — low, réel, anticipant la réutilisation par la Story 2.1. Corrigé : `Grid.RemainingCells()`.
- **Recalcul complet des 100 cases à chaque appel plutôt qu'un suivi incrémental** — low, rejeté. Coût négligeable sur une grille 10x10 ; le reviewer lui-même le qualifie de non-problème.
- **`InvalidOperationException` générique plutôt qu'un type dédié pour "plus de case disponible"** — low, rejeté. Cohérent avec le précédent déjà posé par `GameEngine` (Story 1.1) pour un garde-fou équivalent.
- **Nom de paramètre `targetGrid` jugé ambigu** — low, rejeté. Cohérent avec le nom déjà utilisé dans `Game.ApplyShot` ; renommer n'apporterait pas de clarté supplémentaire réelle.

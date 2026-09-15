---
title: 'Moteur de jeu — grilles et placement des flottes'
type: 'feature'
created: '2026-09-15'
status: 'done'
route: 'oneshot'
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le moteur de jeu n'existe pas encore : aucune structure ne représente une grille de bataille navale ni ne permet d'y placer une flotte de navires de façon valide et indépendante du transport/UI.

**Approach:** Implémenter dans `BattleShip.Models/Domain` un moteur de placement (`Grid` 10x10, `Ship`, `GameEngine`) qui génère un placement aléatoire d'une flotte de 5 navires (tailles 5/4/3/3/2), horizontal ou vertical uniquement, sans chevauchement ni débordement, deux grilles indépendantes par partie. Zéro dépendance ASP.NET/gRPC/FluentValidation (AD-1). Couvert par des tests unitaires dans `BattleShip.Tests/Unit` qui référencent uniquement `BattleShip.Models`.

</frozen-after-approval>

## Implementation Notes

- Fichiers créés dans `BattleShip.Models/Domain/` : `Coordinate.cs` (record struct 0-indexé), `Orientation.cs`, `Ship.cs`, `Grid.cs`, `FleetBlueprint.cs` (composition standard, noms en français cohérents avec `game-design.md`), `GameEngine.cs` (placement aléatoire par tirage/rejet jusqu'à obtenir un placement valide, borné par la taille de grille 10x10).
- `GameEngine` accepte un `Random` optionnel en constructeur (défaut `Random.Shared`) pour permettre des tests déterministes sans changer le comportement de production.
- `GameEngine.CreateGrids()` ajouté en plus de `CreateGridWithRandomFleet()` pour produire directement les deux grilles indépendantes d'une partie (joueur + ordinateur), anticipant le besoin de la Story 1.2/1.4 sans y implémenter la logique de partie elle-même.
- Suppression des fichiers placeholder de scaffold `BattleShip.Models/Class1.cs` et `BattleShip.Tests/UnitTest1.cs`, remplacés par le code réel.
- Tests unitaires ajoutés dans `BattleShip.Tests/Unit/` : `GridTests.cs` (rejet du chevauchement, rejet du débordement, acceptation de navires adjacents non chevauchants) et `GameEngineTests.cs` (composition de flotte, alignement horizontal/vertical strict, absence de chevauchement/débordement sur placement aléatoire, indépendance des deux grilles générées). `dotnet build` et `dotnet test` passent (8/8).
- Aucune surprise : le scope est resté conforme à l'intent, aucune dépendance ASP.NET/gRPC introduite dans `BattleShip.Models`.
- Suite à la revue Blind Hunter : `BattleShip.Tests.csproj` référence maintenant directement `BattleShip.Models` (en plus de `BattleShip.API`, conservé pour les futurs tests d'intégration), conforme à AD-7. `Ship` valide désormais que ses cellules forment une ligne droite contiguë cohérente avec l'orientation déclarée (`ArgumentException` sinon). `Grid.PlaceShip` distingue `ShipOutOfBoundsException` et `ShipOverlapException` (au lieu d'un seul `InvalidOperationException`) pour préparer le mapping d'erreurs de la Story 1.5 (AD-11). `GameEngine` plafonne le tirage-rejet à 10 000 tentatives par navire. Ajout de `ShipTests.cs` et `CoordinateTests.cs`. 19/19 tests passent après correctifs.

## Review Triage Log

- **`BattleShip.Tests.csproj` ne référence pas `BattleShip.Models` directement (violation AD-7)** — medium, réel (vérifié : csproj ne référençait que l'API). Corrigé : ajout de la référence directe.
- **Statut sprint resté `in-progress` alors que les tests passent déjà** — false. Le workflow oneshot ne bascule le statut sur `review` qu'à l'étape Finalize, qui suit délibérément Implement/Review/Classify ; l'observation décrit un état intermédiaire normal, pas un oubli.
- **`GameEngine.GenerateNonOverlappingShip` : boucle de tirage-rejet non bornée** — low (peu probable d'être atteint avec la flotte standard 17/100 cases, mais correctif simple). Corrigé : plafond de 10 000 tentatives avec exception explicite.
- **`Ship` n'impose pas que `Cells` soit cohérent avec `Orientation` / forme une ligne contiguë** — medium, réel et directement rattaché à AD-5 (le domaine garde ses propres gardes). Corrigé : validation de géométrie dans le constructeur.
- **Absence de tests directs sur `Ship` (validation du constructeur)** — low, réel. Corrigé : `ShipTests.cs` ajouté.
- **`Grid.PlaceShip` utilise le même type d'exception pour deux causes distinctes (débordement vs chevauchement)** — medium, réel, anticipant le mapping d'erreurs gRPC de la Story 1.5 (AD-11). Corrigé : `ShipOutOfBoundsException` et `ShipOverlapException` distincts.
- **Absence de commentaires XML doc sur l'API publique du domaine** — false, rejeté. Contredit la convention du projet (pas de commentaires sauf pour capturer un WHY non évident) ; les noms des types/méthodes (`Grid`, `Ship`, `GameEngine`, `PlaceShip`, `CreateGridWithRandomFleet`) sont auto-documentés.
- **Absence de test direct sur `Coordinate.IsWithinBounds`** — low, réel. Corrigé : `CoordinateTests.cs` ajouté (cas limites).

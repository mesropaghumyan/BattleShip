# ADR 0006 : Stratégie adverse (`IOpponentStrategy`)

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le projet doit fournir un adversaire ordinateur, avec au moins un niveau de difficulté au socle et une extension possible (backlog). Sans abstraction, le branchement de difficulté risquerait d'être dispersé en `if/else` dans le moteur de jeu, ou de dupliquer le moteur pour chaque niveau.

## Options envisagées
- Logique de difficulté en `if/else` directement dans `GameEngine`/`Game` : rejeté implicitement, dispersion et couplage du moteur à chaque nouvelle difficulté.
- Un moteur de jeu dupliqué par niveau de difficulté : rejeté implicitement (« moteurs dupliqués »).
- Interface unique `IOpponentStrategy` avec une implémentation par niveau, sélectionnée par paramètre à la création de partie : retenue.

## Décision
Interface unique `IOpponentStrategy` avec deux implémentations, `EasyOpponentStrategy` (tir aléatoire) et `HardOpponentStrategy` (hunt & target, parité damier), sélectionnées par un paramètre `Difficulty` à la création de partie. Ajouter une difficulté = ajouter une implémentation, jamais modifier la boucle de jeu.

## Conséquences
- La Story 2.1 (`HardOpponentStrategy`) a pu être ajoutée sans modifier `GameEngine`/`Game`, confirmant la valeur de l'abstraction.
- `IOpponentStrategy.ChooseShot(Grid)` expose toute la grille (dont les navires non découverts), pas seulement l'historique des tirs — risque de « triche » signalé dès la Story 1.3. Vérifié en Story 2.1 : `HardOpponentStrategy` n'accède jamais à `Grid.Ships`, s'appuyant sur `Grid.GetShotOutcome`/`Grid.UnsunkHits` à la place. Le risque s'est donc concrétisé sainement en pratique, mais la garde de type reste absente (voir `REVUE-IA.md`, revue 4).
- La Story 2.2 a dû faire persister la `Difficulty` choisie sur `Game`, écart non anticipé par la planification initiale de l'Epic 2 (cf. rétrospective Epic 1, « Questions ouvertes »).

## Vérification et réexamen
Tests dédiés par implémentation (`EasyOpponentStrategyTests`, `HardOpponentStrategyTests` — 16 tests couvrant les 3 phases hunt/target/line et les cas limites). À réexaminer si une future implémentation a besoin d'accéder légitimement à `Grid.Ships` (aucune ne le fait à ce jour) : introduire alors une garde de type explicite plutôt que de laisser l'accès ouvert.

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-6`

# ADR 0001 : Placement du noyau de domaine

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le socle impose 4 projets .NET (API, App, Models, Tests). Sans frontière explicite, la logique métier (règles du moteur de jeu) risque d'être dupliquée ou couplée au transport HTTP/gRPC directement dans `BattleShip.API`/`BattleShip.App`, rendant le moteur non testable indépendamment de l'infrastructure.

## Options envisagées
- Écrire le moteur de jeu directement dans `BattleShip.API` (couplé aux endpoints), en dupliquant si besoin côté `BattleShip.App` pour un repli local : rejeté implicitement, cause la duplication que la règle interdit.
- Isoler le moteur de jeu et les DTOs HTTP partagés dans `BattleShip.Models`, sans dépendance à ASP.NET/gRPC/FluentValidation : retenue.

## Décision
Le moteur de jeu (`Grid`, `Ship`, `Shot`, `Game`, `GameEngine`, `IOpponentStrategy`) et les DTOs HTTP d'échange vivent tous dans `BattleShip.Models`. `BattleShip.Models` ne référence aucun package ASP.NET/gRPC/FluentValidation, uniquement le BCL.

## Conséquences
- Le moteur est testable en isolation (`BattleShip.Tests/Unit`) sans dépendre d'un serveur ou d'un navigateur.
- `BattleShip.API` et `BattleShip.App` ne font qu'adapter le domaine à leur transport respectif.
- Toute story suivante qui ajoute une dépendance externe dans `BattleShip.Models` viole cet invariant.

## Vérification et réexamen
`[ADOPTED]` — déjà vrai dans le `.csproj` existant au moment de l'architecture. Chaque story API/App (1.4, 1.5, 1.6) a ajouté un test de régression dédié (`ModelsProject_StillReferencesNo...`) qui vérifie l'absence de ces dépendances dans `BattleShip.Models.csproj` — confirmé sain en rétrospective de l'Epic 1. À revoir seulement si une story future a besoin d'une dépendance domaine externe (improbable dans le périmètre du cours).

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-1`

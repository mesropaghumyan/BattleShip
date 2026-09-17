# ADR 0008 : Frontière App/API — pas de `ProjectReference`

## Statut et date
Accepté — 2026-09-15.

## Contexte
`BattleShip.App` (Blazor WebAssembly) et `BattleShip.API` sont deux projets distincts communiquant par réseau. Un couplage de compilation accidentel entre eux casserait le build WASM ou ferait fuiter des dépendances serveur (FluentValidation, Grpc.AspNetCore) dans le bundle navigateur.

## Options envisagées
- `BattleShip.App` référence directement `BattleShip.API` (ou l'inverse) via `ProjectReference`, pour partager du code au-delà des DTOs : rejeté implicitement, risque de fuite de dépendances serveur dans le bundle client et de couplage de compilation.
- Toute communication passe par le réseau (HTTP/gRPC-Web), seuls les types de `BattleShip.Models` sont partagés en compilation : retenue.

## Décision
`BattleShip.App` ne prend jamais de `ProjectReference` vers `BattleShip.API` (ni l'inverse) ; toute communication passe par le réseau (HTTP / gRPC-Web), les seuls types C# partagés viennent de `BattleShip.Models`.

## Conséquences
- Le bundle WASM ne contient jamais FluentValidation, Grpc.AspNetCore ou toute autre dépendance strictement serveur.
- Chaque story API/App (1.4, 1.5, 1.6) a ajouté son propre test de régression (`ModelsProject_StillReferencesNo...` / vérification de `.csproj`) qui protège cet invariant — confirmé « protection vivante » en rétrospective de l'Epic 1.
- Un test de régression sur les options JSON du client (`GameHttpClient`) manque toujours : le bug de synchronisation camelCase serveur/client (Story 1.6) ne serait pas rattrapé par `dotnet test` s'il revenait (voir `REVUE-IA.md`, revue 1).

## Vérification et réexamen
`[ADOPTED]` — déjà vrai dans les `.csproj` existants au moment de l'architecture, fixé ici comme invariant durable. Vérifié à chaque story qui touche App ou API. À réexaminer seulement si un besoin de partage de code au-delà des DTOs apparaissait (aucun cas identifié dans ce projet).

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-8`

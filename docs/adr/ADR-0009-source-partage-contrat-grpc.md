# ADR 0009 : Source et partage du contrat gRPC

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le contrat de l'action de tir doit être partagé entre `BattleShip.API` (serveur gRPC) et `BattleShip.App` (client gRPC-Web), sans passer par une `ProjectReference` (AD-8) ni introduire un type ambigu partagé entre les deux transports (HTTP et gRPC).

## Options envisagées
- Un POCO C# `FireShotRequest` partagé dans `BattleShip.Models`, dupliquant le message protobuf sans relation définie : rejeté implicitement, crée un type ambigu entre les deux transports.
- Générer le contrat gRPC séparément côté API et côté App à partir de copies indépendantes du `.proto` : alternative écartée implicitement par le choix d'un fichier unique partagé par chemin.
- Un unique `battlefield.proto` à la racine du dépôt, hors des 4 projets, inclus par chemin relatif des deux côtés (`GrpcServices="Server"` / `"Client"`) : retenue.

## Décision
`battlefield.proto` vit dans un dossier `Protos/` à la racine du dépôt, hors des 4 projets, inclus par chemin relatif (`<Protobuf Include="../Protos/battlefield.proto" GrpcServices="Server"/>` côté API, `GrpcServices="Client"` côté App) — jamais via `ProjectReference` (étend AD-8). Les messages protobuf générés sont l'unique contrat de l'action tir ; les DTOs C# de `BattleShip.Models/Contracts` sont l'unique contrat HTTP. Aucun type n'est partagé entre les deux.

## Conséquences
- Le contrat gRPC est versionné à un seul endroit (`Protos/battlefield.proto`), sans dépendre d'une `ProjectReference` interdite par AD-8.
- La génération double (server + client) à partir du même fichier a nécessité `<NoWarn>CS0436</NoWarn>` sur `BattleShip.Tests.csproj` pour éviter un faux conflit de types générés — compromis documenté, masque potentiellement une future collision de types sans rapport (tracé dans `deferred-work.md`, non corrigé).
- Incohérence mineure dans `battlefield.proto` : `FireShotRequest` utilise `row`/`col` bruts alors que `ShotResult` les enveloppe dans un message `Coordinate` — signalée, non corrigée car cela casserait le contrat déjà implémenté et testé (`deferred-work.md`).

## Vérification et réexamen
71 tests au vert dont un round-trip gRPC in-process (Story 1.5) ; génération vérifiée des deux côtés (`dotnet build` sans erreur sur API et App). À réexaminer si un auteur client (future story) trouve l'incohérence `row/col` vs `Coordinate` réellement gênante en pratique.

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-9`

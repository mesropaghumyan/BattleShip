---
title: 'API — jouer un coup via gRPC'
type: 'feature'
created: '2026-09-15'
status: 'done'
route: 'dispatch'
review_loop_iteration: 0
context: []
baseline_commit: 'f3178a83fced2cca9674307233ce9235bf2bf891'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Une partie peut être créée et consultée (Story 1.4), mais rien ne permet de jouer un coup : la boucle de jeu (moteur + adversaire facile, Stories 1.1-1.3) n'est reliée à aucun transport. C'est aussi l'unique exigence gRPC obligatoire du socle (AD-4, AD-9).

**Approach:** Exposer un service gRPC `Battlefield.FireShot` (contrat `Protos/battlefield.proto`, à la racine du dépôt, référencé par chemin relatif depuis `BattleShip.API` en `GrpcServices="Server"`) qui applique le tir du joueur puis, si la partie n'est pas terminée, la riposte immédiate de l'ordinateur (`EasyOpponentStrategy`) dans le même appel, via `GameService` (seul appelant de `IGameStore`, AD-3). FluentValidation valide la requête avant tout appel au domaine (AD-5) ; les erreurs sont mappées aux codes gRPC fixés par AD-11.

## Boundaries & Constraints

**Always:** `battlefield.proto` vit dans `Protos/` à la racine du dépôt, jamais dans un des 4 projets, jamais via `ProjectReference` (AD-8, AD-9). `GameService` reste le seul appelant de `IGameStore` — le service gRPC ne touche jamais le store directement (AD-3). FluentValidation valide `FireShotRequest` (coordonnées dans la grille, `GameId` un GUID syntaxiquement valide) avant tout appel à `GameService`. Codes d'erreur gRPC fixés par AD-11 : coordonnée hors grille ou `GameId` malformé → `InvalidArgument` ; case déjà jouée ou partie déjà terminée → `FailedPrecondition` ; `GameId` inconnu (syntaxiquement valide mais absent du store) → `NotFound`.

**Never:** Aucun endpoint HTTP n'accepte de tir (déjà vrai depuis la Story 1.4, AD-4). Pas de câblage du client Blazor ni de CORS dans cette story — c'est la Story 1.6. Le contrat `FireShotRequest`/`ShotTurnReply` (messages protobuf) est l'unique forme d'échange pour cette action ; aucun DTO C# ne le duplique (AD-9).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Tir valide, partie continue | `FireShot(gameId, row, col)` sur une partie en cours, tir ne coule pas la flotte adverse | `ShotTurnReply` avec `PlayerShot` rempli, `ComputerShot` rempli (riposte immédiate), `Status = InProgress` | N/A |
| Tir valide qui gagne la partie | Dernier navire adverse coulé par ce tir | `ShotTurnReply` avec `PlayerShot = Sunk`, `ComputerShot` absent, `Status = Won` | N/A |
| Coordonnée hors grille | `row`/`col` hors `[0, 9]` | Erreur gRPC | `StatusCode.InvalidArgument` |
| `GameId` malformé | chaîne non-GUID | Erreur gRPC | `StatusCode.InvalidArgument` |
| Case déjà jouée | tir sur une case déjà tirée pour cette partie | Erreur gRPC, aucun état modifié | `StatusCode.FailedPrecondition` |
| Partie déjà terminée | `FireShot` après victoire/défaite | Erreur gRPC, aucun état modifié | `StatusCode.FailedPrecondition` |
| `GameId` inconnu | GUID valide mais absent du store | Erreur gRPC | `StatusCode.NotFound` |

</frozen-after-approval>

## Code Map

- `Protos/battlefield.proto` -- contrat gRPC (`FireShotRequest`, `ShotResult`, `ShotTurnReply`, enums `ShotOutcome`/`GameOutcome`), référencé par chemin relatif (AD-9)
- `BattleShip.API/BattleShip.API.csproj` -- ajouter `Grpc.AspNetCore` + `Grpc.AspNetCore.Web` (2.83.0/2.80.0) et l'item `<Protobuf Include="../Protos/battlefield.proto" GrpcServices="Server" />`
- `BattleShip.API/Grpc/BattlefieldGrpcService.cs` -- implémente `FireShot` : validation, appel à `GameService.PlayShot`, mapping des exceptions du domaine vers les codes gRPC (AD-11)
- `BattleShip.API/Validation/FireShotRequestValidator.cs` -- `GameId` (GUID valide), `Row`/`Col` dans `[0, Grid.Size - 1]`
- `BattleShip.API/Services/GameService.cs` -- ajouter `PlayShot(Guid gameId, Coordinate coordinate)` : tir joueur puis, si partie non terminée, riposte `EasyOpponentStrategy` dans le même accès `IGameStore.WithGame` (atomique, AD-3) ; retourne `null` si partie inconnue
- `BattleShip.API/Program.cs` -- `AddGrpc()`, `UseGrpcWeb()`, `MapGrpcService<BattlefieldGrpcService>().EnableGrpcWeb()`
- `BattleShip.Tests/BattleShip.Tests.csproj` -- ajouter `Grpc.Net.Client` (2.83.0) + `<Protobuf Include="../Protos/battlefield.proto" GrpcServices="Client" />` (rôle client, comme le sera l'App en Story 1.6)
- `BattleShip.Tests/Integration/BattlefieldGrpcServiceTests.cs` -- `WebApplicationFactory<Program>` + canal gRPC in-process (`factory.Server.CreateHandler()`), couvre la matrice ci-dessus

## Tasks & Acceptance

**Execution:**
- [x] `Protos/battlefield.proto` -- créer le contrat -- source unique du message d'échange (AD-9)
- [x] `BattleShip.API/BattleShip.API.csproj` -- ajouter packages + item Protobuf -- active la génération du service serveur
- [x] `BattleShip.API/Validation/FireShotRequestValidator.cs` -- créer le validateur -- FluentValidation avant le domaine (AD-5)
- [x] `BattleShip.API/Services/GameService.cs` -- ajouter `PlayShot` -- seul point d'entrée pour jouer un tour (AD-3)
- [x] `BattleShip.API/Grpc/BattlefieldGrpcService.cs` -- créer le service -- expose `FireShot`, mapping d'erreurs (AD-11)
- [x] `BattleShip.API/Program.cs` -- câbler gRPC/gRPC-Web -- intégration
- [x] `BattleShip.Tests/BattleShip.Tests.csproj` -- ajouter client gRPC de test -- permet d'appeler le service en test
- [x] `BattleShip.Tests/Integration/BattlefieldGrpcServiceTests.cs` -- créer les tests -- couvre la matrice I/O

**Acceptance Criteria:**
- Given une partie créée via `POST /games` (Story 1.4), when on enchaîne plusieurs `FireShot` jusqu'à couler toute la flotte adverse, then `Status` passe à `Won` sur le tir qui coule le dernier navire, et aucun `FireShot` supplémentaire n'est accepté ensuite.
- Given un tir qui coule un navire adverse mais pas toute la flotte, when la riposte de l'ordinateur est résolue dans le même appel, then `ComputerShot` reflète un tir réel sur `HumanGrid` (raté, touché ou coulé selon l'état de cette grille).
- Given `BattleShip.Models`, when le projet est compilé, then il ne référence toujours aucun package ASP.NET/gRPC/FluentValidation (AD-1 non régressé).

## Implementation Notes

- Implémenté par un sub-agent sans contexte préalable, à partir de la seule spec (protocole dispatch). Vérification indépendante : build propre (0 avertissement, 0 erreur), 68/68 tests (58 existants + 10 nouveaux dans `BattlefieldGrpcServiceTests`), chaque ligne de la matrice I/O couverte par un test qui a effectivement tourné (suite exécutée 5 fois de suite pour écarter tout flake lié au tirage aléatoire de la flotte/riposte IA).
- `GameService.PlayShot` retourne un type interne (`PlayShotResult`/`ShotAttempt`, `sealed record`) plutôt que directement le message protobuf : ce n'est pas le contrat de fil (AD-9 porte sur `FireShotRequest`/`ShotTurnReply`, pas sur la plomberie interne `GameService` → `BattlefieldGrpcService`), et ça garde `GameService` sans dépendance sur les types générés.
- `GameService` reçoit `IOpponentStrategy` par DI (résolu vers `EasyOpponentStrategy` dans `Program.cs`) plutôt que d'instancier `EasyOpponentStrategy` en dur : cohérent avec AD-6, sans coût — `Difficulty.Hard` reste rejeté en amont (Story 1.4), donc le comportement observable est identique à un câblage en dur cette epic.
- Les types protobuf générés (`Coordinate`, `ShotOutcome`, `GameOutcome`) portent les mêmes noms courts que des types du domaine/contrats HTTP déjà existants. Comme prescrit par AD-9 ("aucun type n'est partagé entre les deux transports"), ces deux jeux de types restent distincts et coexistent dans le même fichier (`BattlefieldGrpcService.cs`, `BattlefieldGrpcServiceTests.cs`) via un alias d'espace de noms (`using Fb = BattleShip.Grpc;`) — nécessaire pour lever l'ambiguïté de compilation, pas une exigence du Code Map, mais une conséquence directe et attendue d'AD-9.
- `BattleShip.Tests` génère son propre stub client gRPC (`GrpcServices="Client"`, comme demandé par le Code Map) tout en gardant sa `ProjectReference` existante vers `BattleShip.API`, qui génère déjà le stub serveur depuis le même `.proto`. Les deux générations produisent donc chacune leur propre copie des types de messages, ce qui déclenche `CS0436` (type redéfini) — bénin (le compilateur préfère systématiquement la copie locale, ce qui est le comportement correct pour un projet jouant le rôle client) mais bruyant (123 avertissements) ; supprimé explicitement via `<NoWarn>CS0436</NoWarn>` documenté dans `BattleShip.Tests.csproj`. Ce pattern se reproduira à l'identique en Story 1.6 quand `BattleShip.App` générera son propre stub client — sans ce conflit-là, puisque `BattleShip.App` ne référence jamais `BattleShip.API` (AD-8).
- `Grpc.Net.Client` seul n'entraîne pas `Grpc.Tools`/`Google.Protobuf` (contrairement à `Grpc.AspNetCore` côté serveur, qui les inclut transitivement) : ajoutés explicitement à `BattleShip.Tests.csproj` (`Grpc.Tools` en `PrivateAssets="All"`), sinon l'item `<Protobuf GrpcServices="Client">` ne génère aucun code.
- Test `FireShot_OnAGameThatHasAlreadyFinished_...` et `FireShot_ThatSinksTheLastShip_...` amènent la partie à `Won` directement via `IGameStore` (contournement de gRPC, setup uniquement — même pratique que les tests HTTP existants de la Story 1.4) pour couler 16 des 17 cases de la flotte adverse de façon déterministe, puis jouent le dernier tir réel via gRPC : élimine tout risque de flakiness lié à l'ordre des tirs sans dépendre d'un `Random` seedé.

## Review Triage Log

Trois couches (Blind Hunter, Edge Case Hunter, Verification Gap) sur le diff propre (hors fichiers `.agents/`/`skills-lock.json` sans rapport, cf. note utilisateur).

- **Branche `Lost` de `ToGameOutcome` jamais exercée par un test (une régression collapsant tout `Finished` en `Won` passerait inaperçue)** — medium, réel, déposé pré-vérifié par Verification Gap, confirmé indépendamment par Blind Hunter (même cause racine). Route : patch. Corrigé : `LeaveOnlyOneUnplayedCellOnHumanGrid` + test `FireShot_WhereComputerRiposteWinsTheGame_ReturnsLost`.
- **Tests de bornes `FireShotRequestValidator` asymétriques (seuls `Row=Size` et `Col=-1` testés, pas `Row<0` ni `Col>=Size`)** — low, réel, fix trivial. Route : patch. Corrigé : `Theory` à 4 cas.
- **`GameService.PlayShot` : doc XML omet `ArgumentOutOfRangeException`, pourtant attrapée par `BattlefieldGrpcService`** — low, réel, fix trivial. Route : patch. Corrigé.
- **`opponentStrategy.ChooseShot` pourrait lever `InvalidOperationException` (grille pleine) non mappée vers un code gRPC AD-11** — false. Preuve : 100 cases, 17 cases de flotte, 83 hors-flotte ; par principe des tiroirs, une fois les 83 cases hors-flotte jouées, il ne reste que des cases de flotte, donc la flotte est nécessairement coulée au plus tard au 100ᵉ tir. `GameService.PlayShot` ne ré-appelle la stratégie que si `Status == InProgress`, état qui ne peut coexister avec une grille à 0 case restante. Chemin structurellement inatteignable.
- **`sprint-status.yaml` (`in-progress`) et la spec (`done`) en désaccord au moment de la revue** — false. État transitoire normal du workflow dispatch (statut `done` posé par le sub-agent avant la passe de revue ; `in-review` puis synchro `review` au step Present).
- **Écart de version `Grpc.AspNetCore` 2.83.0 / `Grpc.AspNetCore.Web` 2.80.0** — false. Déjà vérifié légitime (lignes de release indépendantes) lors de la revue de l'architecture spine.
- **Pas de config Kestrel explicite pour confirmer HTTP/2 hors tests** — false. Kestrel négocie HTTP/2 par ALPN sur HTTPS par défaut sans config supplémentaire ; le projet utilise déjà le profil `https`.
- **`FireShotRequest` (proto) utilise `row`/`col` bruts alors que `ShotResult` les enveloppe dans un message `Coordinate`** — medium si vrai, réel, mais corriger casserait le contrat déjà testé (nombreux sites d'appel). Route : defer.
- **`<NoWarn>CS0436</NoWarn>` global sur `BattleShip.Tests.csproj` masque aussi de futures collisions de types sans rapport** — low, réel, compromis déjà justifié dans les Implementation Notes. Route : defer.
- **`README.md`/`BattleShip.Tests.csproj` sans retour à la ligne final** — low, rejeté, cosmétique sans impact fonctionnel.

Vérifié indépendamment après correctifs : build propre, 71/71 tests.

---
title: 'Interface Blazor — créer une partie et jouer'
type: 'feature'
created: '2026-09-15'
status: 'done'
route: 'dispatch'
review_loop_iteration: 0
context: []
baseline_commit: '256ec70ca3da39a38b8e8935701530df798d13cc'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le socle est jouable via HTTP/gRPC (Stories 1.1-1.5) mais rien n'est accessible depuis un navigateur : `BattleShip.App` est encore le scaffold Blazor par défaut.

**Approach:** Deux pages Blazor WebAssembly — créer une partie (`NewGame.razor`, difficulté `Easy` fixe cette epic) et y jouer (`Play.razor` : deux grilles 10x10, clic sur la grille adverse pour tirer via gRPC-Web, affichage du résultat et de la fin de partie). `GameHttpClient`/`ShotGrpcClient` encapsulent les appels réseau. `BattleShip.API` ouvre une politique CORS nommée limitée à l'origine de l'App (AD-8, reporté depuis les Stories 1.4/1.5).

## Boundaries & Constraints

**Always:** `BattleShip.App` ne prend jamais de `ProjectReference` vers `BattleShip.API` — uniquement des appels réseau (AD-8). `battlefield.proto` est référencé par chemin relatif avec `GrpcServices="Client"`, jamais dupliqué (AD-9). Coordonnées affichées en notation A1..J10, converties uniquement à l'affichage depuis `Coordinate(Row, Col)` 0-indexé. Un incident de communication (API/gRPC injoignable) affiche un message d'erreur sans casser le reste de l'interface ; l'utilisateur peut réessayer. Une case déjà tirée sur la grille adverse ne déclenche pas de nouvel appel réseau (garde côté client).

**Never:** Aucun test Blazor automatisé n'est ajouté cette story — l'architecture ne prévoit que `Unit/` (domaine) et `Integration/` (API) dans `BattleShip.Tests` (AD-7) ; la vérification de cette story est manuelle (navigateur). Pas de sélection de difficulté, d'historique ni de statistiques cette story (Epic 2). Pas de câblage CORS au-delà de l'origine de l'App (pas de wildcard).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Création de partie | Clic sur "Nouvelle partie" | `POST /games` puis navigation vers `/play/{gameId}`, deux grilles affichées (5 navires côté joueur, grille adverse vide) | N/A |
| Tir sur case adverse valide | Clic sur une case non encore jouée de la grille adverse | `FireShot` gRPC-Web, résultat du tir joueur affiché puis, si la partie continue, celui de l'ordinateur | N/A |
| Case déjà jouée | Clic sur une case déjà tirée | Aucun appel réseau déclenché | Garde côté client |
| Incident réseau | API ou canal gRPC injoignable pendant une action | Message d'erreur affiché, reste de l'UI utilisable, nouvelle tentative possible | `try`/`catch` autour des appels, état d'erreur local au composant |
| Fin de partie | Le dernier navire adverse est coulé | Statut victoire/défaite affiché clairement ; la grille adverse n'accepte plus de clic | N/A |

</frozen-after-approval>

## Code Map

- `BattleShip.App/BattleShip.App.csproj` -- ajouter `Grpc.Net.Client`/`Grpc.Net.Client.Web` (2.83.0/2.80.0), `Google.Protobuf` (3.36.1), `Grpc.Tools` (2.83.0, `PrivateAssets="All"`), item `<Protobuf Include="../Protos/battlefield.proto" GrpcServices="Client" />`
- `BattleShip.API/Program.cs` -- politique CORS nommée limitée aux origines de l'App (`https://localhost:7297`, `http://localhost:5277`), en-têtes gRPC-Web exposés (`Grpc-Status`, `Grpc-Message`, `Grpc-Encoding`), `app.UseCors(...)` avant `UseGrpcWeb`/`Map*`
- `BattleShip.App/Program.cs` -- `HttpClient` pointant vers l'API (`https://localhost:7164`), `GrpcChannel`/`Battlefield.BattlefieldClient` via `GrpcWebHandler(new HttpClientHandler())` (pattern du cours)
- `BattleShip.App/Services/GameHttpClient.cs` -- `CreateGameAsync()`, `GetGameStateAsync(Guid)`
- `BattleShip.App/Services/ShotGrpcClient.cs` -- `FireShotAsync(Guid gameId, int row, int col)`, enveloppe `Fb.Battlefield.BattlefieldClient`
- `BattleShip.App/Components/BattleGrid.razor` -- rendu réutilisable d'une grille 10x10 (navires, tirs, cliquable ou non), notation A1..J10
- `BattleShip.App/Pages/NewGame.razor` -- page `/`, crée une partie, navigue vers `/play/{gameId}`
- `BattleShip.App/Pages/Play.razor` -- page `/play/{gameId}`, deux `BattleGrid`, gestion du tir, de la fin de partie et des erreurs
- `BattleShip.App/Layout/NavMenu.razor`, `BattleShip.App/Pages/{Home,Counter,Weather}.razor` -- retrait du scaffold par défaut inutilisé, remplacé par `NewGame`/`Play`

## Tasks & Acceptance

**Execution:**
- [x] `BattleShip.App/BattleShip.App.csproj` -- ajouter packages + item Protobuf -- génère le client gRPC-Web
- [x] `BattleShip.API/Program.cs` -- ajouter la politique CORS -- débloque les appels cross-origin de l'App
- [x] `BattleShip.App/Program.cs` -- câbler `HttpClient` + canal gRPC-Web -- accès réseau à l'API
- [x] `BattleShip.App/Services/{GameHttpClient,ShotGrpcClient}.cs` -- créer les clients -- encapsulent HTTP/gRPC pour les pages
- [x] `BattleShip.App/Components/BattleGrid.razor` -- créer le composant -- affichage réutilisable des deux grilles
- [x] `BattleShip.App/Pages/NewGame.razor` -- créer la page -- démarre une partie
- [x] `BattleShip.App/Pages/Play.razor` -- créer la page -- boucle de jeu complète, gestion d'erreur
- [x] Nettoyage du scaffold par défaut (`Home`/`Counter`/`Weather`/`NavMenu`) -- aligner la navigation sur le jeu

**Acceptance Criteria:**
- Given l'API et l'App lancées localement, when on crée une partie puis on joue jusqu'à la victoire ou la défaite, then une partie complète se déroule dans le navigateur sans rechargement de page ni erreur non gérée.
- Given `BattleShip.App`, when on inspecte ses références de projet, then aucune `ProjectReference` vers `BattleShip.API` n'existe (AD-8).
- Given `BattleShip.Models`, when le projet est compilé, then il ne référence toujours aucun package ASP.NET/gRPC/FluentValidation (AD-1 non régressé).

## Implementation Notes

- **Post-remise (retour utilisateur) : bug bloquant sur poste tiers.** `ApiBaseAddress` était codé en dur sur `https://localhost:7164`, port du profil de lancement `https`. `dotnet run` sans `--launch-profile` utilise le premier profil de `launchSettings.json`, qui est `http` (5268/5277) — l'API n'écoutait donc pas du tout sur 7164, et `app.UseHttpsRedirection()` redirigeait en plus toute requête HTTP vers une URL HTTPS non écoutée. Résultat : `POST /games` échouait systématiquement (`(failed)` réseau) avec le profil par défaut, même après avoir fait confiance au certificat dev. Corrigé : `ApiBaseAddress` pointe désormais sur `http://localhost:5268`, `UseHttpsRedirection()` retiré (démo locale uniquement, AD non régressée). Vérifié via `curl` : plus de redirection, `POST /games` répond `201` directement, CORS/en-têtes gRPC-Web intacts. Build + 71/71 tests toujours au vert.

- Implémenté par un sub-agent sans contexte préalable, à partir de la seule spec (protocole dispatch). Vérification indépendante : build propre (0 avertissement, 0 erreur), 71/71 tests `BattleShip.Tests` inchangés (aucun test Blazor ajouté, AD-7), et un scénario navigateur réel automatisé avec Playwright/Chromium contre l'API et l'App effectivement lancées en local (`https://localhost:7164` / `https://localhost:7297`) : création de partie, tir jusqu'à victoire (94 tirs, grille adverse qui cesse d'accepter les clics une fois `Won` atteint), puis un second passage où l'API est tuée en cours de partie pour confirmer que le message d'erreur s'affiche, que le reste de l'UI (statut, grille) reste rendu, et qu'une nouvelle tentative (re-clic sur la même case, toujours non tirée côté client) reste possible — sans exception JS non gérée dans les deux cas. Ce script Playwright était un scratch de vérification (hors dépôt, supprimé après usage), pas un test ajouté au projet.
- Bug trouvé et corrigé pendant cette vérification : `GameHttpClient` utilisait un `JsonSerializerOptions` par défaut (`PropertyNameCaseInsensitive = false`, pas de `PropertyNamingPolicy`). L'API sérialise en camelCase (défaut minimal API ASP.NET Core) alors que `GameStateDto`/`CreateGameResponse` ont des propriétés `required` en PascalCase côté C# : sans policy camelCase + case-insensitive côté client, la désérialisation aurait levé `JsonException` (propriétés requises jamais reconnues) dès le premier `GetGameStateAsync`. Corrigé en alignant `JsonSerializerOptions` du client sur les défauts d'ASP.NET Core (`PropertyNamingPolicy = JsonNamingPolicy.CamelCase`, `PropertyNameCaseInsensitive = true`), vérifié isolément (payload JSON réel capturé via `curl` contre l'API) puis via le scénario Playwright complet.
- `Play.razor` réutilise le pattern du proto (`ShotTurnReply` ne porte que le résultat du tour, AD-2) : après `ShotClient.FireShotAsync`, la page ré-appelle `GameHttpClient.GetGameStateAsync` pour rafraîchir les deux grilles complètes, et garde `ShotTurnReply` seulement pour un résumé texte ponctuel ("Vous : touché en B4. Ordinateur : manqué en E3.") — pas de state dupliqué entre les deux contrats.
- `BattleGrid.razor` porte la garde client "case déjà jouée" (aucun `OnCellClick.InvokeAsync` si un tir existe déjà pour la cellule) directement dans le composant réutilisable plutôt que dans `Play.razor`, pour qu'elle s'applique quel que soit l'appelant.
- `BattleShip.App/Program.cs` réutilise le seul `HttpClient` scoped existant (pointait vers `builder.HostEnvironment.BaseAddress` pour `Weather.razor`, aujourd'hui supprimé) et le repointe vers `BattleShip.API` -- pas de second `HttpClient` ajouté.
- `wwwroot/sample-data/weather.json` supprimé avec `Weather.razor` (donnée de scaffold orpheline, plus référencée par rien).
- CORS : `app.UseCors(AppCorsPolicy)` posé une seule fois avant `UseGrpcWeb`/les `Map*`, sans `RequireCors` par endpoint -- suffisant ici (aucun autre `AddDefaultPolicy`/`[EnableCors]` en conflit dans le pipeline) et validé par `curl` en préflight `OPTIONS` : origine autorisée (`http://localhost:5277`) reçoit `Access-Control-Allow-Origin`, une origine arbitraire n'en reçoit pas (pas de wildcard, AD-8).

## Verification

**Manual checks (no CLI for Blazor UI, per AD-7 scope):**
- Lancer `dotnet run --project BattleShip.API` puis `dotnet run --project BattleShip.App` (profils `https`), ouvrir l'App dans un navigateur, jouer une partie complète jusqu'à la victoire, vérifier l'affichage de la défaite en tirant délibérément sur la grille adverse jusqu'à sa fin (ou en simulant côté API), et couper l'API en cours de partie pour vérifier le message d'erreur.

**Commands:**
- `dotnet build BattleShip.slnx` -- expected: succès, 0 avertissement
- `dotnet test BattleShip.Tests/BattleShip.Tests.csproj` -- expected: tous les tests existants toujours au vert (aucun test Blazor ajouté cette story)

## Review Triage Log

Trois couches (Blind Hunter, Edge Case Hunter, Verification Gap) sur le diff propre. Vérification indépendante préalable (hors revue) : build, 71/71 tests, `POST`/`GET /games` en camelCase via `curl`, préflight CORS confirmant l'origine App autorisée et une origine arbitraire refusée.

- **`Play.razor.FireShotAsync` : si `GetGameStateAsync` échoue juste après un tir réussi, `state` reste périmé — la case tirée redevient cliquable côté `BattleGrid`, violant l'invariant "case déjà tirée = aucun nouvel appel réseau"** — medium, réel, signalé indépendamment par Blind Hunter et Edge Case Hunter (même cause racine). Route : patch. Corrigé : les deux appels réseau sont séparés en `try`/`catch` distincts ; en cas d'échec du rafraîchissement, `ApplyLocalShotFallback` fusionne localement le résultat déjà connu (`lastTurn`) dans `state` avant de signaler l'erreur.
- **`lastTurn.PlayerShot` rendu sans garde de nullité, contrairement à `ComputerShot` juste à côté** — low, réel (protobuf autorise un champ message absent, même si le serveur actuel le renseigne toujours). Route : patch. Corrigé : garde symétrique ajoutée.
- **`BattleGrid.razor` duplique `Grid.Size` du domaine dans une constante locale** — low, réel, le nom du paramètre `Grid` du composant masquait le type domaine, d'où la duplication d'origine. Route : patch. Corrigé : alias `@using DomainGrid = BattleShip.Models.Domain.Grid`.
- **Arm par défaut incohérent entre `BattleGrid` (aliase un `ShotOutcome` inconnu sur "miss", silencieux) et `Play.razor` (affiche "inconnu")** — low, réel mais aujourd'hui inatteignable (le serveur ne renvoie jamais de valeur non définie). Route : patch. Corrigé : classe CSS `battle-cell-unknown` dédiée.
- **`NavMenu.razor.css` conservait les règles CSS des liens Counter/Weather supprimés** — low, réel, nettoyage trivial. Route : patch. Corrigé.
- **`GameHttpClient` utilisait `!` sur `ReadFromJsonAsync` sans garde, masquant un corps de réponse vide/malformé derrière le même message générique "API injoignable"** — low, réel. Route : patch. Corrigé : `?? throw new InvalidOperationException(...)`.
- **Exceptions avalées sans trace (`catch (Exception)` sans log) dans `NewGame.razor`/`Play.razor`** — low, réel, gêne le débogage. Route : patch. Corrigé : `Console.Error.WriteLine` avant le message utilisateur.
- **`sprint-status.yaml` (`in-progress`) et la spec (`done`) en désaccord au moment de la revue** — false. État transitoire normal du workflow dispatch (même schéma que les Stories 1.4/1.5).
- **Test de régression manquant pour les options JSON de `GameHttpClient` (le bug camelCase trouvé manuellement ne serait pas rattrapé par `dotnet test`)** — medium, réel, déposé pré-vérifié par Verification Gap. Route : defer — le correctif propre demanderait une nouvelle `ProjectReference` `BattleShip.Tests → BattleShip.App` non prévue par l'architecture (AD-7 scope `Tests` à `Models`+`API`), ce qui dépasse un correctif "sans nouvelle structure".
- **En-têtes CORS exposés (`Grpc-Status`/`Grpc-Message`/`Grpc-Encoding`) non couverts par un test cross-origin automatisé** — medium si régression, réel. Route : defer, disposition déjà posée par Verification Gap (vérification manuelle `curl`/navigateur suffisante au regard du scope AD-7).
- **Garde "case déjà jouée" et verrouillage de fin de partie (`BattleGrid`) non couverts par un test de composant** — medium si régression, réel. Route : defer, disposition Verification Gap (pas d'outillage bUnit dans le dépôt).
- **URLs API/CORS codées en dur, pas de couche de configuration (`appsettings`)** — low, réel, hors scope (déploiement = non-goal du spec). Route : defer.
- **Aucune accessibilité clavier sur les cellules cliquables (`tabindex`, `role`, `@onkeydown`)** — medium, réel, mais extension optionnelle explicitement listée en backlog (diapo 60), pas un AC de cette story. Route : defer.
- **Fenêtre de course possible si un second clic passe avant que `Interactive` ne se mette à jour suite au premier tir** — maybe-false, dépend du scheduling exact du renderer Blazor WASM, non tranchable sans test de stress navigateur réel. Route : defer.

Vérifié indépendamment après correctifs : build propre, 71/71 tests.

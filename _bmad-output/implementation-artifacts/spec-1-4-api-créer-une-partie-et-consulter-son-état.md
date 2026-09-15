---
title: 'API — créer une partie et consulter son état'
type: 'feature'
created: '2026-09-15'
status: 'done'
route: 'dispatch'
review_loop_iteration: 0
context: []
baseline_commit: '70f96e8259d156d95597bb2501f4c12ab23b4ae6'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le moteur de jeu (Stories 1.1-1.3) n'existe qu'en mémoire de test ; rien n'expose la création et la consultation d'une partie depuis l'extérieur, et rien ne relie `GameEngine`/`Game`/`EasyOpponentStrategy` à un état de partie accessible.

**Approach:** Exposer `POST /games` (création, difficulté `Easy` uniquement cette epic) et `GET /games/{id}` (état, vue à deux faces) en Minimal API. Un store en mémoire singleton (`IGameStore`) garde l'état ; `GameService` est le seul appelant du store et orchestre création/lecture ; `GameViewMapper` est l'unique point de conversion Domaine → DTO et ne fuit jamais les navires adverses non coulés ; FluentValidation s'exécute avant tout appel au domaine.

## Boundaries & Constraints

**Always:** `GameService` est le seul composant qui appelle `IGameStore` (AD-3). `GameViewMapper` est le seul point de sérialisation Domaine→DTO ; aucun endpoint ne sérialise `Grid`/`Ship` directement (AD-2). FluentValidation s'exécute avant tout appel à `GameService` (AD-5). `IGameStore` est enregistré en Singleton (AD-3).

**Never:** Aucun endpoint HTTP n'accepte de tir (réservé au gRPC, Story 1.5, AD-4). Pas de câblage CORS dans cette story — l'App ne consomme pas encore l'API (Story 1.6). La difficulté `Hard` n'est pas acceptée cette epic (rejetée par validation) — `HardOpponentStrategy` n'existe pas encore (Epic 2).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Création valide | `POST /games {Difficulty: Easy}` | 201 + `{GameId}` | N/A |
| Création invalide | `POST /games` sans champ, ou `Difficulty: Hard` | 400 `ValidationProblem` | FluentValidation, aucune partie créée |
| Lecture partie existante | `GET /games/{id}` valide | 200 + `GameStateDto` (`OwnGrid`+`OpponentGrid`) | N/A |
| Lecture partie inconnue | `GET /games/{guid-inconnu}` | 404 | N/A |
| Vue adverse après des tirs | `GET /games/{id}` après quelques `Game.ApplyShot` simulés en test | `OpponentGrid` ne contient aucune coordonnée de navire non coulé | Test dédié |

</frozen-after-approval>

## Code Map

- `BattleShip.API/BattleShip.API.csproj` -- ajouter `FluentValidation` + `FluentValidation.DependencyInjectionExtensions` (12.1.1)
- `BattleShip.Models/Domain/Difficulty.cs` -- `enum { Easy, Hard }`, utilisé par `CreateGameRequest`
- `BattleShip.Models/Contracts/CreateGameRequest.cs` -- DTO HTTP d'entrée (`Difficulty`)
- `BattleShip.Models/Contracts/GameStateDto.cs` (+ sous-DTOs coordonnée/navire/résultat de tir) -- vue composite `OwnGrid`/`OpponentGrid` (AD-2)
- `BattleShip.API/State/IGameStore.cs`, `InMemoryGameStore.cs` -- `ConcurrentDictionary<Guid, Game>`, accès protégé par verrou par partie (AD-3)
- `BattleShip.API/Services/GameService.cs` -- `CreateGame` (`GameEngine.CreateGrids` + `EasyOpponentStrategy` + `IGameStore`), `GetGameState` (lecture + `GameViewMapper`)
- `BattleShip.API/Mapping/GameViewMapper.cs` -- `Game` → `GameStateDto`, statut relatif au joueur humain (InProgress/Won/Lost)
- `BattleShip.API/Validation/CreateGameRequestValidator.cs` -- `Difficulty` définie et égale à `Easy` cette epic
- `BattleShip.API/Endpoints/GameEndpoints.cs` -- `POST /games`, `GET /games/{id:guid}`
- `BattleShip.API/Program.cs` -- DI (`IGameStore` singleton, `GameService`, validators FluentValidation), retrait de l'endpoint `/weatherforecast` de démo (scaffold non utilisé), ajout `public partial class Program` pour `WebApplicationFactory`
- `BattleShip.Tests/Integration/GameEndpointsTests.cs` -- `WebApplicationFactory<Program>`, couvre la matrice ci-dessus

## Tasks & Acceptance

**Execution:**
- [x] `BattleShip.Models/Domain/Difficulty.cs` -- créer l'enum -- support du champ de création de partie
- [x] `BattleShip.Models/Contracts/{CreateGameRequest,GameStateDto}.cs` -- créer les DTOs -- contrat HTTP stable, séparé du domaine (AD-9)
- [x] `BattleShip.API/State/{IGameStore,InMemoryGameStore}.cs` -- créer le store -- état en mémoire, singleton, accès atomique (AD-3)
- [x] `BattleShip.API/Mapping/GameViewMapper.cs` -- créer le mapping -- frontière de visibilité (AD-2)
- [x] `BattleShip.API/Validation/CreateGameRequestValidator.cs` -- créer le validateur -- FluentValidation avant le domaine (AD-5)
- [x] `BattleShip.API/Services/GameService.cs` -- créer le service -- seul appelant de `IGameStore` (AD-3)
- [x] `BattleShip.API/Endpoints/GameEndpoints.cs` -- créer les endpoints -- expose création/lecture
- [x] `BattleShip.API/Program.cs` -- câbler la DI, retirer le scaffold démo, exposer `Program` -- intégration + testabilité
- [x] `BattleShip.Tests/Integration/GameEndpointsTests.cs` -- créer les tests -- couvre la matrice I/O

**Acceptance Criteria:**
- Given une partie créée en `Easy`, when on récupère son état juste après création, then `OwnGrid` contient 5 navires et `OpponentGrid` ne contient aucun tir.
- Given le store, when deux parties sont créées, then elles ont des `GameId` distincts et des états totalement indépendants.
- Given `BattleShip.Models`, when le projet est compilé, then il ne référence toujours aucun package ASP.NET/FluentValidation (AD-1 non régressé).

## Implementation Notes

- Implémenté par un sub-agent sans contexte préalable, à partir de la seule spec (protocole dispatch). Vérification indépendante : build propre (0 avertissement), 54/54 tests (45 existants + 9 nouveaux), chaque ligne de la matrice I/O couverte par un test qui a effectivement tourné.
- `CreateGameRequest.Difficulty` est `Difficulty?` (nullable), pas l'enum brut : un enum non-nullable vaudrait `0` = `Easy` par défaut sur un champ JSON absent, ce qui aurait fait passer silencieusement la validation et contredit la ligne "champ manquant → 400" de la matrice.
- `IGameStore` expose `Add`/`WithGame<TResult>` (verrou par partie) plutôt qu'un `Mutate` nommé littéralement ainsi — même garantie (accès atomique par partie, AD-3), forme légèrement différente du Code Map (guidance, hors bloc frozen).
- `EasyOpponentStrategy` n'est pas câblée dans `GameService.CreateGame` : cette story ne traite aucun tir (réservé à la Story 1.5, AD-4), donc l'injecter sans point d'appel aurait été du code mort. `IOpponentStrategy`/`EasyOpponentStrategy` restent intacts pour la Story 1.5.
- **Point de vigilance pour la Story 1.5/2.2** : la `Difficulty` demandée à la création n'est persistée nulle part (ni sur `Game`, ni dans le store) — seulement validée puis jetée. Story 1.5 peut donc utiliser `EasyOpponentStrategy` en dur sans problème (seule valeur possible cette epic). Mais la Story 2.2 (sélection de difficulté) devra ajouter un moyen de retrouver, pour une partie donnée, quelle stratégie utiliser — ce n'est pas fait ici, à traiter explicitement à l'ouverture de 2.2 plutôt que deviné maintenant.

## Review Triage Log

Trois couches (Blind Hunter, Edge Case Hunter, Verification Gap) exécutées sur le diff complet.

- **Exceptions non gérées (`GameService.CreateGame`, `IGameStore.Add` sur collision) remontent en 500 brut/non structuré** — medium, réel (Blind Hunter + Edge Case Hunter, même cause racine). Route: patch.
- **README.md mentionne encore `/weatherforecast` (supprimé) et ne documente pas `POST/GET /games`** — low, réel. Route: patch.
- **`BattleShip.API.http` référence encore l'ancien `/weatherforecast`** — low, réel (Edge Case Hunter, confidence high). Route: patch.
- **`CreateGame_WithHardDifficulty_DoesNotCreateAGame` ne prouve pas ce que son nom promet (sonde un Guid aléatoire non lié)** — low, réel mais non exploitable aujourd'hui (la couche Verification Gap confirme que l'assertion 400 existante suffit à attraper une régression réelle). Route: patch (renommage seulement, pas d'extension de `IGameStore`).
- **Aucun test sur `IGameStore.Add` appelé deux fois avec le même `GameId` (comportement documenté mais jamais vérifié)** — low, réel, fix trivial. Route: patch.
- **`Results.NotFound()` (corps vide) vs `ValidationProblem` (corps structuré) — formes de réponse 4xx incohérentes** — low, réel. Route: patch.
- **Aucun test sur `GET /games/{id}` avec un id non-GUID** — low, réel, fix trivial. Route: patch.
- **`GameViewMapper.ToOutcome` suppose `Winner` non-null sans garde quand `Status == Finished`** — medium, réel, rattaché à AD-5 (le domaine/la frontière garde ses propres invariants). Route: patch.
- **Contrat "enums en toutes lettres sur le fil" non verrouillé par un test (le client de test tolère aussi les entiers)** — medium, réel, verdict déposé par la couche Verification Gap avec repro vérifiée. Route: patch.
- **`Difficulty` non-bindable (ex. `99`) échoue au model binding avant FluentValidation, forme de réponse différente de `ValidationProblem`** — medium, réel, absent de la matrice I/O. Route: patch partiel (caractériser le comportement actuel par un test ; unification complète des formes reportée).
- **`InMemoryGameStore`/`_locks` sans éviction/TTL, croissance non bornée** — medium si vrai mais non spécifié par l'architecture (AD-3 ne couvre pas le cycle de vie). Route: defer.
- **Absence de test de concurrence réel sur le verrou par partie** — maybe-false (dépend de la vraisemblance d'une vraie course), fix non trivial (risque de flakiness). Route: defer.
- **Métadonnées OpenAPI absentes sur les nouveaux endpoints (`WithName`, `Produces<T>`)** — low, cosmétique, non requis par un AC ni un test. Route: defer.
- **Note sur `baseline_commit`** — false (SHA vérifié correct contre `git log`) ; pas une action, juste une vigilance générale du reviewer.

**Correctifs appliqués** (les 10 entrées routées `patch` ci-dessus) : `AddProblemDetails`/`UseExceptionHandler` dans `Program.cs` ; `README.md` et `BattleShip.API.http` mis à jour (retrait de `/weatherforecast`, doc des nouveaux endpoints) ; test renommé pour refléter ce qu'il prouve réellement ; test sur `IGameStore.Add` en double ; `GetGameState` renvoie un 404 structuré (`Results.Problem`) ; test sur id non-GUID ; garde explicite dans `GameViewMapper.ToOutcome` (`InvalidOperationException` si `Winner` null en fin de partie) ; test verrouillant les enums en toutes lettres sur le fil (lecture du JSON brut) ; test caractérisant le 400 sur une valeur `Difficulty` non-bindable. Vérifié indépendamment : build propre, 58/58 tests.

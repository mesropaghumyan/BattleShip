# Epic 1 Context: Partie jouable de bout en bout contre l'ordinateur

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Deliver a complete, playable game loop against a computer opponent, demonstrating the full technical foundation end to end: a transport- and UI-independent game engine, a Minimal API contract for creating/reading a game, a gRPC-Web shot action that resolves the player's shot and the computer's riposte in one round trip, server-side validation on both transports, and a Blazor WebAssembly UI that lets a player create a game, play it, and see it end. Every story ships with its own tests (unit and/or integration) able to catch a violated rule, not just the happy path.

## Stories

- Story 1.1: Moteur de jeu — grilles et placement des flottes
- Story 1.2: Moteur de jeu — résolution des tirs et fin de partie
- Story 1.3: Adversaire ordinateur — stratégie aléatoire (facile)
- Story 1.4: API — créer une partie et consulter son état
- Story 1.5: API — jouer un coup via gRPC
- Story 1.6: Interface Blazor — créer une partie et jouer

## Requirements & Constraints

- The game engine builds two independent 10x10 grids per game, each with a fleet of 5 ships (sizes 5/4/3/3/2) placed randomly, horizontal or vertical only, never overlapping or out of bounds, and never diagonal.
- A shot on an already-played cell is rejected without changing state and does not count as a move; a shot after game end is rejected outright.
- A shot resolves to miss/hit/sunk (sunk when every cell of a ship is hit); the game transitions to Won/Lost for the losing side once its whole fleet is sunk.
- The easy computer opponent picks a valid, not-yet-played cell uniformly at random and must be able to select the single remaining cell when only one is left.
- Creating a game accepts difficulty as an input, but only the easy strategy needs to be wired up in this epic (hard strategy is Epic 2).
- Reading game state must never leak an undiscovered opponent ship coordinate: only own fleet is fully visible; the opponent view exposes hit/miss/sunk results only.
- An unknown GameId on state lookup returns HTTP 404.
- Firing a shot is exposed exclusively via gRPC (never HTTP); it applies the player's shot then, if the game isn't over, immediately applies the computer's riposte in the same call, returning both results plus game status.
- gRPC error mapping is fixed: out-of-grid coordinate → InvalidArgument; already-played cell or shot after game end → FailedPrecondition; unknown GameId → NotFound.
- All server inputs (HTTP and gRPC) are validated with FluentValidation before reaching game logic; invalid HTTP requests return a RFC 7807 ValidationProblem.
- Game state lives in memory only — no database or file persistence, and nothing here should assume otherwise.
- Every story's tests must demonstrate a rule violation is actually caught (not only the nominal case).

## Technical Decisions

- Layered domain-kernel design: `BattleShip.Models` holds all game logic (Grid, Ship, Shot, Game, GameEngine, IOpponentStrategy) and shared HTTP DTOs, with zero dependency on ASP.NET, gRPC, or FluentValidation — only BCL.
- Domain → DTO conversion happens exclusively through a `GameViewMapper`; endpoints/gRPC never serialize the raw `Grid` entity. `GET /games/{id}` returns one composite `GameStateDto` (`OwnGrid` + `OpponentGrid`) — no separate endpoints per view.
- All game state lives behind a single `IGameStore` abstraction (Singleton, `ConcurrentDictionary<Guid, Game>`), mutated only via an atomic `Mutate(gameId, Action<Game>)` method (per-game lock). `GameService` is the only allowed caller of `IGameStore`; HTTP endpoints and the gRPC service both go through `GameService`, never the store directly.
- Transport split: game creation/read is Minimal API (`POST /games`, `GET /games/{id}`); the shot action is gRPC-only (`BattlefieldService.FireShot`), consumed as gRPC-Web from Blazor. `ShotTurnReply` carries the player shot result, the computer shot result (absent if the game ended after the player's shot), and game status. No HTTP endpoint accepts a shot.
- FluentValidation validators (`CreateGameRequestValidator`, `FireShotRequestValidator`) live in `BattleShip.API` and run before any call into `GameService`, on both transports. The domain also keeps its own guards/exceptions independent of FluentValidation.
- Opponent strategy is a single `IOpponentStrategy` interface; `EasyOpponentStrategy` (random shot) is the implementation needed in this epic. Adding difficulty means adding an implementation, never branching inside the game loop.
- `battlefield.proto` lives in a repo-root `Protos/` folder (outside the 4 projects), included by relative path from `BattleShip.API` (`GrpcServices="Server"`) and `BattleShip.App` (`GrpcServices="Client"`) — never via `ProjectReference`. Protobuf messages are the sole contract for the shot action; C# DTOs in `BattleShip.Models/Contracts` are the sole contract for HTTP. No type is shared between the two.
- `BattleShip.App` never takes a `ProjectReference` to `BattleShip.API`; the two communicate only over the network (HTTP, gRPC-Web). Only `BattleShip.Models` types are shared by reference.
- Side identity uses `enum Side { Human, Computer }` — no individual player identity; `GameId` (`Guid`) is the sole domain identifier.
- `Coordinate(Row, Col)` is 0-indexed internally across domain, API, and protobuf; letter+number notation (A1..J10) is a display-only conversion in Blazor.
- CORS: one named policy in `BattleShip.API`, scoped to the `BattleShip.App` origin.
- Tests live in one `BattleShip.Tests` project: `Unit/` references only `BattleShip.Models`; `Integration/` uses `WebApplicationFactory<Program>` for HTTP and gRPC in-process.
- The 4-project scaffold (.NET 10; API = Minimal API, App = Blazor WASM, Models = classlib, Tests = xUnit) already exists and is adopted as-is — this epic enriches it, it does not regenerate the solution.

## UX & Interaction Patterns

- `NewGame.razor`: player creates a game, which calls `POST /games` via `GameHttpClient` and navigates to `Play.razor` with the returned `GameId`.
- `Play.razor`: clicking a cell on the opponent grid calls `FireShot` over gRPC-Web via `ShotGrpcClient`, and the UI displays the player's shot result followed by the computer's.
- Communication failures (API unreachable, network error) surface as an on-screen error message without breaking the rest of the UI; the player can retry.
- When game status changes to Won/Lost, the UI clearly shows the end-of-game outcome.
- Coordinates are shown to the player in A1..J10 notation, converted only at the display layer from the internal 0-indexed `Coordinate`.

## Cross-Story Dependencies

- Story 1.2 (shot resolution) builds directly on the grid/fleet state produced by Story 1.1.
- Story 1.3 (easy opponent) consumes the grid/shot model from 1.1/1.2 to choose valid, unplayed cells.
- Story 1.4 (HTTP create/read) and Story 1.5 (gRPC FireShot) both depend on the engine (1.1, 1.2) and, for 1.5, the opponent strategy (1.3) via `GameService`/`IGameStore`.
- Story 1.6 (Blazor UI) depends on both Story 1.4 (HTTP client for create/read) and Story 1.5 (gRPC client for shots) being available.
- Epic 2 (difficulty selection, hard strategy, history, stats) builds on this epic's `IOpponentStrategy` contract, `GameService`, and `ShotTurnReply` shape — no changes anticipated to those contracts, only additive implementations.

# Independent Review — ARCHITECTURE-SPINE.md (Bataille Navale)

**Reviewed document:** `_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md`
**Traceability sources:** `_bmad-output/specs/spec-battleship/SPEC.md`, `_bmad-output/specs/spec-battleship/game-design.md`
**Brownfield spot-check:** `BattleShip.API/BattleShip.API.csproj`, `BattleShip.App/BattleShip.App.csproj`, `BattleShip.Models/BattleShip.Models.csproj`, `BattleShip.Tests/BattleShip.Tests.csproj`

**Overall verdict:** Solid spine with a sound layering paradigm and 8 well-scoped ADs; it correctly ratifies the brownfield `.csproj` setup and correctly treats deployment as a non-goal. However it has one real, unfixed divergence point (a named-but-unseeded `GameService`, with an unresolved naming collision between the gRPC wire contract and the `Models.Contracts` POCOs), one orphaned concept (`PlayerId`), and one internal version inconsistency in the gRPC package family that should be corrected before builders start.

---

## 1. Real divergence points fixed for the level below

**Assessment:** Mostly yes — AD-1, AD-2, AD-4, AD-6, AD-7, AD-8 each fix a genuine point where two independent builders could otherwise diverge, and each is stated as a testable/checkable rule. Two gaps remain:

**Findings:**

- **[HIGH] `GameService` is named as the sole enforcement mechanism of AD-4 but has no seed location.** AD-4's rule (line 53) states: "Les deux transports délèguent au même service applicatif (`GameService`), qui seul appelle le domaine." This is the actual mechanism that prevents CAP-2/CAP-6 duplication — but the Structural Seed's `BattleShip.API/` tree (lines 144–158) lists `Endpoints/`, `Grpc/`, `Validation/`, `State/`, `Mapping/` and never places `GameService`. Two builders following the seed literally would each invent their own home for it (e.g. `Services/GameService.cs` vs. `Application/GameService.cs` vs. no class at all, just shared static/extension methods), and would plausibly diverge on what it owns: does `GameService` also own `IGameStore` access (AD-3 line 47 implies both the HTTP endpoint *and* the gRPC service call `IGameStore` directly), or is `IGameStore` reached only through `GameService`? AD-3 and AD-4 read as mildly inconsistent on this call chain and neither is anchored to a file. This is exactly the kind of divergence the spine is supposed to close.

- **[MEDIUM] Naming collision between the gRPC wire type and the `Models.Contracts` POCO of the same name is unresolved.** The Structural Seed places `FireShotRequest.cs` under `BattleShip.Models/Contracts/` (line 139), yet AD-4 (line 53) mandates that firing a shot is exposed **exclusively** via gRPC, and the Deferred section (line 193) says `.proto` message fields (implicitly including a protobuf `FireShotRequest`/`FireShotReply`) are left to implementation. That means there will almost certainly be a protobuf-generated `FireShotRequest` type (used to actually cross the wire) *and* a same-named POCO in `BattleShip.Models.Contracts` — with no rule on their relationship (is the POCO a validation-layer intermediate that `BattlefieldGrpcService` maps to/from? is it dead code left over from an HTTP-first draft?). Two builders will likely resolve this differently — one keeping the POCO as an internal validation model, another deleting it, another aliasing namespaces ad hoc in `BattlefieldGrpcService.cs`. `ShotResultDto` is lower risk since it has a legitimate second use (representing history entries in `GameStateDto`, per CAP-10), but `FireShotRequest` has no HTTP use given AD-4's exclusivity, so its presence in `Models/Contracts` is questionable as seeded.

- **[LOW] Concurrent mutation of a single `Game` aggregate is not addressed.** AD-3 fixes the *store* (one `ConcurrentDictionary<Guid, Game>`, Singleton) but `ConcurrentDictionary` only makes dictionary-level get/set atomic — it says nothing about two concurrent `FireShot` calls racing on the same retrieved `Game` object (e.g. a double-click or retry from the Blazor client). CAP-1's success criterion explicitly requires that "a shot on an already-played cell is not counted" — under a race this could double-apply. Given the game is turn-based/solo vs. AI this is low-probability, but the spine gives no rule (e.g. per-game lock, or atomic replace-on-mutate) so two builders could produce different behavior under load/tests that happen to hit this race.

- **[LOW] `PlayerId : Guid` is introduced in Consistency Conventions (line 99) but never used by any AD, endpoint, or structural component.** Neither `SPEC.md` nor `game-design.md` mentions a `PlayerId` concept (the game is solo human-vs-computer, no multiplayer, no auth/session per the spec's non-goals). It's unclear whether this is meant to travel with requests (header? body field?) or is vestigial. An unused-but-named identifier is itself a divergence risk: one builder may thread it through `CreateGameRequest`/DTOs "because the convention says so," another may ignore it entirely.

---

## 2. Every AD's Rule is enforceable and prevents its stated divergence

| AD | Enforceable? | Notes |
| --- | --- | --- |
| AD-1 (domain placement) | Yes | Already true today (`[ADOPTED]`); mechanically checkable (`BattleShip.Models.csproj` has no `PackageReference`). |
| AD-2 (visibility boundary) | Yes | Names its own test ("aucune coordonnée de navire non coulé n'apparaît dans la vue adverse sérialisée") — directly testable. |
| AD-3 (state ownership) | Partially | "Singleton `IGameStore`" and "`ConcurrentDictionary`" are checkable via DI-container/integration tests. But "aucun autre composant ne détient sa propre copie de l'état" is a code-review-only guarantee, not something a test can exhaustively enforce. Weakest AD in the set on enforceability, though acceptable at this altitude if paired with the AD-4 clarification above. |
| AD-4 (transport split) | Yes, rule itself | The rule ("no HTTP endpoint accepts a shot") is testable via route enumeration. Its supporting mechanism (`GameService`) is unseeded — see Finding 1 above; that's a structural-seed gap, not a flaw in the rule's wording. |
| AD-5 (validation location) | Yes | Named validator classes, checkable via DI registration + a test asserting validation runs before any domain call. |
| AD-6 (opponent strategy) | Yes | Interface + two named implementations mapped directly to CAP-12's stated test. |
| AD-7 (test organization) | Yes | Folder convention, trivially checkable. |
| AD-8 (project reference boundary) | Yes | Already true today (`[ADOPTED]`), mechanically checkable via `dotnet list reference`. |

**Finding:**
- **[LOW]** AD-3's "no other component owns its own copy of state" clause is not independently testable; recommend either dropping it (redundant with "Singleton IGameStore is the only registration") or rephrasing as a checkable rule (e.g., "no other `IGameStore`-shaped field/service is registered in DI").

---

## 3. Deferred — could anything there let two units diverge in a way that matters?

**Assessment:** The five explicit Deferred entries (`.proto` field detail, Blazor component breakdown, ADR/PROMPTS/REVUE-IA format, deployment/hosting environment, epics/stories breakdown) are all legitimately below spine altitude and are appropriately anchored to the ADs/conventions that do matter (AD-4 for the proto scope, AD-2/AD-8 for Blazor detail). None of them, as scoped, would let two builders build incompatibly.

**Finding:**
- **[MEDIUM]** The real gap is not in what's listed under Deferred — it's what's *missing* from that list. `GameService`'s existence/location/call-chain (Finding 1) and the `FireShotRequest` naming collision (Finding 2) are genuine open points that are currently **silent**, not **deferred**. Silence is worse than an explicit deferral here, because a reader has no signal that a decision is still owed. Recommend either resolving them with a 9th AD or explicitly adding them to Deferred with a rationale.

---

## 4. Named tech versions — plausibility

**Assessment:** Most versions are plausible for a document dated 2026-09-15 and are appropriately labeled (`[ADOPTED]` for what's already in the brownfield `.csproj`s vs. "vérifié NuGet 2026-09" for newly introduced packages) — good practice, not a finding by itself.

**Findings:**

- **[MEDIUM] Internal version inconsistency inside the gRPC package family.** `Grpc.AspNetCore`, `Grpc.Net.Client`, and `Grpc.Tools` are all pinned to `2.83.0` (lines 113, 115, 117), while `Grpc.AspNetCore.Web` and `Grpc.Net.Client.Web` are pinned to `2.80.0` (lines 114, 116). These five packages ship from the same `grpc-dotnet` repository on the same release train and normally carry matching version numbers. A 3-minor-version gap between the "core" packages and the "-Web" companions is the kind of detail that looks like a stale/hallucinated lookup rather than a real, deliberate pin, and it's worth re-verifying against NuGet before implementation — a mismatched `Grpc.AspNetCore.Web` version has caused real compatibility issues in gRPC-Web setups before.

- **[LOW] Stack table omits `xunit.runner.visualstudio`.** The actual `BattleShip.Tests.csproj` already references `xunit.runner.visualstudio` version `3.1.4` alongside `xunit` `2.9.3`, but the Stack table (lines 119–121) only lists `xUnit`, `Microsoft.NET.Test.Sdk`, and `coverlet.collector` as `[ADOPTED]`. Minor incompleteness in the brownfield ratification — worth adding for completeness, not a functional risk (the version pairing itself, xunit v2 core + runner.visualstudio v3, is a known-supported combination).

---

## 5. Does it ratify or contradict the brownfield codebase?

**Assessment: Ratifies correctly, no contradictions found.** Verified directly against the four `.csproj` files:

- `BattleShip.API.csproj`: `Sdk="Microsoft.NET.Sdk.Web"`, references `Microsoft.AspNetCore.OpenApi` 10.0.12, `ProjectReference` to `Models` only — matches AD-1/AD-8 and the Stack table exactly.
- `BattleShip.App.csproj`: `Sdk="Microsoft.NET.Sdk.BlazorWebAssembly"`, references `Components.WebAssembly`/`.DevServer` 10.0.12, `ProjectReference` to `Models` only, **no** reference to `API` — matches AD-8's `[ADOPTED]` claim exactly.
- `BattleShip.Models.csproj`: plain `Sdk="Microsoft.NET.Sdk"`, zero `PackageReference`, zero `ProjectReference` — matches AD-1's `[ADOPTED]` claim exactly.
- `BattleShip.Tests.csproj`: `xunit`, `Microsoft.NET.Test.Sdk`, `coverlet.collector` (+ `xunit.runner.visualstudio`, see §4 finding), `ProjectReference` to `API` only — matches AD-7.

The repo is currently template-scaffolded (`Class1.cs`, `UnitTest1.cs`, `Weather.razor`, `Counter.razor`, default `Program.cs` files, no `docs/adr/` yet) with no real domain/API code written, so there is nothing for the spine to contradict beyond the `.csproj` shape — and that shape is correctly ratified.

No findings.

---

## 6. Coverage of all 12 capabilities (CAP-1..CAP-12)

**Assessment:** All 12 appear in the Capability → Architecture Map (lines 176–189), each with a location and a governing AD, except CAP-9 which is explicitly and reasonably marked as "hors architecture logicielle, processus documentaire" (it's about `PROMPTS.md`/`docs/adr/`/`REVUE-IA.md` existing and being linked to evidence — a repo/process concern, not a code-architecture concern). That's a legitimate call, not a gap.

No findings — coverage is complete.

---

## 7. Structural dimensions owned by this altitude — decided, deferred, or silently missing?

**Assessment:** The spine covers domain placement, visibility, state ownership/concurrency (partially), transport split, validation placement, opponent strategy, test organization, and project-reference boundaries — the dimensions a "feature-altitude" spine for a 4-project student solution should own. The operational/environmental envelope is explicitly and correctly handled:

> "Environnement de déploiement/hébergement : hors périmètre, le projet est démontré en local (non-goal du spec) — rien à fixer ici." (line 196)

This is the right call — `SPEC.md`'s Non-goals explicitly states "Déploiement en environnement hébergé hors périmètre : le projet est démontré en local," so an explicit, sourced deferral (rather than silence) is exactly correct handling, and it's placed under "Deferred" where a reader will find it.

**Finding:**
- **[LOW]** CORS is addressed as a one-line convention ("Une seule politique nommée dans `BattleShip.API`, limitée à l'origine de `BattleShip.App`," line 102) but the local dev multi-process topology (two `dotnet run` processes, gRPC-Web needing HTTP/2-vs-HTTP/1.1 or protocol negotiation on Kestrel, ports) is left entirely to the existing `README.md` / implementation, with no cross-reference from the spine. This is acceptable (it's genuinely below spine altitude and the README already documents the run commands) but a one-line pointer from the spine's CORS convention to "see README for local topology" would close the loop more cleanly. Not a real divergence risk, just a documentation-linkage nit.

---

## Summary of Findings by Severity

| Severity | Finding | Location |
| --- | --- | --- |
| High | `GameService` named in AD-4 as sole domain-call point, absent from Structural Seed; ambiguous relationship to `IGameStore` access (AD-3 vs AD-4) | ARCHITECTURE-SPINE.md:53, 144-158, cf. 47 |
| Medium | `FireShotRequest` POCO in `Models/Contracts` collides in name/purpose with the gRPC wire type implied by AD-4's exclusivity + Deferred proto note | ARCHITECTURE-SPINE.md:139, 53, 193 |
| Medium | These two gaps are silent rather than listed under Deferred | ARCHITECTURE-SPINE.md: Deferred section, 191-197 |
| Medium | Grpc.AspNetCore.Web / Grpc.Net.Client.Web pinned to 2.80.0 vs. 2.83.0 for the rest of the gRPC family — internally inconsistent, re-verify before implementation | ARCHITECTURE-SPINE.md:113-117 |
| Low | AD-3's "no other component owns its own state copy" clause not independently testable | ARCHITECTURE-SPINE.md:43-47 |
| Low | Concurrent mutation of a single `Game` aggregate under simultaneous `FireShot` calls not addressed | ARCHITECTURE-SPINE.md:43-47 |
| Low | `PlayerId : Guid` introduced in conventions but unused by any AD/endpoint/DTO | ARCHITECTURE-SPINE.md:99 |
| Low | Stack table omits already-adopted `xunit.runner.visualstudio` 3.1.4 | ARCHITECTURE-SPINE.md:119-121, cf. BattleShip.Tests.csproj:14 |
| Low | CORS convention doesn't cross-reference README's local dev topology | ARCHITECTURE-SPINE.md:102 |

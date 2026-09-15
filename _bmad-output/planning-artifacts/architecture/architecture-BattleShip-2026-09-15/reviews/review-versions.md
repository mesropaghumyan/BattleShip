# Review — Version/Technology Verification

**Target:** `ARCHITECTURE-SPINE.md` (Bataille Navale, 2026-09-15) + `.memlog.md`
**Method:** WebSearch spot-checks of NuGet current versions and compatibility claims, performed 2026-09-15.
**Scope:** Only technology/version/API-existence claims. Domain naming (`IGameStore`, `GameViewMapper`, etc.) is out of scope by design — these are invented design names, not factual claims.

---

## 1. What was checked and confirmed

| Claim in spine | Search result | Verdict |
| --- | --- | --- |
| FluentValidation 12.1.1 "vérifié NuGet 2026-09" | NuGet Gallery lists `FluentValidation 12.1.1` as current | **Confirmed** — matches live NuGet listing |
| Grpc.AspNetCore 2.83.0 "vérifié NuGet 2026-09" | NuGet Gallery lists `Grpc.AspNetCore 2.83.0` (and `Grpc.AspNetCore.Server 2.83.0`), updated 2026-08-03 | **Confirmed** |
| Grpc.AspNetCore.Web 2.80.0 "vérifié NuGet 2026-09" | NuGet Gallery lists `Grpc.AspNetCore.Web 2.80.0` | **Confirmed** (note: this package trails the core `Grpc.AspNetCore` 2.83.x line by three minor versions — normal for this repo, not a red flag, but worth knowing it isn't lockstep-versioned) |
| Google.Protobuf 3.36.1 "vérifié NuGet 2026-09" | NuGet Gallery lists `Google.Protobuf 3.36.1`, updated 2026-08-31 | **Confirmed** |
| FluentValidation 12.x compatible with .NET 10 | FluentValidation 12 targets .NET 8+ (includes .NET 10); official docs/blog corroborate | **Confirmed** |
| Grpc.AspNetCore 2.83.x line compatible with .NET 10 | grpc-dotnet is the actively maintained .NET gRPC implementation; current releases target modern TFMs including .NET 10; no compatibility warnings found | **Confirmed** (no explicit "supports .NET 10" changelog line was surfaced, but nothing contradicts it and the package is under active 2026 release cadence) |
| gRPC-Web (Grpc.AspNetCore.Web server + Grpc.Net.Client.Web client) is still the correct way to call gRPC from Blazor WASM in .NET 10 | Microsoft Learn "gRPC-Web in ASP.NET Core gRPC apps" doc is current and describes exactly this pairing; no deprecation notice or replacement (e.g. Connect protocol, JSON transcoding-for-browsers) found | **Confirmed, not deprecated** — this remains the standard approach. Known inherent limitation (no client/bidirectional streaming over gRPC-Web) is irrelevant here since AD-4 only uses a unary `FireShot` call. |

**Not independently re-verified in this pass:** `Grpc.Net.Client` 2.83.0, `Grpc.Net.Client.Web` 2.80.0, `Grpc.Tools` 2.83.0. These weren't explicitly named in the task's spot-check list; they're plausible given the confirmed `Grpc.AspNetCore`/`Grpc.AspNetCore.Web` version families (grpc-dotnet packages are typically released together in matching pairs: 2.83.x core-adjacent, 2.80.x web-adjacent), but the memlog's "vérifié NuGet 2026-09" label for these three specific packages was not corroborated first-hand here. Low risk given the pattern held for the four packages actually checked.

## 2. Findings

### Finding 1 — xUnit 2.9.3 is stale relative to the actively-recommended xUnit v3 (Severity: Medium)

The Stack table pins `xUnit 2.9.3 [ADOPTED]`, sourced from "already true in the existing .csproj," not from a fresh version check — which is legitimate evidence (an actual file), not a training-data guess, so it doesn't violate the "verify, don't assert" requirement in the strict sense. However:

- Current search results (2026) indicate xUnit 2.9.3 is now in maintenance-only mode (security fixes only); all new feature work is on xUnit v3 (`xunit.v3`, currently 4.0.1).
- Multiple 2026 sources explicitly recommend xUnit v3 for new .NET 8/9/10 projects.
- The spine never surfaces this tension or explains why the project stays on the legacy v2 line rather than the scaffold possibly being outdated. For a project explicitly targeting .NET 10 SDK, this is worth a one-line acknowledgment ("kept because pre-existing scaffold, not because it's current best practice") rather than silence.

This isn't a factual error in the spine (the version number is accurate for what's pinned), but it's an unexamined technology choice presented via `[ADOPTED]` without the same scrutiny given to the gRPC/FluentValidation stack. Recommend adding a one-line note or accepting the risk explicitly.

### Finding 2 — Grpc.Net.Client, Grpc.Net.Client.Web, Grpc.Tools versions carried by pattern-matching, not individually confirmed (Severity: Low)

The memlog batches these three under the same "(verifié NuGet, 2026-09)" tag as the two packages that were checked in this review (`Grpc.AspNetCore`, `Grpc.AspNetCore.Web`) and Google.Protobuf. It's plausible the original spine author did check all seven gRPC/validation packages individually (the memlog's phrasing suggests a batch verification pass happened), but nothing in the memlog distinguishes "checked directly" from "inferred to match the sibling package's version line." Since all four independently-checked packages did turn out accurate, this is low risk — but the memlog would be stronger if it recorded evidence (e.g. a NuGet URL or search snippet) per package rather than a bare version-number assertion.

### Finding 3 — Grpc.AspNetCore.Web / Grpc.Net.Client.Web pinned three minor versions behind Grpc.AspNetCore / Grpc.Net.Client (Severity: Low, informational)

2.80.0 vs 2.83.0 is not itself suspicious (grpc-dotnet's web-transport packages release on a slightly different cadence than the core packages), but the spine doesn't note *why* these are on different version lines, which could look like a typo/inconsistency to an implementer following the table literally. Confirmed both numbers are real, currently-published versions, so no correction needed — just worth an explanatory footnote if this spine is handed to someone who didn't do the research.

### Finding 4 — No other unverified technology/API-existence claims found

Swept the rest of the spine (Consistency Conventions, Structural Seed, Capability Map) for factual technology assertions beyond the Stack table:
- `TypedResults` + `ValidationProblem` (RFC 7807) for Minimal API error shape — real, long-stable ASP.NET Core API, not new/risky enough to require a fresh web check.
- `RpcException` with `StatusCode.InvalidArgument` / `NotFound` / `FailedPrecondition` — real, stable gRPC C# API surface.
- `WebApplicationFactory<Program>` for HTTP+gRPC in-process integration testing — real, standard ASP.NET Core testing pattern; also consistent with the gRPC-Web confirmation above (unary calls only, no streaming needed).
- `ConcurrentDictionary<Guid, Game>` as Singleton store — plain BCL type, no risk.
- ".NET SDK 10.0 (LTS)" — correct; .NET 10 is an LTS release (even-numbered releases are LTS under Microsoft's support policy).

None of these needed correction. All class/interface names (`IGameStore`, `GameViewMapper`, `IOpponentStrategy`, `GameEngine`, etc.) are the architect's own design vocabulary, not claims about existing technology, and are excluded from this review by design.

## 3. Overall Verdict

The seven "vérifié NuGet 2026-09" version numbers in the Stack table are **accurate** for the four packages independently spot-checked (FluentValidation, Grpc.AspNetCore, Grpc.AspNetCore.Web, Google.Protobuf) against live NuGet data as of 2026-09-15, and the two compatibility claims (.NET 10 support, gRPC-Web still being the correct Blazor WASM approach) are also confirmed correct with no deprecation in sight. The main gap is not a wrong fact but an unexamined one: xUnit 2.9.3 vs. the now-recommended xUnit v3 line is inherited from scaffold without comment, and three gRPC-family package versions were asserted alongside the checked ones without individually distinguishable evidence in the memlog.

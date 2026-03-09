# Unattended Upgrades: Startup Availability Research

## Executive Summary

When `UpgradeUnattended: true` is configured and a database migration is needed, the entire migration pipeline runs **before the HTTP server starts accepting requests**. This means the application is completely unresponsive during migration — no health checks, no maintenance page, nothing. IIS kills the process after its default 120-second `startupTimeLimit`; container orchestrators kill pods whose liveness probes fail. The result is a silent crash with no error surfaced to the operator, and a requirement to restart the application in order to retry.

The attended upgrade path (`UpgradeUnattended: false`) does not have this problem: the HTTP server starts immediately, serving a maintenance page (HTTP 503) to visitors while waiting for a backoffice operator to manually trigger the upgrade.

The proposal in [discussion #21987](https://github.com/umbraco/Umbraco-CMS/discussions/21987) describes the desired end state: an unattended upgrade that starts fast, responds to health checks and serves a maintenance page (HTTP 200) for the duration of the migration, then transitions to normal operation when done.

---

## 1. How the Startup Pipeline Works Today

### 1.1 The startup sequence (`Program.cs`)

```
await app.BootUmbracoAsync()       ← MIGRATIONS RUN HERE (synchronously)
app.UseUmbraco().WithMiddleware()  ← middleware pipeline configured
await app.RunAsync()               ← HTTP server starts listening
```

`BootUmbracoAsync` (`src/Umbraco.Web.Common/Extensions/WebApplicationExtensions.cs`) resolves `IRuntime` and calls `StartAsync` on it, awaiting completion before returning. The HTTP server (`app.RunAsync()`) does not start until after `BootUmbracoAsync` returns. This means **the process is not listening on any port during migrations**.

### 1.2 Inside `CoreRuntime.StartAsync` (`src/Umbraco.Infrastructure/Runtime/CoreRuntime.cs`)

1. `AcquireMainDom()` — distributed lock
2. Publish `RuntimeUnattendedInstallNotification` → `UnattendedInstaller` handles fresh installs
3. `DetermineRuntimeLevel()` — checks database state, sets `IRuntimeState.Level`
4. If `UpgradeUnattended = true` and DB needs migration: **Level is set to `RuntimeLevel.Run`** with `Reason = UpgradeMigrations`
5. Publish `RuntimePremigrationsUpgradeNotification` → `PremigrationUpgrader` handles pre-migration steps
6. Publish `RuntimeUnattendedUpgradeNotification` → **`UnattendedUpgrader` runs all migrations synchronously**
7. `DetermineRuntimeLevel()` again — re-checks state now that migrations are done
8. `await _components.InitializeAsync(...)` — cache warm-up, index seeding, etc.
9. Fire `UmbracoApplicationStartingNotification`

All of steps 1–9 complete before `BootUmbracoAsync` returns.

### 1.3 `RuntimeState.DetermineRuntimeLevel` (`src/Umbraco.Infrastructure/Runtime/RuntimeState.cs:214`)

```csharp
Level = _unattendedSettings.Value.UpgradeUnattended ? RuntimeLevel.Run : RuntimeLevel.Upgrade;
```

This is the key fork: when `UpgradeUnattended = true`, the level is **`Run`** (not `Upgrade`). The entire attended-upgrade maintenance-page machinery — which gates on `RuntimeLevel.Upgrade` — is therefore never engaged.

---

## 2. Why This Kills the Process

### 2.1 IIS / ANCM (`startupTimeLimit`)

IIS uses the ASP.NET Core Module (ANCM) to manage the process. The `startupTimeLimit` attribute (default: **120 seconds**) measures the time from process launch until the process starts listening on the configured port. If no response is received within the limit, ANCM terminates and relaunches the process.

Because `app.RunAsync()` (which binds the port) is never reached during a long migration, ANCM fires its kill signal. The restart attempt immediately re-enters the same migration at the same failure point, repeating indefinitely.

There is no user-visible error. Application logs may contain migration progress up to the kill point.

**Workaround**: add `startupTimeLimit="3600"` to `<aspNetCore>` in `web.config`. Most operators are unaware this setting exists or that it is the cause of the failure.

### 2.2 Containers / Kubernetes liveness probes

Container orchestrators use HTTP liveness probes (e.g., `GET /health/live`) to determine whether a container is alive. If no HTTP server is running during migration, the probe fails immediately (connection refused), and the container is killed and restarted. Same cycle as IIS: killed, restarted, migrations begin again, killed again.

This is a distinct concern from IIS: IIS measures *time to first listen*, containers measure *responsiveness at a specific endpoint*. Both share the root cause: no HTTP server during migration.

### 2.3 Azure App Service

Azure App Service enforces similar constraints via its own health monitoring. Issue [#13051](https://github.com/umbraco/Umbraco-CMS/issues/13051) documents sites with ~200,000 nodes hitting Azure App Service timeouts on startup due to long-running NuCache/Examine initialization (a related but distinct category of slow startup).

---

## 3. The Attended Upgrade Path (What Works)

When `UpgradeUnattended = false`, the flow is entirely different and serves as a design reference:

1. `DetermineRuntimeLevel()` sets `Level = RuntimeLevel.Upgrade`
2. `CoreRuntime.StartAsync` finishes quickly (no migrations run)
3. `BootUmbracoAsync` returns quickly
4. HTTP server starts, accepting requests
5. **`EagerMatcherPolicy`** (`src/Umbraco.Web.Website/Routing/EagerMatcherPolicy.cs:232–286`) detects `RuntimeLevel.Upgrade`, reroutes all dynamic HTTP requests to `RenderController.Index`
6. **`MaintenanceModeActionFilterAttribute`** (`src/Umbraco.Web.Common/Controllers/MaintenanceModeActionFilterAttribute.cs`) detects `Level == RuntimeLevel.Upgrade` and sets the result to **`MaintenanceResult`**
7. **`MaintenanceResult`** (`src/Umbraco.Web.Common/ActionsResults/MaintenanceResult.cs`) serves `~/umbraco/UmbracoWebsite/Maintenance.cshtml` with **HTTP 503**
8. A backoffice operator navigates to `/umbraco`, the install/upgrade UI is served (static routes bypass the maintenance page rerouting), and they trigger the upgrade manually

The `ShowMaintenancePageWhenInUpgradeState` flag in `GlobalSettings` (default `true`) controls this behaviour. If `false`, the routing policy skips the maintenance page reroute and lets static routes through as normal.

The attended path works because the maintenance page machinery is in the **HTTP pipeline** (request-time), not in the startup pipeline.

---

## 4. Package Migrations (`PackageMigrationsUnattended`)

Package migrations (`PackageMigrationsUnattended = true`, the default) share the same problem. `UnattendedUpgrader.HandleAsync` runs `RunPackageMigrationsAsync` synchronously during `StartAsync`, before the HTTP server is up. For packages with lightweight migrations this is usually fine, but a package performing a large data migration (e.g. iterating over all content nodes) has the same risk.

---

## 5. Gap Analysis: What Is Missing

| Scenario | HTTP server up? | Health checks respond? | Maintenance page? | Operator visibility |
|---|---|---|---|---|
| Attended upgrade (`UpgradeUnattended=false`) | Yes | Yes (static routes) | Yes (503) | Good |
| Unattended upgrade, migration fast (< 30s) | No | No | No | Not needed |
| Unattended upgrade, migration slow (> 120s) | No | No | No | None — silent kill |
| Container with liveness probe | No | No | No | Pod restart loop |

The missing capability is: **"the HTTP server is responding, and requests during migration are handled gracefully"**, specifically for the unattended path.

---

## 6. Discussion #21987 — Proposal Analysis

The proposal at https://github.com/umbraco/Umbraco-CMS/discussions/21987 (filed 2026-03-03/04) describes a two-phase startup:

**Phase 1 — Lightweight Shell**
- Boot quickly enough to bind a port and respond to IIS/container health checks
- Serve a maintenance page ("Database migration in progress…") for all normal web requests
- Use HTTP 200 (not 503) for the maintenance page body, so liveness probes don't misread the site as broken

**Phase 2 — Background Migration**
- Run migrations via `IHostedService` (background task), returning `StartAsync` immediately
- Optionally broadcast real-time progress via SignalR
- On completion, transition runtime to full `RuntimeLevel.Run`

The author explicitly notes this applies only when `UpgradeUnattended: true` is set, and wants no behavioural change for normal boots.

### 6.1 HTTP 200 vs HTTP 503 for the maintenance page

This is a deliberate choice in the proposal, differing from the current attended-upgrade behaviour (HTTP 503). The reasoning:

- **HTTP 503** tells IIS that the application pool is unhealthy. Depending on the IIS health monitoring configuration, this may trigger a recycle before migrations complete.
- **HTTP 503** on a container liveness path causes the container to be restarted.
- **HTTP 200** with maintenance content tells all health checkers "the process is alive and responding", while the content communicates to users that the site is temporarily unavailable.

The correct mapping in container terminology is:
- **Liveness probe** (`/health/live`): HTTP 200 — the process is alive
- **Readiness probe** (`/health/ready`): HTTP 503 — the process is not ready for traffic

For a plain web maintenance page served to browsers and CDN/load-balancer health checks, HTTP 200 prevents unwanted kill/restart cycles while still communicating the maintenance state.

### 6.2 Architectural scope

The proposal acknowledges that this is a meaningful architectural change: "today, Umbraco's entire runtime assumes a fully migrated database". Routing, content resolution, backoffice controllers — all code assumes the schema is in its final state. This is why migrations must currently complete before any HTTP traffic is served.

The proposal calls for a separation between:
- **"App is running"** — can bind a port, accept HTTP requests, respond to probes, serve a static maintenance page
- **"App is ready"** — database schema is current, full CMS functionality is available

---

## 7. Existing Machinery That Is Relevant

The codebase already contains several pieces that a solution could build on:

| Component | Location | Relevance |
|---|---|---|
| `RuntimeLevel` enum | `src/Umbraco.Core/RuntimeLevel.cs` | Needs a new `Upgrading` value (or reuse of `Upgrade` level) |
| `IRuntimeState.Level` / `.Reason` | `src/Umbraco.Core/Services/IRuntimeState.cs` | Central state signal; all routing checks this |
| `EagerMatcherPolicy` | `src/Umbraco.Web.Website/Routing/EagerMatcherPolicy.cs` | Already reroutes for `Upgrade`; same mechanism needed for unattended path |
| `MaintenanceModeActionFilterAttribute` | `src/Umbraco.Web.Common/Controllers/MaintenanceModeActionFilterAttribute.cs` | Already shows maintenance page for `Upgrade` level; gates on this level |
| `MaintenanceResult` | `src/Umbraco.Web.Common/ActionsResults/MaintenanceResult.cs` | Returns HTTP 503 + Maintenance.cshtml; would need a 200 variant for liveness |
| `BootFailedMiddleware` | `src/Umbraco.Web.Common/Middleware/BootFailedMiddleware.cs` | Short-circuits all requests on `BootFailed`; similar pattern needed for migration |
| `UnattendedUpgrader` | `src/Umbraco.Infrastructure/Install/UnattendedUpgrader.cs` | Currently a synchronous notification handler; needs to become async/deferred |
| `CoreRuntime.StartAsync` | `src/Umbraco.Infrastructure/Runtime/CoreRuntime.cs` | Needs to be split into fast startup and deferred work |
| `ShowMaintenancePageWhenInUpgradeState` | `src/Umbraco.Core/Configuration/Models/GlobalSettings.cs` | Existing configuration hook; similar config could control unattended behaviour |
| `IRuntime` / `IHostedService` | `src/Umbraco.Core/Services/IRuntime.cs` | `IRuntime` extends `IHostedService`; `StartAsync` is called manually via `BootUmbracoAsync`, not via the generic host |

---

## 8. Technical Path to the Ideal End State

### 8.1 What needs to change in the startup sequence

Currently `BootUmbracoAsync` → `IRuntime.StartAsync` must complete fully before `app.RunAsync()` is called. The fundamental change required is:

> Move the migration work out of `BootUmbracoAsync` and into a background task that runs **after** the HTTP server is bound.

In ASP.NET Core, if a registered `IHostedService.StartAsync` returns a completed `Task` immediately (and kicks off a background `Task`), the host proceeds to start the web server. User traffic is accepted before the background work finishes. This is the standard "background work" pattern used by `BackgroundService`.

The challenge is that `IRuntime` is **not** registered via `AddHostedService`; it is called manually in `BootUmbracoAsync`. This was a deliberate choice to ensure the middleware pipeline is configured after the runtime is in a known state. Any solution needs to respect this constraint or carefully restructure it.

### 8.2 Proposed implementation approach

**Step 1: New runtime level signal**

Add `RuntimeLevel.Upgrading` (or a property on `IRuntimeState` such as `bool IsUpgradeInProgress`) to represent the in-progress unattended migration state. This is distinct from:
- `RuntimeLevel.Upgrade` — attended upgrade, HTTP server running, waiting for backoffice trigger
- `RuntimeLevel.Run` — fully operational

**Step 2: Split `CoreRuntime.StartAsync` into fast and slow phases**

Fast phase (runs inside current `BootUmbracoAsync`, completes quickly):
- `AcquireMainDom()`
- `DetermineRuntimeLevel()`
- If `UpgradeUnattended = true` and DB needs migration: set `Level = RuntimeLevel.Upgrading` and **return** — do NOT run migrations
- If attended upgrade: set `Level = RuntimeLevel.Upgrade` and return (existing behaviour unchanged)
- If no migration needed: proceed as today

Slow phase (runs as background task after HTTP server starts):
- Run `PremigrationUpgrader`
- Run `UnattendedUpgrader` (migrations)
- Re-run `DetermineRuntimeLevel()` to confirm success
- Initialize components (`_components.InitializeAsync`)
- Transition state to `RuntimeLevel.Run`

**Step 3: HTTP behaviour during `RuntimeLevel.Upgrading`**

Extend `EagerMatcherPolicy` to recognise `RuntimeLevel.Upgrading` in addition to `RuntimeLevel.Upgrade`. During the `Upgrading` state:
- Route dynamic web page requests to a maintenance/upgrading endpoint
- Allow static routes (backoffice static files, health check endpoints) to pass through unchanged

Introduce a new action result (e.g. `UpgradingResult`) that returns **HTTP 200** with an "upgrade in progress" page. The 200 status is critical for liveness probes. If operators want HTTP 503 for SEO/CDN reasons, this should be configurable.

For API endpoints (Management API, Delivery API), the appropriate response during migration is **HTTP 503 Service Unavailable** with a `Retry-After` header, since API clients can handle transient errors gracefully. This requires the `RequireRuntimeLevelAttribute` (which already exists in `src/Umbraco.Cms.Api.Management/Filters/`) to also reject requests during `Upgrading`.

**Step 4: Health check integration**

Umbraco should register ASP.NET Core health check endpoints explicitly:
- `GET /umbraco/api/health/live` → always returns `Healthy` while the process is running (including during migration)
- `GET /umbraco/api/health/ready` → returns `Unhealthy` / `Degraded` during migration, `Healthy` once `RuntimeLevel.Run`

These must be registered early (before any Umbraco routing), so they are always reachable regardless of runtime state.

**Step 5: Transition back to normal operation**

When the background migration task completes:
1. Set `IRuntimeState.Level = RuntimeLevel.Run`
2. `EagerMatcherPolicy` picks up the new level (it already monitors live settings changes via `IOptionsMonitor` and could monitor `IRuntimeState`)
3. Subsequent requests are routed normally

If migration fails:
1. Set `IRuntimeState.Level = RuntimeLevel.BootFailed`
2. `BootFailedMiddleware` takes over (existing behaviour)

### 8.3 Breaking change considerations

Any changes to `IRuntimeState` interface must follow the default interface implementation pattern (section 5.3 of CLAUDE.md). Adding `RuntimeLevel.Upgrading` is a non-breaking enum value addition (existing comparisons using `>` are fine; switch statements need a `default` case, which all existing ones have). Adding an `IsUpgradeInProgress` property to the interface needs a default implementation.

Changing when `BootUmbracoAsync` returns (before vs after migrations) is a **behavioural breaking change** for anyone who relies on the runtime being fully initialised after `BootUmbracoAsync`. This should be documented and version-gated.

---

## 9. Issues and Related Work

| Issue / PR | Summary |
|---|---|
| [#21987](https://github.com/umbraco/Umbraco-CMS/discussions/21987) | The proposal this document analyses. Describes two-phase startup with background migration. |
| [#13051](https://github.com/umbraco/Umbraco-CMS/issues/13051) | Sites with ~200k nodes hitting Azure App Service timeouts on startup. Partly related: long NuCache/Examine init. Separate concern from migrations but same root symptom (slow startup → timeout). |
| [#6243](https://github.com/umbraco/Umbraco-CMS/issues/6243) | 2019 feature request: custom visitor page during `RuntimeLevel.Upgrade`. Closed as stale in 2022. The attended-upgrade maintenance page (added since) partially addresses this, but was HTTP 503 and not customisable. |

---

## 10. Summary of Findings

1. **The problem is real and reproducible.** Any `UpgradeUnattended = true` install performing a migration that takes > 120 seconds will be killed by IIS ANCM. Any containerised deployment with liveness probes will restart-loop. The real-world example in #21987 (70–80k content nodes, RTE migration across 10 languages) is representative of production-scale Umbraco sites.

2. **The attended upgrade path already solves the HTTP-availability part.** It serves a maintenance page because the runtime enters `RuntimeLevel.Upgrade` and the HTTP server is already running. The unattended path bypasses this entirely by entering `RuntimeLevel.Run` immediately and running migrations synchronously before the server starts.

3. **The workaround (`startupTimeLimit="3600"`) is inadequate as a long-term solution.** It requires `web.config` access, doesn't help containers, and doesn't give operators any visibility into what the app is doing during the long startup period.

4. **Achieving the proposal's end state requires a meaningful architectural change:** moving migration execution from the pre-HTTP startup phase into a background task, and handling HTTP traffic during the migration window. The existing machinery (`EagerMatcherPolicy`, `MaintenanceModeActionFilterAttribute`, `RuntimeLevel`, `IRuntimeState`) provides a solid foundation.

5. **The HTTP 200 vs HTTP 503 choice for the maintenance page is deliberate and correct** for the unattended-upgrade container hosting scenario. HTTP 200 + maintenance content body satisfies liveness probes (process is alive) while communicating unavailability to users. HTTP 503 on a liveness probe path would cause container restarts — the opposite of the desired behaviour. The two concerns should be separated: web page responses can be HTTP 200, API responses should be HTTP 503, dedicated liveness probe endpoints should always be HTTP 200.

6. **There is no fundamental obstacle** to implementing this. The required changes are isolated to `CoreRuntime`, `RuntimeState`, `EagerMatcherPolicy`, the action results layer, and a new or updated `MaintenanceResult`. No database schema changes are required. The change is opt-in via `UpgradeUnattended = true`.

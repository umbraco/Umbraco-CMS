---
name: efcore-notracking-requires-astracking
description: UmbracoDbContext sets QueryTrackingBehavior.NoTracking globally — any EF Core repository method that reads-then-mutates-then-saves an entity must add .AsTracking() explicitly or SaveChangesAsync silently does nothing (or throws a duplicate-tracking exception)
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-06T08:21:01.786Z
---

`UmbracoDbContext` configures `optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` globally (`src/Umbraco.Infrastructure/Persistence/EFCore/UmbracoDbContext.cs:198`). Every LINQ query against it returns fresh, untracked entity instances by default — mutating one and calling `SaveChangesAsync()` does nothing, since EF Core doesn't know the instance needs saving.

**Why this matters more than it looks:** the `IEFCoreScope<T>.ExecuteWithContextAsync` DbContext instance is cached per ambient scope (`EFCoreScope<T>` has a single `_dbContext` field, reused across every repository call within one `NewScopeProvider.CreateScope()`/`ICoreScope`). So if one repository call does `db.Foo.Add(...)` + `SaveChangesAsync()` (which DOES track the added entity, regardless of the global query default), and a *later* call in the same scope queries that same row again without `.AsTracking()`, EF Core returns a *new*, untracked clone — and if that clone is then passed to `.Remove()` or otherwise attached, you get `InvalidOperationException: cannot be tracked because another instance with the same key value ... is already being tracked`. This was hit for real while implementing `AsyncPublishableContentRepositoryBase.PersistContentScheduleAsync` — a schedule-collection round-trip test (`Persist` → `Get` → `Persist` again) crashed exactly this way the first time.

**How to apply:** any time an EF Core repository method needs to load an entity, mutate a property, and save the change (rather than doing a pure `ExecuteUpdateAsync`/`ExecuteDeleteAsync` bulk statement), add `.AsTracking()` to that specific query. The established, already-commented precedent for this is `PersistUpdatedPropertyDataAsync` in `AsyncDocumentRepository.cs` — copy its comment/reasoning rather than re-discovering this from scratch. Don't assume "I queried it, so EF Core is tracking it" — check whether the query has `.AsTracking()`; if not, it isn't.

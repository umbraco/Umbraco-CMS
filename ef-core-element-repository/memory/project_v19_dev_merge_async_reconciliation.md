---
name: v19-dev-merge-async-reconciliation
description: "How to resolve v18/feature/ef-core-repositories merges from origin/v19/dev — IIdKeyMap/IPublicAccessService are async-only here but sync upstream, so new v19 files compile-break silently"
metadata: 
  node_type: memory
  type: project
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-08-18T11:27:11.174Z
---

Merging `origin/v19/dev` into `v18/feature/ef-core-repositories` on 2026-08-18 (merge commit `9a94ef52151`) surfaced a recurring pattern worth knowing before the next merge from v19/dev.

**The core mismatch**: this branch made `IIdKeyMap` and `IPublicAccessService` async-only (sync `GetIdForKey`/`GetKeyForId`/`GetEntryForContent`/`IsProtected`/`GetAll` removed entirely — see [[feedback_this_branch_allows_breaking_changes]]). `IContentTypeService` similarly lost its sync `Get`/`GetMany` because it now inherits `IAsyncContentTypeBaseService<TItem>` instead of `IContentTypeBaseService<TItem>` (media/member type services are unaffected — they still use the sync base). v19/dev's tree has none of this, so **any new file it adds that calls these sync members merges in cleanly with zero git conflicts and then fails to compile** — grepping only the 21 flagged conflicts is not enough; a full `dotnet build` after completing the merge is required to find these (this merge needed 6 production files + 3 test files fixed this way, none of which were flagged as conflicts).

**How to find them**: after resolving the textual conflicts and committing the merge, `grep -rn "\.GetIdForKey(\|\.GetKeyForId(\|\.GetEntryForContent(\|\.IsProtected(" --include="*.cs" src tests` and `IContentTypeService`-typed variables calling `.Get(`/`.GetMany(`.

**How to fix each site** (established precedent from this merge):
- If the enclosing method is already `async Task`-returning: `await`.
- If the enclosing method must stay sync (a sync `INotificationHandler`/`IDistributedCacheNotificationHandler` implementation, or a private helper with `out` params like `TryGetParentId`/`TryGetPathIds` feeding a fake-async `Task.FromResult(...)` wrapper): `.GetAwaiter().GetResult()` at the call site — this branch already uses this pattern at genuine sync/async boundaries, don't try to thread real async through `out`-parameter bool-returning helpers or notification-handler interfaces that have no async variant.
- Test doubles implementing `IIdKeyMapRepository` (e.g. `CountingIdKeyMapRepository` in the HybridCache integration tests) need their method signatures renamed to the `*Async` shape, not just their call sites.

**A second, unrelated bug this merge's new tests exposed**: `MediaCacheService.GetByKeyAsync(Guid)` did an unconditional upfront `_idKeyMap.GetIdForKeyAsync` lookup before calling `GetNodeAsync`, while `DocumentCacheService.GetByKeyAsync` goes straight to `GetNodeAsync`. This is pre-existing (not caused by any merge edit) but only got caught because v19/dev added `MediaHybridCacheIdKeyMapTests` asserting zero DB round-trips on cache hits — the equivalent Document test already existed and passed. Fixed by deleting the redundant check so Media matches Document's pattern. Worth checking `MediaCacheService.cs` against `DocumentCacheService.cs` for other such asymmetries if similar test failures appear again.

See also [[project_ef_migration_merge_drift]] for the EF Core migration-snapshot side of merge risk (not triggered this time, but worth re-checking on every v19/dev merge).

## 2026-10-08 merge (v19/dev `ad6d8b05def` into ef-core-repositories, 55 conflicts)

Same pattern, new specifics worth knowing next time:
- Most conflicts came from upstream's Code Tidy (#24015) deleting obsolete sync members next to lines we made async: resolve as "ours minus the obsolete members" (AuditService, RelationService, IContentService, IPublishableContentService, ContentService).
- Three modify/delete conflicts were the NPoco `DocumentRepository`/`DocumentBlueprintRepository` and the retired `Umbraco.Cms.Persistence.EFCore/CLAUDE.md`; upstream only removed obsolete ctors there, so `git rm` was right.
- Real upstream fixes that had to be ported into async forms: root-culture validation before branch publish (#24099, `ContentPublishingService`), in-memory forward-only internal id in `LastSyncedManager` (#24034, keep our Attempt signatures), `AsyncMigrationBase` column helpers rewrite (#23984, re-apply `FormatAddColumn` in `TryAddColumn`), the null/success check in `ByKey*VersionController`, and the list-view property loading feature (`GetByIds(ids, propertyAliases, loadTemplates)` → `GetByIdsAsync(keys, propertyAliases, loadTemplates, ct)` on `IPublishableContentService`, `GetManyAsync(keys, propertyAliases, loadTemplates, ct)` on `IAsyncPublishableContentRepository`, async `ContentSearchServiceBase` abstract members).
- `AddUmbracoDbContext<T>` gained a required `shareUmbracoConnection` parameter upstream; our `UmbracoBuilder.Database.cs` call compiled into a confusing "string does not contain UseUmbracoDatabaseProvider" error until `shareUmbracoConnection: true` was passed.
- Git auto-merge dropped `using Umbraco.Cms.Core.Configuration.Models;` from `TemplateService` because upstream removed it as unused while we still need it; `Attempt` in the Search.Core strategies needed `using Umbraco.Cms.Core;` back. Missing-using breaks are a fourth loss class next to the three in [[project_merge_audit_procedure]].
- Cleanly merged new upstream tests needing the async port: 212 errors in 19 integration test files plus 4 unit test files; sync → async conversions were mechanical and were delegated.

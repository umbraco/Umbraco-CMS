---
name: document-repository-retirement-plan
description: "User's decided direction for retiring NPoco's IDocumentRepository/DocumentRepository in favor of IAsyncDocumentRepository/AsyncDocumentRepository — key decisions on scope pattern, CancellationToken, DocumentBlueprintRepository, ElementService, and sequencing. DocumentUrlService and DeferredSearchReindexService are fully migrated; ContentService's member-by-member retirement is in progress (59 sync members retired as of commit d75535a4b99: Blueprints 8/8, Create 3/3, Sort/PersistContentSchedule/Rollback, CheckDataIntegrity, MoveToRecycleBin, Move, plus DeleteOfType(s)). New async members now use the Attempt<TStatus> pattern with bespoke per-member-family OperationStatus enums, not OperationResult/thrown exceptions. PerformMoveLocked (the old sync move engine) and all its private sub-helpers are now FULLY DELETED — both its callers (Move, DeleteOfTypes) migrated to PerformMoveLockedAsync, zero remaining references anywhere (confirmed by build + grep) — that whole 3-increment arc is closed. Remaining on IContentService: Copy(x2)/PublishBranch/Publish. Remaining on IPublishableContentService (shared with Element): Publish/SaveAndPublish/Unpublish/PerformScheduledPublish. Established pattern for IPublishableContentService members: Document gets a real async implementation on AsyncPublishableContentServiceBase, Element bridges to its own still-sync engine (kept as a plain non-interface method) until it gets an async repository. CheckDataIntegrity introduced a NEW pattern — explicit interface reabstraction — for members shared with Media/Member (IContentServiceBase, wider than IPublishableContentService); MoveToRecycleBin/Move/DeleteOfTypes did NOT need it (not shared that widely) — check which interface tier a member sits on before assuming which pattern applies. A real ContentBase.ParentKey hazard was found and fixed during the Move increment — see the dedicated session note and [[feedback_contentbase_parentkey_can_throw]]. GetPermissions was already done, an earlier version of this memory wrongly listed it as remaining. Read before doing any further work on this."
metadata: 
  node_type: memory
  type: project
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-09-17T08:00:36.323Z
---

**Corrected understanding of the current dual-repository setup**: `IDocumentRepository`/`DocumentRepository` (NPoco) is NOT a permanent architectural sibling to `IAsyncDocumentRepository`/`AsyncDocumentRepository` (EF Core) — it exists only as a **temporary measure to give the new EF Core repository something to test against**, exactly the same transitional pattern already used for every other repository migrated in this codebase's NPoco→EF Core journey (Language, KeyValue, Dictionary, RedirectUrl, PublicAccess, Domain, ContentType, etc.). Once `DocumentRepository` is no longer referenced anywhere, **`AsyncDocumentRepository`/`IAsyncDocumentRepository` will be renamed to `DocumentRepository`/`IDocumentRepository`** — this is an in-place-conversion end state, not a permanent parallel-interfaces end state. Don't plan around a permanent two-interface future; plan around a rename-at-the-end future.

**Why:** User corrected an initial (wrong) framing that this branch had deviated from the codebase's dominant "in-place conversion" pattern by keeping `IAsyncDocumentRepository` as a separate, permanent interface. It hasn't deviated — the naming is just temporary scaffolding for the migration period, same as every other repo.

**How to apply:** Don't design anything (comments, architecture docs, naming) around `IDocumentRepository`/`IAsyncDocumentRepository` coexisting forever. Any new "AsyncX" sibling class created during this effort (e.g. `AsyncDocumentBlueprintRepository`) is also temporary scaffolding under the same eventual-rename plan.

## User's decisions on the ContentService/IDocumentRepository retirement (2026-08-06)

1. **Scope strategy**: Accept the established "sync scope held across awaits" pattern already used everywhere else in this codebase (e.g. `ContentEditingServiceBase`/`ContentPublishingServiceBase` open a sync `ICoreScope`, await only genuinely-async leaf calls, call sync methods un-awaited within it). Do **not** build a first-class async `ICoreScopeProvider`/`ICoreScope` API — none exists anywhere in the codebase today, and building one is out of scope. `IEFCoreScope<TDbContext>.ExecuteWithContextAsync(...)` is what actually enables writing real `async`/`await` methods against the EF Core repository within that sync-scope shell.
2. **`CancellationToken` adoption**: Yes — adopt it on the new async service surface, even though no prior service migration in this codebase has done so (the closest precedent, `IContentTypeService`'s async conversion via `IAsyncContentTypeBaseService<TItem>`, notably did NOT add `CancellationToken` anywhere). This migration is the first to do so.
3. **`DocumentBlueprintRepository`**: `DocumentBlueprintRepository : DocumentRepository` (NPoco) is a **class-inheritance** dependency, not just a service-level consumer — the single sharpest blocker to deleting `DocumentRepository`. Resolved by creating `AsyncDocumentBlueprintRepository : AsyncDocumentRepository` — done, see below.
4. **`ElementService`**: Also inherits the shared `PublishableContentServiceBase<TContent>` base that `ContentService` uses — any base-class change ripples to it. **Defer `ElementService`'s conversion if at all possible** — don't force it into scope just because the base class changes; only touch it if truly unavoidable.
5. **`IAsyncDocumentRepository` vs `IDocumentRepository` method-surface parity check**: done, see below.

**Explicit sequencing instruction from the user**: "Don't do a complete plan to fix all of this at once, but keep it in your memory. For now start make a plan to resolve DocumentBlueprintRepository and then after we've done that let's do 5 and check for parity." — work through this incrementally, one self-contained increment at a time, not as one giant upfront plan. The full `ContentService`-to-async conversion (the actual point of this whole effort) is NOT yet scoped into a concrete plan.

### Step 3 done (2026-08-06): `AsyncDocumentBlueprintRepository` created

`AsyncDocumentRepository` un-sealed (`internal sealed class` → `internal class` — confirmed safe, no `is`/`typeof`/`as` exact-type checks anywhere in the codebase). New `IAsyncDocumentBlueprintRepository : IAsyncDocumentRepository` marker interface (`src/Umbraco.Core/Persistence/Repositories/`) and `AsyncDocumentBlueprintRepository : AsyncDocumentRepository, IAsyncDocumentBlueprintRepository` (`src/Umbraco.Infrastructure/Persistence/Repositories/Implement/EFCore/`) mirror NPoco's `DocumentBlueprintRepository` exactly: constructor forwards all parameters unchanged to `base(...)`, overrides only `EnsureUniqueNaming => false` and `NodeObjectTypeKey => Constants.ObjectTypes.DocumentBlueprint`. New test file `AsyncDocumentBlueprintRepositoryTest.cs` (3 tests: duplicate names not suffixed, correct `NodeObjectType` persisted, isolation from a plain `AsyncDocumentRepository`'s child queries).

### Step 5 done (2026-08-06): `IAsyncDocumentRepository` vs `IDocumentRepository` parity check

- **Bulk of the interface matches**: every schedule method, version method, count/children/descendants/recycle-bin method, `CheckDataIntegrity`, and all 4 permission methods have a direct `Async`-suffixed, `CancellationToken`-accepting Guid-keyed counterpart.
- **Real gap 1 — `UpdateSortOrder(IReadOnlyList<int> orderedNodeIds)`**: no async equivalent anywhere in the chain. Backs backoffice tree drag-drop reordering. Closed — see below.
- **Arbitrary `IQuery<IContent>`-based querying — RESOLVED, not a gap to fix**: intentional and permanent — the EF Core side deliberately only exposes purpose-built query shapes (`GetChildrenAsync`, `GetDescendantsAsync`, `GetRecycleBinAsync`, etc.), not a generic arbitrary-predicate mechanism. Do NOT build an EF Core equivalent of `IQuery<T>`. Every current `ContentService.cs` call site using `_documentRepository.Get(query)`/`GetPage(query, ...)` will need to be re-expressed against a specific purpose-built method when that conversion happens.
- **Int-keyed reads — RESOLVED**: `Get(int)`/`GetMany(int[])`/`Exists(int id)` are absent on the async side because `IAsyncDocumentRepository` is deliberately Guid-first throughout. The user explicitly rejected bridging this at the `ContentService` boundary via `IIdKeyMap` — **`ContentService`'s own public signatures should change to accept only Guids**, pushing int→Guid resolution up to *callers*. Every caller of `IContentService.GetById(int)`/`GetByIds(IEnumerable<int>)` needs to be updated to pass a Guid instead. This ripples to `ContentService`'s callers, not just its internals.

### Step: `UpdateSortOrderAsync` added (2026-08-06)

Added `UpdateSortOrderAsync(IReadOnlyList<Guid> orderedNodeKeys, CancellationToken)` to `IAsyncContentRepository<TEntity>` (generic tier, matching NPoco's own placement on `IContentRepository<TId, TEntity>`) and implemented on `AsyncContentRepositoryBase<TEntity, TRepository>` — inherited by `AsyncDocumentRepository`/`AsyncDocumentBlueprintRepository` with no override. Reuses the established batched-fetch (`InGroupsOf(Constants.Sql.MaxParameterCount)`) + `.AsTracking()` mutate + single `SaveChangesAsync` pattern, not NPoco's raw-SQL `CASE WHEN` batch update — EF Core's `SaveChangesAsync` issues one `UPDATE` per changed row, so it isn't subject to the 2100-parameter ceiling NPoco's raw SQL had to batch around.

## Session of 2026-08-07 (home machine): DI registration, two of the four production consumers fully migrated off IDocumentRepository

### DI registration bug found and fixed
The DI registration (`AddUnique<IAsyncDocumentRepository, AsyncDocumentRepository>()`) looked fine — full test suite passed — but that was misleading, since every existing test constructs the repository manually (`new AsyncDocumentRepository(...)`), never through the container. The first real DI-constructed consumer (`DocumentUrlService`) failed at runtime: the constructor was `internal`, and `Microsoft.Extensions.DependencyInjection`'s default container only reflects over public constructors regardless of the containing class's own accessibility. Fixed by making both `AsyncDocumentRepository`'s and `AsyncDocumentBlueprintRepository`'s constructors `public` (classes stay `internal`). See [[feedback_internal_class_needs_public_constructor_for_di]]. Committed as `fe2175eaffa` + `4ef772dd164`.

### `DocumentUrlService` — fully migrated off `IDocumentRepository`
`RebuildAllUrlsAsync()` now takes a `CancellationToken` (breaking signature change — no obsolete-overload dance, since the breaking change is already announced for this version), threaded from `InitAsync` down into `_documentRepository.GetAllAsync(cancellationToken)` on the new `IAsyncDocumentRepository`. Committed as `4ef772dd164`.

### `DeferredSearchReindexService` — fully migrated off `IDocumentRepository`
Both document-facing methods converted; `IDocumentRepository` is now entirely unused in this class and was removed (field + constructor param deleted).

- `ReindexDocumentsReferencingElements` → `ReindexDocumentsReferencingElementsAsync`: renamed `FindDocumentIdsReferencingElements` → `FindDocumentKeysReferencingElements`, now collects `document.Key` (Guid) instead of `document.Id` — no `IIdKeyMap` bridge needed, since `IUmbracoEntity` already carries `.Key` alongside `.Id`. Committed as `c9ef8179ecb`.
- `ReindexContentOfContentTypes` → `ReindexContentOfContentTypesAsync`: now reads through a new purpose-built repository method, `IAsyncDocumentRepository.GetPagedOfContentTypesAsync`. Committed as `764c6938f52` (new method) and `6086fb4fe9d` (wiring + refactor).

### New repository method: `GetPagedOfContentTypesAsync`
Pages `IContent` filtered by a set of content-type keys, ordered by any existing field plus a new `"path"` case. Not a violation of the "no generic `IQuery<T>` port" decision — a narrow, purpose-built, already-reused shape (same filter-by-content-type-ids-paged pattern already backs `IContentService.GetPagedOfType(s)` and 3 real NPoco call sites). Two decisions worth remembering for the next purpose-built method added to this interface:
1. **Takes `Guid[] contentTypeKeys`, not `int[] contentTypeIds`** — see [[feedback_guid_keys_not_int_ids_on_async_repository]].
2. **`ApplyDocumentOrdering`'s new `"path"` case** is an *optional, nullable* selector parameter with a guarded switch arm, so existing callers/tests that don't pass it are unaffected.

### `AsyncPageAndReindex<TEntity>` + `ResolveKeysAsync` extracted (`DeferredSearchReindexService.cs`)
- `AsyncPageAndReindex<TEntity>` — async counterpart of `PageAndReindex<TEntity>`, mirroring its exact `page`/`page * pageSize < total` loop shape (an equivalent `skip`-accumulator version was rejected on review for looking inconsistent with its sync sibling — match established loop idioms exactly when a method is explicitly framed as another one's "counterpart"). Takes a fetch delegate rather than a repository interface, since document/media/member don't share a common async paged-query interface yet. Media/member reindexing itself was **not** touched — still on sync `PageAndReindex`/NPoco, since no async repository exists for them yet.
- `ResolveKeysAsync` — int-id→Guid-key bridge via `IIdKeyMap.GetKeyForIdAsync`, needed because cache-refresher notifications feeding `QueueContentTypeReindex` are still sync and int-keyed. Carries a `TODO` to remove once that pipeline carries Guid keys directly — scaffolding, not permanent.

### Current consumer status (of the original 4)
- `DocumentUrlService` — fully migrated.
- `DeferredSearchReindexService` — fully migrated.
- `DocumentBlueprintRepository` — resolved via class-inheritance (`AsyncDocumentBlueprintRepository`).
- `ContentService` — **the only one left, and by far the largest** (nearly the whole interface). Still 100% synchronous. Still NOT scoped into a concrete plan — wait for explicit direction before planning it. See [[project_ef_core_document_repository_status]] for the method-by-method tier breakdown.

## Research findings worth keeping

- **`IDocumentRepository` production consumers** (4 total, now down to 1 unmigrated): `ContentService` (heaviest, unmigrated), `DocumentBlueprintRepository` (resolved via class inheritance), `DocumentUrlService` (migrated), `DeferredSearchReindexService` (migrated).
- **`ContentService`/`PublishableContentServiceBase<TContent>` current API state**: ~55 public members, only 1 (`EmptyRecycleBinAsync`) is `Task`-returning, zero accept `CancellationToken`, zero return a real typed `Attempt<TResult, TStatus>`.
- **No async scope API exists anywhere in this codebase** — `ICoreScopeProvider.CreateCoreScope()`/`ICoreScope.Complete()` are fully synchronous. `IEFCoreScope<TDbContext>` (`ExecuteWithContextAsync`) is the actual enabling mechanism.
- **Controller-layer blast radius is small**: only 3-4 Management API files call `IContentService` directly; the rest go through `IContentEditingService`/`IContentPublishingService` (already-async wrapper services) which call straight into sync `ContentService` methods un-awaited inside a sync scope. Converting `ContentService` will require touching those wrapper services' call sites more than controllers.
- **Notification publishing**: `ContentService` only ever calls sync `Publish`/`PublishCancelable`. The sync `EventAggregator.Publish` already internally does `Task.WaitAll(...)` for async notification handlers — converting to real `await PublishAsync(...)` is a contained, mechanical follow-on, not a blocker.

## Status as of the 2026-08-07 home-session handover (READ THIS for "where do we stand")

- **Test suites** (all passing on real rebuilds, not `--no-build`): `AsyncDocumentRepositoryTest` 133/133 (includes `AsyncDocumentBlueprintRepositoryTest`), `AsyncDocumentRepositoryOrderingTests` 4/4, `DocumentUrlServiceTests` (unit) 25/25, `DocumentUrlServiceTests` (integration) 74/74, `DeferredSearchReindexServiceTests` (unit) 11/11.
- **Known pre-existing, unrelated failure**: `DeferredSearchReindexServiceElementTests` (integration, 3 tests) fails with `NotImplementedException: GetParentEntitiesByChildIds depends on EntityRepository being migrated to EF Core.` — confirmed this fails identically on commits from before any of this effort's work; a separate, already-existing gap in `RelationRepository`/`EntityRepository`'s own EF Core migration, not part of this effort.
- **Building this repo locally**: plain `dotnet build` on `Umbraco.Core`/`Umbraco.Infrastructure` csproj files can trigger a broken frontend npm/TypeScript build via `Umbraco.Cms.StaticAssets` (unrelated pre-existing issue). Use `dotnet build <project> -p:UmbracoBuild=true` to skip it.
- **Nothing is currently queued.** Candidates for the next increment, roughly in order of readiness:
  1. Start the `ContentService` conversion itself — only when explicitly asked, pick ONE small method first. `GetById(Guid key)` on `PublishableContentServiceBase<TContent>` was identified as the lowest-risk starting point (zero signature change, zero other-repo coupling, direct 1:1 async equivalent already exists: `GetAsync(Guid, CancellationToken)`).
  2. `DeferredSearchReindexService`'s media/member reindexing — still on NPoco, needs `IAsyncMediaRepository`/`IAsyncMemberRepository` built first; much bigger scope.
  3. Nothing else is currently a known small, self-contained gap.
- **Two temporary "handover" commits are now baked into shared history**: `cba35a2ae2a` (written on the office machine) and `64419bc85f8` (written on the home machine), plus a merge commit tying them together, all already pushed to the real public `origin` (`umbraco/Umbraco-CMS`). They're no longer a simple "drop the last commit" cleanup — real work commits are interleaved between them. Don't rewrite this shared branch's history without explicit user confirmation first (it's the public repo). **Update: this was resolved (2026-08-10)** — with the user's explicit go-ahead, confirmed the last real-work commit (`6086fb4fe9d`) had zero diff against the home-session handover tip other than the added `HANDOVER.md` file, so `git reset --hard 6086fb4fe9d` (dropping both chore commits and the merge in one move) followed by `git push --force-with-lease` to origin was sufficient — no cherry-picking needed. History is clean again; this bullet is now historical only.

## Session of 2026-08-10: `ContentService`'s async conversion actually started — first method done

Per the "one increment at a time" instruction, this session did the first real increment of the `ContentService`-to-async conversion (previously only decided, never started).

**Design decision — shared interface built now, not deferred**: rather than adding `GetByIdAsync` directly to `IContentService`, the user asked to create `IAsyncContentServiceBase<TItem>` (new file, `src/Umbraco.Core/Services/IAsyncContentServiceBase.cs`) as a **sibling** to the existing `IContentServiceBase<TItem>` — mirroring `IAsyncContentTypeBaseService<TItem>`'s precedent shape (a full parallel interface, not an extension of the sync one). Reasoning given: Media and Member are getting migrated "right after" Content, so the shared async base should exist now rather than being redesigned three times. Explicitly **not** wired into `IMediaService`/`IMemberService` yet — only `IContentService` implements it so far (`IContentService : IPublishableContentService<IContent>, IAsyncContentServiceBase<IContent>`, added alongside the sync base, not replacing it — full-interface-swap-per-service, like `IContentTypeService` did, would force converting all ~62 `IContentService` call sites in one step, contradicting the incremental approach). `IAsyncContentServiceBase<TItem>` currently declares only `GetByIdAsync(Guid, CancellationToken)`; grows member-by-member as more of `ContentService` converts, same discipline as the repository interface.

**`ContentService.GetByIdAsync`** (SUPERSEDED — see "AsyncPublishableContentServiceBase" section below for the final shape): first implemented directly on `ContentService` (not the shared `PublishableContentServiceBase<TContent>`), so `ElementService` stays completely untouched. Body mirrored the sync `GetById(Guid)` exactly: same `ScopeProvider.CreateCoreScope(autoComplete: true)` + `scope.ReadLock(ReadLockIds)` shell, `await _asyncDocumentRepository.GetAsync(key, cancellationToken)` instead of the sync repo call. This placement was later replaced by a proper `AsyncPublishableContentServiceBase<TContent>` base class in the same session — the method itself and its body are unchanged, only WHERE it lives moved.

**Real bug found and fixed — circular DI dependency**: wiring `IAsyncDocumentRepository` into `ContentService` closed a real cycle that had never been exercised before (nothing had previously constructed `AsyncDocumentRepository` from a chain rooted at `IContentService`): `ContentService → AsyncDocumentRepository → IUserGroupService → IUserGroupPermissionService → ContentService`. Fixed by changing `AsyncPermissionRepository<TEntity>`'s and `AsyncDocumentRepository`'s `IUserGroupService` dependency to `Lazy<IUserGroupService>` (`.Value` at the two actual use sites) — the established de-cycling tool in this codebase (`LazyResolve<T>`, already used for `ContentService`'s own `Lazy<IPropertyValidationService>`). Test call sites updated to pass `new Lazy<IUserGroupService>(GetRequiredService<IUserGroupService>)`.

**Constructor evolution — then simplified after user feedback**: first pass followed CLAUDE.md's obsolete-constructor-plus-new-constructor pattern to add `IAsyncDocumentRepository`, which surfaced a second real problem — the new "current" constructor tied in parameter count with a *pre-existing* legacy obsolete constructor (`IAuditRepository` + `IAuditService` overload, 18 params each), which the default DI container's tie-breaking can't resolve on its own (confirmed `[ActivatorUtilitiesConstructor]` is NOT honored by plain `AddUnique<TService,TImpl>()` registrations — only by `ActivatorUtilities.CreateInstance`, the actual working precedent already in this codebase for `MemberRepository`). **User then corrected the whole approach**: this branch is allowed breaking changes, no need to dance around them. Since both pre-existing legacy constructors were already marked `"Scheduled for removal in Umbraco 19"` and `version.json` confirms current version is `19.0.0-beta1` — already past their own declared removal point — deleted both, added `IAsyncDocumentRepository` directly to the one remaining constructor, and reverted the DI registration to the plain `Services.AddUnique<IContentService, ContentService>()` one-liner. Net effect: `ContentService.cs` is ~140 lines shorter, single constructor, no `[ActivatorUtilitiesConstructor]`/factory-registration workaround needed at all. See [[feedback_this_branch_allows_breaking_changes]].

**Pre-existing `SaveAndPublish` failure — ROOT CAUSE FOUND (2026-08-10), fix confirmed but NOT applied yet**: `ContentServiceTests`'s `SaveAndPublish` family (8 tests: `Can_Publish_And_Unpublish_Cultures_In_Single_Operation`, `Can_SaveAndPublish_And_Child_Without_Identity`, `Can_SaveAndPublish_Invariant_Content`, `Can_SaveAndPublish_Invariant_Content_Without_Prior_Save`, `Can_SaveAndPublish_Variant_Content_Multiple_Cultures`, `Can_SaveAndPublish_Variant_Content_Single_Culture`, `Failed_SaveAndPublish_Preserves_Edited_State`, `SaveAndPublish_No_Cultures_On_Variant_Saves_But_Does_Not_Publish`) all fail with `Microsoft.Data.Sqlite.SqliteException: SQLite Error 19: 'UNIQUE constraint failed: umbracoContentVersion.key'`.

**Root cause**: `PublishableContentRepositoryBase.PersistNewItem` (`src/Umbraco.Infrastructure/Persistence/Repositories/Implement/PublishableContentRepositoryBase.cs`, the "publishing immediately" branch around line 1076-1088) does a double-insert of `ContentVersionDto` when new content is saved-and-published in one call (draft version row, then a second "current" version row). The SAME `ContentVersionDto` C# instance is reused for both inserts — `Id` is reset to `0` before the second insert (so NPoco/SQLite assigns a fresh autoincrement id), but `Key` (a `Guid`, set once via `Guid.NewGuid()` in `ContentBaseFactory.BuildContentVersionDto`) is never regenerated, so the second insert tries to write a second row with the exact same `Key` value as the first — violating `IX_umbracoContentVersion_key`'s unique constraint.

**CORRECTED root-cause attribution (2026-08-10, after checking v19/dev directly)**: initially concluded this was "a long-standing product bug unrelated to this migration effort" based on `git blame` on the double-insert BLOCK (`PublishableContentRepositoryBase.PersistNewItem`'s publish-immediately branch), which dates to 2026-02-10 (commit `cf70d7ff13ae`, Kenn Jacobsen). **This was wrong** — the double-insert code itself is old, but it was harmless until `ContentVersionDto.Key` (the `Guid` column with the unique index that actually gets violated) was added much later, on **2026-04-15**, by commit `2a6b8d30f28` ("Add key to version dto", author: the user themselves, `mole`) — part of this exact EF-Core-migration effort's own DTO-porting work (adding Guid keys so EF Core repositories could use them). That commit touched only the DTO/migration/`IAsyncContentRepository` files — it never touched `PersistNewItem`'s double-insert code, so the reused-DTO-with-stale-Key bug became live and unnoticed at that point. **So this is squarely a bug introduced by this same multi-month EF-Core-migration effort** (just from an earlier, different work item than anything in this session) — not an unrelated pre-existing upstream defect.

**Directly confirmed by checking out and running tests on pure `origin/v19/dev`** (not just reading code): built cleanly, ran the same 8 tests — **all 8 passed**. Investigated why despite `v19/dev` having byte-identical double-insert code: `v19/dev`'s `ContentVersionDto` has **no `Key` column at all** — the April 2026 "Add key to version dto" commit was never merged/ported to `v19/dev`. So `v19/dev` isn't "fixed" — it simply doesn't have the column whose unique constraint gets violated yet.

**SECOND CORRECTION, important**: commit `2a6b8d30f28` ("Add key to version dto") only exists on `v18/feature/ef-core-document-repository` — confirmed via `git branch --all --contains 2a6b8d30f28` (and `git merge-base --is-ancestor` returning false against `v18/feature/ef-core-repositories`). **`ContentVersionDto.Key` does not exist on `v18/feature/ef-core-repositories` at all** — same as `v19/dev`, for the same reason (that commit is specific to the document-repository line, added there to support `AsyncDocumentRepository`'s own write path). Discovered this the hard way: tried applying the one-line `Key = Guid.NewGuid()` fix on a new branch cut from `ef-core-repositories` and got a compile error (`'ContentVersionDto' does not contain a definition for 'Key'`) — reverted those two edits immediately. **The bug, and its fix, belong on `v18/feature/ef-core-document-repository` (or a branch based on THAT), not on `v18/feature/ef-core-repositories`.** Don't repeat the mistake of assuming a `git blame`/`git log --all -S...` hit is on the branch you're currently working on — verify with `git merge-base --is-ancestor <commit> HEAD` first.

It only manifests on the specific "brand-new content, saved and published in a single call" code path — `Save()` then `Publish()` as two separate calls goes through `PersistUpdatedItem` instead, unaffected. `git stash` back to the unmodified baseline reproduced it identically, and running one test 3× in a row confirmed it's fully deterministic, not flaky.

**Fix confirmed experimentally** (implemented, verified all 8 tests pass, then reverted — NOT currently applied to the working tree): add `contentVersionDto.Key = Guid.NewGuid();` right before the second `Database.Insert(contentVersionDto);` call (line ~1082 of `PublishableContentRepositoryBase.cs`). Ran all 8 previously-failing tests together against this one-line change — 8/8 passed. Reverted afterward since the ask at the time was to investigate, not fix. Decide explicitly whether/when to apply and commit this — it's an unrelated NPoco bugfix, not part of the `ContentService`/`AsyncDocumentRepository` migration work itself, so probably deserves its own separate commit regardless of when it lands.

**Side investigation while checking v19/dev**: merging `origin/v19/dev` into `v18/feature/ef-core-repositories` surfaced that the two branches have diverged enough that the FULL test suite doesn't compile cleanly on the merged result. Created `v18/bugfix/reconcile-v19-dev-merge` off the merged `ef-core-repositories` (commit `7111d423a88`) and fixed all remaining compile breaks there: `IRelationService.GetByParentOrChildId` → `GetByParentOrChildIdAsync`, `ContentTypeService.Get(int)` → `GetAsync(int)` (both test-only, both trivial async-swap fixes, committed as `d08746f0e2d`). Test suite now compiles cleanly and the two fixed test files pass (130/130). **This branch is pure merge-reconciliation, ready to go back into `ef-core-repositories` — it deliberately does NOT contain the `ContentVersionDto.Key` fix** (see below for why).

**Round 2 (user reported "still issues")**: had only built `Umbraco.Core`/`Umbraco.Infrastructure`/`Umbraco.Tests.Integration` before, not the full solution. `dotnet build umbraco.sln` turned up 3 more compile errors, all in `Umbraco.Tests.UnitTests` (a project never checked): `IIdKeyMap.GetKeyForId` (removed sync method, `ServerEventSenderTests.cs`) and `IContentTypeService.GetAll()`/`.Get(string)` (removed sync methods, `ContentNavigationServiceBaseTests.cs`). Fixed all three (mock setups switched to the async equivalents with `ReturnsAsync`). **Fixing the `ContentNavigationServiceBaseTests.cs` one surfaced a genuine, real production bug once the test could actually run**: `AsyncContentNavigationServiceBase.TryGetContentTypeKey` lazily builds a plain `Dictionary<string, Guid>` alias-to-key cache, then mutates it with `.TryAdd(...)` from concurrent callers with zero synchronization — under real concurrent misses this throws `InvalidOperationException` (dictionary internal state corrupted) on every subsequent lookup until process restart. This is exactly the defect the (new, from v19/dev) test was written to catch. Fixed by changing the cache to `ConcurrentDictionary<string, Guid>` (field, `Lazy<T>` type param, `LoadContentTypes()`'s return type — 4 call sites total). Committed as `4c069756bc7`. Full `umbraco.sln` build: 0 errors. Full `Umbraco.Tests.UnitTests` run: 6592/6593 passing (one known-flaky, unrelated hosted-service timing test, confirmed by running it in isolation 3× — always passes alone, only flakes under full-suite parallel load).

**Lesson for next time reconciling a merge**: always run `dotnet build <full-solution>.sln`, not just the projects you assume are affected — compile errors can hide in projects (like a separate unit-test project) that were never touched by the specific files you were looking at.

## Round 3 (user reported more failures after the PR): 2 more test failures, both genuine pre-existing gaps, neither caused by the merge

`Can_Renormalize_Edited_Flag_When_Property_Becomes_Invariant(True)` and `GetParentEntitiesByChildIds_Returns_Parents_For_All_Children_Filtered_By_Alias` — both new tests that came in via the `v19/dev` merge (confirmed via `git log -S<test name>`, neither existed pre-merge on `ef-core-repositories`), both exposing real, pre-existing gaps on `ef-core-repositories` that predate this whole reconciliation effort:

1. **`GetParentEntitiesByChildIds`**: `RelationRepository.GetParentEntitiesByChildIds` throws `NotImplementedException("...depends on EntityRepository being migrated to EF Core.")` — a DELIBERATE stub, committed `2026-07-31` by a colleague (`5ecdf7710f8`, "Fix merge" — confirmed via `git merge-base --is-ancestor` to be in `ef-core-repositories`'s own pre-merge ancestry, NOT something from v19/dev or my merge). Checked v19/dev's real, working implementation: it depends on an `IEntityRepository.GetPagedResultsByQuery` overload taking a raw SQL-customization `Action<Sql<ISqlContext>>` delegate that **does not exist on our branch's `IEntityRepository` interface at all** — porting v19/dev's implementation isn't a copy-paste, it requires extending `IEntityRepository`'s querying capability first. Genuinely out of scope for merge reconciliation; same category as the already-documented `DeferredSearchReindexServiceElementTests` gap on the other branch.

2. **`Can_Renormalize_Edited_Flag_When_Property_Becomes_Invariant(True)`** (Element case; `(False)`/Document passes): spent significant time on two wrong hypotheses before finding the real cause — see [[feedback_verify_commit_is_on_current_branch]]-style lesson: **don't assume the code you're looking at is the code actually being exercised.** Wrong hypothesis 1: the merge-conflict-resolved `LanguageRepository.GetDefaultIdAsync().GetAwaiter().GetResult()` bridge in NPoco's `ContentTypeRepositoryBase.RenormalizeEditedFlags` — tried swapping to the existing, proven `GetDefaultLanguageId()` sibling method in the same class, rebuilt, retested: **no change, test still failed identically.** That was the signal the wrong file was being debugged. Added temporary `Console.WriteLine` tracing inside the suspected method — **zero output**, even on rebuild — proof the NPoco `ContentTypeRepositoryBase` path isn't reached at all by this test. Reverted all of it (`git checkout --` cleanly restored the file).

   **Real cause**: `IContentTypeService` is fully async on this branch (`AsyncContentTypeServiceBase`/`AsyncContentTypeRepositoryBase`, EF Core) — the NPoco `ContentTypeRepositoryBase` is dead code for this call path. The EF Core `AsyncContentTypeRepositoryBase.MovePropertyTypeVariantDataAsync` (line ~1256) never splits impacted content types by `IsElement` — it unconditionally calls `RenormalizeDocumentEditedFlagsAsync` for everything, and **there is no `RenormalizeElementEditedFlagsAsync` at all**. Confirmed via `git diff c6bb10d60f4^1 c6bb10d60f4 -- .../AsyncContentTypeRepositoryBase.cs` (empty — the merge never touched this file) that this predates the merge entirely; it's a genuine, silent, pre-existing correctness bug (Element property-variance changes never get their "edited" flags renormalized) that just had no test until `v19/dev`'s parity-fix commit (`528af4417e0`, "Elements: Fix property-variance parity gaps with Documents (#23467)") brought one in.

   **Not yet fixed** — this is a real fix (generify `RenormalizeDocumentEditedFlagsAsync`/add an Element counterpart, mirroring the NPoco sibling's already-solved Document/Element unification pattern, adapted to EF Core LINQ), not a quick patch. Asked the user how to proceed before starting.

**How to apply this session's meta-lesson going forward**: when a test fails and the "obviously relevant" code looks right, check whether that code is even the one actually running (grep for a fully-async sibling class/service before spending time in a sync one, especially in this codebase where full sync→async swaps are common and leave the old code technically present but dead).

## `ContentVersionDto.Key` fix — landed on the correct branch

Tried applying the fix on `v18/bugfix/reconcile-v19-dev-merge` first and hit a compile error: `'ContentVersionDto' does not contain a definition for 'Key'`. Investigated and found the real reason (see [[feedback_verify_commit_is_on_current_branch]]): commit `2a6b8d30f28` ("Add key to version dto") — the one that adds this column — only exists on `v18/feature/ef-core-document-repository`, confirmed via `git branch --all --contains 2a6b8d30f28` and `git merge-base --is-ancestor`. It was never part of `v18/feature/ef-core-repositories`'s history, so the bug (and the fix) don't apply there at all. Reverted the two edits on the reconciliation branch. Asked the user how to proceed (cherry-pick the Key column onto the reconciliation branch, vs. fix it separately on `ef-core-document-repository`) — **chose the latter: leave the reconciliation branch alone, fix on `ef-core-document-repository` directly.**

**Fix applied directly on `v18/feature/ef-core-document-repository`** (not yet committed as of this memory update): added `contentVersionDto.Key = Guid.NewGuid();` to `PublishableContentRepositoryBase.PersistNewItem`'s publish-immediately branch (the gap this whole investigation started from). While doing this, discovered the **sibling `PersistUpdatedItem` branch already had this exact fix** — `contentVersionDto.Key = Guid.NewGuid(); // each version needs its own unique key` was added back on **2026-06-03** by commit `64b739e0ec70` ("Fix erros found by tests"). So the bug was already half-fixed; `PersistNewItem` was the one branch that got missed. Verified via full rebuild (0 errors) and the full `ContentServiceTests`/`AsyncDocumentRepositoryTest`/`AsyncDocumentBlueprintRepositoryTest` suite: **269/269 passing** (up from 261/269) — all 8 previously-failing tests now pass, zero regressions. **Committed as `909b7c9c9c9`** on `v18/feature/ef-core-document-repository`. Working tree clean.

**Two branches now hold related-but-separate fixes, both ready to go back upstream independently**: `v18/bugfix/reconcile-v19-dev-merge` (pure v19/dev merge reconciliation, off `ef-core-repositories`, commits `7111d423a88`+`d08746f0e2d`, not yet pushed) and `v18/feature/ef-core-document-repository` (the `ContentVersionDto.Key` fix, commit `909b7c9c9c9`, on top of all this session's `ContentService`/`AsyncPublishableContentServiceBase` work, not yet pushed).

## Session of 2026-08-10 (continued): `AsyncPublishableContentServiceBase<TContent>` — the real end-state architecture for the ContentService conversion

After a review pass came back clean on the `GetByIdAsync`-directly-on-`ContentService` shape above, the user rejected that architecture on a different basis: it doesn't allow a **true bit-by-bit replacement**. Their reasoning: `PublishableContentServiceBase<TContent>` is confirmed (via grep) to be referenced ONLY via inheritance — no code anywhere holds a field/variable typed as `PublishableContentServiceBase<TContent>` directly, only via `IContentService`/`IElementService`/concrete `ContentService`/`ElementService`. That makes it safe to swap `ContentService`'s base class entirely for a **new, independent copy**, then convert that copy's members one at a time — add the async version, migrate every internal (production, not test) caller of the sync version to await it, then delete the sync version outright from the copy. Repeat until "none of the sync content of the base class exists." This is the exact same "AsyncX scaffolding, eventually rename over the original" pattern already used for the repository layer (`AsyncDocumentRepository` alongside `DocumentRepository`), now applied one layer up at the service base-class level.

**What was built**:
- **`src/Umbraco.Core/Services/AsyncPublishableContentServiceBase.cs`** (new file) — created via `cp` + a word-boundary `sed` rename of the 3 self-referencing occurrences (class declaration, constructor name, the abstract `Logger` property's generic argument) — literally every other line is byte-identical to `PublishableContentServiceBase.cs` at the time of copying. Declared as `public abstract class AsyncPublishableContentServiceBase<TContent> : RepositoryService, IPublishableContentService<TContent>, IAsyncContentServiceBase<TContent>` — generic over `TContent`, same as the original, so it's ready for Media/Member/Element to adopt later without redesign, exactly mirroring why `IAsyncContentServiceBase<TItem>` itself was made generic.
- New constructor param `IAsyncPublishableContentRepository<TContent> asyncContentRepository` (stored as `_asyncContentRepository`) — this generic async repository interface **already existed** (`src/Umbraco.Core/Persistence/Repositories/IAsyncPublishableContentRepository.cs`), with `IAsyncDocumentRepository : IAsyncPublishableContentRepository<IContent>` already satisfying it — no new repository-layer interface needed, this was already built during the repository migration phase and just hadn't been consumed by a service yet.
- `GetByIdAsync(Guid, CancellationToken)` implemented generically in the new base class (moved out of `ContentService` where it was first written): `await _asyncContentRepository.GetAsync(key, cancellationToken)` inside the same `CreateCoreScope`/`ReadLock(ReadLockIds)` shell as the sync `GetById(Guid)`, which is **still present, unchanged, right next to it** — not yet deleted (see below).
- **`ContentService`** now declares `: AsyncPublishableContentServiceBase<IContent>, IContentService` (was `PublishableContentServiceBase<IContent>`). Its own constructor threads `asyncDocumentRepository` (typed `IAsyncDocumentRepository`) into the new `base(...)` parameter position instead of storing it as a `ContentService`-local field — the field/method that used to live directly on `ContentService` were removed entirely (deleted, not deprecated) once the base class took over.
- **`ElementService` is completely untouched** — still `: PublishableContentServiceBase<IElement>, IElementService`, still using the original, unmodified class. This was the entire point of copying rather than editing in place.
- Verified `ILogger<AsyncPublishableContentServiceBase<TContent>>`-typed abstract `Logger` property is satisfied by `ContentService`'s existing `ILogger<ContentService>` override via C#'s covariant `ILogger<out TCategoryName>` — no override signature change needed, since `ContentService` is still a subtype of whichever base class it currently extends.
- Full rebuild (non-`--no-build`, `--no-incremental` used once to rule out a stale/cached false-clean build) across Core + Infrastructure + Integration tests: 0 errors. Full `ContentServiceTests` + `AsyncDocumentRepositoryTest` + `AsyncDocumentBlueprintRepositoryTest` rerun: same **261/269**, same 8 pre-existing `SaveAndPublish` failures, zero behavioral change from the restructuring (expected, since it's a like-for-like copy plus the same `GetByIdAsync` body that already existed).

**Where the "delete the sync method" half of the plan stands — deliberately NOT done yet, by explicit user choice**: Before touching anything, scoped out how big "find all internal callers of `GetById(Guid)` and migrate them" actually is. `GetById(Guid)` is declared on the SHARED `IContentServiceBase<TItem>` interface (also implemented independently by `MediaService`/`MemberService`) — so the interface itself can't lose the method yet without affecting Media/Member's contract too; only `ContentService`'s own sync *implementation* is the deletion target for now. Grepped for real `IContentService`-typed field/property holders calling `.GetById(Guid)= and found roughly **50 files** across `Umbraco.Core`, `Umbraco.Infrastructure`, and `Umbraco.Cms.Api.Management` — cache refreshers, notification handlers (`AddDomainWarningsWhenPublishingNotificationHandler`, `UserNotificationsHandler`, etc.), several Examine indexers/deferred-index handlers, property editors (`ContentPickerPropertyEditor`, `DataValueEditor`, `MultiNodeTreePickerPropertyEditor`), `PublicAccessService`, `DomainService`, `ContentPermissionService`, `UserGroupPermissionService`, `ContentEditingServiceBase`/`AsyncContentEditingServiceBase`, a couple of Management API controllers. A meaningful number of these callers are themselves fully synchronous methods, so migrating them to `await GetByIdAsync(...)` would cascade into converting those methods too — this is NOT a contained single-file swap. Presented this honestly to the user (via `AskUserQuestion`: stop here / scope it fully first / start with the easy non-cascading ones) — **user chose to stop here for this increment.** `GetById(Guid)` (sync) stays in `AsyncPublishableContentServiceBase` unchanged and still fully used; only the NEW `GetByIdAsync` was added. Caller migration + eventual sync-method deletion is its own separate, not-yet-started future increment — don't assume it's scoped or in progress.

**Status of `ContentService`'s conversion after this session**: `AsyncPublishableContentServiceBase<TContent>` is the confirmed, real architecture going forward (not the "add directly to ContentService" shape from earlier in this same session — that's superseded). `GetById`→`GetByIdAsync` is the first (and so far only) method with BOTH versions coexisting; nothing has been deleted yet.

**Fresh review completed (2026-08-10) — clean, modulo one unrelated incident**: a review agent diffed `AsyncPublishableContentServiceBase.cs` directly against `PublishableContentServiceBase.cs` (the highest-value check for a 2300-line copy operation) and confirmed the ONLY differences are the intended ones — no missed renames, no stray edits. Confirmed `ElementService.cs` is byte-for-byte unchanged. Confirmed the `Lazy<IUserGroupService>` fix and `ILogger<T>` covariance reasoning are both sound. **One real but unrelated incident**: mid-review, `ContentService.cs` line 28 was found corrupted on disk (`AsyncPubliHow shableContentServiceBase<IContent>` — literal text "How " spliced into the identifier), which the agent said appeared partway through its run without it editing the file — cause never identified (possibly a concurrent write from something else touching the same working directory). Verified real via hexdump (not just trusted or dismissed), fixed directly, then confirmed via full non-incremental rebuild across Core/Infrastructure/Integration-tests that nothing else was similarly affected — 0 errors. Redid the TDD-honesty check myself (the corruption had blocked the review agent from finishing it): temporarily broke `GetByIdAsync` to always return null, force-rebuilt, confirmed `GetByIdAsync_ExistingContent_ReturnsContent` genuinely fails, restored, force-rebuilt again, confirmed clean via `diff` against the original file, then reran the full suite — **261/269, same 8 pre-existing `SaveAndPublish` failures, nothing new.** See [[feedback_verify_agent_reported_file_corruption_directly]] for the corruption-handling lesson.

**Naming follow-up (2026-08-10)**: user asked to align the interface name with the class implementing it — renamed `IAsyncContentServiceBase<TItem>` → `IAsyncPublishableContentService<TContent>` (file renamed too: `IAsyncContentServiceBase.cs` → `IAsyncPublishableContentService.cs`), matching `AsyncPublishableContentServiceBase<TContent>`'s naming and mirroring the sync `IPublishableContentService<TContent>` naming convention exactly. **Deliberately kept the generic constraint unchanged** (`where TContent : class, IContentBase`, not narrowed to `IPublishableContentBase`) — narrowing would exclude Media/Member from ever implementing this interface later, contradicting the original reasoning for building it now ("Media and Member are migrated right after Content"). If a future session finds this constraint too loose (e.g. because the interface ends up only ever implemented by publishable content), that's a deliberate choice made here, not an oversight — revisit only if the "share with Media/Member later" plan changes. Rebuilt + retested after the rename: same 261/269, same 8 pre-existing failures.

**Committed as `3f05efd25a7`** — "add ContentService.GetByIdAsync via a new AsyncPublishableContentServiceBase". Working tree clean, 10 files changed (2 new: `AsyncPublishableContentServiceBase.cs`, `IAsyncPublishableContentService.cs`).

**Verified**: full rebuild (0 errors) + `ContentServiceTests` + `AsyncDocumentRepositoryTest` + `AsyncDocumentBlueprintRepositoryTest` → 261/269 passing both before and after the constructor simplification, with the 8 `SaveAndPublish` failures above being the only ones, unchanged either way.

**Status**: `ContentService`'s async conversion has genuinely started — `GetByIdAsync` is the first converted method, `IAsyncContentServiceBase<IContent>` is the new home for future ones. Not yet committed as of this memory update — check `git status`/`git log` before assuming it's landed. Next increment still needs explicit direction per the "one increment at a time" rule.

## Session of 2026-08-12: `GetById` fully retired for Document AND Element

Removed the synchronous `GetById(Guid)` from `IContentService`/`IElementService` entirely (Media/Member untouched — they still only have the sync tier), converted every call site to `GetByIdAsync`. Surfaced and fixed two real, pre-existing EF Core repository bugs while doing it:
1. `AsyncDocumentRepository.AssembleEntitiesAsync` didn't reset dirty-tracking after applying culture variations — fixed with `entity.ResetDirtyProperties(false)`.
2. The async repositories' default cache policy used the wrong Guid-key prefix (`"uRepo_"` instead of `"uRepoGuid_"`) — fixed with a new `AsyncGuidReadRepositoryCachePolicy<TEntity>`.

Verified: full solution build clean, 1073+ integration tests passing. `IElementService` needed its own async `GetByIdAsync`/`Save` shims added directly on `ElementService.cs` (it stayed on the original, untouched `PublishableContentServiceBase<IElement>`, so it can't inherit the new base class's implementation — has to duplicate the shim, by design, per the "ElementService stays untouched" decision above).

## Session of 2026-08-13: `GetById(int)` and `GetByIds(int)` fully retired from `IContentService`

Two more Tier-A `IContentService` members retired. **Standing instruction confirmed during planning, applies for the rest of this whole effort**: no obsolete-constructor/`StaticServiceProvider` staging for any of this branch's new `IIdKeyMap` dependencies — plain breaking constructor/method parameter additions, all call sites updated directly (per [[feedback_this_branch_allows_breaking_changes]]).

**`GetById(int)`**: removed entirely. 21 production + ~217 test call sites migrated to `GetByIdAsync(Guid, CancellationToken)`. Also deleted `DomainService.GetAssignedDomainsAsync(int, bool)` (already `[Obsolete]`, genuinely zero callers). **Mistake made and corrected** — see [[feedback_public_api_survives_even_with_zero_internal_callers]]: initially deleted `ContentServiceExtensions.GetAnchorValuesFromRTEs`/`GetAnchorValuesFromRTEContent` as "dead code" based on zero *internal* callers; they're public, non-obsolete extension methods. Restored both, unchanged signatures, re-plumbed via `IIdKeyMap`/`StaticServiceProvider`.

**`GetByIds(IEnumerable<int>)`**: removed entirely. Unlike every other member in this series, no async replacement existed yet — introduced `GetByIdsAsync(IEnumerable<Guid>, CancellationToken)` from scratch, backed by `IAsyncReadRepository.GetManyAsync`, preserving the same order-preserving dictionary-reprojection semantics as the old sync method (empty input short-circuits, unresolvable/duplicate ids handled correctly). 5 production + 2 test call sites migrated. **Second mistake made and corrected (placement)** — see [[feedback_async_interface_tier_matches_sync_tier]]: initially declared on the wide `IAsyncContentServiceBase<TContent>` tier; moved to the narrow `IAsyncPublishableContentService<TContent>` tier once the user pointed out sync `GetByIds` only ever lived there. Audited the other three already-migrated async members afterward — all four already correctly placed.

Both increments: full solution build clean, targeted suites passing (`ContentServiceTests`+`ContentTypeServiceVariantsTests`+10 other touched integration fixtures: 627 tests, 619 passed/3 failed(pre-existing, confirmed via stash-against-base)/5 skipped; `ContentServiceTests`+`ContentTypeServiceTests` again for the `GetByIds` increment: 199/199; `PublicAccessCheckerTests`+`UserEditorAuthorizationHelperTests`: 27/27), independent review agent runs finding no other issues. Committed as `135d31d347b` (`GetById(int)`) and `0814a66c1fb` (`GetByIds(int)`). **Both commits are on `v18/feature/ef-core-document-repository` but NOT YET PUSHED to origin as of this memory update** — 6 commits ahead of `origin/v18/feature/ef-core-document-repository` in total (these 2 plus the 4 from the 2026-08-10ish session: `CountPublished`/`GetContentSchedulesByKeys`/`GetContentScheduleByContentId` async conversions). Check `git log --oneline origin/v18/feature/ef-core-document-repository..HEAD` before assuming push state.

**Current `IContentService` sync-int-member retirement tally** (all committed locally, in order): `GetById(Guid)`, `CountPublished`, `GetContentSchedulesByKeys`, `GetContentScheduleByContentId`, `GetById(int)`, `GetByIds(IEnumerable<int>)`. Still fully synchronous: `Save`/`Publish`/`Unpublish`/`Move`/`Copy`/`Delete`/`Rollback`/`Sort`/etc. — the actual bulk of the interface (Tier B–F orchestration methods), deliberately saved for last.

**Immediate next step (per the 2026-08-13 handover, nothing queued as of this memory update)**: wait for explicit direction — don't pick the next increment unprompted, per the standing "one increment at a time" rule. Natural candidates: another Tier-A member with a straightforward `IAsyncReadRepository`-backed equivalent (re-derive the current candidate list from `IAsyncDocumentRepository`'s actual surface, don't trust a stale tier breakdown), or — only if explicitly told to move past the easy members — start on one small piece of the real Tier B–F orchestration methods (`Save`/`Publish`/`Move`/etc.).

**Build gotcha**: plain `dotnet build` on `Umbraco.Core`/`Umbraco.Infrastructure` csproj files can trigger a broken frontend npm/TypeScript build via `Umbraco.Cms.StaticAssets` (unrelated pre-existing issue). Use `dotnet build <project> -p:UmbracoBuild=true` to skip it.

## Session of 2026-08-17: `GetByLevel(int)` retired — first increment requiring brand-new async code (not just a swap)

Unlike every prior increment, `GetByLevel(int level)` had **no existing async equivalent anywhere** — it's declared directly on `IContentService` (never shared with Media, which has its own separate `MediaService.GetByLevel`), so this required building both a new repository method and a new service method from scratch, not just retargeting callers onto something already built.

**User's explicit design decisions**: (1) the new async version should be **paged** (`skip`/`take`, `PagedModel<IContent>`), matching every other new EF Core query method built in this effort, rather than perpetuating the old unbounded whole-tree scan; (2) the old method's documented behavior — *"Contrary to most methods, this method filters out trashed content items"* — **must be preserved** in the new version's leading node predicate.

**What was built**:
- `IAsyncDocumentRepository.GetByLevelAsync(int level, int skip, int take, Ordering? ordering, CancellationToken)` — placed directly on `IAsyncDocumentRepository` (not the shared `IAsyncContentRepository<TEntity>` tier), matching where the sync method was declared (Document-only, never shared with Media). Implementation is a copy-adapt of `GetPagedRecycleBinAsync` (same baseQuery construction, same three-way ordering dispatch), with the only change being the leading predicate: `node.Level == level && !node.Trashed` (adds the level filter, keeps the trashed exclusion).
- `IContentService.GetByLevelAsync(...)` / `ContentService.GetByLevelAsync(...)` — placed directly on `IContentService`/`ContentService` (not either async base class), matching where the sync method lived. Required adding a **new** `_asyncDocumentRepository` field to `ContentService.cs` — the constructor already received `IAsyncDocumentRepository asyncDocumentRepository` as a parameter (passed straight to `base(...)`), but nothing stored it locally until now, since no Document-specific (non-shared-tier) async repository call had been needed directly from `ContentService` before this.
- Sync `GetByLevel(int level)` deleted outright from both `IContentService` and `ContentService.cs` in the same change (this branch allows breaking changes — no obsolete-overload staging). `IMediaService`/`MediaService.GetByLevel` deliberately untouched.
- One test caller (`Can_Get_Content_By_Level`) migrated to `async Task` + `await GetByLevelAsync(2, 0, 100, null, CancellationToken.None)`. Added 3 new repository-level tests to `AsyncDocumentRepositoryTest.cs`: trashed-exclusion (written so it fails if the trashed filter were dropped), paging (skip/take across 4 same-level siblings), and custom-field ordering (smoke test reusing `CreateIntPropertyContentTypeAsync`).

**Verified**: full `dotnet build umbraco.sln -p:UmbracoBuild=true` — 0 errors, 0 warnings. `ContentServiceTests` 133/133 (same count as before, no regressions). `AsyncDocumentRepositoryTest` 136/136 (133 + 3 new). Not yet committed as of this memory update.

**Current `IContentService` sync-member retirement tally** (all committed locally, in order): `GetById(Guid)`, `CountPublished`, `GetContentSchedulesByKeys`, `GetContentScheduleByContentId`, `GetById(int)`, `GetByIds(IEnumerable<int>)`, `GetByIds(IEnumerable<Guid>)`, `GetByLevel(int)`. Still fully synchronous: `Save`/`Publish`/`Unpublish`/`Move`/`Copy`/`Delete`/`Rollback`/`Sort`/etc.

## Session of 2026-08-17 (continued): `GetAncestors(int)`/`GetAncestors(IContent)` — first increment using `[Obsolete]` instead of deletion

**User's explicit instruction, a deliberate deviation from every prior increment on this branch**: keep
`GetAncestors(int id)`/`GetAncestors(IContent content)`, mark them `[Obsolete]` (message format:
`"Use GetAncestorsAsync(...) instead. Scheduled for removal in Umbraco 21."`, since current version is
`19.0.0-beta1` per `version.json` and CLAUDE.md §5.4 schedules removal at current+2), add **paged** async
replacements, and migrate the one internal usage onto the new paged version. Don't generalize this to
future increments unless told — this is a one-off exception to the branch's usual "just delete it" rule
(per [[feedback_remove_dont_fix_v19_obsolete]]), made explicitly for these two members.

**What was built**:
- `IAsyncDocumentRepository.GetAncestorsAsync(Guid key, int skip, int take, CancellationToken)` — Document-tier
  placement (matches sync, which was never shared with Media). Implementation: resolve the node's own
  `NodeId`+`Path` via one `Nodes.Where(UniqueId == key)` lookup, parse `Path` into an ordered (root-first)
  `List<int>` of ancestor node ids excluding `Constants.System.Root` and the node's own id (mirrors
  `ContentExtensions.GetAncestorIds()`'s value-based exclusion, just with ints instead of strings), page
  that **id list itself** with `Skip`/`Take` (paging happens over the structural id list, not a SQL
  `ORDER BY`, since ancestor order isn't a sortable column), fetch matching `DocumentRow`s via the usual
  baseQuery join chain filtered by `pageNodeIds.Contains(node.NodeId)`, then **re-sort the fetched rows in
  memory** to match `pageNodeIds` order before `AssembleEntitiesAsync` (DB fetch order for a `Contains`
  filter isn't guaranteed). No `Ordering` parameter — deliberately simpler than `GetByLevelAsync`/
  `GetPagedOfContentTypesAsync`'s three-way ordering dispatch, since ancestor order is inherently
  structural, not content-column-sortable.
- `IContentService.GetAncestorsAsync(Guid key, ...)` and `GetAncestorsAsync(IContent content, ...)` — both
  directly on `IContentService`/`ContentService` (matches sync tier). The `IContent`-overload is a one-line
  forward to the `Guid`-overload (`GetAncestorsAsync(content.Key, ...)`) — a deliberate simplification
  versus the old sync design (which parsed `content.Path` in-memory to avoid a DB round trip); new cost is
  one trivial indexed `UniqueId` lookup, traded for a single source of truth (no duplicated Path-parsing
  logic across two service methods).
- The two obsolete sync methods were **rewritten to delegate to the new async ones** (per CLAUDE.md §5.2's
  "old method calls new method" pattern — `GetAncestors(int)` → `GetAncestorsAsync(key, 0, int.MaxValue,
  CancellationToken.None).GetAwaiter().GetResult().Items`), removing the old `_idKeyMap`/`GetAncestorIds()`
  logic entirely from `ContentService.cs`. No `#pragma warning disable CS0618` needed anywhere — the
  obsolete methods call *new, non-obsolete* methods, and the one former internal caller
  (`GetAncestors(int)` calling `GetAncestors(IContent)`) disappeared as part of the rewrite.
- Preserved current (undocumented) sync behavior deliberately: root-first order, self-excluded, **trashed
  ancestors NOT filtered** (unlike `GetByLevelAsync`'s explicit trash exclusion) — pinned with a dedicated
  regression test rather than left implicit.
- `ContentExtensions.GetAncestorIds()` left untouched (public, non-obsolete, per
  [[feedback_public_api_survives_even_with_zero_internal_callers]]) even though this change removes its
  only internal caller. `IMediaService`/`MediaService.GetAncestors` deliberately untouched (confirmed
  separate/unshared implementation, out of scope).
- Added 5 new tests to `AsyncDocumentRepositoryTest.cs`: root-first-order+self-exclusion, paging across 2
  pages, unknown key → empty/zero-total, content directly under root → empty/zero-total (its only
  "ancestor" is the excluded root), and trashed-ancestor-is-included (the explicit behavioral pin above).
  The one existing test (`ContentServiceTests.cs` `GetAncestors_Returns_Empty_List_When_Path_Is_Null`) was
  left unchanged — still valid, now exercising the obsolete method's delegation path.

**Verified**: full `dotnet build umbraco.sln -p:UmbracoBuild=true` — 0 errors. The only CS0618 warning
anywhere in the solution is the one expected pre-existing test call site — confirmed via full-solution
build output that no other internal caller was missed. `ContentServiceTests` 133/133 (no regressions).
`AsyncDocumentRepositoryTest` 141/141 (136 baseline + 5 new). Not yet committed as of this memory update.

**Current `IContentService` sync-member retirement tally** (mixed deletion/obsolete strategy now — check
each member's actual current state, don't assume): `GetById(Guid)`, `CountPublished`,
`GetContentSchedulesByKeys`, `GetContentScheduleByContentId`, `GetById(int)`, `GetByIds(IEnumerable<int>)`,
`GetByIds(IEnumerable<Guid>)`, `GetByLevel(int)`, `GetAncestors(int)` — all **deleted** outright.
`GetAncestors(Guid)`, `GetAncestors(IContent)` — **obsoleted, not deleted** (kept per explicit user
request to give consumers migration time; the only members handled this way). Still fully synchronous:
`Save`/`Publish`/`Unpublish`/`Move`/`Copy`/`Delete`/`Rollback`/`Sort`/etc.

## Session of 2026-08-18 (continued): `ContentService.GetPublishedChildren(int id)` — deleted as dead code, not migrated

Research (3 parallel Explore agents) found this one wasn't a real retirement candidate at all:
**not declared on any interface** (`IContentService`, `IContentServiceBase<TItem>`, and
`IPublishableContentService<TContent>` all lack it — it's a plain `public` method that exists only on
the concrete `ContentService` class, unreachable via the normal DI-injected `IContentService` type),
and **zero callers anywhere** in `src/` or `tests/`. `IMediaService`/`IElementService` have no
equivalent either — this was never generalized into the shared base, just left behind.

I initially planned to still add a proper async paged replacement (`GetPublishedChildrenAsync` on both
`IAsyncDocumentRepository` and `IContentService`, filling the interface gap this method never had) before
deleting the sync one. **User rejected that during plan review**: "If it's not used anywhere, and not
part of any interface, don't migrate it, delete it outright, it's probably a leftover that someone
forgot to delete." Deleted `ContentService.cs:417-430` outright — no `[Obsolete]`, no async sibling, no
repository-layer changes at all. This is a narrower case than every other item in this effort: when a
member has **zero interface presence** (not just zero callers), don't treat it as a retirement/migration
task — treat it as dead-code cleanup and just remove it. Verified: full solution build 0 errors, zero
remaining references to `GetPublishedChildren` anywhere; `ContentServiceTests` 134/134, no regressions
(no test ever exercised this method). Diff is a pure single-file deletion (15 lines,
`src/Umbraco.Core/Services/ContentService.cs` only). Not yet committed as of this memory update.

**Done (2026-08-18), corrected mid-session**: first pass added a `GetAncestors(int)` → `GetAncestors(Guid)`
→ `GetAncestorsAsync(Guid, ...)` three-step obsoletion chain (keeping `int` around one more hop). **User
explicitly rejected that** — "we don't want int id signatures anymore at all" — so `GetAncestors(int id)`
was instead **deleted outright** (interface declaration + implementation both removed, no obsolete
shim, no delegation), leaving `IContentService.GetAncestors(Guid key)` as a clean, brand-new
`[Obsolete("Use GetAncestorsAsync(Guid, int, int, CancellationToken) instead. Scheduled for removal in
Umbraco 21.")]` method (`GetAncestorsAsync(key, 0, int.MaxValue, CancellationToken.None)
.GetAwaiter().GetResult().Items`), with zero remaining int-based surface for ancestors lookups anywhere.
**Lesson for future obsoletion work on this branch**: when a member takes `int`, default to full removal
of the `int` signature (matching every other Tier-A retirement this session), not an obsolete
int→Guid stepping-stone — only keep an *existing* obsolete method in place (not introduce a new
obsolete-on-day-one one) when the user explicitly asks for a migration grace period, as they did for
`GetAncestors(IContent)` specifically. Also corrected: don't add `// Arrange`/`// Act`/`// Assert`
comments to new tests in this codebase (removed them from the one new test added this round;
left them alone on pre-existing neighboring tests, since that wasn't in scope). Added
`ContentServiceTests.GetAncestors_Guid_Returns_Ancestors_Of_Content` (uses the `Textpage`/`Subpage`
fixtures from `UmbracoIntegrationTestWithContent`) to cover the new overload directly. Verified: full
solution build 0 errors, only the two expected `CS0618` test warnings (`GetAncestors(IContent)` and
`GetAncestors(Guid)`, both intentionally-kept obsolete calls); `ContentServiceTests` 134/134 (133
baseline + 1 new), no regressions. Not yet committed as of this memory update.

## Session of 2026-08-18 (continued): `GetPagedChildren(int id, ...)` — deferred deletion; two new async methods added, root-content bug found and fixed

**Deliberately split into two increments** (user's explicit choice, presented via `AskUserQuestion`): this
increment adds `IContentService.GetChildrenAsync`/`GetChildrenWithoutTemplatesAsync` (both new — the
interface never had an async children method before) and migrates the 3 real production call sites that
always pass `filter: null`. **`GetPagedChildren(int id, ...)` itself is deliberately left untouched** — not
obsoleted, not deleted — because `ContentSearchServiceBase`/`ContentSearchService.SearchChildrenAsync`
(backing the backoffice Document Collection search box) is the one real caller that passes a genuine,
dynamically-built `IQuery<IContent>` filter (name-contains-or-key-equals), and the EF Core layer
deliberately never supports arbitrary predicates. Properly retiring that path needs a purpose-built
"search children by name/key" async repository method — **explicitly deferred to a future increment.**
~21 existing test call sites of the old method were **not touched** — since the method isn't being
removed, they don't need to change; only new tests for the two new methods were added.

**Design refinement from the user**: mirror the repository tier's existing split — two distinct service
methods (`GetChildrenAsync`/`GetChildrenWithoutTemplatesAsync`), not one method with a `loadTemplates`
bool parameter — matching how `IAsyncContentRepository<TEntity>.GetChildrenAsync` (shared tier) /
`IAsyncDocumentRepository.GetChildrenWithoutTemplatesAsync` (Document-only tier) already exist and were
simply wrapped, not reshaped. **No repository-layer work was needed this round** — both repo methods
already existed, fully implemented, from earlier work in this effort.

**Real bug found while migrating `ContentEditingService.cs`'s `GetPagedChildrenAsync` override** (backs
the content Sort feature's `LoadAllChildrenAsync` paging loop): naively resolving `int parentId` → `Guid`
via `_idKeyMap.GetKeyForIdAsync(parentId, UmbracoObjectTypes.Document)` and then calling the new
`GetChildrenAsync(Guid, ...)` breaks silently for **root-level content** (`parentId ==
Constants.System.Root`, i.e. `-1`). See [[feedback_root_content_has_no_guid_key]] — `Constants.System.RootKey`
is literally `null`; root has no Guid identity at all, so `_idKeyMap` resolution fails, and the bug doesn't
throw — it silently returns an empty page, making the Sort operation a silent no-op. Caught by
`ContentEditingServiceTests.SortByField.cs`'s `Can_Sort_Root_Content_By_Field`/`Can_Sort_Root_Content`
(7 failures, all and only the root-content test cases; one test case — CreateDate-ascending —
"accidentally" passed because doing nothing happens to preserve creation order, which coincidentally
matched that one expected sort order).

**First fix (superseded)**: special-cased `parentId == Constants.System.Root` in the
`ContentEditingService` override to fall back to the still-in-place sync `ContentService.GetPagedChildren(...)`
for that one case, using the new async method for every real (non-root) parent. Worked, but the user
correctly pushed for a cleaner design: **"the root key can simply be determined as if id = -1, key =
null"** — i.e. treat `null` as a first-class "root of the content tree" value, matching an established
codebase convention (`ContentCreateModel.ParentKey`/`Constants.System.RootKey` already use `null` =
root elsewhere).

**Final fix**: widened `GetChildrenAsync`/`GetChildrenWithoutTemplatesAsync` to take `Guid? parentKey`
(not `Guid`) at every layer — `IAsyncContentRepository<TEntity>.GetChildrenAsync` (shared tier),
`AsyncContentRepositoryBase<...>`'s abstract declaration, `IAsyncDocumentRepository.GetChildrenWithoutTemplatesAsync`,
both `AsyncDocumentRepository.cs` implementations + the shared private `GetChildrenCoreAsync`, and both
new `IContentService`/`ContentService` methods. In `GetChildrenCoreAsync`: `int parentNodeId =
parentKey.HasValue ? await ResolveNodeIdAsync(db, parentKey.Value, cancellationToken) :
Constants.System.Root;` — no `ResolveNodeIdAsync` call at all for the root case, since there's no node
row to resolve. Confirmed via grep that `AsyncDocumentRepository` is the **only** concrete implementer
of the abstract `GetChildrenAsync` (no `AsyncMediaRepository` yet), so the blast radius was fully
contained — and widening `Guid` → `Guid?` is source-compatible with every existing caller passing a real
(non-null) Guid, confirmed by the ~13 pre-existing repo-level tests in `AsyncDocumentRepositoryTest.cs`/
`AsyncDocumentBlueprintRepositoryTest.cs` needing zero changes. `ContentEditingService`'s override now
routes root through the *same* new async method (`parentKey = null` when `parentId ==
Constants.System.Root`) instead of falling back to the old sync method — no special-casing at the
repository boundary anymore, just at the point where an `int` first needs interpreting.

**EntityXmlSerializer.cs's two call sites were never affected by this bug** — both already hold a real
`IContent` object (`content`/`child`) with a genuine non-null `.Key`, never the synthetic root.

Verified (after the final fix): full solution build 0 errors/0 warnings. `ContentEditingServiceTests`
162/162. `EntityXmlSerializerTests` 6/6. `AsyncDocumentRepositoryTest` 142/142 (141 baseline + 1 new:
`GetChildrenAsync_WithNullParentKey_ReturnsRootContent` — note this fixture's root also includes a
second root document, `_publishedPage`, easy to miss and initially got the test wrong asserting
`Total == 1` before correcting to `2`). `AsyncDocumentBlueprintRepositoryTest` 3/3 (inherits the widened
signature, unaffected). `ContentServiceTests` 137/137 (136 baseline + 1 new:
`GetChildrenAsync_WithNullParentKey_Returns_Root_Content`, `UmbracoIntegrationTestWithContent`'s
`Textpage` is the only root content there). Diff: 10 files (`IAsyncContentRepository.cs`,
`IAsyncDocumentRepository.cs`, `ContentEditingService.cs`, `ContentService.cs`,
`EntityXmlSerializer.cs`, `IContentService.cs`, `AsyncContentRepositoryBase.cs`,
`AsyncDocumentRepository.cs`, `ContentServiceTests.cs`, `AsyncDocumentRepositoryTest.cs`).
`ContentEditingService` gained a new `IIdKeyMap idKeyMap` constructor parameter (plain breaking-change
addition, matching [[feedback_this_branch_allows_breaking_changes]] — `internal sealed`, registered via
plain `AddUnique<IContentEditingService, ContentEditingService>()`, no DI wiring changes needed). Not
yet committed as of this memory update.

**Next step, explicitly deferred**: build a purpose-built async "search children by name/key" repository
method and migrate `ContentSearchServiceBase`/`ContentSearchService` onto it, then finally retire
`GetPagedChildren(int id, ...)` itself. Not started — wait for explicit direction.

## Session of 2026-08-18 (continued): `AsyncContentEditingServiceWithSortingBase.GetPagedChildrenAsync(int, int, int, Ordering?, out long)` removed entirely

Small, well-contained cleanup: this `protected abstract` method (internal wiring only, no interface,
invisible outside the service-implementation layer) existed purely because C# disallows `out` parameters
on `async` methods — its `Task<IEnumerable<TContent>>` + `out long total` shape was just an unpacked
`PagedModel<TContent>`. Removed the `out` parameter, changed the return type to `Task<PagedModel<TContent>>`
directly. Research confirmed exactly one override (`ContentEditingService.cs`) and exactly one caller
(`LoadAllChildrenAsync<TResult>`'s paging loop) — `MediaEditingService` derives from a *different*,
fully-synchronous sibling base (`ContentEditingServiceWithSortingBase`, marked `// TODO EFCore: Remove
this once media and member type have been migrated`) with its own non-async `GetPagedChildren`, entirely
untouched by this change; no Member equivalent exists.

**Bonus outcome**: since the `out` parameter was the only reason this method couldn't be `async`,
`ContentEditingService`'s override is now genuinely `async` — both `.GetAwaiter().GetResult()` bridges
in it (one for `_idKeyMap.GetKeyForIdAsync`, one for `ContentService.GetChildrenAsync`) are gone, replaced
with real `await`. `LoadAllChildrenAsync<TResult>` in the base class updated mechanically to read
`page.Total`/`page.Items` instead of the `out`/return-value pair — identical behavior (same page size,
loop condition, capacity hint, projection).

Verified: full solution build 0 errors/0 warnings, zero remaining references to the old signature anywhere.
`ContentEditingServiceTests` 162/162 (unchanged — this is a pure internal reshape, no test touches the
method directly). `MediaEditingServiceTests` 31/31 (confirms the untouched sync sibling still works).
Diff: 2 files (`AsyncContentEditingServiceWithSortingBase.cs`, `ContentEditingService.cs`), fully
mechanical.

## Session of 2026-08-18 (continued): `GetPagedChildrenAsync`'s `int parentId` → `Guid? parentKey`

User pushed one step further: "we don't want it to be int parentid, we want guid parentkey instead."
Researched the full int-identifier chain in `AsyncContentEditingServiceWithSortingBase.cs` before
converting, since `HandleSortByFieldAsync` also calls a SECOND int-based abstract method,
`SortChildrenInBulk(int parentId, IReadOnlyList<int> orderedChildIds, int userId)`, which writes the new
order via the much older `IContentService.SortChildren(int, ...)` → NPoco `IDocumentRepository.UpdateSortOrder(IReadOnlyList<int>)`
path — genuinely bigger surface (would need a new Guid-based `SortChildren` on `IContentService`, even
though the EF Core repo already has `UpdateSortOrderAsync(IReadOnlyList<Guid>, CancellationToken)` sitting
unused for this purpose). **Presented this fork via `AskUserQuestion`; user chose "just the loading path"**
— convert `GetPagedChildrenAsync`/`LoadAllChildrenAsync`/`LoadOrderedChildIdsAsync` to `Guid? parentKey`,
leave `SortChildrenInBulk`'s int-based write path for a later, separate increment.

**What changed**: `GetPagedChildrenAsync(Guid? parentKey, ...)` (matches the `null` = root convention from
the prior increment). `HandleSortAsync`/`HandleSortByFieldAsync` no longer resolve their own `Guid?
parentKey` parameter down to an `int contentId` just to call the loading helpers — they pass `parentKey`
straight through now. `HandleSortByFieldAsync` still needs an `int` for the one remaining
`SortChildrenInBulk` call in its bulk-update branch, so it resolves `parent?.Id ?? Constants.System.Root`
right there, once, only for that purpose (comment explains why, referencing the deferred write-path
conversion). **Big simplification in `ContentEditingService.cs`**: its `GetPagedChildrenAsync` override
was doing an *int→Guid* round-trip via `_idKeyMap` specifically to call the already-Guid-based
`ContentService.GetChildrenAsync` — with the parameter now `Guid?` natively, that whole round-trip
disappears; the override collapsed from ~24 lines to a two-line expression-bodied method. Since
`_idKeyMap` became entirely unused as a result, removed it — field, constructor parameter, and XML doc —
reversing the addition from two increments ago now that it's dead code (plain `AddUnique<IContentEditingService,
ContentEditingService>()` DI registration, no factory to update).

Verified: full solution build 0 errors/0 warnings, zero remaining references to the old `int`-parameter
signature anywhere. `ContentEditingServiceTests` 162/162, `MediaEditingServiceTests` 31/31 (confirms
`MediaEditingService`'s separate, untouched sync sibling base — `ContentEditingServiceWithSortingBase`,
marked `// TODO EFCore: Remove this once media and member type have been migrated` — still works). Diff:
2 files (`AsyncContentEditingServiceWithSortingBase.cs`, `ContentEditingService.cs`). Not yet committed
as of this memory update.

**Next step, explicitly deferred (per the `AskUserQuestion` decision above)**: convert
`SortChildrenInBulk`/`IContentService.SortChildren`'s write path to Guid — would mean adding a Guid-based
async `SortChildren`-equivalent to `IContentService` that calls `_asyncDocumentRepository.UpdateSortOrderAsync(IReadOnlyList<Guid>,
CancellationToken)` directly instead of the old NPoco `IDocumentRepository.UpdateSortOrder(IReadOnlyList<int>)`,
plus rebuilding the audit/notification logic (`ContentService.cs:1671-1707`) in Guid terms.

## Session of 2026-08-18 (continued): `SortChildrenInBulk`/`IContentService.SortChildren` — done, this was the deferred write-path conversion

Completed the exact next step flagged above. `IContentService.SortChildren(int parentId, IReadOnlyList<int>
orderedChildIds, int userId)` deleted entirely (it was a default-interface-method stub marked
`// TODO (V19): Remove the default implementation.` — deletion resolves that TODO too, since there's no
member left to eventually make required). Replaced with `SortChildrenAsync(Guid? parentKey,
IReadOnlyList<Guid> orderedChildKeys, Guid userKey, CancellationToken)`, the **first genuine async write
method in `ContentService.cs`** this whole session (every prior async method was a read, or — like
`EmptyRecycleBinAsync` — a thin Guid→int shim delegating to a sync method). Persists via the
already-existing, already-tested `IAsyncContentRepository<TEntity>.UpdateSortOrderAsync(IReadOnlyList<Guid>,
CancellationToken)` (built earlier in this effort, previously unused for this purpose), then owns
notification-publishing (`ContentTreeChangeNotification` root/branch refresh, same shape as the old sync
method) and audit (`await AuditAsync(...)`, an already-async helper on the base class nobody had awaited
from an async caller yet) itself, per that repository method's explicit contract ("does not load the
entities or fire any notifications; callers are responsible").

**One int surface remains, deliberately**: `IAuditService.AddAsync` has no Guid overload, so the audit
call still needs an `int objectId` — resolved by reusing the `IContent? parent` already fetched for the
notification branch (`parent?.Id ?? Constants.System.Root`), avoiding a second lookup. Not a missed
conversion; a genuine, currently-irreducible boundary (would need a new `IAuditService` overload to close).

**Correction caught before implementing**: the plan assumed `ResolveKeys`/`TryGetParentKey` (the two
int→Guid helper methods `SortChildren` used) would become dead code and could be deleted alongside it —
checking actual usage first (`grep`) showed both are used by several *other*, unrelated `ContentService.cs`
methods (lines 247, 316, 593, 1090, 1151, 1399, 1662) — left both completely untouched. Lesson: always
verify a helper's usage is actually exclusive to the method being removed before deleting it, even when a
plan already asserts it is dead code.

**`AsyncContentEditingServiceWithSortingBase.cs` simplified further than the previous increment left it**:
`SortChildrenInBulk(int, IReadOnlyList<int>, int)` → `SortChildrenInBulkAsync(Guid?, IReadOnlyList<Guid>,
Guid)` (now genuinely awaitable — previously called as a *sync* abstract method from inside the already-
`async` `HandleSortByFieldAsync`). `LoadOrderedChildIdsAsync` (selected `child.Id`) →
`LoadOrderedChildKeysAsync` (selects `child.Key`). This also let `HandleSortByFieldAsync` drop the
`TContent? parent`/int-resolution scaffolding the *previous* increment had added solely to feed the old
`SortChildrenInBulk` — the NotFound check reverted to the simple `GetByIdAsync(...) is null` form
(matching `HandleSortAsync`'s shape), and `parentKey` now passes straight through with no resolution at
all. `ContentEditingService.SortChildrenInBulk` override updated to match, now genuinely `async` too
(`await ContentService.SortChildrenAsync(...)`) instead of calling a sync method.

`MediaEditingService`'s equivalent (`ContentEditingServiceWithSortingBase`, the separate sync sibling
base, `// TODO EFCore: Remove this once media and member type have been migrated`) confirmed untouched —
same isolation pattern as the read-side conversion. `IMediaService.SortChildren(int, ...)` still exists,
still used by `MediaEditingService`/`MediaServiceTests.cs` — Media is not part of this migration.

Migrated the two direct test callers in `ContentServiceTests.cs`
(`Sort_Preserves_Template_And_Property_Data_When_Items_Loaded_Without_Them` — converted from `public void`
to `public async Task`; `SortChildren_Persists_The_Supplied_Order` — tracked `childKeys` alongside
`childIds` since the test's `ChildIdsInSortOrder()` helper still reads back via the untouched, int-based
`GetPagedChildren`). `MediaServiceTests.cs`'s two `SortChildren` calls untouched (separate `IMediaService`
method).

Verified: full solution build 0 errors/0 warnings, zero remaining `SortChildren`/`ResolveKeys`/
`TryGetParentKey`-related warnings anywhere (production code compiled clean on the first full-solution
build — only the two already-known test call sites needed migration). `ContentServiceTests` 137/137
(includes both migrated tests). `ContentEditingServiceTests` 162/162. `MediaEditingServiceTests` 31/31.
Combined `SortChildren`-name filter across the whole test assembly: 32/32. Diff: 5 files
(`AsyncContentEditingServiceWithSortingBase.cs`, `ContentEditingService.cs`, `ContentService.cs`,
`IContentService.cs`, `ContentServiceTests.cs`). Not yet committed as of this memory update.

**This closes out the `GetPagedChildren`→`GetChildren(WithoutTemplates)Async` / `SortChildren`→
`SortChildrenAsync` pair of increments** — the content-editing Sort feature (`SortAsync`/`SortByFieldAsync`)
is now fully Guid-native end-to-end on the Content side, with zero remaining `int` parameters anywhere in
`AsyncContentEditingServiceWithSortingBase.cs`'s public/protected surface. The only remaining int-based
surface adjacent to this area is `GetPagedChildren(int, ...)` itself (still needed by
`ContentSearchService`'s search-filter use case — see the still-deferred "search children by name/key"
increment noted earlier in this file).

## Sessions of 2026-08-19 through 2026-08-24 (this machine): 15 more increments, all committed

Continued the same one-member-at-a-time pattern, no architectural changes. In commit order (all on
`v18/feature/ef-core-document-repository`, all pushed): `GetPagedChildren`→`GetChildrenAsync` (`ebc3563a2a4`),
`GetPagedDescendants`→`GetDescendantsAsync` (`41d25f73a88`), `GetParent(int)`→`GetParentAsync(Guid)`
(`72ce305df94`), `GetRootContent()`→`GetRootContentAsync` (`f3826a372ac`), `GetPagedContentInRecycleBin`→
async (`e8deed67bcb`), `IsPathPublishable`/`IsPathPublished`→async (`446aed1872a`), `GetPagedOfType`→async
(`0b17064608f`), `GetPagedOfTypes`→async (`ebedf16f115`), `GetVersion(int)`/`GetVersions(int)`→async
(`523a92cc10e`), `GetVersionsSlim`→async (`fdd900266d1`), `GetVersionIds`→async, replacing an unused
maxRows-based `GetVersionKeysAsync` rather than adding alongside it (`c253d83941c`),
`GetContentForExpiration`/`GetContentForRelease`→async, bundled with a real fix (unbatched `WHERE...IN`
in the schedule query, now uses `InGroupsOf(Constants.Sql.MaxParameterCount)`) (`75a60f44e35`),
`GetParent(IContent)`→async overload alongside the already-existing `GetParentAsync(Guid)` (`942f446895b`),
`CountChildren`/`HasChildren`→async, plus deleting a zero-caller internal `ContentExtensions.HasChildren`
helper (`d32fa8944ae`), `Count`→`CountAsync` (`8c06ad5faa2`), `CountDescendants`→`CountDescendantsAsync`
(`151aa133f95`).

**Confirmed/reused conventions across all 15**: async member placed on whichever tier (`IContentService`
direct vs. shared `IPublishableContentService<TContent>`) the sync original lived on — every one of these
turned out to be `IContentService`-direct (none forced Element/Media impact). Sync method deleted outright
when zero internal callers existed; when an internal self-call existed (`GetParent`'s `StrategyCanPublish`,
`CountChildren`'s `CommitContentChangesInternal`), bridged that one call site via
`.GetAwaiter().GetResult()` rather than keeping a parallel sync copy — see [[feedback_no_int_stepping_stone_when_obsoleting]]-adjacent
pattern, now applied consistently ~4 times. Independent review agent spawned after every single increment
regardless of size (explicit standing instruction, see [[feedback_parallel_review_and_test_agents]]) — one
review agent hit a session spend-limit mid-run and was simply re-launched once the limit reset, no other
consequence. `IPublishedContentStatusFilteringService`/`ContentExtensions.GetAnchorValuesFromRTEs`-style
"don't delete public zero-internal-caller code" caution did NOT recur in this batch — no near-misses.

**Also produced (2026-08-24)**: an HTML status-dashboard artifact summarizing the whole `IContentService`
retirement tally (32 async members done / 47 still sync / 41%, broken down by category: Reads & counts
~done, Publishing workflow/Move-copy-sort partial, Save&Delete/Create/Blueprints/Permissions not started),
published for the user, not committed to the repo (lives only as a claude.ai artifact).

## 2026-08-25/26: cross-machine handover received (home-machine session did 4 more increments)

The user did more work on a **different machine** ("at home") using a separate Claude Code session with
its own local memory (not this one) — that session did NOT have access to this file. Before ending, it
wrote `/home/mole/github/V18/Umbraco-CMS/HANDOVER.md` (committed as part of its work) as a **self-contained,
one-time-read condensation** meant for whichever session picks the branch back up next — explicitly
described in the file itself as following the same pattern as two earlier such handover commits
(`72c215be6fd`, `af4ae9291be`), both of which were later `git reset --hard`+force-pushed away once
absorbed (see the 2026-08-10 entry above, "Two temporary handover commits..."). **Do the same here once
this memory update is saved and the user confirms — but only with explicit user confirmation first, since
it requires a history rewrite + force-push on a branch already pushed to `origin`.** Don't do it
proactively/silently.

**4 more increments landed on the home machine, on top of `151aa133f95`** (all committed + pushed, HEAD
now `3d22e09753d`): `DeleteVersions`/`DeleteVersion`→async equivalents (`cc6963b43b7`); a **cross-cutting
follow-up** migrating the shared `DeletedVersionsNotificationBase<T>`/`DeletingVersionsNotification<T>`/
`DeletedVersionsNotification<T>` notification family (used by Content, Media, AND Element — not
Content-only) from `int Id` to `Guid Key` identity, touching 4 producers and 3 webhook consumers
(`4181254634d`); `RecycleBinSmells`→`RecycleBinSmellsAsync`, initially composed from
`CountChildrenAsync(RecycleBinContentKey,...) > 0` then corrected on review to call the already-existing,
purpose-built, zero-caller `IAsyncDocumentRepository.RecycleBinSmellsAsync` instead — see convention #5
below (`c9ae782dfba`); `GetBlueprintById(int)`/`GetBlueprintById(Guid)`→`GetBlueprintByIdAsync`
(`07ea7677077`) — this one briefly grew an obsolete-constructor-forwarding chain on `ContentService` before
being corrected back down to a single constructor (see convention #2 below); then `HANDOVER.md` itself
(`3d22e09753d`).

**New/refined standing conventions from the handover, folded in going forward** (supplementing, not
replacing, everything already captured above in this file):

1. **Constructor changes are ALSO breaking on this branch, not just interface members.** When a class
   needs a new constructor dependency, add the parameter directly to the existing constructor — no
   `[Obsolete]` old-constructor overload, no `StaticServiceProvider.Instance.GetRequiredService<T>()`
   forwarding. Real mistake made+corrected on `ContentService`/`GetBlueprintByIdAsync`. Known,
   deliberately-left inconsistency: `MediaService` gained `IIdKeyMap` via the obsolete-forwarding pattern
   in the notification Id→Key migration (before this rule was stated this explicitly) — don't proactively
   "fix" it, only simplify if touching that constructor for another reason anyway.
2. **Before composing a new async service method from generic `AsyncPublishableContentServiceBase<TContent>`
   pieces, grep `IAsyncDocumentRepository`/`IAsyncDocumentBlueprintRepository` for an already-built,
   Document-specific, purpose-built method first.** Caught on `RecycleBinSmellsAsync` (see above) — the
   composed version worked but did an avoidable extra Guid→id lookup plus a full `COUNT` instead of
   `EXISTS`; the real, already-tested method just had zero callers yet.
3. **Independent review agent should NOT be asked to re-run `dotnet build`/`dotnet test`** if concrete
   pass/fail counts were already run and reported in the conversation for that same increment — redundant.
   (This session's own reviews have sometimes had the agent re-verify tests anyway; not wrong, just
   not required — align with this going forward when the user/prior turns already reported a result.)
4. **`git status --short` before every commit; investigate any unexplained diff rather than reverting it
   silently twice in a row** — a stray one-character whitespace diff turned up unexplained in
   `ContentService.cs` mid-session on the home machine; fixed, not ignored.

**3 known, deliberately-unfixed bugs, tracked for their own future increments** (from `HANDOVER.md`,
confirmed pre-existing / not caused by this campaign):
1. `AsyncDocumentRepository.GetChildrenCoreAsync` throws on `ordering: null` despite a nullable `Ordering?`
   signature (`ArgumentNullException.ThrowIfNull(ordering)`) — repro:
   `AsyncDocumentBlueprintRepositoryTest.GetChildrenAsync_OnPlainDocumentRepository_DoesNotReturnBlueprints`.
   Needs a default (e.g. `Ordering.None`) when null.
2. `PublishableContentServiceBase<TContent>.DeleteVersions`/`DeleteVersion` hardcode the Content-flavored
   notification types even when `TContent` is `IElement` — since `ContentService` moved off this base long
   ago, `ElementService` is the only remaining consumer, so `ElementDeletedVersionsWebhookEvent` never
   fires for real Element version deletions today. User explicitly said leave alone, log for its own
   increment — do NOT opportunistically fix this if touching these lines for an unrelated reason.
3. `AsyncEntityRepositoryBase.GetAsync(TKey?, CancellationToken)` accepts a token but never forwards it into
   `CachePolicy.GetAsync`/`PerformGetAsync`/`PerformGetAllAsync` — inert past the service tier for every
   entity type's async `GetAsync` (Document, Media, Element, blueprints — base-class issue, not
   blueprint-specific). Worth a narrow, dedicated fix touching a broadly-inherited base class; verify
   across entity types before calling it done.

**Working tree state as of this update**: clean except the 3 known `package-lock.json` noise files already
tracked throughout this file, PLUS (per the handover, not yet seen dirty on this machine but expected to
recur) two generated localization `.ts` files under
`src/Umbraco.Web.UI.Client/src/libs/localization-api/` — also pre-existing noise, not part of this
campaign, leave alone unless the user asks.

**Current full retirement tally** (36 `IContentService` sync members fully retired as of `3d22e09753d`,
chronological): `GetById(Guid)`, `CountPublished`, `GetContentSchedulesByKeys`, `GetContentScheduleByContentId`,
`GetById(int)`, `GetByIds(IEnumerable<int>)`, `GetByLevel(int)`, `GetPagedChildren`, `SortChildren(int,...)`,
`GetPagedDescendants`, `GetParent(int)`, `GetRootContent()`, `GetPagedContentInRecycleBin`,
`IsPathPublishable`/`IsPathPublished`, `GetPagedOfType`, `GetPagedOfTypes`, `GetVersion(int)`/`GetVersions(int)`,
`GetVersionsSlim`, `GetVersionIds`, `GetContentForExpiration`/`GetContentForRelease`, `GetParent(IContent)`,
`CountChildren`/`HasChildren`, `Count`, `CountDescendants`, `DeleteVersions`/`DeleteVersion`,
`RecycleBinSmells`, `GetBlueprintById(int)`/`GetBlueprintById(Guid)`. (`GetAncestors(int)`/`GetAncestors(IContent)`
are `[Obsolete]`-marked with async replacements already added, not yet a full deletion — see the
2026-08-17 entry above for why that pair is a deliberate exception.)

**What's genuinely left** (re-derived from a direct read of current `IContentService.cs` — see the
published progress-dashboard artifact above for the fuller categorized breakdown): `Save`(×2)/`Delete`/
`DeleteOfType`, `Move`(×2)/`Copy`(×2)/`MoveToRecycleBin`, `EmptyRecycleBin` (the existing
`EmptyRecycleBinAsync(Guid)` is currently backwards — just resolves the int userId and calls sync
`EmptyRecycleBin` underneath; the real logic runs through `DeleteLocked`, a deep sync/recursive/
notification-heavy path shared with the not-yet-retired `Delete` — NOT a quick win, deliberately deferred,
~13 test call sites whenever tackled), `Sort`(×2) (note: `SortChildrenAsync` already exists but is
deliberately different behavior — no per-item notifications, doesn't load children — not a drop-in
replacement), `PublishBranch`/`SendToPublication`/`Publish`, `SetPermissions`/`SetPermission`/
`GetPermissions`, `Create`(×4)/`CreateAndSave`(×2), rest of Blueprints (`GetBlueprintsForContentTypes`,
`SaveBlueprint`, `MoveBlueprint`/`CreateBlueprintFromContent` — both currently stub
`NotImplementedException` default impls, `DeleteBlueprint`, `DeleteBlueprintsOfType(s)`), and the shared
`CheckDataIntegrity` on `IContentServiceBase`. **No concrete plan exists for this remaining block** — keep
picking one small, well-isolated candidate at a time right before implementing it, per the standing
"one increment at a time" rule; don't pre-plan the whole remainder.

## 2026-08-26/27/28: `SaveAsync` added with the Attempt-pattern; then a large home-machine burst retired
9 more members (`Save`, `CreateAndSave`, `SendToPublication`, `GetAncestors`, `EmptyRecycleBin`, `Delete`,
`DeleteBlueprint`, `MoveBlueprint`, `CreateBlueprintFromContent`)

**New standing convention established this stretch**: newly-added async `IContentService` members return
`Task<Attempt<TStatus>>` (or `Task<Attempt<TResult, TStatus>>`) with a small bespoke `*OperationStatus`
enum per member-family (e.g. `ContentSaveOperationStatus`, `ContentDeleteOperationStatus`,
`ContentBlueprintOperationStatus`, `ContentEmptyRecycleBinOperationStatus`,
`ContentSendToPublicationOperationStatus`, `ContentSortChildrenOperationStatus`, all in
`src/Umbraco.Core/Services/OperationStatus/`) — validation failures return a typed `Attempt.Fail(...)`
instead of throwing. This matches the dominant convention already used by every other EF-Core-migrated
domain service (Language/Domain/DictionaryItem/DataType/Webhook/RelationType/ContentType) and is now the
required shape for every future member in this campaign — do not go back to `OperationResult`/thrown
exceptions for new async members. A follow-up commit (`180969ef591`, "use the Attempt<TStatus> pattern
consistently across IContentService's async surface") swept back over already-landed members to fix any
that had drifted from this shape.

**`Save` retirement (`047d793f328`) was the single largest increment of the whole campaign so far**:
~1095 call sites across ~145 files (5 production, ~1090 test/TestData). Executed as: 5 production call
sites fixed directly (including converting `ImageCropperPropertyEditor.Handle(ContentCopiedNotification)`
from `INotificationHandler<T>` to `INotificationAsyncHandler<T>`, with its DI registration switched to
`AddNotificationAsyncHandler`, and bridging `FileUploadContentCopiedOrScaffoldedNotificationHandler`'s
one call site via `.GetAwaiter().GetResult()` since its shared sync helper is also used by an unrelated
sibling handler), then ~16 parallel subagent batches for the test tree (mechanical rule: swap
`X.Save(item, ...)` → `await X.SaveAsync(item, ..., CancellationToken.None)`, converting any sync `void`
test method/lambda to `async Task`/`async (...) =>` as needed; leave the bulk `Save(IEnumerable<...>,
int)` overload and all NPoco/EF-Core repository-level `.Save(...)` calls untouched — those are separate,
unaffected methods). **Lesson for next time a batch this large is attempted**: several subagents got
killed/hit spend limits mid-file-list due to a session interruption; two had defensively `git stash`'d
their in-progress work rather than losing it — recovered cleanly via `git stash list` + `git stash apply`
(oldest-to-newest, checking `git stash show --stat` first) + `git stash drop`, no data lost. After
recovery, a second, smaller wave of cleanup subagents (re-grep the whole tree for remaining
`ContentService.Save(`/`ElementService.Save(` hits, cross-check each against the bulk-overload/array
pattern before assuming it's unmigrated) closed out the stragglers. Full solution build confirmed 0
errors before considering the increment done.

**Current full retirement tally** (45 `IContentService` sync members fully retired as of `180969ef591`,
in chronological order, extending the prior 36-member list above): `SetPermissions`/`SetPermission`
retired earlier still leaves `GetPermissions` sync-only (unretired, low priority); then this stretch:
`Save`(single-item; bulk `Save(IEnumerable<...>, int)` deliberately NOT retired, no async equivalent
exists), `CreateAndSave`(×2, redirect to `CreateAndSaveAsync`), `SendToPublication`, `GetAncestors`(×2,
was `[Obsolete]`-only since 2026-08-17, now a full deletion), `EmptyRecycleBin` (real async cascading
delete this time, not the old backwards int-resolve-then-call-sync shim), `Delete`, `DeleteBlueprint`,
`MoveBlueprint`, `CreateBlueprintFromContent`.

**Verified this session (2026-08-28, resuming after the above landed on a different machine)**: full
solution build 0 errors; `ContentServiceTests`+`AsyncDocumentRepositoryTest`+`ContentEventsTests`+
`ContentServiceNotificationTests`+`ContentEditingServiceTests` — 572/572 passing. All work already
pushed to `origin/v18/feature/ef-core-document-repository` (0 commits ahead locally) — nothing pending
to push. Working tree clean except the 3 long-standing `package-lock.json` noise files.

**What's genuinely left now** (re-derive exact list from `IContentService.cs` at next increment's start,
don't trust this as gospel by then): `DeleteOfType`, `Move`(×2)/`Copy`(×2)/`MoveToRecycleBin`, `Sort`(×2),
`PublishBranch`/`Publish`, `GetPermissions` (read-only, `SetPermissions`/`SetPermission` already retired
separately), `Create`(×4), rest of Blueprints (`GetBlueprintsForContentTypes`, `SaveBlueprint`,
`DeleteBlueprintsOfType(s)`), shared `CheckDataIntegrity`. Still no concrete plan for this remainder —
keep picking one small increment at a time.

## Session of 2026-08-28 (continued): Blueprints category fully retired (8/8) — `SaveBlueprint`, `GetBlueprintsForContentTypes`, `DeleteBlueprintsOfType(s)`

Three more increments landed, closing out the entire "Blueprints" category. Commits (in order):
`943fd6e295f` (`SaveBlueprint`), `ad859c8ff58`+`e2c8d12bec1` (`GetBlueprintsForContentTypes`), `7f13291701f`
(`DeleteBlueprintsOfType(s)`).

**`SaveBlueprint`**: retired the real method plus its already-`[Obsolete]` 2-param forwarding shim (zero
callers). New `SaveBlueprintAsync(IContent, IContent? createdFromContent, Guid userKey,
CancellationToken)` follows the established `Attempt<ContentBlueprintOperationStatus>` shape.

**`GetBlueprintsForContentTypes`**: new `GetBlueprintsForContentTypesAsync` branches empty-array (→
repository `GetAllAsync`, matches old "no filter = all blueprints" semantics) vs. non-empty (→
`GetPagedOfContentTypesAsync` with `take = int.MaxValue`) — no `IIdKeyMap` needed, every caller already had
`IContentType`/`ISimpleContentType`/`IContent.ContentType.Key` directly available.

**`DeleteBlueprintsOfType`/`DeleteBlueprintsOfTypes`**: collapsed into two new methods —
`DeleteBlueprintsOfTypeAsync(Guid contentTypeKey, Guid userKey, CancellationToken)` (singular convenience,
one-line forward) and `DeleteBlueprintsOfTypesAsync(IEnumerable<Guid> contentTypeKeys, Guid userKey,
CancellationToken)` (main implementation, same empty-means-all branching as `GetBlueprintsForContentTypesAsync`).
Sole production caller: `ContentTypeService.DeleteItemsOfTypesAsync(IEnumerable<int> typeIds)`.

**Two real design decisions worth remembering for future increments in this same family**:
1. **`params` was tried, then explicitly reverted in favor of separate singular+`IEnumerable<Guid>`
   overloads** once both existed — see [[feedback_cancellationtoken_first_with_params]] for the
   CancellationToken-position rule this produced along the way (still applies to any *other* method that
   legitimately uses `params`, e.g. `GetBlueprintsForContentTypesAsync` kept `params Guid[]` since it has no
   separate singular overload). Once a method has both a singular and a plural overload, prefer that pair
   over a single `params`-based method — matches the original sync API's shape (`DeleteBlueprintsOfType`
   int + `DeleteBlueprintsOfTypes` `IEnumerable<int>`) more closely and reads better at call sites.
2. **`AsyncContentTypeServiceBase<TRepository,TItem>.DeleteItemsOfTypesAsync(IEnumerable<int> typeIds)`
   deliberately KEPT its int-only signature** rather than gaining a parallel `IEnumerable<Guid> typeKeys`
   parameter (an earlier draft did this, then was reverted per explicit user instruction) — marked with
   `// TODO (V19): Change to take Guid keys instead of int IDs, once IIdKeyMap resolution can be removed
   from callers.` Instead, `ContentTypeService.cs`'s override resolves `int typeId` → `Guid` locally via a
   newly-injected `IIdKeyMap` (`GetKeyForIdAsync(typeId, UmbracoObjectTypes.DocumentType)`, silently
   skipping failed lookups — the same idiom as `ContentService.ResolveKeys`/`EntityService.GetPathKeys`).
   **Load-bearing safety guard**: since an empty `Guid[]`/`IEnumerable<Guid>` means "delete ALL blueprints"
   in `DeleteBlueprintsOfTypesAsync`, the override only calls it when `typeKeys.Count > 0` — otherwise a
   total `IIdKeyMap` resolution failure would silently escalate from "delete these specific types'
   blueprints" to "delete every blueprint in the system." A review agent caught this as a real bug before
   it shipped — worth the same scrutiny anywhere else int→Guid resolution feeds an "empty means everything"
   method.

**Current full retirement tally**: Blueprints category now 8/8 — `GetBlueprintByIdAsync`,
`GetBlueprintsForContentTypesAsync`, `SaveBlueprintAsync`, `MoveBlueprintAsync`, `DeleteBlueprintAsync`,
`CreateBlueprintFromContentAsync`, `DeleteBlueprintsOfTypeAsync`, `DeleteBlueprintsOfTypesAsync` — all
async, all landed. **Correction to an earlier note in this file**: `GetPermissions` is NOT still sync —
re-derivation from `IContentService.cs` at the start of the `Create` increment (below) found
`GetPermissionsAsync(Guid, CancellationToken)` already fully async, no sync counterpart remaining
(`SetPermissions`/`SetPermission` were retired separately, as already noted; `GetPermissions` must have
been retired in the same earlier burst without this file being updated to say so). **Lesson**: always
re-derive the remaining-members list from `IContentService.cs` directly rather than trusting a prior
session's tally — this file has been wrong about at least one member's status before.

**Verified**: full solution build 0 errors (both incremental and `--no-incremental`), targeted
`ContentServiceTests`+`ContentTypeServiceTests` 236/236 passing, independent review agent runs clean after
each sub-round (including the guard fix above). All three increments committed to
`v18/feature/ef-core-document-repository`; push state not re-checked this session — verify with `git log
--oneline origin/v18/feature/ef-core-document-repository..HEAD` before assuming pushed.

## Session of 2026-08-28 (continued further): `Create` (4 sync overloads) retired, 3 async equivalents added

Next increment after Blueprints: the plain (non-persisting, no-save) `Create` family — `IContent
Create(...)`, declared directly on `IContentService` (never shared with `IElementService`/any base class,
confirmed via grep, same placement as the already-migrated sibling `CreateAndSaveAsync`).

**Design**: collapsed 4 sync overloads (2 real implementations + 2 thin int/alias-resolving forwarders)
into 3 async ones, matching `CreateAndSaveAsync`'s established `Guid?`-parentKey-null-means-root
convention:
- `CreateAsync(string name, Guid? parentKey, string contentTypeAlias, Guid userKey, CancellationToken)`
- `CreateAsync(string name, Guid? parentKey, IContentType contentType, Guid userKey, CancellationToken)` —
  kept as a distinct overload (not collapsed into the alias-based one) specifically to avoid a redundant
  content-type repository lookup for the 5 real `ContentServiceVariantTests.cs` call sites that already
  held an `IContentType` object in hand.
- `CreateAsync(string name, IContent parent, string contentTypeAlias, Guid userKey, CancellationToken)`

Reused the existing `GetContentTypeAsync(ICoreScope scope, string alias, CancellationToken)` helper
(`AsyncPublishableContentServiceBase.cs`, previously only consumed by `CreateBlueprintFromContentAsync`) —
its scope-reuse, no-extra-explicit-lock shape (relies on the helper's own internal `ReadLock`) is the
template all 3 new methods copy exactly.

**Gotcha hit and fixed**: passing a bare `null` literal as the second argument is ambiguous between the
`Guid? parentKey` overload and the `IContent parent` overload (both accept `null` equally well) — every
root-sentinel test call site needed an explicit `(Guid?)null` cast, not bare `null`. ~27 call sites needed
this cast; a batch regex pass over "CreateAsync(<expr>, null," missed one case where the first argument
itself contained a comma (`new string('a', 256)`), caught by the next build pass and fixed individually —
worth double-checking batch regex substitutions against arguments containing embedded commas.

**`ContentServiceExtensions.CreateContent`** (public, non-obsolete extension, kept per
[[feedback_public_api_survives_even_with_zero_internal_callers]]): resolves `Guid userKey` from its own
`int userId` parameter via `StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>().GetAsync(userId)`,
mirroring the same file's existing `IIdKeyMap`-via-`StaticServiceProvider` bridge (used by
`GetAnchorValuesFromRTEs` for Document-key resolution) — same pattern, different interface.

**`tests/Umbraco.TestData/LoadTestController.cs`**: added a companion static `Guid _containerKey` field
alongside the existing static `int _containerId` (both set together in the same `lock (_locko)` block from
the same `container` object) — needed because that field is cached across requests, not just a
local-scope variable with `.Key` available at each call site like almost every other test/tooling caller
in this increment.

~35 call sites total across `ContentServiceTests.cs`, `ContentServiceVariantTests.cs`,
`ContentTypeServiceTests.cs`, `ContentTypeEditingServiceTests.Update.cs`, `ThreadSafetyServiceTest.cs`
(raw-thread lambda bodies, bridged with `.GetAwaiter().GetResult()`, matching that file's existing
`SaveAsync` bridging pattern), and `tests/Umbraco.TestData/{LoadTestController,UmbracoTestDataController}.cs`
(part of `umbraco.sln`, would have surfaced as build errors if missed — didn't, all caught by the full
solution build). All handled directly (no subagent swarm — well within single-session scope).

**User gave explicit standing authorization for this specific increment** to skip the usual "wait for
explicit commit instruction" convention: "use auto mode... when done, don't ask permission, just commit
and push." Committed as `4e6bab4504c` and pushed to `origin/v18/feature/ef-core-document-repository`
(`7f13291701f..4e6bab4504c`) immediately after a clean independent review, without a separate
user-confirmation turn. **This was a one-time authorization for this task, not a new standing rule** — the
default "wait for explicit commit instruction" convention resumes for future increments unless the user
grants it again.

**Verified**: full solution build 0 errors (both incremental and `--no-incremental`), targeted
`ContentServiceTests`+`ContentServiceVariantTests`+`ContentTypeServiceTests`+`ContentTypeEditingServiceTests`
439/439 passing, `ThreadSafetyServiceTest` 2/2 passing, independent review agent clean (no issues found).
Pushed — confirmed via the push output itself this time, not just a local commit.

**Current full retirement tally**: `IContentService`'s `Create` family fully async (3 overloads). What's
genuinely left (re-derive from `IContentService.cs` at next increment's start, this file has been wrong
before): `DeleteOfType`, `Move`(×2)/`Copy`(×2)/`MoveToRecycleBin`, `Sort`(×2), `PublishBranch`/`Publish`,
shared `CheckDataIntegrity`. `Save(IEnumerable<IContent>, int)` (bulk) deliberately left alone — no async
equivalent exists yet, not the same as the already-retired single-item `Save`. Still no concrete plan for
this remainder — keep picking one small increment at a time.

## Session on a home machine (commits dated 2026-08-28 through 2026-09-11): 4 more commits landed, not written up by that session — reconstructed here from `git show`

The user did further work on this campaign at home (author `nikolajlauridsen`, same person under a
different git identity — not flagged as a discrepancy, just noting the byline differs from the `mole`
commits above). 4 commits landed on `v18/feature/ef-core-document-repository` after `4e6bab4504c`, already
pushed to `origin` (confirmed: local `HEAD` == `origin/v18/feature/ef-core-document-repository`, both at
`9d507048136`, no divergence). No accompanying memory updates existed for this work — this section was
written by re-deriving intent from `git show` on each commit, not from a live planning session, so treat it
as a faithful summary rather than a first-hand account of the design discussion.

### `ec287f9c41f` — "Fix duplicate code" (dated 2026-08-28, i.e. same day as the `Create` increment)
Small self-cleanup of the `Create` increment above: `CreateAsync(string, Guid?, string, Guid,
CancellationToken)` (the alias-taking overload) had its own parent-resolution logic duplicated from the
`IContentType`-taking sibling overload; this commit deletes the duplication and has the alias overload
resolve the content type then delegate straight into `CreateAsync(name, parentKey, contentType, userKey,
cancellationToken)`. 15 lines removed, 1 added. No behavior change, no test changes.

### `328c205c1a8` — `IContentService.Sort` retired in favor of `SortAsync`
Both sync overloads (`Sort(IEnumerable<IContent> items, int userId)` and `Sort(IEnumerable<int>? ids, int
userId)`) collapsed into one: `Task<Attempt<ContentSortOperationStatus>> SortAsync(IReadOnlyList<Guid>
orderedKeys, Guid userKey, CancellationToken cancellationToken)`. New
`ContentSortOperationStatus` enum: `Success`/`NoOperation`/`CancelledByNotification`.

Faithfully preserves the original's full behavior rather than delegating to the already-existing, lighter
`SortChildrenAsync` (which just persists order directly with no per-item notifications): reloads all items
within the write lock via `GetByIdsAsync` (guards against partially-loaded callers wiping template/property
data on save, per the `#23120` comment already in the code), fires cancelable `ContentSortingNotification`
then `ContentSavingNotification`, only actually saves/audits items whose sort position changed, publishes
`ContentTreeChangeNotification`/`ContentPublishedNotification` afterward. `IContentService.cs`'s doc comment
on `SortChildrenAsync` was updated to point at `SortAsync` (was pointing at the now-deleted sync `Sort`)
distinguishing the two: `SortAsync` reloads + fires per-item notifications; `SortChildrenAsync` persists the
order directly with no per-item notifications.

`AsyncContentEditingServiceWithSortingBase`'s abstract `protected ContentEditingOperationStatus
Sort(IEnumerable<TContent> items, int userId)` hook was itself converted to `protected abstract
Task<ContentEditingOperationStatus> SortAsync(IReadOnlyList<Guid> orderedKeys, Guid userKey,
CancellationToken cancellationToken)` — its 2 call sites now pass `.Select(child => child.Key)` instead of
the `TContent` items themselves, and no longer need to resolve `userId` first (the async override does its
own resolution). `ContentEditingService`'s override bridges the new `Attempt<ContentSortOperationStatus>`
back into the same `OperationResult`-based status-translation shape `ContentEditingService` uses everywhere
else (`OperationResultToOperationStatus`), mapping `Success`→`Succeed`, `NoOperation`→a `NoOperation`
`OperationResult`, anything else→`Cancel`.

Test call sites (`ContentServiceTests.cs`, `ContentEventsTests.cs`) all switched to pass `.Key` arrays and
`await`, `Constants.Security.SuperUserKey` in place of the old default `userId`.

### `acde4529037` — `IPublishableContentService.PersistContentSchedule` retired in favor of `PersistContentScheduleAsync`
`void PersistContentSchedule(IPublishableContentBase content, ContentScheduleCollection contentSchedule)`
removed from the shared `IPublishableContentService<TContent>` interface (replaced with a 3-line comment
pointing at the async replacement, no XML doc left dangling). New `Task PersistContentScheduleAsync(...,
CancellationToken)` added to `IAsyncPublishableContentService<TContent>`.

**Establishes the pattern this whole "shared with Element" category now follows** (see Rollback below for
the same shape repeated): `AsyncPublishableContentServiceBase.PersistContentScheduleAsync` is a real async
implementation for Document, calling the already-wired `_asyncContentRepository.PersistContentScheduleAsync`.
`PublishableContentServiceBase`'s old sync `PersistContentSchedule` method body is **kept, unchanged**, but
its doc comment is replaced with an explanatory comment: it's no longer an interface member, kept only
because `ElementService.PersistContentScheduleAsync` bridges to it (`PersistContentSchedule(...); return
Task.CompletedTask;`) until Element gets its own async repository. `ContentPublishingServiceBase`'s 2 call
sites (both already inside `async` methods) switched to `await ...PersistContentScheduleAsync(...,
CancellationToken.None)`.

### `9d507048136` — `IPublishableContentService.Rollback` retired in favor of `RollbackAsync`
Same shape as `PersistContentSchedule` above, one increment later: `OperationResult Rollback(int id, int
versionId, string culture, int userId)` removed from `IPublishableContentService<TContent>` (replaced with
an explanatory comment), new `Task<Attempt<ContentRollbackOperationStatus>> RollbackAsync(Guid key, int
versionId, string culture, Guid userKey, CancellationToken)` added to `IAsyncPublishableContentService<TContent>`.
New `ContentRollbackOperationStatus` enum: `Success`/`ContentNotFound`/`CancelledByNotification`/`SaveFailed`.

`AsyncPublishableContentServiceBase.RollbackAsync` is a native async translation of the old engine (no
`.GetAwaiter().GetResult()` bridging anywhere in the new body, unlike the old version which blocked on
`_idKeyMap.GetKeyForIdAsync`/`GetByIdAsync`/`GetVersionAsync` internally) — same cancelable
`RollingBackNotification`→`content.CopyFrom(version, culture)`→`SaveAsync`→`RolledBackNotification`/audit
shape as before. `PublishableContentServiceBase`'s old sync `Rollback` kept unchanged, same "no longer an
interface member, kept for Element's bridge" comment pattern as `PersistContentSchedule`.
`ElementService.RollbackAsync` bridges via `IdKeyMap.GetIdForKeyAsync` (Guid→int, since the sync engine is
still int-keyed) + `_userIdKeyResolver` (Guid→int) then calls the old sync `Rollback`, translating
`OperationResultType` back to `ContentRollbackOperationStatus` — this needed a new `_userIdKeyResolver`
field added to `ElementService` (wasn't previously stored, constructor already received it).

**Production caller updated**: `ContentVersionServiceBase.RollBackAsync` (`src/Umbraco.Core/Services/`) —
previously called the sync `_contentService.Rollback(...)` after resolving `userId` via
`_userIdKeyResolver.GetAsync(userKey)`; now calls the async `_asyncContentService.RollbackAsync(key, ...,
userKey, CancellationToken.None)` directly with the Guid key, translating `ContentRollbackOperationStatus`
instead of `OperationResultType`. The one int→Guid resolution this caller still needs (`version.ContentId`
is an int) is done via the **already-injected** `_entityService.GetKey(version.ContentId, ItemObjectType)`
rather than adding a new dependency — worth remembering as a precedent: check for an already-injected
int↔Guid resolver (`IEntityService`, `IIdKeyMap`, `IUserIdKeyResolver`) before reaching for a new one.

New test: `ContentServiceTests.RollbackAsync_Reverts_Content_To_Prior_Version` (publish → change → save →
capture `PublishedVersionId` → change again → save → roll back to the captured version → assert reload
matches the pre-change value). The pre-existing `Can_Rollback_Version_On_Content` test (sync path, still
exercises `IContentVersionService`/`ContentVersionServiceBase`, not the retired method directly) was left
untouched — still valid, now exercising the new async delegation path underneath.

### Where this leaves the campaign
`Sort`, `PersistContentSchedule`, and `Rollback` are now off the "still sync" list — 3 more members
retired since the last written-up session, bringing the running total to roughly 55 (the 52 already
recorded, minus the 2 sync `Sort` overloads collapsing into 1 `SortAsync` plus `PersistContentSchedule` and
`Rollback`, net +3 members named `*Async` even though 2 sync overloads collapsed to 1). Re-derive the exact
number from `IContentService.cs`/`IPublishableContentService.cs` directly next time rather than trusting
this arithmetic — this file has been wrong about tallies before.

**Still fully synchronous** (re-confirmed via grep against current `IContentService.cs`/
`IPublishableContentService.cs`, 2026-09-14): on `IContentService` — `DeleteOfType`, `Move`(×2), `Copy`(×2),
`MoveToRecycleBin`, `Publish`, `PublishBranch`, `CheckDataIntegrity`. On `IPublishableContentService`
(shared with Element) — `DeleteOfTypes`, `Publish`, `SaveAndPublish`, `Unpublish`, `PerformScheduledPublish`.
`Save(IEnumerable<IContent>, int)` (bulk) still deliberately left alone, no async equivalent exists.

No memory bookkeeping, no independent review notes, and no explicit test-run confirmation survive from
that home-machine session for these 4 commits — they were already committed and pushed by the time this
write-up happened. Nothing else queued; next increment still needs explicit direction, same standing rule
as always.

## Session of 2026-09-14: `CheckDataIntegrity` retired — introduces a new pattern for members shared with Media/Member

Committed as `dc6783a99cf` (not yet pushed — check `git log --oneline origin/v18/feature/ef-core-document-repository..HEAD` before assuming pushed). Planned via plan mode, implemented, tested, independently reviewed (clean, 2 trivial nits fixed), then **iterated twice more on the user's explicit design pushback after the review already passed** — worth reading in full since the final shape is meaningfully different from the first "done" version.

**Why this one was harder than Sort/PersistContentSchedule/Rollback**: those three are declared on `IPublishableContentService<TContent>` (shared only with Element, which is itself mid-migration), so the sync member could just be deleted from the interface outright. `CheckDataIntegrity` is declared one level higher, on `IContentServiceBase` — shared with `IMediaService`/`IMemberService` too, which are genuinely out of scope for this whole campaign and have no async repositories. Deleting the sync member there would break Media/Member's public API, so it had to stay reachable, while the actual EF Core work (already done at the repository tier — `IAsyncContentRepository<TEntity>.CheckDataIntegrityAsync`, built earlier, never wired to a service — this was a "wire it up" increment, not new EF Core logic) needed a home.

**Design iteration (3 rounds, each prompted by direct user pushback, not self-directed)**:
1. First pass: `ContentService.CheckDataIntegrity` (sync) delegates to the new `CheckDataIntegrityAsync` via `.GetAwaiter().GetResult()`, written directly on `ContentService.cs`. **User rejected**: "not too fond of ContentService still implementing CheckDataIntegrity... the rest is spillover from to-do repositories like media."
2. Second pass: moved the one-line sync bridge onto `AsyncPublishableContentServiceBase` (the shared base class) as a concrete (non-abstract) method, so `ContentService.cs` itself contained zero sync code for this member. **User rejected again**: "It's still in the async base class, we should remove it entirely from the async* chain" — even a bridge living on a class/interface named `Async*` was unacceptable.
3. Final shape: the sync member is satisfied via **explicit interface reabstraction** declared directly on `IContentService` itself (not "Async"-named): `ContentDataIntegrityReport IContentServiceBase.CheckDataIntegrity(ContentDataIntegrityReportOptions options) => CheckDataIntegrityAsync(options, CancellationToken.None).GetAwaiter().GetResult();`. Neither `ContentService` nor `AsyncPublishableContentServiceBase` contains any implementation of this member at all — it exists purely as an interface-level default, reachable only through an `IContentService`-typed reference.

**C# mechanics discovered the hard way (verified with a minimal repro in `/tmp/dimtest`, not just reasoned about)**: a plain redeclaration of an ancestor interface's abstract member with a default body on a derived interface (`ContentDataIntegrityReport CheckDataIntegrity(...) => ...;`, no special syntax) does **NOT** satisfy the ancestor's requirement for implementing classes — C# treats it as an unrelated new member (CS0108 "hides inherited member" warning), and any concrete class still gets CS0535 "does not implement interface member." Even adding `new` doesn't fix it (makes it worse — now genuinely two unrelated members). The only syntax that actually works is **explicit interface implementation written inside the derived interface**, targeting the ancestor interface by name: `ReturnType IAncestorInterface.Member(...) => ...;`. This is the first use of this pattern anywhere in this codebase — CLAUDE.md §6.3's documented "Default Interface Implementation" examples are all the simpler same-interface-declares-its-own-new-member-with-a-default shape, not this ancestor-reabstraction shape. **If this pattern needs to be reused for another member shared with Media/Member in a future increment, this session's `/tmp/dimtest` finding is the reference** — don't rediscover it from scratch.

**Consequence worth remembering**: because the implementation lives only as an explicit interface member, it's invisible through a concrete-class-typed reference. `UmbracoIntegrationTestWithContent.ContentService` (test base class property) is typed as the concrete `ContentService` class — calling `.CheckDataIntegrity(...)` directly on it does NOT compile; must cast to `IContentService` first. The one real production caller (`DatabaseIntegrityCheck.cs`) already holds `IContentService`-typed fields, so it's unaffected. A dedicated test (`CheckDataIntegrity_SyncEntryPoint_DelegatesToAsyncEngine`) was added specifically to exercise this exact call shape and prove the DIM dispatch resolves correctly at runtime, not just compiles.

**TODO added** (per explicit user request, on the explicit-reabstraction declaration in `IContentService.cs`): remove this default implementation once Media/Member get their own async repositories and `IContentServiceBase.CheckDataIntegrity` itself can retire in favor of an async-only equivalent.

**Tests added**: `AsyncDocumentRepositoryTest.cs` — `CheckDataIntegrityAsync_WithConsistentData_ReportsOk`, `CheckDataIntegrityAsync_DetectsAndFixes_CorruptedLevel` (corrupts a node's `Level` directly via raw EF Core `ExecuteUpdateAsync` on `db.Nodes`, bypassing Save/Move, to get an isolated deterministic corruption). `ContentServiceTests.cs` — `CheckDataIntegrityAsync_WithConsistentData_ReportsOk`, `CheckDataIntegrityAsync_FixIssues_CorrectsCorruptedLevel`, `CheckDataIntegrity_SyncEntryPoint_DelegatesToAsyncEngine` (the interface-dispatch test described above). **Caught one real test bug while writing these**: an anonymous `using (NewScopeProvider.CreateScope())` block (no named variable) never got `.Complete()` called, so the corruption silently rolled back — the test still "passed" in a useless way (weakly, since one of the two assertions in `Assert.Multiple` failed loudly, so it wasn't a false green, but still a lesson) — always name scope variables you intend to complete, even in a tight `using` block.

**Verified**: full solution build 0 errors (both incremental and `--no-incremental`, re-run after each of the 3 design iterations). Targeted suite: `ContentServiceTests`+`AsyncDocumentRepositoryTest`+`ElementServiceTests`+`ElementRepositoryTest` 330/330 passing. Independent review agent run once, after iteration 1 (before the two later design changes) — came back clean (no blockers, 2 nits both fixed); **not re-run after iterations 2/3** since those were direct, unambiguous user-directed design changes verified by build+test rather than needing independent review.

**Current full retirement tally**: 56 members. Remaining on `IContentService`: `DeleteOfType`, `Move`(×2), `Copy`(×2), `MoveToRecycleBin`, `PublishBranch`, `Publish`. Remaining on `IPublishableContentService` (shared with Element): `DeleteOfTypes`, `Publish`, `SaveAndPublish`, `Unpublish`, `PerformScheduledPublish`. `CheckDataIntegrity` is the only member retired so far that needed the explicit-reabstraction pattern — everything else in both remaining lists is declared no higher than `IPublishableContentService<TContent>`, so the simpler "just delete the sync member" pattern should apply to all of them; re-derive this from the actual interfaces at the start of the next increment rather than trusting this claim blindly.

## Session of 2026-09-14 (continued): `MoveToRecycleBin` retired, AND a reusable async `PerformMoveLocked` engine built for `Move`/`DeleteOfType(s)` to adopt later

Committed as `6138cf25550` (not yet pushed — check `git log --oneline
origin/v18/feature/ef-core-document-repository..HEAD` before assuming pushed). Planned via plan mode with a
**user-specified second goal beyond the single member**: don't just make `MoveToRecycleBin` async in
isolation — build a real async replacement for the shared private engine it calls into
(`PerformMoveLocked`), so `Move` and `ContentTypeService`'s type-deletion cascade (`DeleteOfTypes`) can
reuse it in their own future increments instead of redoing this work. Both of those callers are explicitly
**out of scope** — their sync call sites into the OLD `PerformMoveLocked` are completely untouched; old and
new engines coexist (same "AsyncX scaffolding alongside sync X" pattern used throughout this whole
campaign, just one layer lower than usual — a private engine method, not a repository).

**Why this was picked over the other remaining candidates**: unlike `CheckDataIntegrity`, none of the
remaining `IContentService`/`IPublishableContentService` members has a pre-built async repository method
waiting to be wired up — every one needs genuine new async engine code. `MoveToRecycleBin` was the smallest
real slice: Document-only (not shared with Element/Media), fixed move target (no parent-resolution
branching, unlike `Move`), and `EmptyRecycleBinAsync`/`DeleteLockedAsync` (already in `ContentService.cs`)
were proven, directly-adaptable templates for the "paged async descendant fetch + per-item async repository
write" shape needed.

**Real design win, not just a mechanical port**: the old `PerformMoveLocked` fetched descendants via a
private NPoco arbitrary-`IQuery<IContent>` helper (`GetPagedDescendantQuery`+`GetPagedLocked`) *after* the
moved node's own path had already changed in memory, so it had to search by the pre-move path STRING. The
new `PerformMoveLockedAsync` instead fetches ALL descendants via the already-existing public
`GetDescendantsAsync(content.Key, ...)` *before* saving the moved node's own new parent. This works because
`AsyncDocumentRepository.GetDescendantsCoreAsync` matches descendants via the ancestor's `NodeId` embedded
in each descendant's own `Path` string (`EF.Functions.Like(node.Path, "%,{parentNodeId},%")`) — so as long
as the fetch happens before any row (ancestor or descendant) gets mutated, it returns the same set the old
path-prefix query did. This is a real simplification that removes an NPoco dependency rather than
re-implementing it — reviewed and confirmed correct by tracing the actual EF Core implementation, not just
asserted. (One review nitpick: the in-code comment first written for this said `GetDescendantsAsync`
"resolves the ancestor fresh from the DB on every call" — technically imprecise about the *why*, since it's
really about descendant rows not yet being mutated, not about re-reading the ancestor's row. Comment fixed
to describe the NodeId-in-Path matching mechanism instead.)

**New engine** (`ContentService.cs`, private, coexists with the old one): `PerformMoveLockedAsync` +
`LeaveDescendantInRecycleBinLockedAsync` + `PerformMoveDescendantLockedAsync` +
`PerformMoveContentLockedAsync` — a faithful line-for-line behavioral port of the sync
`PerformMoveLocked`/its 3 sub-helpers (same path/level math, same trashed-flag handling), including the
`leaveDescendantsInRecycleBin` branch (restoring a single item out of the bin without its descendants) even
though nothing exercises it yet — only `Move`'s future increment will reach it. `_asyncDocumentRepository`
(already a `ContentService` field) provides `SaveAsync` for the write side.

**Deleted, not bridged**: `IContentService.MoveToRecycleBin` (sync) — unlike `CheckDataIntegrity`, this
member is declared directly on `IContentService`, not on `IPublishableContentService<TContent>`/
`IContentServiceBase`, so nothing forces it to survive; the explicit-interface-reabstraction trick from the
previous increment was correctly NOT needed or used here. New `ContentMoveToRecycleBinOperationStatus`
enum: `Success`/`CancelledByNotification`.

**One real internal sync caller needed a bridge, kept out of `Async*`-named territory**: `Move`'s
recycle-bin redirect (`if (parentId == Constants.System.RecycleBinContent) { ... }`) now does a blocking
`.GetAwaiter().GetResult()` call into the new `MoveToRecycleBinAsync`, translating
`Attempt<ContentMoveToRecycleBinOperationStatus>` back to `OperationResult` via the same status-switch idiom
`ContentEditingService.SortAsync`/`DeleteAsync` already established. Per
[[feedback_async_named_classes_stay_pure]], this bridge lives inline in `ContentService.cs`'s already-sync
`Move` method — not on any `Async*`-named class — which the memory explicitly flagged as fine (the objection
was to `Async*`-named types carrying sync surface, not to an ordinary mixed-sync-and-async service class
having one more bridge in a method that isn't converted yet).

**One real production caller updated for genuine async propagation**:
`ContentEditingService.MoveToRecycleBinAsync` (override of `AsyncContentEditingServiceBase`'s abstract
member) previously did `Task.FromResult(syncCall)` — a fake-async wrapper. Now genuinely awaits. Needed a
new `GetUserKeyAsync(int) : Task<Guid>` helper on `AsyncContentEditingServiceBase`, mirroring the existing
`GetUserIdAsync(Guid) : Task<int>` exactly (same injected `_userIdKeyResolver`, opposite direction) — no
reverse-direction helper existed before this.

**Mechanical call-site migration, larger than usual in volume**: ~55 call sites across ~25 integration test
files (search-index tests, entity service tests, content events tests, etc.) — every one passed just
`(content)`, so the conversion was uniform: `ContentService.MoveToRecycleBin(content)` → `await
ContentService.MoveToRecycleBinAsync(content, Constants.Security.SuperUserKey, CancellationToken.None)`.
Handled directly (not delegated to subagents) via a scripted regex pass per file group, with manual fixes
for: (a) argument expressions containing nested parens (`Get(RootKey)`, `.Last()`) that a naive
`[^()]+` regex couldn't match — widened to allow one level of nesting; (b) 2 sites capturing `OperationResult
result = ...` needing the type changed to `Attempt<ContentMoveToRecycleBinOperationStatus>`; (c) 3 call sites
living inside a `WaitForIndexing(alias, Func<Task> action)` lambda that needed the lambda itself marked
`async` — a broad regex pass over that file also accidentally converted 3 *unrelated* `ContentService.Unpublish(...)`
lambdas in the same method family to `async`, caught and manually reverted back to their original
non-async `() => { ...; return Task.CompletedTask; }` shape before it became a stray, unrelated diff.
**Lesson**: a regex-based bulk edit scoped to "convert lambdas near this pattern" can over-match structurally
similar-but-unrelated code in the same file — always diff the full file after a scripted pass, not just grep
for the intended pattern's success count.

**New tests**: `ContentServiceTests.cs` gained `MoveToRecycleBinAsync_MovingNotificationCancelled_ReturnsCancelledStatusAndDoesNotMove`
(new `INotificationHandler<ContentMovingToRecycleBinNotification>` added to the file's existing
`ContentNotificationHandler` test double + `CustomTestSetup` registration, same static-handler-reset-in-`finally`
pattern already used for `SavingContent`). The existing `Can_Move_Content_Structure_To_RecycleBin_And_Empty_RecycleBin`
test (already in the file, now migrated as part of the mechanical pass) turned out to be the main regression
coverage for the `GetPagedDescendantQuery`→`GetDescendantsAsync` rewrite — it moves content with 4 real
descendants into the bin and asserts their `Path`/`Trashed` state, so no new dedicated "move with
descendants" test needed writing from scratch.

**Verified**: full solution build 0 errors (both incremental and `--no-incremental`). Targeted suites:
`ContentServiceTests`+`ContentEventsTests`+`EntityServiceTests`+`ContentEditingServiceTests`+
`ContentPublishingServiceTests`+`AsyncDocumentRepositoryTest`+`ContentServiceTagsTests`+
`TrackedReferencesServiceTests`+`PatchDocumentControllerTests` → 750/752 passing (2 pre-existing unrelated
skips). Search/Examine suites (`InvariantContentTests`+`InvariantContentStructureTests`+
`PublishedContentCacheRefresherTests`+`IndexedEntitySearchServiceTests`+`InvariantDocumentTreeTests`+
`InvariantContentTreeTests`+`EntityServiceTestsIsolated`) → 170/170 passing. Independent review agent: no
blockers, one nitpick (the descendant-fetch-ordering comment's imprecise "why", fixed).

**Current full retirement tally**: 57 members. Remaining on `IContentService`: `DeleteOfType`, `Move`(×2),
`Copy`(×2), `PublishBranch`, `Publish` — `Move` and `DeleteOfType(s)` now have `PerformMoveLockedAsync`
ready and waiting; their own increments should be "swap which engine we call" work, not "build the engine"
work. Remaining on `IPublishableContentService` (shared with Element): `DeleteOfTypes`, `Publish`,
`SaveAndPublish`, `Unpublish`, `PerformScheduledPublish`. `Copy` still has no async engine work started at
all (doesn't reuse `PerformMoveLockedAsync` — it creates new entities rather than mutating existing ones).

## Session of 2026-09-14 (continued): `Move` retired — `PerformMoveLocked` now down to its last caller

Committed as `680aa7ddd54` (not yet pushed — check `git log --oneline
origin/v18/feature/ef-core-document-repository..HEAD` before assuming pushed). Planned via plan mode,
implemented, tested, independently reviewed clean. Direct continuation of the `MoveToRecycleBin` increment's
explicit second goal: get every `PerformMoveLocked` caller onto `PerformMoveLockedAsync` so the sync engine
can eventually be deleted. After this increment, `PerformMoveLocked` has exactly **one** remaining caller:
`DeleteOfTypes`'s type-deletion cascade (inside `ContentService.cs`) — a clearly-scoped next increment,
though that one also has its own arbitrary-`IQuery<IContent>` dependencies to resolve (not just an engine
swap like `Move` was).

**Design**: `IContentService.Move`'s 2 sync overloads collapsed into one `MoveAsync(IContent content, Guid?
parentKey, bool includeDescendants, Guid userKey, CancellationToken)` — deleted outright from the interface
(not shared with Element/Media, same as `MoveToRecycleBin`'s precedent, no explicit-reabstraction trick
needed). New `ContentMoveOperationStatus` enum: `Success`/`CancelledByNotification` (the "parent not found
or trashed" case stays a thrown `InvalidOperationException`, matching the established `CreateAsync`
precedent of keeping analogous exceptions rather than inventing a status for every failure mode).

**A genuine, deliberate simplification in the wrapper-service layer, not just a mechanical swap**:
`AsyncContentEditingServiceBase.HandleMoveAsync` already received `Guid? parentKey`/`Guid userKey` as its
own parameters, and only round-tripped them to `int parentId`/`int userId` because the OLD abstract `Move`
member needed ints. Since `MoveAsync` takes `Guid?`/`Guid` directly, the abstract member's signature changed
to match (`protected abstract Task<OperationResult?> MoveAsync(TContent content, Guid? parentKey, bool
includeDescendants, Guid userKey);`), and `HandleMoveAsync` now passes its own already-validated
`parentKey`/`userKey` straight through — the `GetUserIdAsync(userKey)` call that existed only to feed the
old abstract member became dead and was deleted. `TryGetAndValidateParentIdAsync`'s own validation
(content-type compatibility, "moving beneath itself" check, parent-not-found status) was left completely
untouched — it's still needed before delegating, it just no longer needs to hand a resolved int down.

**This abstract member is genuinely separate from (not shared with) the one Media/Member use**: confirmed
via class hierarchy — `ContentEditingService`/`ElementEditingService`/`ContentBlueprintEditingService` all
derive from `AsyncContentEditingServiceBase` (the async-scaffolding hierarchy), while
`MediaEditingService`/`MemberContentEditingService` derive from the **original, untouched**
`ContentEditingServiceBase`. Both declare an identically-shaped abstract `Move` member today, purely by
historical coincidence (one was copied from the other) — changing `AsyncContentEditingServiceBase`'s has
zero effect on Media/Member. **3 overrides needed updating** (all 3 on the async hierarchy):
`ContentEditingService` (real conversion, now genuinely awaits), `ElementEditingService` and
`ContentBlueprintEditingService` (both just `=> throw new NotImplementedException();` for unrelated reasons
— Element has its own custom move flow elsewhere, blueprints don't support moving at all — only their
signatures needed updating to match, bodies unchanged).

### Real bug found and fixed mid-session: `ContentBase.ParentKey` can throw — see [[feedback_contentbase_parentkey_can_throw]]

First draft of `MoveAsync` used `if (content.ParentKey == parentKey) { return Success; }` as its very first
line, mirroring the old sync method's `content.ParentId == parentId` no-op short-circuit but in Guid-space.
This broke 6 `ContentEventsTests.cs` tests with `System.NotSupportedException: ParentKey is not available
for parent id X: it can only be resolved from ParentId alone for the root and recycle-bin pseudo-parents.`
— `ContentBase.ParentKey`'s getter (`src/Umbraco.Core/Models/ContentBase.cs:107-138`) throws (deliberately,
by design — see the memory below) when a content object's parent key was never populated and the parent id
isn't a root/recycle-bin pseudo-parent. The failing tests all built content via a local `CreateContent(int
parentId)` test helper that sets `ParentId` directly without going through a reload, so `ParentKey` genuinely
wasn't populated on those objects — this wasn't a flaky/edge-case test setup, it's a realistic shape any
caller could hand `MoveAsync`.

**Fix**: restructured into 3 separate no-op checks, all using `ParentId` (int, never throws) instead of
`ParentKey` (Guid?, can throw): (a) `parentKey == Constants.System.RecycleBinContentKey &&
content.ParentId == Constants.System.RecycleBinContent` → succeed without even calling
`MoveToRecycleBinAsync`; (b) `parentKey is null && content.ParentId == Constants.System.Root` → succeed; (c)
after resolving `parent`/`parentId` (int) inside the scope — unavoidable at that point anyway, needed for
the not-found/trashed validation — `content.ParentId == parentId` → succeed. This costs one extra DB fetch
in the "already at this real (non-null, non-recycle-bin) parent" no-op case that the original sync method
avoided (it had the caller's int directly, no fetch needed) — a small, deliberate, safety-over-micro-
optimization trade-off, not something to "fix" later.

**Verified via a full test run, not just the specific failing tests**: after the fix, all 59
`ContentEventsTests` tests passed (up from 53/59), confirming the fix didn't just paper over the specific
failures.

**Test updates worth remembering**: `Move_With_Descendants_Populates_ParentKey_Without_Redundant_IIdKeyMap_Calls`
(pre-existing test, asserting "exactly 1 `IIdKeyMap` call is unavoidable" for the old int-parentId sync
`Move`) had its assertion **changed**, not just mechanically converted — the new `MoveAsync` takes the
parent's Guid key directly, so the "1 unavoidable lookup" premise no longer holds; updated to assert **zero**
`IIdKeyMap` calls, and the independent review agent verified this is genuinely true of the new code path
(not just asserted). New test `MoveAsync_RestoringWithoutDescendants_LeavesDescendantsTrashedAtRecycleBinRoot`
added — confirmed via grep before writing it that `includeDescendants: false` (the `leaveDescendantsInRecycleBin`
branch, carried over from the sync engine during the `MoveToRecycleBin` increment) had **never** been
exercised by any test, in this campaign or before it.

**Verified**: full solution build 0 errors (both incremental and `--no-incremental`). Targeted suites:
`ContentServiceTests`+`ContentEventsTests`+`ContentServiceTagsTests`+`InvariantContentTests`+
`AsyncDocumentRepositoryTest` → 431/433 passing (2 pre-existing unrelated skips, tracked upstream as
umbraco/Umbraco-CMS#3821). `ContentEditingServiceTests`+`ElementEditingServiceTests`+
`ContentBlueprintEditingServiceTests` (touched by the abstract-member signature change) → 304/304 passing.
Independent review agent: no blockers, no nitpicks — traced every `ParentKey` read in the new code path to
confirm none of them can hit the same hazard.

**Current full retirement tally**: 58 members. Remaining on `IContentService`: `DeleteOfType`, `Copy`(×2),
`PublishBranch`, `Publish`. `PerformMoveLocked` (sync) has exactly one caller left — `DeleteOfTypes` — making
that the natural next "finish retiring PerformMoveLocked" increment, though it also needs its own 2
arbitrary-`IQuery<IContent>` dependencies resolved (not just an engine swap). Remaining on
`IPublishableContentService` (shared with Element): `DeleteOfTypes`, `Publish`, `SaveAndPublish`,
`Unpublish`, `PerformScheduledPublish`.

## Session of 2026-09-14 (continued): `DeleteOfType(s)` retired — `PerformMoveLocked` fully deleted, closing the 3-increment arc

Committed as `d75535a4b99` (not yet pushed — check `git log --oneline
origin/v18/feature/ef-core-document-repository..HEAD` before assuming pushed). Planned via plan mode,
implemented, tested, independently reviewed clean. This closes the arc explicitly started by the
`MoveToRecycleBin` increment: get every `PerformMoveLocked` caller onto `PerformMoveLockedAsync` so the sync
engine could be deleted. `DeleteOfTypes` (`ContentTypeService`'s type-deletion cascade) was the engine's last
caller — once migrated, `PerformMoveLocked` and its 5 private sub-helpers
(`PerformMoveDescendantLocked`/`PerformMoveContentLocked`/`LeaveDescendantInRecycleBinLocked`/
`GetPagedDescendantQuery`/`GetPagedLocked`) had zero remaining references anywhere and were **deleted
outright** — confirmed via full-solution build + repo-wide grep, not just within `ContentService.cs`.
**Caution for later sessions**: `MediaService.cs` has its own, completely separate, coincidentally-identically-named
private helpers (`PerformMoveLocked`, `GetPagedDescendantQuery`, `GetPagedLocked`,
`LeaveDescendantInRecycleBinLocked`, `PerformMoveDescendantLocked`) — Media's own untouched engine, not part
of this campaign — don't confuse a grep hit there for a missed reference in `ContentService.cs`.

**Both of `DeleteOfTypes`'s two arbitrary NPoco `IQuery<IContent>` dependencies were replaceable by
already-existing async service methods — no new repository work needed, unlike what earlier research had
feared**: "all content of the given content type ids" → `GetPagedOfTypesAsync(Guid[] contentTypeKeys, ...)`
(already built for `GetPagedOfType(s)` reads); "direct children of one content item" →
`GetChildrenAsync(Guid? parentKey, ..., take: int.MaxValue)` (same idiom `EmptyRecycleBinAsync` already
uses). Verified both replacements are semantically equivalent to the old arbitrary queries — neither excludes
trashed items (matching the old queries, which had no trashed filter either).

**A real gap the plan didn't anticipate, found only when the build broke**: the plan only accounted for
updating `ContentTypeService.DeleteItemsOfTypesAsync`'s Document-side call
(`_contentService.DeleteOfTypes(typeIdsA)`). The build ALSO broke on the same method's
`_elementService.DeleteOfTypes(typeIdsA)` call — Element's sync `DeleteOfTypes` came from the exact same
`IPublishableContentService<TContent>` interface tier being retired, so it disappeared from `IElementService`
too, even though `ElementService`'s own CONCRETE engine (still real, untouched, sync) didn't go anywhere.
Fixed by restructuring the whole method: moved the pre-existing `typeIds`→`typeKeys` (Guid, via `IIdKeyMap`)
resolution loop to run first, then made all three calls (`_contentService.DeleteOfTypesAsync`,
`_contentService.DeleteBlueprintsOfTypesAsync`, `_elementService.DeleteOfTypesAsync`) share the same
resolved `typeKeys` list, all gated behind one `if (typeKeys.Count > 0)` guard (previously only the blueprint
call was gated this way; Element's call was unconditional on the raw ints, bypassing Guid resolution
entirely). **Deliberate, reasoned trade-off, not an oversight**: this means if `IIdKeyMap` resolution fails
for ALL given content type ids, Element's content of those types would now silently not be deleted, whereas
before it always would have been (since Element used the raw ints directly, never going through the
resolution that could fail). Confirmed this is a vanishingly rare edge case (would require `IIdKeyMap` to
fail resolving a valid, existing content type id) and confirmed (by reading
`AsyncDocumentRepository.GetPagedOfContentTypesAsync` directly) that an **empty** `contentTypeKeys` array
resolves to "match nothing," not "match everything" — so the guard is a tidiness/audit-message optimization
here, not a load-bearing safety requirement (unlike the identical-looking guard on the blueprint call, where
empty genuinely means "delete all blueprints" and the guard IS load-bearing).

**Followed the now-thrice-established shared-with-Element pattern exactly** (same as `Sort`/`Rollback`/
`PersistContentSchedule`): sync `DeleteOfTypes` deleted outright from `IPublishableContentService<TContent>`
(replaced with the same "retired in favour of" comment style used for its siblings), new
`DeleteOfTypesAsync` added to `IAsyncPublishableContentService<TContent>`. `ElementService`'s bridge resolves
each `Guid` content-type key via `IdKeyMap.GetIdForKeyAsync(key, UmbracoObjectTypes.DocumentType)` — note
`UmbracoObjectTypes.DocumentType`, not `ContentObjectType` (which would be wrong here — this resolves
content-*type* keys, not Element-instance keys) — then calls Element's untouched real sync engine. Bridge's
own doc comment honestly states it can't detect cancellation (the sync engine returns `void`) and always
reports `Success` — documented, not silently assumed away.

**A user question after this landed, worth remembering for future PR/design explanations**: asked why
`DeleteOfTypesAsync` (plural) appears on both `IAsyncPublishableContentService<TContent>` and
`IContentService` — clarified there's no duplicate declaration: the plural lives ONLY on
`IAsyncPublishableContentService<TContent>` (needed by both Document and Element via inheritance, same
shape as `RollbackAsync`/`PersistContentScheduleAsync`/`CheckDataIntegrityAsync`), while `IContentService`
separately declares only the *singular* `DeleteOfTypeAsync` convenience overload directly (Document-only,
matching the old `DeleteOfType`'s placement and the `DeleteBlueprintsOfType`/`DeleteBlueprintsOfTypes`
precedent). `IContentService` just inherits the plural — normal interface inheritance, not redundancy.

**Verified**: full solution build 0 errors (both incremental and `--no-incremental`). Targeted suite:
`ContentServiceTests`+`ContentServiceNotificationTests`+`ElementServiceNotificationTests`+
`ContentTypeServiceTests`+`AsyncDocumentRepositoryTest`+`MediaEditingServiceTests` → 476/476 passing.
Independent review agent: no blockers, no nitpicks beyond the already-reasoned Element-deletion-guard
trade-off discussed above.

**Current full retirement tally**: 59 members. Remaining on `IContentService`: `Copy`(×2), `PublishBranch`,
`Publish`. Remaining on `IPublishableContentService` (shared with Element): `Publish`, `SaveAndPublish`,
`Unpublish`, `PerformScheduledPublish`. `Copy` still has no async engine work started at all — it creates new
entities rather than mutating existing ones, so it can't reuse `PerformMoveLockedAsync` (which is now fully
retired anyway, alongside its sync predecessor). The `Publish`/`SaveAndPublish`/`Unpublish`/`PublishBranch`/
`PerformScheduledPublish` family all share one core write path, `CommitContentChangesInternal`, still on
NPoco — this campaign's memory has repeatedly flagged that converting it is a large, cross-cutting effort
deserving its own dedicated planning session, not something to pick up incidentally.

### Follow-up, same day (commit `7e88a3e6ce5`): dead sync `DeleteLocked` removed

After `DeleteOfTypes` landed, the user asked directly whether `ContentService`'s sync `DeleteLocked`
override and `AsyncPublishableContentServiceBase`'s abstract `DeleteLocked` could now be removed — both had
become internally unused once `ContentService.DeleteLockedAsync` (the real async override, already serving
`EmptyRecycleBinAsync`/`DeleteAsync`/`DeleteOfTypesAsync`) took over every call site. Confirmed via grep that
`ContentService`'s sync override had zero remaining callers, and that `ContentService` is (and, per
`IAsyncPublishableContentService`'s own doc comment, always will be) the sole subclass of
`AsyncPublishableContentServiceBase<TContent>` — Element/Media/Member all deliberately stay on the separate,
untouched `PublishableContentServiceBase<TContent>` hierarchy, which has its OWN unrelated abstract
`DeleteLocked` (still required by `ElementService`, not touched). Removed the abstract sync `DeleteLocked`
and its virtual `DeleteLockedAsync` bridging default from `AsyncPublishableContentServiceBase` (the
"bridge for content kinds without an async-native delete path yet" scenario the bridge's own comment
described never actually applies to any real or planned subclass), leaving `DeleteLockedAsync` as a plain
abstract member; removed the now-orphaned sync override from `ContentService.cs` entirely. Verified: full
solution build 0 errors (incremental and `--no-incremental`), 360 targeted tests passing. Small, mechanical,
handled directly without a full plan-mode/review-agent cycle given the low risk and clear reasoning.

**Housekeeping note**: this same session, the `CheckDataIntegrity` session write-up above (the one just
before `MoveToRecycleBin`) was found badly out of order in this file — a prior append had landed it at the
very end, after `DeleteOfTypes`, instead of in its correct chronological position. Moved back into place;
no content was lost or duplicated, just reordered. If a future session finds another out-of-order section,
the safe fix is the same: find the correct chronological position (compare commit hashes/dates), cut, and
reinsert — never delete without confirming the content is genuinely a duplicate first.

### Next session: `Copy` retired (commit `da2cc8e57f7`)

Retired both sync `Copy` overloads on `IContentService`, replaced with a single
`CopyAsync(IContent content, Guid? parentKey, bool relateToOriginal, bool recursive, Guid userKey, CancellationToken cancellationToken)`.

**Return type — explicit user override of my own recommendation**: I initially recommended (via
AskUserQuestion) a plain `Task<IContent?>`, matching the pattern several other new async members use. The
user explicitly rejected this after the fact ("I answered your question incorrectly, you can return both a
status and the content with an attempt") and directed the two-generic `Attempt<TResult, TStatus>` shape
instead: `Task<Attempt<IContent?, ContentCopyOperationStatus>>`, via `Attempt.SucceedWithStatus`/
`FailWithStatus<IContent?, ContentCopyOperationStatus>`. **Worth remembering for future increments**: when a
sync predecessor returns a real payload (not just success/failure), lean toward the two-generic `Attempt`
shape by default rather than a bare nullable `Task<T>` — it was already the established idiom elsewhere
(`AsyncContentEditingServiceBase.HandleCopyAsync` already used it), I just didn't apply it here on the first
pass.

**Deliberate new validation, not a faithful-only port**: the old sync `Copy` never validated that `parentId`
resolved to a real parent — a failed `TryGetParentKey` silently left `copy.ParentKey` null with no error
surfaced. New `CopyAsync` resolves `parentKey` via `GetByIdAsync` up front and returns a new
`ContentCopyOperationStatus.ParentNotFound` if a non-null `parentKey` doesn't resolve. New test
`Cannot_Copy_Content_To_A_Parent_That_Does_Not_Exist` covers this (no prior coverage existed since the old
method couldn't fail this way).

**Bonus dead-code removal found along the way**: migrating `Copy` turned out to eliminate the LAST use of
the sync NPoco `_documentRepository` field anywhere in `ContentService.cs` (all 3 usages — `Save`×2,
`AddOrUpdatePermissions`×1 — were inside `Copy`). Removed the field and its constructor assignment; the
`documentRepository` constructor *parameter* stays (still forwarded to `base(...)`, needed by
`AsyncPublishableContentServiceBase`'s own `_contentRepository`).

**`ElementEditingService.CopyAsync`'s real, independent copy engine** (doesn't delegate to `IContentService`
at all) needed its `_idKeyMap` resolution direction reversed: old code resolved int→Guid
(`GetKeyForIdAsync`) purely to build the notification/audit payload while using the raw int for
`copy.ParentId`; new code resolves Guid→int (`GetIdForKeyAsync`) for `copy.ParentId`, using the `parentKey`
parameter directly (no longer a locally-computed variable) for the notification/audit payload.
`parentKey is null` still means root, since `Constants.System.RootKey` is null (established fact, see
[[feedback_root_content_has_no_guid_key]]).

**Test conversions surfaced one stale test assumption that needed fixing, not just mechanical translation**:
`Copy_Recursive_Populates_Descendant_ParentKey_Without_Redundant_IIdKeyMap_Calls` asserted
`callCountBeforeCopy + 1` `IIdKeyMap.GetKeyForIdAsync` calls, reasoning that resolving the new parent's int
id to a Guid was "the one unavoidable lookup" of the old `Copy(..., int parentId, ...)` signature. Since
`CopyAsync` now takes the parent's Guid key directly with zero `IIdKeyMap` involvement in the parent
resolution path, the correct expectation became `callCountBeforeCopy` (zero additional calls) — updated
both the assertion and its comment, mirroring the identical fix already made for the analogous `Move` test
in an earlier increment.

**Minor accepted gap, flagged by the independent reviewer, not fixed**: unlike `MoveAsync` (which
special-cases `parentKey == Constants.System.RecycleBinContentKey` before attempting `GetByIdAsync`
resolution), `CopyAsync` has no such special case — passing the recycle-bin sentinel key would now return
`ParentNotFound` instead of the old code's silent direct mapping. Confirmed no reachable caller (API,
backoffice, or test) ever passes this key to `Copy`/`CopyAsync`, so left as-is rather than adding an
unrequested special case.

**Verified**: full solution build 0 errors (incremental and `--no-incremental`). 557/557 tests passing
across `ContentServiceTests`(176)/`ContentEventsTests`(59)/`ContentEditingServiceTests`(162)/
`ElementEditingServiceTests`(101)/`ContentBlueprintEditingServiceTests`(41)/`DomainAndUrlsTests`(18).
Independent review agent: no blockers, one minor non-blocking note (the recycle-bin-key gap above).

**Current full retirement tally**: 60 members. Remaining on `IContentService`: `PublishBranch`, `Publish`.
Remaining on `IPublishableContentService` (shared with Element): `Publish`, `SaveAndPublish`, `Unpublish`,
`PerformScheduledPublish`. All five remaining members share one core write path, `CommitContentChangesInternal`,
still on NPoco — as repeatedly flagged in this campaign's memory, converting it is a large, cross-cutting
effort deserving its own dedicated planning session, not something to pick up incidentally.

### Next session: `Publish` retired, `CommitContentChangesInternal` pipeline opened (commit `3d065eab073`)

Retired sync `Publish(TContent, string[], int userId)` from `IPublishableContentService<TContent>` (and
its redundant duplicate declaration on `IContentService`), replaced with
`Task<PublishResult> PublishAsync(TContent content, string[] cultures, Guid userKey, CancellationToken cancellationToken)`
on `IAsyncPublishableContentService<TContent>`. **74 files, the campaign's largest increment so far.**

**Return type is the ONE campaign exception to `Attempt<TStatus>`**: `Publish` already returned
`PublishResult` — a pre-existing rich result type (`PublishResultType` enum with ~20 values +
`EventMessages` + `Content` + `InvalidProperties`) that the whole publish family and the management-API
translation layer (`ContentPublishingServiceBase.ToContentPublishingOperationStatus`) consume directly.
Making it async was right; inventing a parallel status enum would have duplicated `PublishResultType` and
rippled pointlessly. Same class of exception as `OperationResult` for the still-sync plural `Save`.

**The key structural move — parallel engine, not in-place conversion.** `Publish`'s engine
(`CommitContentChangesInternal`) is SHARED with `Unpublish`/`SaveAndPublish`/`PerformScheduledPublish`,
which all stay sync this increment. So the async engine was built ALONGSIDE the sync one:
`CommitContentChangesAsync`/`CommitContentChangesInternalAsync` + `StrategyCanPublishAsync`/
`StrategyCanUnpublishAsync`/`StrategyUnpublishAsync`, with the sync originals left untouched.
`StrategyPublish` (pure logic, no I/O) and `GetPublishedDescendantsLocked` (plain sync helper, not
Task-returning) are called unchanged from the async engine — no duplicate needed. Same
"shared internal engine, adopted one caller at a time" playbook as `PerformMoveLockedAsync`.
All async repository capability already existed on the base class's `_asyncContentRepository` field
(`SaveAsync`/`GetContentScheduleAsync`/`PersistContentScheduleAsync`) — a genuine "wire it up" increment.

**Technique worth reusing: mechanically diff the sync/async engine pair instead of eyeballing it.** A
~40-line Python script that extracts both method bodies and normalises away the expected differences
(`await`, `*Async` names, repo-call swaps, `.GetAwaiter().GetResult()` unwraps) proved faithfulness far
better than reading 530 lines: `StrategyCanPublishAsync` identical over 146 normalised lines,
`StrategyCanUnpublishAsync` identical, `CommitContentChangesInternalAsync` differing only by
`cancellationToken` threading. Do this on every future engine port.

**Two real attribution defects found by review and fixed (both pre-existing, one made more reachable):**
1. `AuditAsync` used the THROWING `_userIdKeyResolver.GetAsync(int)`. A `WriterId` of
   `Constants.Security.UnknownUserId` (0 — the ordinary state for imported/package content, since
   `umbracoContentVersion.userId` is nullable and NULL materialises as 0) has no matching `umbracoUser`
   row, so audit metadata could throw and roll back the entire scheduled-publish batch, killing scheduled
   publishing site-wide until someone hand-fixed the row. Fixed at the layer that owns it — `AuditAsync`
   in BOTH `AsyncPublishableContentServiceBase` and `PublishableContentServiceBase` now uses
   `TryGetAsync` and skips the audit entry with a `LogWarning`. **Rule: a content operation must never
   fail because its audit metadata could not be attributed.** (`AuditService.AddAsync` already failed
   softly this way, and `LogDto.UserId` already models unknown as null — only the eager resolve didn't.)
2. My own first fix attempt round-tripped `WriterId` int→Guid→int with a `SuperUserKey` fallback, which
   silently re-attributed unknown-writer content to the super user (-1 persists as -1; 0 round-trips to
   NULL correctly). Replaced with a private `PublishAsync(..., int userId, ...)` overload that
   `PerformScheduledPublishingRelease` calls with the raw `d.WriterId`, matching the `CommitContentChanges`
   calls on the sibling branches. **Rule: don't round-trip an int user id through a Guid when the caller
   already holds the int — the round trip is lossy for ids with no matching user.**

**Coherent rule that emerged for user-id handling**: `PublishAsync(Guid userKey)` still THROWS on an
unresolvable key (caller error), while `AuditAsync(int userId)` degrades gracefully (a `WriterId` read
from storage has "unknown" as a legitimate persisted state). Different provenance, correctly different
handling — worth stating explicitly in future increments.

**Deliberate non-change**: `StrategyUnpublishAsync` omits the sync original's provably-dead guard
(`// TODO: What is this check??` + `if (attempt.Success == false) return attempt;`). Reviewer suggested
making the pair byte-identical; declined — copying dead code into new code is worse, and editing the sync
method would break the "sync engine untouched" property the reviews relied on.

**Element footprint was smaller than expected**: `ElementService` just bridges (`PublishAsync` blocks on
`GetAsync` then calls its untouched sync `Publish`). BUT a gap the delegation briefs got wrong —
removing `Publish` from the SHARED `IPublishableContentService<TContent>` also broke every
`IElementService`-INTERFACE-typed test receiver, while concrete-`ElementService`-typed receivers kept
compiling against the still-present sync method. Result: `ElementServiceNotificationTests` (concrete type)
still calls sync `Publish`, `TagServiceTests`/`TrackedReferencesServiceElementTests` (interface type) use
`PublishAsync`. Correct, but check receiver TYPE not just receiver name when scoping a shared-tier removal.

**Delegation note**: 7 parallel subagents batched the ~500 test call sites by directory (2 biggest files
solo). Worked well, but three separate agents independently rediscovered the Element interface gap because
my brief asserted Element was unaffected — a wrong premise in a brief costs more than an uncertain one.

**Verified**: `dotnet build umbraco.sln -v minimal` 0 errors. 428/428 across `ContentServiceTests`/
`ContentServicePublishBranchTests`/`ContentServiceNotificationTests`/`ContentEventsTests`/
`ContentServiceVariantTests`/`ElementServiceNotificationTests`/`ContentPublishingService`; 11/11 `Audit`;
6546 passed + 6 skipped unit tests. Regression test proven to fail before the `AuditAsync` fix and pass
after (per CLAUDE.md §10). Two independent reviews (Sonnet + Opus) plus a third external review round.

**Current full retirement tally**: 61 members. Remaining on `IContentService`: `PublishBranch`. Remaining
on `IPublishableContentService` (shared with Element): `SaveAndPublish`, `Unpublish`,
`PerformScheduledPublish`, plural `Save`. The async engine now EXISTS — the next increments are
adoptions of `CommitContentChangesInternalAsync` rather than fresh engine work, which should make
`Unpublish` (the next-simplest, 2-branch culture handling + a result-type swap) considerably cheaper than
this one. Once all sync callers are gone, delete `CommitContentChangesInternal` and its three
`Strategy*` helpers outright (a `TODO (V19)` on the async engine already flags this).

### Next session: `Unpublish` retired — the first *adoption* of the async engine (commit `29651fc8446`)

Retired sync `Unpublish(TContent, string?, int)` from `IPublishableContentService<TContent>`, replaced with
`Task<PublishResult> UnpublishAsync(TContent, string? culture, Guid userKey, CancellationToken)` on
`IAsyncPublishableContentService<TContent>`. **31 files (8 prod, 23 test) — a quarter of the `Publish`
increment**, exactly as predicted, because the engine already existed: the two `CommitContentChanges` calls
simply became `CommitContentChangesAsync`. Prediction confirmed — *adoption increments are ~4x cheaper than
the increment that builds the engine.*

**Ordering rule that decided this increment, worth reusing**: pick by DEPENDENCY order, not by test-count.
`PerformScheduledPublishingExpiration` calls `Unpublish` internally, so `Unpublish` is a callee of the last
and most entangled remaining member — converting the callee first avoids reworking
`PerformScheduledPublishAsync` later. `SaveAndPublish` has a smaller blast radius (37 sites/3 files vs 86
sites/23 files) but no dependents, so it unblocks nothing. My own prior note called `Unpublish` "next
simplest" on structural grounds and was right, but for the wrong reason — its raw test count is the
*largest* of the three. Check the internal call graph before sizing by grep count.

**The receiver-type rule paid off immediately** (see [[feedback_shared_interface_removal_breaks_by_receiver_type]]).
Verified up front instead of assumed: 86 Document sites broke; all 3 Element sites live in
`ElementServiceNotificationTests.cs` whose fixture is *concrete*-typed, so they kept resolving Element's
untouched sync method and needed **zero** changes. Also caught a false positive before touching it —
`UnpublishElementControllerTests.cs:52` matches `.Unpublish(` but is an unrelated *controller action*.

**The unknown-writer fix carried forward cleanly**: same public-Guid-wrapper + private-int-overload split as
`PublishAsync`, so `PerformScheduledPublishingExpiration` passes `d.WriterId` through with no lossy Guid
round-trip. Note for accuracy (both reviewers flagged my framing): the accompanying test is a **regression
guard against reintroducing** last increment's defect, NOT a CLAUDE.md §10 bug-fix test — HEAD already
passed `WriterId` through, so the test only fails against the naive round-trip alternative. Describe such
tests as guards, not fixes.

**Two defect classes I caught proactively this time** — both were review findings on the `Publish`
increment, so the pattern is now predictable and should be checked *while* converting, not after:
1. A validation-bypass path: `ContentPublishingServiceBase.UnpublishAsync`'s early
   `return Attempt.Fail(CultureMissing)` reaches none of the three helpers, so the up-front
   `_ = await _userIdKeyResolver.GetAsync(userKey)` is load-bearing and must be kept when the helpers stop
   taking a resolved `int userId`.
2. A dangling `/// <inheritdoc />` on Element's now-uninherited sync `Unpublish` in
   `PublishableContentServiceBase` — removing a member from an interface orphans the `<inheritdoc/>` on
   every concrete implementation that survives. **Always grep the surviving implementations for
   `<inheritdoc` after removing an interface member.**

**Bonus cleanup**: passing `userKey` straight through let `UnpublishTrashedElementOnRestore` drop its
`IUserIdKeyResolver` parameter, which orphaned the `_userIdKeyResolver` *fields* in both
`ElementEditingService` and `ElementContainerService` (constructor params stay — both forward to `base(...)`).
Same shape as the `_documentRepository` cleanup in the `Copy` increment; reference-type fields that are
assigned-but-never-read produce NO compiler warning, so grep for them explicitly.

**Scripted-conversion note**: the 86 sites were uniform enough to script (59 one-arg relying on the
`culture = "*"` default, 27 two-arg, 0 with explicit userId), but the script mis-bound ONE call —
`Unpublish(content, userId: -1)`, a *named* argument sitting in the culture's positional slot. Caught by the
compiler, not by review. Reinforces [[feedback_regex_bulk_edit_overmatch]]: named arguments defeat
positional arg-splitting. The post-pass diff audit (categorise every changed line, require zero
"unexplained") again showed 0 strays.

**Verified**: `dotnet build umbraco.sln -v minimal` 0 errors. Integration 756/756 (core service suites) +
1005/1005 (all `Umbraco.Search`); unit 6546 passed/6 skipped (baseline). Engine port mechanically diffed
**identical over 75 normalised lines** ([[feedback_mechanically_diff_ported_method_pairs]]). Two independent
reviews (Sonnet + Opus) — **zero defects**, two minor nits applied (stale TODO list, AAA comments).

**Flaky-test note**: `RecurringHostedServiceBaseTests.Loop_Executes_Periodically_And_Respects_Cancellation`
failed once while two integration suites saturated the CPU, then passed in 8ms isolated and on a quiet
re-run. Timing-sensitive, unrelated to content services — don't chase it, but do re-run on a quiet machine
rather than waving it off.

**Current full retirement tally**: 62 members. Remaining on `IContentService`: `PublishBranch`. Remaining on
`IPublishableContentService` (shared with Element): `SaveAndPublish`, `PerformScheduledPublish`, plural
`Save`. **Recommended next**: `SaveAndPublish` (2 prod call sites, 37 test sites/3 files, no dependents,
2 overloads — public multi-culture delegating to a private single-culture one). Then
`PerformScheduledPublish` LAST, since it depends on everything else and is already half-converted
internally (its release and expiration paths now both block on the async engine via
`.GetAwaiter().GetResult()`, which its own conversion should finally remove).

### Next session: `SaveAndPublish` retired — smallest increment yet (commit `b0647ef53aa`)

Retired sync `SaveAndPublish(TContent, string[], int)`, replaced with
`Task<PublishResult> SaveAndPublishAsync(TContent, string[] culturesToPublish, Guid userKey, CancellationToken)`.
**9 files (7 prod, 2 test)** — down from 31 (`Unpublish`) and 74 (`Publish`). The cost curve across the three
is 74 → 31 → 9: building the engine dominates, adoption is cheap, and each adoption shrinks the next.

**The key design call: NO private int-userId overload here, deliberately.** `PublishAsync`/`UnpublishAsync`
have one *solely* because `PerformScheduledPublishing{Release,Expiration}` pass `d.WriterId` (possibly
`UnknownUserId` 0) which must not be Guid-round-tripped. `SaveAndPublish` has no `WriterId` caller — verified
by grep, and re-verified independently by both reviewers — so an int overload would be **dead API**. Adding
one "for consistency" would be cargo-culting. **General rule: copy a neighbour's shape only when the reason
for that shape applies.** Brief reviewers on such deviations explicitly, with the premise to verify rather
than accept, or they get reported as drift.

There IS still a public→private overload pair, for an unrelated pre-existing reason: the public
multi-culture method delegates to a private single-culture one for invariant content. Both converted; both
now take `Guid userKey`; each call path resolves exactly once (the invariant path returns before the public
overload's resolve line and resolves inside the private one instead).

**`Assert.Throws` → `Assert.ThrowsAsync`: 9 sites, and the planning grep under-counted (predicted 4).** Worth
knowing *why* the conversion is required, since it isn't obvious: in an `async` method, exceptions thrown
**before the first await** are still captured into the returned `Task` rather than thrown synchronously. So
a leftover sync `Assert.Throws` does NOT pass vacuously — NUnit reports "expected exception, none thrown" —
but it does fail spuriously. `ThrowsAsync` is both required and sufficient.

**Collapsed a degenerate parameterised test.** `Can_SaveAndPublish_With_Different_User_Ids(int userId)` had
`[TestCase(Constants.Security.SuperUserId)]` + `[TestCase(-1)]` — and `SuperUserId` **is** `-1`, so both
cases passed the identical value. `[TestCase]` can't carry a `Guid` (attribute args must be compile-time
constants). Collapsed to a single `[Test] Can_SaveAndPublish_As_The_Super_User()`; both reviewers confirmed
no coverage lost. **When a Guid conversion hits an int-parameterised test, check whether the parameterisation
was ever meaningful before inventing string-encoded-Guid ceremony to preserve it.**

**Process lesson — update the "remaining callers" TODO as part of the conversion.** The
`// TODO (V19): ... once every caller of the sync engine (X, Y) has an async equivalent` comment on
`CommitContentChangesInternalAsync` went stale for the **second consecutive increment**, caught by review
both times. Retiring a member that appears in such a list means editing the list in the same change.

**Fourth consecutive orphaned `/// <inheritdoc />`** on Element's surviving sync method
(`PublishableContentServiceBase.cs:936`) — caught proactively this time by grepping before review.
The rule in [[feedback_shared_interface_removal_breaks_by_receiver_type]] is now four-for-four.

**Receiver-type rule again, and it saved work**: all 3 `ElementServiceNotificationTests.cs` sites are
concrete-`ElementService`-typed → **zero** Element test changes needed. Only the 34 Document sites broke
(31 `ContentServiceTests` + 3 `ContentServiceNotificationTests`, both concrete-`ContentService`-typed, which
DOES break because the method is *replaced* on the async base rather than merely removed from an interface).

**Verified**: `dotnet build umbraco.sln -v minimal` 0 errors. Both overloads mechanically diffed
**identical** (46 and 40 normalised lines). Integration 495/495 targeted; unit 6546 passed/6 skipped.
Test-diff audit: 75 changed lines, zero unexplained. Two independent reviews — **zero defects**, one nit
(the stale TODO) already fixed.

**The `RecurringHostedServiceBaseTests` flake is reproducible under CPU load** — two failures while the
integration suite ran concurrently, then 27/27 in 731ms isolated and 6546 clean on a quiet re-run. Same
class as last increment. Don't chase it; do re-run quiet before believing it.

**Current full retirement tally**: 63 members. Remaining on `IContentService`: `PublishBranch`. Remaining on
`IPublishableContentService`: `PerformScheduledPublish`, plural `Save`. **`PerformScheduledPublish` is now
the SOLE remaining caller of the sync engine** (`AsyncPublishableContentServiceBase.cs:1107` and `:1218`) —
converting it retires `CommitContentChangesInternal`, `CommitContentChanges` and the three `Strategy*`
helpers outright, which is the real prize. It is also already half-converted internally: its release and
expiration paths both block on the async engine via `.GetAwaiter().GetResult()`, and its own conversion
should finally remove those two bridges. Note it returns `IEnumerable<PublishResult>` and is called from
`ScheduledPublishingJob` — decide `Task<IEnumerable<...>>` vs `IAsyncEnumerable` when planning it.

> **The "SOLE remaining caller" claim above was WRONG** — see the next session. It came from a grep scoped
> to `AsyncPublishableContentServiceBase.cs` only. `PublishBranch` also uses the sync engine, from
> `ContentService.cs`.

### Next session: `PerformScheduledPublish` retired (commit `e511ebfe231`) — and a planning error worth keeping

Retired sync `PerformScheduledPublish(DateTime)`, replaced with
`Task<IEnumerable<PublishResult>> PerformScheduledPublishAsync(DateTime, CancellationToken)`. 10 files.
Not just an engine adoption: both scheduling helpers also moved off the sync NPoco `_contentRepository`
onto `_asyncContentRepository`, so their reads/writes are now genuinely async. Removed the two
`.GetAwaiter().GetResult()` bridges left by the `Publish`/`Unpublish` increments, and made
`ScheduledPublishingJob.ExecuteAsync` genuinely async (it previously returned `Task.CompletedTask`).

**THE LESSON — I scoped a grep to one file and drew a repo-wide conclusion.** The plan's headline claim was
that this increment would make the ~600-line sync engine dead and delete it. I verified that by grepping
`AsyncPublishableContentServiceBase.cs` for callers and finding only `PerformScheduledPublish*`. But
`CommitContentChangesInternal` is `protected`, so **subclasses can call it from other files** —
`ContentService.cs` does, twice: `PublishBranchItem` (`:802`, part of the still-sync `PublishBranch`) and
`CommitDocumentChanges` (`:462`, internal, test-only callers). I attempted the deletion, the build failed
with 3 errors, and I reverted it. **Rule: when asking "does anything still call X", scope the grep to the
repo, not to the file X lives in — especially for `protected`/`internal` members, where the whole point is
that other files can reach them.** Cheap habit: `git grep -n "X(" -- '*.cs'` before ever writing "sole
remaining caller" in a plan. Related but distinct from [[feedback_verify_code_path_actually_executes]].

**Consequence for the roadmap**: the sync engine survives until `PublishBranch` is converted, so
**`PublishBranch` is now the increment that finishes the arc**, and it is bigger than previously assumed —
it owns both the last `IContentService` sync member *and* the ~600-line engine deletion
(`CommitContentChanges`, `CommitContentChangesInternal`, `StrategyCanPublish`, `StrategyCanUnpublish`,
`StrategyUnpublish` — NOT `StrategyPublish` or `GetPublishedDescendantsLocked`, which the async engine
shares). `CommitDocumentChanges` (internal, 4 test callers, no production callers) must be dealt with in
that increment too — convert or delete it, it is the other sync-engine caller.

**Recovering from the revert**: I restored the file by copying a saved snapshot over it rather than via
git, which makes "the file is correct" an assumption. I briefed both reviewers to check for truncation,
duplicated members or lost edits specifically; both proved cleanliness the same way — `git diff HEAD` on
that file showed hunks *only* inside the three converted methods, which is positive proof the remaining
~2,800 lines are byte-identical. **Good technique for validating any snapshot-restore.**

**Design decisions, both upheld by review:**
- `Task<IEnumerable<PublishResult>>`, **not** `IAsyncEnumerable` — each helper closes its `ICoreScope`
  (holding `WriteLockIds`) before returning, so streaming would force the write lock to stay open across
  consumer iteration. The sole consumer `.GroupBy()`s the whole set anyway.
- `Lazy<List<ILanguage>>` → `Lazy<Task<List<ILanguage>>>`, shared across both helpers. Preserves laziness
  (both `await allLangs.Value` sites sit inside `if (await Has…Async(...))`, so a no-work tick fetches
  nothing) and single-execution (default `ExecutionAndPublication`; the async lambda returns at its first
  await so the init lock is never held across I/O).
- `GetContentSchedule(d.Id)` → `GetContentScheduleAsync(d.Key, ct)` — third use of this swap. Both reviewers
  re-verified equivalence field-by-field this time, including the `?? InvariantCulture` fallback and the
  Release/Expire ternary, plus that both call sites source `d` from `GetContentFor{Expiration,Release}Async`
  so `.Key` is always populated.

**The Moq trap that didn't fire, and the better argument for why**: `ScheduledPublishingJobTests` has
`.Verify(...)` but **no `.Setup`** for the member, so a null `Task` would NRE on `await`. Moq's loose-mock
default returns `Task.FromResult(<empty>)`, so it's fine — but the *proof* is better than the theory:
`Executes_And_Performs_Scheduled_Publishing` asserts `Times.Once()` on the **element** service, which is
called *after* the awaited content call inside a catch-all `try`. An NRE on the content await would be
swallowed and the element assertion would fail. It passes, so the await path demonstrably runs. Opus noted
an explicit `.Setup` would still be more robust against a future Moq default-value change — not added.

**Also**: I skipped the standing review step and reported before spawning reviewers; the user had to
prompt. The failed deletion derailed the routine. **Run the reviewers even when — especially when — the
increment went sideways.** See [[feedback_parallel_review_and_test_agents]].

**Verified**: `dotnet build umbraco.sln -v minimal` 0 errors/0 warnings. Both helper bodies mechanically
**identical** (53 and 93 normalised lines). Integration 231/231 across ContentService/ElementService/
notification suites, 7/7 scheduled-publishing filters; `ScheduledPublishingJobTests` 3/3. Two independent
reviews — zero defects; one applied nit (a comment explaining laziness via the caller, one altitude below
the service layer per CLAUDE.md §9).

**Left deliberately**: `protected virtual CommitContentChanges` (`:88`) now has **zero** callers repo-wide.
Opus's framing is worth keeping — it is not merely dead, it is a `protected virtual` extension seam where a
derived override would **silently do nothing**. Delete it with the rest of the engine in the `PublishBranch`
increment.

**Current full retirement tally**: 64 members. Remaining on `IContentService`: `PublishBranch`. Remaining on
`IPublishableContentService`: plural `Save`. **Next and final: `PublishBranch`** — the last sync member of
the publish family, the owner of the engine deletion, and the thing that ends this arc.

## Session of 2026-09-17: `PublishBranch` retired AND the sync engine finally deleted — the publish arc is closed

Two commits: `4894c6f7ffb` (conversion, 29 files) and `e7cef04c3a5` (deletion, −655 lines). Not yet pushed.

**Commit A** converted the three-method chain in `ContentService.cs` plus `CommitDocumentChanges`:
- `public PublishBranch(..., int userId)` → `PublishBranchAsync(..., Guid userKey, CancellationToken)`
- `internal PublishBranch(document, shouldPublish, publishCultures, int userId)` → `...Async(..., int userId,
  CancellationToken)` — **deliberately keeps `int userId`**; it is internal and the public wrapper resolves
  the key once, mirroring the `SaveAndPublish` public→private shape.
- `private PublishBranchItem(..., out IDictionary<string, object?>? initialNotificationState)` →
  `PublishBranchItemAsync(...)` returning a **named tuple**
  `(PublishResult? Result, IDictionary<string, object?>? NotificationState)`. Async methods cannot have `out`
  parameters — this was the one genuinely structural production change in the whole increment. The user
  rejected the first ExitPlanMode specifically to require a *named* tuple over a positional one.
- `internal CommitDocumentChanges` → `CommitDocumentChangesAsync(IContent, Guid userKey, CancellationToken)` —
  **converted, not deleted**, even though it has zero production callers: it is `internal` (so
  [[feedback_public_api_survives_even_with_zero_internal_callers]] doesn't protect it) but 4 tests exercise
  real commit behaviour and deleting it would mean deleting them.
- Sole production call site `ContentPublishingService.cs`; its now-dead `_userIdKeyResolver` **field** removed
  while the **constructor parameter stays** (the base class still needs it).

**Commit B** deleted `CommitContentChanges`, `CommitContentChangesInternal`, `StrategyCanPublish`,
`StrategyCanUnpublish`, `StrategyUnpublish` from `AsyncPublishableContentServiceBase.cs`. Survivors, all
verified to still have live callers: `StrategyPublish`, `GetPublishedDescendantsLocked`, the sync `Audit`
bridge and `_contentRepository` — the plural `Save(IEnumerable<TContent>, int)` is genuinely synchronous and
uses them, so **this increment does NOT remove the sync repository dependency**. `PublishableContentServiceBase.cs`
(Element's separate hierarchy) untouched throughout.

**A deletion script bug worth remembering**: my first brace-matching deletion helper found "the first `{`
after the signature" — but `CommitContentChanges` was **expression-bodied** (`=> CommitContentChangesInternal(...)`),
so it matched the *next* method's brace and silently deleted 70 lines instead of 9. Caught it by sanity-checking
the reported line counts against what I'd read. Fixed by paren-matching the parameter list first, then branching
on whether `=>` or `{` follows. **Verify a scripted deletion with a member-declaration set-diff** (`grep` all
declarations in `HEAD:file` vs the working tree and `diff` the two sorted lists) — that proved exactly the five
intended members left and nothing else, and it also exposed that a 130-insertion/792-deletion diffstat was pure
alignment noise, not real re-additions.

**Orphaned crefs are a recurring tax of this campaign.** The Sonnet reviewer found the `<remarks>` on
`CommitDocumentChangesAsync` still said `CommitContentChangesInternal` and `<see cref="CommitDocumentChanges" />`.
Fixing those surfaced that `<see cref="Unpublish" />` in the same block had been orphaned by an *earlier*
increment. And my replacement `<see cref="UnpublishAsync" />` **still emitted CS1574** — an unqualified cref
doesn't resolve to a member inherited through a generic base/interface; it needed
`<see cref="IAsyncPublishableContentService{TContent}.UnpublishAsync" />`. **Build with `--no-incremental` and
grep CS1574 after any rename** — nothing else catches this.

**Opus found a real latent bug, left as a TODO rather than fixed here** (`ContentService.cs:773`):
`initialNotificationState` is allocated empty and never written to, so the branch `ContentPublishedNotification`
always carries an empty state dictionary and cannot see what a `ContentSavingNotification` handler wrote —
while `savingNotification.State`, which handlers *do* write to, goes into the engine and is dropped for the
branch-level notification. Works for single-document publishes, silently no-ops for branch publishes.
**Unchanged from HEAD** — the `out` parameter behaved identically — so the translation is faithful. User asked
for it as a TODO: `// TODO: ... Return savingNotification.State instead. [NL]`. Needs its own increment with a
failing-first test.

**Verified**: `dotnet build umbraco.sln` 0 errors. Mechanical normalised diff of all four converted methods —
every remaining line is signature / userKey resolve / blocking→await / `out`→tuple / token threading. Test-diff
audit by category: zero unexplained lines; 138 call sites before, 138 after. Integration 382/382 post-Commit-B
across the PublishBranch/Notification/PublishStatus/DocumentUrl/PublishedUrlInfo/ContentServiceTests filters.
Opus independently normalised each **deleted** sync body against its surviving async port and confirmed no
logic is missing (`StrategyUnpublish`'s async port correctly drops a provably-dead guard the sync body's own
`// TODO: What is this check??` flagged), and confirmed every surviving async method is **byte-identical to
HEAD** — Part B is a pure deletion.

**Current full retirement tally**: 66 members. `IContentService` has **no sync members left to retire**.
Remaining on `IPublishableContentService`: the plural `Save(IEnumerable<TContent>, int)` — the last one, and
the thing still holding `_contentRepository`, the sync `Audit` bridge and `GetPublishedDescendantsLocked` alive.
Retiring it should free all three and finish the base class.

## 2026-09-22: THE CAMPAIGN IS FINISHED — NPoco's DocumentRepository is deleted

Work done by the user away from this session. 189 commits on top of `e7cef04c3a5`, **all pushed**
(`origin/v18/feature/ef-core-document-repository..HEAD` is empty). Includes a merge of
`v18/feature/ef-core-repositories`, which itself merged `origin/v19/dev` — so a large slice of those 189 is
upstream traffic, not campaign work. Verified against the tree, not just commit subjects.

**The goal is reached.** `src/Umbraco.Infrastructure/.../Implement/EFCore/DocumentRepository.cs` is the only
`DocumentRepository` left; the NPoco one and its blueprint sibling are gone
(`a575882b0c7`, `9b056cfd622`). Every `NPoco` string remaining in the EF Core file is a **provenance comment**
("mirrors NPoco's …"), not a dependency. The rename-at-the-end this memory predicted from the start actually
happened (`a682ba34072`): `AsyncDocumentRepository`/`IAsyncDocumentRepository` →
`DocumentRepository`/`IDocumentRepository`. `dotnet build umbraco.sln` — 0 errors.

**The closing sequence, in order:**
`3d88cbfcc99` retire the bulk `Save` → `7badae81e91` rename `IAsyncPublishableContentService` →
`IPublishableContentService` → `a033dedc79b` drop dependencies the conversion left unused →
`73b06855ed8` port `GetPublishedDescendantsLocked` off NPoco → `9b056cfd622` + `a575882b0c7` delete the NPoco
repositories → `a682ba34072` drop the `Async` prefix. Plus real fixes found along the way:
`3547ab46c60` (three EF Core caching defects), `bd32be068d2` (a vetoed version delete was not reported to the
caller), `5098d9f9af1` (descendant path predicate anchoring), `56040b87ab2`/`ae068c2c50e` (async navigation
service root-set parity + snapshot-per-mutation).

**The end-state shape** (differs from what I was analysing in the prior session — don't reason from that):
- `IPublishableContentService<TContent>` is now the **async** contract; it took the retired sync contract's
  name. `IPublishableContentService<TContent> : IAsyncContentServiceBase<TContent>`.
- `IContentService : IContentServiceBase, IPublishableContentService<IContent>` — and `IElementService` the
  same shape. So the `CheckDataIntegrity` question I raised was settled by **declaring `IContentServiceBase`
  directly** (option (a) of my analysis), not by going async on the health check.
- `PublishableContentServiceBase<TContent>` (Element's sync engine) **no longer implements any interface** —
  plain `RepositoryService`. `ElementService` still bridges: 10 `GetAwaiter().GetResult()` call sites.
- `ContentService : AsyncPublishableContentServiceBase<IContent>, IContentService`.
- `_contentRepository` is **gone** from `AsyncPublishableContentServiceBase` — retiring the bulk `Save`
  severed the sync repository dependency exactly as predicted.

**Two loose ends I verified are still open (neither is a blocker):**
1. `AsyncPublishableContentServiceBase.cs:1854` — `protected void Audit(...)` now has **zero callers
   repo-wide** (the bulk `Save` was its last one). Protected, so no diagnostic. Its sibling `AuditAsync` is the
   live one.
2. The naming pass stopped short of the service/base tier. Still `Async`-prefixed:
   `AsyncPublishableContentServiceBase`, `IAsyncContentServiceBase` (+`{TContent}`),
   `IAsyncPublishableContentRepository`, `AsyncPublishableContentRepositoryBase`, `AsyncContentRepositoryBase`,
   `AsyncPermissionRepository`. Some of these (`AsyncMigrationBase`, `AsyncLocal`, `IAsyncComponent`) are
   unrelated and must not be swept up.
3. My branch-publish notification-state TODO survives at `ContentService.cs:763` — still unfixed, still wants
   its own increment with a failing-first test.

## 2026-09-29: `ContentVersionDto.Key` removed again

Everything above about `contentVersionDto.Key = Guid.NewGuid()`, `UpdatableColumnNames`, `AddContentVersionKeyColumn` and the Guid-keyed `GetVersionAsync`/`DeleteVersionAsync` is history: the version key was removed by decision on 2026-09-29. See [[project_content_version_has_no_guid_key]].

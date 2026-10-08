# Campaign: Element repository and service from NPoco to EF Core

Status board (live): https://claude.ai/artifact/Q5Bttu57VfLyS1ajt7nKtU (rendered from `status.json` next to this file; the same URL is stored there under `artifact.url`).
Written 08-10-2026 on `v18/feature/ef-core-repositories` at `f1583e70294`. Dates in this file are DD-MM-YYYY.

This folder is a personal handover area. It lives on the orphan branch `campaign/ef-core-element-repository`, checked out as a git worktree at `.claude/campaign/` inside the main checkout (that path is gitignored there), so it never appears in a PR diff or on the shared integration branch. §10 says how to set it up on another machine and how it is removed at the end.

This document is written for agents. Read it top to bottom before touching any file in the campaign. It records the goal, the end state, the rules that apply on this branch, the PR sequence, and the bookkeeping every PR must do (including updating the progress page).

---

## 1. Goal

Replace the NPoco `ElementRepository` with an EF Core repository and move `ElementService` onto the asynchronous engine, exactly as was done for documents in PR #24001, but delivered as a sequence of small, independently reviewable PRs instead of one 433-file PR.

When the campaign is finished:

- `IElementRepository` is an EF Core, async, Guid-keyed contract: `IElementRepository : IAsyncPublishableContentRepository<IElement>`.
- `ElementRepository` lives in `src/Umbraco.Infrastructure/Persistence/Repositories/Implement/EFCore/` and is a thin subclass of `AsyncPublishableContentRepositoryBase<IElement, ElementRepository, EFCoreDtos.ElementDto, EFCoreDtos.ElementVersionDto, EFCoreDtos.ElementCultureVariationDto>`, the same shape the NPoco `ElementRepository` has today (about 140 lines over the NPoco base).
- `ElementService : AsyncPublishableContentServiceBase<IElement>, IElementService`, with no synchronous bridging members.
- The synchronous `PublishableContentServiceBase<TContent>` and the NPoco `PublishableContentRepositoryBase<...>` and `IPublishableContentRepository<T>` are deleted (ElementService and ElementRepository are their last users).
- Every Element integration and unit test fixture is green on SQLite; the SQL Server run is green except for the known-flaky search jobs (see §3).

## 2. Why the shape is different from the Document campaign

The Document campaign built the async engine while converting: `AsyncPublishableContentServiceBase` grew member by member over ~60 increments because nothing existed yet. For Elements the engine already exists and is complete. The service swap is therefore one step, not sixty. The real work is in the repository layer, and specifically in one design problem:

`DocumentRepository.cs` (EF Core, 2 530 lines) is written against the concrete `DocumentDto` / `DocumentVersionDto` / `DocumentCultureVariationDto` types and `db.Documents` etc. (75 concrete references). The generic base `AsyncPublishableContentRepositoryBase` has the right type parameters but only ~16 uses of them; almost everything that NPoco's `PublishableContentRepositoryBase` does generically was implemented concretely in `DocumentRepository` because it was the only subclass.

Decision (see §8): hoist the generic logic into the base first, then add `ElementRepository` as a thin subclass. The alternative (copy-adapt `DocumentRepository` into a 2 000-line `ElementRepository`) was rejected: it creates a second fork that goes stale on every merge-up (this branch has already lost five fixes to exactly that failure mode, see memory `project_async_fork_merge_hazard`), and it contradicts the NPoco side, which has had a common base since #21637.

## 3. Rules that apply (read even if you know the repo)

Repo-wide: `/CLAUDE.md`, `/src/Umbraco.Core/CLAUDE.md`, `/src/Umbraco.Infrastructure/CLAUDE.md` §12 (EF Core DTO guide), `/tests/Umbraco.Tests.Integration/CLAUDE.md`.

Branch-specific decisions already made during the Document campaign. Do not relitigate them:

1. Breaking changes are allowed on this branch. The work ships in Umbraco 20 even though the integration branch is still named `v18/feature/ef-core-repositories` and `version.json` says 19; any `[Obsolete]` text that is unavoidable says "Scheduled for removal in Umbraco 22". Change signatures directly; no obsolete-constructor or obsolete-overload staging; delete sync members outright.
2. Every new repository member takes Guid keys, never int ids, even when the column and first caller are int-based. Resolve internally.
3. Do not port `IQuery<T>`. The EF Core side exposes purpose-built query shapes only (`GetChildrenAsync`, `GetDescendantsAsync`, `GetPagedOfContentTypesAsync`, ...). Re-express each `Get(query)` call site against one of those.
4. EF Core code never references a NPoco repository class, not even a static helper. Copy the helper.
5. `UmbracoDbContext` is globally NoTracking. Read-then-mutate-then-save needs `.AsTracking()`. Materialisation bypasses DTO setters; use `UsePropertyAccessMode(Property)` where setter logic matters.
6. Never use `is`-patterns inside an EF Core query lambda (CS8122). Modern C# (property patterns, `is null`, collection expressions) everywhere else in files this campaign rewrites.
7. No `autoComplete: true` on scopes; call `scope.Complete()` explicitly.
8. `CancellationToken` is the last parameter on every new async member.
9. A new `*Async` member goes on the interface tier where its sync counterpart was declared (wide `IAsyncContentServiceBase<T>` vs publishable-only `IPublishableContentService<T>` vs `IElementService`).
10. Async-named scaffolding classes (`IAsyncElementRepository`, `AsyncElementRepository`) stay pure async; no sync bridges on them. They are temporary and get renamed in PR 6.
11. Style: Allman braces, braces on every body, no section-header or Arrange/Act/Assert comments, no provenance comments ("mirrors NPoco's ..." is a provenance comment: do not add new ones), `[]` for empty enumerables.
12. Content versions have no Guid key. `umbracoContentVersion` is int-identified; never reintroduce a version key.
13. `Constants.System.RootKey` is null. `IIdKeyMap` cannot resolve `-1`; root-level handling must special-case `null` parent keys.
14. SQL Server search integration jobs are flaky on every branch. Diff the failed list against the parent branch's last run before treating a failure as a regression.

Contract rules (the shape every member on the async side must have; these came out of the Document campaign and are not negotiable):

- Repository read surface is `IAsyncReadWriteRepository<Guid, TEntity>`: `GetAsync(Guid? key, ct)`, `GetManyAsync(Guid[] keys, ct)`, `GetAllAsync(ct)`, `ExistsAsync(Guid key, ct)`, `SaveAsync(entity, ct)`, `DeleteAsync(entity, ct)`. There is no `Get(int)`, no `GetMany(params int[])` and no `GetAll(params int[])`; the old "GetAll with ids" overload is split into `GetAllAsync()` and `GetManyAsync(Guid[])`.
- No `IQuery<T>` parameter anywhere, no `out long totalRecords`, no `pageIndex`/`pageSize`. Paged members take `int skip, int take, Ordering? ordering` and return `PagedModel<T>`.
- Keys are `Guid`; the only `int` identifiers allowed are version ids (`umbracoContentVersion` is int-identified by decision).
- Users are identified by `Guid userKey`, never `int userId`; resolve through `IUserIdKeyResolver` inside the service.
- Service members that can fail return `Attempt<TStatus>` or `Attempt<TResult, TStatus>` with a bespoke `*OperationStatus` enum from `Services/OperationStatus/` (`ContentSaveOperationStatus`, `ContentDeleteOperationStatus`, `ContentRollbackOperationStatus`, `ContentScheduleOperationStatus`, `ContentDeleteOfTypesOperationStatus`, ...). No `OperationResult`, no thrown exceptions for expected failures, no `Attempt<OperationResult?>`. `PublishResult` stays for the publish family because the backoffice consumes it.
- Every async member takes `CancellationToken` as its last parameter and is named `*Async`. No sync bridge (`Task.FromResult(sync)`, `.GetAwaiter().GetResult()`) survives on a class that implements the async contract.
- Notifications are published with `PublishCancelableAsync`/`PublishAsync` where the engine already does so; mirror `ContentService`, not the sync base.

The `IElementService` contract itself already has this shape (it carries no members of its own; everything is on `IPublishableContentService<IElement>`). What this campaign retires is the 29 public sync members `ElementService` still exposes through its concrete type via `PublishableContentServiceBase` (`GetById(int)`, `GetByIds(IEnumerable<int>)`, `GetPagedOfType(s)` with `IQuery`/`out total`, `Save`/`Publish`/`Unpublish`/`Delete` with `int userId` returning `OperationResult`, `Rollback(int, int, ...)`, `DeleteVersion(s)(int ...)`, `Count*(int parentId)`, `HasChildren(int)`, `GetParent`, ...) and the sync, int-keyed `IElementRepository`. PR 5's acceptance includes a grep proving none remain.

Build and test discipline:

- `dotnet build umbraco.sln` fresh before trusting any green. With the .NET 11 SDK the first solution build can fail with MSB3030 on `appsettings-schema.Umbraco.Cms.json`; run it again.
- Never trust `--no-build`. Rebuild the test project itself after a subagent edits production code.
- `dotnet build` rewrites three `package-lock.json` files; `git checkout --` them before committing.
- `dotnet format style <project>.csproj --diagnostics IDE0005 --severity info --include <touched files>` per project, touched files only.
- `git status --short` before every commit; investigate stray diffs.
- Tests for a fix must fail before the fix. Verify by reverting the production change once.
- EF Core migrations: use the `/umb-ef-core-empty-migration` skill. `appsettings.Local.json` overrides environment variables (Program.cs adds it last); move it aside while running `dotnet ef`, restore afterwards. The scaffolder may emit `_<timestamp>_Name` as the class name; rename it in the Designer file to the plain name.

Process (validated on the Document campaign):

- Research before delegating: read the NPoco reference method end-to-end and the EF Core precedent, then write the subagent brief with verbatim snippets and `file:line` references.
- One phase per subagent, TDD inside the phase, and independently re-verify every claim (`git diff --stat`, rebuild, rerun the full fixture, read the riskiest code).
- After every PR's implementation, spawn two agents in one message: an independent reviewer (3 axes: behaviour, convention alignment, duplication) and a mechanical test runner. Unconditional, even for small PRs.

## 4. Current inventory (verified 08-10-2026)

Repository layer (NPoco, to be replaced):

| File | Lines | Notes |
|---|---|---|
| `src/Umbraco.Core/Persistence/Repositories/IElementRepository.cs` | 11 | `: IPublishableContentRepository<IElement>` (sync, int-keyed) |
| `src/Umbraco.Infrastructure/Persistence/Repositories/Implement/ElementRepository.cs` | 138 | Overrides: `This`, `BuildEntityDto`, `BuildEntity`, `OnUowRefreshedEntity` (fires `ElementRefreshNotification`), `NodeObjectTypeId`, `GetEntityDeleteClauses` (nulls `umbracoUserGroup.startElementId`, deletes `umbracoElement`, `umbracoElementCultureVariation`, `umbracoElementVersion` rows), `IsPathPublished` (`content is { Trashed: false, Published: true }`), `RecycleBinId = -22`, `RecycleBinCacheKey` |
| `src/Umbraco.Infrastructure/Persistence/Repositories/Implement/PublishableContentRepositoryBase.cs` | 2 090 | NPoco generic base; after this campaign has no subclasses |
| `src/Umbraco.Core/Persistence/Repositories/IPublishableContentRepository.cs` | | Only user is `IElementRepository` |

Consumers of `IElementRepository` (production): `ElementService` (2 call sites: `Get(query)` in `DeleteOfTypes`, `Delete` in `DeleteLocked`) and `ElementContainerService` (4 call sites: `Get(query)` for trashed root elements at line ~234, `Get(int)` + `Save` at ~326-341, `Get(int)` + `Delete` at ~499-501). Nothing else.

Service layer:

| File | Lines | Notes |
|---|---|---|
| `src/Umbraco.Core/Services/IElementService.cs` | 15 | `: IContentServiceBase, IPublishableContentService<IElement>`; no members of its own |
| `src/Umbraco.Core/Services/ElementService.cs` | 346 | `: PublishableContentServiceBase<IElement>`; every interface member is a bridge onto the sync engine |
| `src/Umbraco.Core/Services/PublishableContentServiceBase.cs` | 2 526 | Sync engine, implements no interface, ElementService is its only subclass |
| `src/Umbraco.Core/Services/AsyncPublishableContentServiceBase.cs` | 2 227 | The async engine ContentService uses; abstract hooks: `ContentObjectType`, `ReadLockIds`, `WriteLockIds`, `SupportsBranchPublishing`, `Logger`, `DeleteLockedAsync`, 16 notification factories, `CheckDataIntegrityAsync`, `DeleteOfTypesAsync` |

Everything else that touches elements goes through `IElementService`, which is already async: `ElementEditingService`, `ElementPublishingService` (`ContentPublishingServiceBase<IElement, IElementService>`), `ElementVersionService`, `ElementContainerService`, `ContentTypeService`, `ScheduledPublishingJob`, `PackageDataInstallation`, `CreatedPackageSchemaRepository`, block property value handlers, `ElementPickerPropertyEditor`, Management API controllers. None of these change signature in this campaign.

Out of scope, same as for documents (list for the follow-up backlog, do not pull in):

- `IElementVersionRepository` / `ElementVersionRepository` (NPoco `ContentVersionRepositoryBase`; `DocumentVersionRepository` is also still NPoco). Migrate both together later.
- `IElementContainerRepository` / `ElementContainerRepository` (NPoco `EntityContainerRepository`; all container repositories are still NPoco).
- `ElementCacheService` (`IDatabaseCacheRepository`, NPoco).
- `ElementNavigationService`, `ElementPublishStatusService`, `ElementRecycleBinQueryService` (no `IElementRepository` dependency).

EF Core DTO layer: no Element DTOs exist yet. `Dtos/EFCore/` has `DocumentDto : IPublishableContentDto<DocumentVersionDto>`, `DocumentVersionDto : IContentVersionDto`, `DocumentCultureVariationDto` (no interface; the base constrains `TContentCultureVariationDto : class, new()`). NPoco side has `ICultureVariationDto`; EF Core side does not.

Tests:

| Fixture | Lines | Tests | Fate |
|---|---|---|---|
| `tests/.../Persistence/Repositories/ElementRepositoryTest.cs` | 1 117 | 28 | Ported to the EF Core harness in PR 4, deleted in PR 5 |
| `tests/.../Persistence/Repositories/ElementVersionRepositoryTest.cs` | 187 | | Uses services, not the NPoco repo; must stay green |
| `tests/.../Services/ElementServiceNotificationTests.cs` | 759 | | Calls sync `ElementService.Publish/SaveAndPublish/Unpublish/GetById(int)` via a cast; ported in PR 5 |
| `tests/.../Services/ElementServiceNotificationWithCacheTests.cs` | 655 | | Already on `IElementService` async |
| `ElementServiceTests*`, `ElementEditingServiceTests*` (13 files), `ElementContainerServiceTests*` (11 files), `ElementPublishingServiceTests*` (4 files), `ElementVersionCleanupServiceTest`, `ElementCacheServiceTests`, `ElementIndexingNotificationHandlerTests`, `DeferredSearchReindexServiceElementTests` | ~7 000 | | Regression suite for every PR from PR 4 on |
| `DocumentRepositoryTest` (189), `DocumentBlueprintRepositoryTest` (4), `DocumentRepositoryUrlSegmentTests`, `ContentServiceTests` | | | Regression suite for PR 2 and PR 3 (must be byte-for-byte behaviour-neutral) |

Reference points to copy from:

- EF Core repository harness: `DocumentRepositoryTest.CreateRepository(AppCaches, IEventAggregator)` at `tests/.../DocumentRepositoryTest.cs:120-149` (resolves `IEFCoreScopeAccessor<UmbracoDbContext>` and the real repositories from DI, mocks `IRepositoryCacheVersionService` and `ICacheSyncService`).
- Service hooks to mirror: `ContentService.DeleteOfTypesAsync` (`ContentService.cs:1509`), `CheckDataIntegrityAsync` (`:1471`), `DeleteLockedAsync`, and the `protected override` block at `:1819-1825`.
- Element-specific delete clauses: `ElementRepository.GetEntityDeleteClauses` (NPoco) vs `DocumentRepository.PersistEntitySpecificDeleteClausesCoreAsync` (EF Core, `DocumentRepository.cs:429`). The EF Core `UserGroupDto` already has `StartElementId`.
- Constants: `Constants.System.RecycleBinElementKey`, `Constants.ObjectTypes.Element`, `CacheKeys.ElementRecycleBinCacheKey`, `Constants.Locks.ElementTree`, `UmbracoObjectTypes.Element`.

## 4a. Known-failing baseline (verified 08-10-2026)

Four search fixtures fail on `v18/feature/ef-core-repositories` today, before any element work:

| Fixture (`tests/Umbraco.Tests.Integration/Umbraco.Cms.Search.Core/`) | Tests |
|---|---|
| `ElementIndexingNotificationHandlerTests` | 13 |
| `ExternalBlockElementIndexingTests` | 4 |
| `ExternalBlockElementVarianceTests` | 7 |
| `ExternalElementReindexOnRepublishTests` | 1 |

Cause: `ElementIndexingNotificationHandler.GetParentEntities` calls `IRelationService.GetParentEntitiesByChildIdsAsync`, which calls `IRelationRepository.GetParentEntitiesByChildIds`. On this branch that member of the EF Core `RelationRepository` (`src/Umbraco.Infrastructure/Persistence/Repositories/Implement/RelationRepository.cs:366`) throws `NotImplementedException("... depends on EntityRepository being migrated to EF Core.")`, together with `GetPagedParentEntitiesByChildIdAsync` (`:324`) and `GetPagedChildEntitiesByParentIdAsync` (`:337`). The stubs date from the relation repository migration (#22809, `8f540009a83`) and the merge fix `5ecdf7710f8`; the matching tests in `RelationRepositoryTest` and `RelationServiceTests` are `[Ignore]`d with the same reason. Upstream `v19/dev` implements the member through an `IEntityRepository.GetPagedResultsByQuery` overload that takes a raw SQL customisation delegate, which this branch's `IEntityRepository` does not have.

This campaign does not change that call path, so migrating the element repository and service will **not** make these fixtures pass. They need one of:

- the `EntityRepository` migration (owned elsewhere; if it lands first, the stubs get implemented there), or
- the small standalone PR 0 below, which implements the three stubs now without waiting for it: resolve the parent (or child) node ids with EF Core (`db.Relations` filtered by child ids and relation types, joined to `db.Nodes` on `NodeObjectType == entityType`, batched with `InGroupsOf(Constants.Sql.MaxParameterCount)`, de-duplicated by node id as upstream does), then hydrate with the existing `IEntityRepository.GetAll(Guid objectType, params int[] ids)`. That is the "depend on a narrower interface, not the repository being replaced" pattern already used on this branch, and it keeps EF Core code free of NPoco class references.

Checkpoint: PR 5's acceptance runs these four fixtures and records the result in `status.json`. Green means the stubs were implemented (by PR 0 or by the entity migration). Still red with the same `NotImplementedException` is expected and is not a PR 5 regression; any other failure in them is.

## 5. PR sequence

Branch naming: `v20/feature/ef-core-element-repository-<n>-<slug>` (the `v20` prefix reflects the release the work ships in), cut from the latest `origin/v18/feature/ef-core-repositories`. PR target: `v18/feature/ef-core-repositories`. New EF Core migrations stay in the `V_19_0_0` folder like every other migration on the integration branch; that folder name is inherited convention, not something to fix here. PR title prefix: `EF Core:` (matches `EF Core: Migrate template repository and service (#24114)`). Squash-merge. Each PR must build clean and leave the full integration suite green on its own; nothing may depend on an unmerged sibling PR except where stated.

Dependency graph: PR 1 is independent and can run in parallel with PR 2. PR 3 depends on PR 2. PR 4 depends on PR 1 and PR 3. PR 5 depends on PR 4. PR 6 depends on PR 5.

### PR 0 (optional, independent): Relation parent and child entity lookups

Scope: implement `GetParentEntitiesByChildIds`, `GetPagedParentEntitiesByChildIdAsync` and `GetPagedChildEntitiesByParentIdAsync` on the EF Core `RelationRepository` as described in §4a (ids via EF Core, hydration via `IEntityRepository.GetAll(objectType, ids)`; the paged variants page the id query with `skip`/`take` ordered by node id and return `PagedModel<IUmbracoEntity>` with the total from a `CountAsync` on the same filter). Un-`[Ignore]` the three `RelationRepositoryTest` tests and the one `RelationServiceTests` test; they must fail with `NotImplementedException` first and pass after. Then run the four §4a fixtures.

Acceptance: the four §4a fixtures and the un-ignored relation tests green on SQLite; `grep -n NotImplementedException RelationRepository.cs` returns nothing. Skip this PR if the `EntityRepository` migration has already removed the stubs; then only the checkpoint in PR 5 applies.

### PR 1: EF Core Element DTOs and the no-op migration

Scope:
- `Dtos/EFCore/ElementDto.cs : IPublishableContentDto<ElementVersionDto>` (`NodeId`, `Published`, `Edited`, navigations `ContentDto`, `CurrentVersion`, `PublishedVersion`), `ElementVersionDto : IContentVersionDto` (`Id`, `Published`, `ContentVersionDto`; no `TemplateId`), `ElementCultureVariationDto` (`Id`, `NodeId`, `LanguageId`, `Edited`, `Available`, `Published`, `Name`).
- New EF Core `Dtos/EFCore/ICultureVariationDto` with settable members; make both `DocumentCultureVariationDto` and `ElementCultureVariationDto` implement it. This is what lets PR 3 build culture-variation rows generically.
- Configurations mirroring the NPoco attributes exactly: `IX_umbracoElement_Published`; `IX_umbracoElementVersion_id_published` (id, published); `IX_umbracoElementVersion_published` (published) with included column `id` in a SQL Server customizer only; `IX_umbracoElementCultureVariation_NodeId` unique on (nodeId, languageId); `IX_umbracoElementCultureVariation_LanguageId`; FKs to `umbracoContent`, `umbracoContentVersion`, `umbracoNode`, `umbracoLanguage` as on the NPoco DTOs. Register the customizer in `Umbraco.Cms.Persistence.EFCore.SqlServer/UmbracoBuilderExtensions.AddCustomizers()`.
- `UmbracoDbContext` DbSets `Elements`, `ElementVersions`, `ElementCultureVariations` (the repository bulk-deletes all three with `ExecuteDeleteAsync`, so they qualify for DbSets).
- Migration `AddElementDtos` via `/umb-ef-core-empty-migration` in `V_19_0_0` (inherited folder convention on this branch; not a defect), both providers, empty `Up`/`Down`, snapshot contains all three table names, `has-pending-model-changes` clean on both providers.

Acceptance: solution builds; both snapshots list `umbracoElement`, `umbracoElementVersion`, `umbracoElementCultureVariation`; unattended-install integration smoke still passes (any one `UmbracoIntegrationTest` with a database); the existing migration-plan tests pass.

Out of scope: no repository, no factory changes.

### PR 2: Hoist the generic read path out of `DocumentRepository`

Pure refactor, zero behaviour change. Everything that NPoco's `PublishableContentRepositoryBase` does generically for reads moves from `DocumentRepository` into `AsyncPublishableContentRepositoryBase` (or `AsyncContentRepositoryBase` where it does not depend on publishing).

Scope:
- Replace `DocumentJoinRow`/`DocumentRow` with generic `ContentJoinRow<TEntityDto, TContentVersionDto>` / `ContentRow<...>` records on the base.
- Add one abstract hook `protected abstract IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> BuildBaseQuery(UmbracoDbContext db, IQueryable<NodeDto> nodes)` that each concrete repository implements with its concrete joins and projection (about 40 lines). Keeping the joins concrete sidesteps any EF Core translation risk around interface-member access on generic type parameters; everything downstream of the row is generic.
- Hoist: `PerformGetAsync`, `PerformGetAllAsync`, `PerformGetManyAsync`, `PerformGetRangeAsync`, `AssembleEntitiesAsync`, `LoadPropertyDataAsync`, `ApplyVariations`, `ResolveIsoCode`, the ordering family (`ApplyDocumentOrdering` becomes `ApplyOrdering`, `ApplyVariantNameOrdering`, `ResolveCustomFieldOrderedNodeIdsAsync`, `FetchCustomFieldOrderedPageAsync`, `FetchCultureNameOrderedAsync`, `ReorderRowsByNodeIds`, comparers), `GetAllVersionsAsync`, `GetAllVersionsSlimAsync`, `GetVersionAsync`, `GetChildrenCoreAsync`, `GetDescendantsCoreAsync`, `GetRootContentAsync`, `GetRecycleBinAsync`, `GetPagedRecycleBinAsync`, `GetPagedOfContentTypesAsync`.
- Templates stay Document-only through one virtual post-assembly hook (`OnEntitiesAssembledAsync(entities, rows, db, loadTemplates)` or equivalent) that `DocumentRepository` overrides to call `ResolveValidTemplateIdsAsync`; the base default is a no-op. `GetChildrenWithoutTemplatesAsync`, `GetDescendantsWithoutTemplatesAsync`, `GetByLevelAsync`, `GetAncestorsAsync`, `RecycleBinSmellsAsync`, `IsPathPublishedAsync`, permissions stay on `DocumentRepository`.
- First commit of the PR is a spike: the generic row + `BuildBaseQuery` hook + `GetAsync` only, run on both providers, before hoisting the rest. If translation fails on either provider, stop and record the finding in §8 before changing approach.

Acceptance: `DocumentRepositoryTest`, `DocumentBlueprintRepositoryTest`, `DocumentRepositoryUrlSegmentTests`, `ContentServiceTests`, `ContentServiceTests.SchedulePublish`, the HybridCache document fixtures, and the full unit test project all green with the same counts as before; snapshots untouched (`git diff --stat` on both `Migrations/` folders is empty); `DocumentRepository.cs` shrinks by roughly 900 to 1 200 lines.

### PR 3: Hoist the generic write path out of `DocumentRepository`

Same discipline as PR 2.

Scope:
- Hoist `PersistNewItemAsync`, `PersistUpdatedItemAsync`, `PersistNewNodeAsync`, `PersistNewContentAsync`, `PersistNewVersionsAsync`, the property-data reconcile, `BuildContentVariationDtosAsync`, `BuildEntityVariationDtosAsync` (generic over `TContentCultureVariationDto : ICultureVariationDto, new()` from PR 1), `SetEntitySortableValues`, `SetEntityTagsAsync`/`ClearEntityTags`, `ApplyPostPublishFlagFlipsAsync`, `GetParentNodeDtoAsync`, `SortorderExistsAsync`, `GetNewChildSortOrderAsync`, `GetReservedIdAsync`, `ResolveParentKey`, `ValidatePath`.
- Move `IIdKeyMap`, `ITagRepository`, `IJsonSerializer` into the base constructor (they are used by hoisted code). `ITemplateRepository` and `IShortStringHelper` stay on `DocumentRepository`.
- Version rows: add an abstract `TContentVersionDto BuildVersionDto(TEntity entity, int versionId, bool published)` (or equivalent) so `DocumentRepository` sets `TemplateId` and `ElementRepository` does not. `AssignDefaultTemplateIfMissing` stays Document-only behind a virtual pre-persist hook.
- `PersistEntitySpecificDeleteClausesAsync` already is an abstract hook; leave it.

Acceptance: identical to PR 2, plus `DocumentRepositoryTest`'s write-path tests (tags, publish-on-save, culture variants, IsMoving fast path, sortable values) all green; `DocumentRepository.cs` ends around 600 to 800 lines, nearly all Document-only (templates, URL segments, permissions, level/ancestors, smells).

### PR 4: `AsyncElementRepository` (additive, no consumers switched)

Scope:
- `src/Umbraco.Core/Persistence/Repositories/IAsyncElementRepository.cs : IAsyncPublishableContentRepository<IElement>` (marker; add members only if a later PR needs one).
- `src/Umbraco.Infrastructure/Persistence/Repositories/Implement/EFCore/AsyncElementRepository.cs : AsyncPublishableContentRepositoryBase<IElement, AsyncElementRepository, EFCoreDtos.ElementDto, EFCoreDtos.ElementVersionDto, EFCoreDtos.ElementCultureVariationDto>, IAsyncElementRepository`. Members: `BuildBaseQuery` (concrete joins over `db.Elements`, `db.ElementVersions`), `BuildEntity`/`BuildEntityDto` via new `ContentBaseFactory.BuildEntity(EFCoreDtos.ElementDto, IContentType?)` and `BuildElementDto(IElement, Guid objectType, bool publishing)` (mirror the existing EF Core document builders), `BuildVersionDto`, `PersistEntitySpecificDeleteClausesAsync` (null `UserGroups.StartElementId`, delete `Elements`, `ElementCultureVariations`, `ElementVersions` rows via `ExecuteDeleteAsync`/`ExecuteUpdateAsync`; nothing else, same as NPoco), `NodeObjectTypeKey = Constants.ObjectTypes.Element`, `RecycleBinKey = Constants.System.RecycleBinElementKey`, `RecycleBinCacheKey = CacheKeys.ElementRecycleBinCacheKey`, `IsPathPublishedAsync => Task.FromResult(content is { Trashed: false, Published: true })`, `OnUowRefreshedEntityAsync` publishing `ElementRefreshNotification`, `GetManyAsync` (bypasses cache, like the document one), `This`.
- Public constructor (DI only reflects over public constructors even on internal classes). Register `AddUnique<IAsyncElementRepository, AsyncElementRepository>()` in `UmbracoBuilder.Repositories.cs`. Check for the `Lazy<IUserGroupService>`-style cycle that bit the document repository; elements have no permission repository, so none is expected.
- `tests/.../Persistence/Repositories/AsyncElementRepositoryTest.cs`: port all 28 tests from the NPoco `ElementRepositoryTest` onto the `DocumentRepositoryTest`-style harness, each test written first and confirmed red, plus element-specific tests: delete clears `umbracoUserGroup.startElementId`; delete removes all three element rows and leaves documents untouched; `ElementRefreshNotification` is published on persist; culture-variation rows round-trip; recycle bin paging and `GetRecycleBinAsync` return every trashed element at any depth; `GetPagedOfContentTypesAsync` filters by element type key; cache key prefix differs from documents (`RepositoryCacheKeys.GetGuidKey<IElement>()`); isolated from `DocumentRepository` (an element and a document with the same name under the same parent).
- Leave the NPoco `ElementRepository`, `IElementRepository`, their DI line and `ElementRepositoryTest` untouched in this PR.

Acceptance: new fixture green on SQLite and SQL Server; all Document fixtures still green; solution builds; `IAsyncElementRepository` and `AsyncElementRepository` expose no member that takes an `int` id, an `IQuery<T>`, an `out` parameter or lacks a trailing `CancellationToken` (check with `grep -nE 'int [a-z]*[iI]d\b|IQuery<|out long' <file>` on both files, expecting only version-id parameters).

### PR 5: `ElementService` onto the async engine, consumers cut over, NPoco repository deleted

Scope:
- `ElementService : AsyncPublishableContentServiceBase<IElement>, IElementService`. Constructor takes `IAsyncElementRepository` and passes it as the base's `IAsyncPublishableContentRepository<IElement>`. Delete every bridging member (`GetByIdAsync`, `GetByIdsAsync` x2, `PersistContentScheduleAsync`, `PublishAsync`, `UnpublishAsync`, `SaveAndPublishAsync`, `PerformScheduledPublishAsync`, `RollbackAsync`, `GetContentSchedulesByKeysAsync`, `GetContentScheduleByContentIdAsync`, `SaveAsync` x2, `DeleteAsync`, `CheckDataIntegrityAsync`, `DeleteOfTypesAsync`, sync `DeleteOfTypes`, sync `CheckDataIntegrity`, sync `DeleteLocked`): the base provides them. Implement the abstract hooks: `DeleteLockedAsync` (elements have no child elements: `await repo.DeleteAsync(content, ct)` then publish `ElementDeletedNotification`), `CheckDataIntegrityAsync` (mirror `ContentService.cs:1471` with `new Element("root", -1, new ContentType(_shortStringHelper, -1))`), `DeleteOfTypesAsync` (mirror `ContentService.cs:1509` minus the child-move loop; page through `GetPagedOfTypesAsync`, publish `ElementDeletingNotification`/`ElementUnpublishedNotification`/`ElementTreeChangeNotification`, `AuditAsync`), `SupportsBranchPublishing => false`, `WriteLockIds => [Constants.Locks.ElementTree]`, `ReadLockIds => WriteLockIds`, `ContentObjectType => UmbracoObjectTypes.Element`, `Logger`, and the 16 notification factories (already present, keep).
- `IElementService`: align with `IContentService`. Verify whether `IContentService` still declares `IContentServiceBase` (on 08-10-2026 it does not; `IElementService` still does). If dropping it, check `DatabaseIntegrityCheck` and any other `IContentServiceBase` caller first.
- `ElementContainerService`: replace the four NPoco call sites. Trashed root elements (`ParentId == RecycleBinElement`): use `GetChildrenAsync(Constants.System.RecycleBinElementKey, 0, int.MaxValue, null, null, ct)` if direct-children semantics are required by the notification, else `GetRecycleBinAsync`; read the surrounding code and choose deliberately, then write the test that distinguishes them. `Get(descendant.Id)` becomes `GetAsync(descendant.Key, ct)`; `Save` becomes `SaveAsync`; `Delete` becomes `DeleteAsync`. Constructor takes `IAsyncElementRepository`.
- Delete the NPoco `ElementRepository.cs`, `IElementRepository.cs` (the name is reused in PR 6), the `AddUnique<IElementRepository, ElementRepository>()` line, and the NPoco `ElementRepositoryTest.cs` (its tests live in `AsyncElementRepositoryTest` since PR 4).
- Port `ElementServiceNotificationTests` to the async surface (`PublishAsync`, `SaveAndPublishAsync`, `UnpublishAsync`, `GetByIdAsync(Guid)`); drop the `(ElementService)` cast if no concrete member remains.
- Check `ElementService` is still constructible through DI (resolve `IElementService` in an integration test; a manual `new` proves nothing).

Acceptance: every fixture in the test table of §4 green on SQLite; SQL Server run differs from the parent branch only by the known-flaky search jobs; `grep -r "IElementRepository\b" src tests` returns nothing; `grep -rn "PublishableContentServiceBase<IElement>" src` returns nothing; `grep -nE 'public (?!override|async|Task)' -P src/Umbraco.Core/Services/ElementService.cs` lists only the constructor (no sync public member remains on the concrete type); `grep -nE 'OperationResult|int userId|IQuery<|GetAwaiter\(\)\.GetResult\(\)|Task\.FromResult' src/Umbraco.Core/Services/ElementService.cs src/Umbraco.Core/Services/ElementContainerService.cs` returns nothing; unit tests green. The four §4a search fixtures are run and their result recorded in `status.json` (green if the relation stubs are implemented, otherwise the same `NotImplementedException` as the baseline and nothing else).

### PR 6: Retire the sync engine and drop the scaffolding names

Scope:
- Delete `src/Umbraco.Core/Services/PublishableContentServiceBase.cs` (no subclasses left), `src/Umbraco.Infrastructure/Persistence/Repositories/Implement/PublishableContentRepositoryBase.cs` (no subclasses left; `MediaRepository`/`MemberRepository` use `ContentRepositoryBase`, which stays), `src/Umbraco.Core/Persistence/Repositories/IPublishableContentRepository.cs`. Remove the NPoco `ContentBaseFactory.BuildEntity(ElementDto)` / `BuildDto(IElement, Guid)` overloads only if a repo-wide grep shows zero callers (public API with zero internal callers survives; these are `internal`, so deletion is fine when unreferenced). Keep the NPoco `ElementDto`/`ElementVersionDto`/`ElementCultureVariationDto` classes: they drive schema creation.
- Rename `IAsyncElementRepository` to `IElementRepository` and `AsyncElementRepository` to `ElementRepository` (file renames too, `git mv`), `AsyncElementRepositoryTest` to `ElementRepositoryTest`.
- Campaign cleanup, after PR 6 is merged and `sync-memory.sh push` has been run one final time: `git worktree remove .claude/campaign` on each machine, then delete the `campaign/ef-core-element-repository` branch locally and on origin. Nothing in the main tree changes. The progress artifact stays published as the record; its page needs no source file after that.
- Decision point for the user (record the answer in §8): the deletions free the names `PublishableContentServiceBase`, `PublishableContentRepositoryBase` and `IPublishableContentRepository`. Renaming `AsyncPublishableContentServiceBase`, `AsyncPublishableContentRepositoryBase` and `IAsyncPublishableContentRepository` onto them finishes the naming pass the Document campaign left open. `AsyncContentRepositoryBase`, `IAsyncContentRepository` and `IAsyncContentServiceBase` cannot be renamed yet because their sync twins are still used by media and members. Default if no answer: do the three free renames in this PR, leave the rest.

Acceptance: solution builds; full integration and unit suites green; `grep -rn "Async\(Element\|PublishableContent\)" src tests` returns only the names the decision above leaves in place.

## 6. Per-PR checklist (do all of it, in order, every PR)

0. On a machine that may be behind: `git -C .claude/campaign pull`, then `.claude/campaign/ef-core-element-repository/sync-memory.sh pull` (Windows: `sync-memory.ps1 pull`) so the local Claude memory matches the committed snapshot. Read this file again if the decisions log (§8) has new rows.
1. `git fetch origin && git checkout -b v20/feature/ef-core-element-repository-<n>-<slug> origin/v18/feature/ef-core-repositories`.
2. Set the PR's status to `in-progress` in `status.json` and republish the progress page (§7). Note the branch name there.
3. Research the NPoco reference and the EF Core precedent yourself before writing any brief.
4. Implement in phases; each phase: failing test, implementation, green, full fixture rerun, `git status --short`.
5. `dotnet build umbraco.sln` (fresh; rerun once on MSB3030), IDE0005 on touched files, rebuild, `git checkout -- '**/package-lock.json'`.
6. Spawn the independent reviewer and the test runner in parallel; fix genuine findings in the same turn; re-review after fixes.
7. Run `/umb-review` on the branch diff against `origin/v18/feature/ef-core-repositories` as a last self-check if time allows.
8. Commit with Conventional Commits (`refactor(infrastructure): ...`, `feat(core): ...`), ending with the attribution trailer the session provides. Commit and push only when the user asks.
9. Open the PR with the `EF Core:` title prefix and the repo PR template; the body states what is deliberately out of scope and which fixtures were run on which provider.
10. Set status to `in-review`, record the PR number and URL in `status.json`, republish the page, append a one-line entry to the `log` array.
11. On merge: set status `merged`, record the merge commit, republish. If upstream (`v19/dev`) was merged into `ef-core-repositories` since the branch was cut, run the merge audit (memory `project_merge_audit_procedure`) before starting the next PR.
12. Update the memory file `project_ef_core_element_repository_campaign.md` with anything non-obvious learned (decisions, traps), and this document's §8 if a decision changed.
13. Before the session ends: `.claude/campaign/ef-core-element-repository/sync-memory.sh push` (Windows: `sync-memory.ps1 push`), then commit inside the worktree (`git -C .claude/campaign add -A && git -C .claude/campaign commit`) and push the `campaign/ef-core-element-repository` branch. That commit is the handover; no separate handover document is needed. Never commit this folder on a PR branch.

## 7. Keeping the progress page current (the artifact)

Source of truth for status is `.claude/campaign/ef-core-element-repository/status.json`. The page `progress.html` in the same folder renders it; it never contains status itself.

To update:

1. Edit `status.json`. Allowed `status` values per PR: `planned`, `in-progress`, `in-review`, `merged`, `blocked`. Set `done: true` on tasks as they land. Set the top-level `updated` field to today (DD-MM-YYYY). Add a line to `log` (newest first) for anything a teammate would want to see on the board: PR opened, PR merged, blocker found, decision taken.
2. Republish with the Artifact tool: `publish` with `file_path` = the absolute path of `progress.html`, `url` = the value of `artifact.url` in `status.json`, and `files` = `{"status.json": "<absolute path of status.json>"}`. Pass no `icon` (keep the existing one). If this session has not read or published that artifact yet, do `Artifact read` on the URL first; a refused publish hands back the live version to merge against.
3. Only change `progress.html` when the page itself needs a change (new column, new section). A status change never requires touching it.
4. If the artifact URL is ever lost, `Artifact list` shows it under the title "Element Repository Campaign".

Steps 2 and 10 to 11 of §6 are the points in each PR where this must happen. A PR is not finished until the board shows it.

## 8. Decisions log

| Date | Decision | Why |
|---|---|---|
| 08-10-2026 | Hoist generic logic into `AsyncPublishableContentRepositoryBase` (PR 2 and PR 3) before adding the element repository, instead of copy-adapting `DocumentRepository`. | Avoids a 2 000-line fork that goes stale on every merge-up; matches the NPoco design (#21637); makes `ElementRepository` as thin as its NPoco predecessor. User to confirm. |
| 08-10-2026 | Keep the concrete joins and projection per repository (`BuildBaseQuery` hook) and make everything downstream of the row generic. | Removes EF Core translation risk around interface members on generic type parameters while still sharing all logic. |
| 08-10-2026 | Deliver as six PRs into `v18/feature/ef-core-repositories`, with PR 1 and PR 2 parallelisable. | User asked for reviewable increments after the 433-file Document PR. |
| 08-10-2026 | `IAsyncElementRepository`/`AsyncElementRepository` are temporary names, renamed in PR 6. | Same scaffolding pattern as every repository migration on this branch. |
| 08-10-2026 | Version, container and database-cache repositories stay NPoco. | Parity with the Document campaign; they share bases with other entity kinds and deserve their own campaigns. |
| 08-10-2026 | Status lives in `status.json`; the page only renders it. | One source of truth that any agent can edit with a text tool and republish with the Artifact tool. |
| 08-10-2026 | Branches use the `v20/` prefix; PR target stays `v18/feature/ef-core-repositories`; migrations stay in `V_19_0_0`. | User: the work has been retargeted and ships in Umbraco 20. |
| 08-10-2026 | The plan folder, `status.json`, the page source and a snapshot of the Claude memory files live on the personal orphan branch `campaign/ef-core-element-repository`, checked out as a worktree at `.claude/campaign/`; synced with `sync-memory.sh`; branch and worktree removed after PR 6. | User works on two machines and does not want to keep writing handover documents; the first attempt (committing the folder on the integration branch) was reverted because that branch is shared with coworkers and the folder would have ridden along in every PR. |
| 08-10-2026 | Four search fixtures (25 tests) recorded as a known-failing baseline caused by three `NotImplementedException` stubs in the EF Core `RelationRepository`, not by anything this campaign changes; optional PR 0 added to implement them via EF Core ids + `IEntityRepository.GetAll`, and PR 5 gets a checkpoint. | User asked that the plan verify these fixtures start working. |
| 08-10-2026 | Contract rules written out in §3 (Guid keys, GetAll/GetMany split, no IQuery or out totals, Attempt with bespoke status enums, Guid userKey, trailing CancellationToken) and enforced by greps in PR 4 and PR 5 acceptance. | User asked whether the service contract changes from the Document campaign were carried over; they were implied, not stated. |

## 9. Follow-up backlog (not this campaign)

- `IDocumentVersionRepository` + `IElementVersionRepository` to EF Core (shared `ContentVersionRepositoryBase`).
- All `EntityContainerRepository` subclasses to EF Core (document type, media type, member type, data type, blueprint, element containers).
- `IDatabaseCacheRepository` (HybridCache) to EF Core.
- `AsyncPublishableContentServiceBase.Audit(...)` has zero callers; delete.
- Branch-publish notification-state TODO at `ContentService.cs` (search for `TODO` near `PublishBranchAsync`).

## 10. Working across machines (personal handover area)

The folder `ef-core-element-repository/` is the only content of the orphan branch `campaign/ef-core-element-repository`. It is checked out as a worktree at `.claude/campaign/` inside the main checkout; `.claude/*` is gitignored there, so the main tree never sees it and it cannot leak into a PR.

| File | Purpose |
|---|---|
| `CAMPAIGN.md` | This plan. |
| `status.json` | Progress source of truth; rendered by the artifact. |
| `progress.html` | Page source for the artifact. Only changes when the page layout changes. |
| `sync-memory.sh`, `sync-memory.ps1` | Same tool for Linux/macOS (bash) and Windows (PowerShell): `pull` copies `memory/*.md` into the local Claude Code memory directory for the main checkout; `push` copies the local directory back. Newer file wins. Both find the main checkout through `git rev-parse --git-common-dir`, so they work from inside the worktree, and both derive the memory directory from that path the way Claude Code names project folders (every non-alphanumeric character becomes `-`). If the derived directory does not exist, pass it explicitly as the second argument (bash) or `-MemoryDir` (PowerShell), or set `CLAUDE_MEMORY_DIR`. |
| `memory/` | Snapshot of the Claude Code memory files for this repository (`MEMORY.md` index plus one file per memory). Treat it as read-only on disk; edit the live memory and `push`. |

First-time setup on a machine (run from the main checkout):

```
git fetch origin campaign/ef-core-element-repository
git worktree add .claude/campaign campaign/ef-core-element-repository
.claude/campaign/ef-core-element-repository/sync-memory.sh pull        # Linux/macOS
.claude\campaign\ef-core-element-repository\sync-memory.ps1 pull      # Windows (PowerShell)
```

Routine: `git -C .claude/campaign pull` and `sync-memory.sh pull` at the start of a session on a machine that may be behind; `sync-memory.sh push`, commit inside the worktree and push the branch at the end of every session (§6 step 13). Status changes travel in the same commit. If two machines both changed `status.json`, keep both sides' task and log updates; the `updated` date is the later one.

Agents: when a tool reports the working directory as `.claude/campaign`, you are inside this worktree. Code work happens in the main checkout one level up; only plan, status and memory files are committed here.

Cleanup: after PR 6 is merged, `git worktree remove .claude/campaign` on each machine and delete the branch locally and on origin.

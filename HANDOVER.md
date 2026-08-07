# ⚠️ TEMPORARY FILE — DELETE AFTER PULLING IN THE OTHER SESSION

This file exists only to carry session context (including Claude Code's local memory, which
does **not** sync across sessions/machines) from this session to another one. If you're passing
this to a different machine, commit it so it can be `git pull`ed there, then remove it from
history once pulled — see "Cleanup" at the very bottom. If you're just handing it to another
session on the same machine, the committed file (or even just its content) is enough — no pull
needed.

**How to use this in the other session**: open Claude Code in this repo and say something like:
> Read HANDOVER.md and continue from where the previous session left off.

Everything below is either a session summary or a verbatim copy of this session's persistent
memory files (from `~/.claude/projects/.../memory/` on the machine that wrote this), so a fresh
session has the full picture without needing the original memory directory.

---

## 1. Current state (as of 2026-08-07)

- **Repo**: `Umbraco-CMS`, branch `v18/feature/ef-core-document-repository`
- **Remote**: `origin` = `https://github.com/umbraco/Umbraco-CMS.git` (the real public repo)
- **Working tree**: clean
- **Ahead of `origin/v18/feature/ef-core-document-repository` by 5 commits** (not pushed yet)
- **Test suites**: `AsyncDocumentRepositoryTest` 133/133, `AsyncDocumentRepositoryOrderingTests` 4/4,
  `AsyncDocumentBlueprintRepositoryTest` included in the 133, `DocumentUrlServiceTests` (unit) 25/25,
  `DocumentUrlServiceTests` (integration) 74/74, `DeferredSearchReindexServiceTests` (unit) 11/11 —
  all passing on real rebuilds (not `--no-build`)
- **Known pre-existing, unrelated failure**: `DeferredSearchReindexServiceElementTests` (integration,
  3 tests) fails with `NotImplementedException: GetParentEntitiesByChildIds depends on EntityRepository
  being migrated to EF Core.` — confirmed this fails identically on commits from *before* any of this
  session's work, so it's a separate, already-existing gap in `RelationRepository`/`EntityRepository`'s
  own EF Core migration, nothing to do with the document repository. Not something to fix under this
  effort.
- **Note on building this repo locally**: plain `dotnet build` on `Umbraco.Core`/`Umbraco.Infrastructure`
  csproj files can trigger a broken frontend npm/TypeScript build via `Umbraco.Cms.StaticAssets`
  (unrelated pre-existing issue). Use `dotnet build <project> -p:UmbracoBuild=true` to skip it.
- **Recent commits** (newest first):
  ```
  6086fb4fe9d wire ReindexContentOfContentTypes to the async document repository
  764c6938f52 add GetPagedOfContentTypesAsync to IAsyncDocumentRepository
  c9ef8179ecb fix(infrastructure): migrate ReindexDocumentsReferencingElements to IAsyncDocumentRepository
  4ef772dd164 fix(core): thread CancellationToken through RebuildAllUrlsAsync and unblock DocumentUrlService's async repository DI
  fe2175eaffa register AsyncDocumentRepository and AsyncDocumentBlueprintRepository in DI
  f6a138deb3d add UpdateSortOrderAsync to the EF Core content repository chain
  09341483ebc add AsyncDocumentBlueprintRepository
  df8730cd7a8 port sibling name-uniqueness to AsyncDocumentRepository
  803232d7813 justify omitting ForUpdate lock in AsyncDocumentRepository
  c9925a6da88 implement publish-scheduling and publish-status methods
  ```

## 2. What this whole effort is

Migrating Umbraco's `AsyncDocumentRepository`/`IAsyncDocumentRepository` (EF Core) to fully replace
NPoco's `IDocumentRepository`/`DocumentRepository`, eventually renaming the EF Core classes to take
over the old names once nothing references the NPoco versions anymore. This is the same transitional
pattern already used for ~11 other repositories in this codebase's NPoco→EF Core migration (Language,
KeyValue, Dictionary, RedirectUrl, PublicAccess, Domain, ContentType, etc.) — `DocumentRepository`
existing in parallel is temporary scaffolding, not a permanent architecture.

**Where things stand right now**: `AsyncDocumentRepository` itself is functionally complete and
thoroughly tested (read path, write path, tags, IsMoving fast path, recycle bin, permissions,
publish-scheduling/publish-status, sibling-name-uniqueness, `UpdateSortOrderAsync`,
`GetPagedOfContentTypesAsync`). It's now DI-registered and actually resolved by real consumers, not
just tested via manual construction. Of the 4 original production consumers of NPoco's
`IDocumentRepository`:

| Consumer | Status |
|---|---|
| `DocumentBlueprintRepository` | Resolved via class-inheritance (`AsyncDocumentBlueprintRepository`) |
| `DocumentUrlService` | **Fully migrated** (this session) |
| `DeferredSearchReindexService` | **Fully migrated** (this session) |
| `ContentService` | **Not started** — still 100% synchronous, by far the largest (nearly the whole interface) |

`ContentService`'s conversion is the actual point of this whole effort and is the only piece left.
It has NOT been scoped into a concrete plan — per explicit user instruction, work through this
**one increment at a time**, don't plan the whole `ContentService` conversion in one shot without
being asked.

## 3. Immediate next step

Nothing is currently queued. The last exchange ended with `ReindexContentOfContentTypes` wired up,
refactored (`AsyncPageAndReindex`/`ResolveKeysAsync` extracted), committed, and this handover written.
Wait for the user to say what the next increment is. Candidates, roughly in order of how "ready" they
are:

- **Start the `ContentService` conversion itself** — but only when explicitly asked, and even then
  pick ONE small method first. The `ef-core-document-repository-implementation-status` memory below
  has a full tier breakdown (Tier A "start here" through Tier F "save for last") from an HTML ledger
  artifact built earlier — `GetById(Guid key)` on `PublishableContentServiceBase<TContent>` was
  identified as the lowest-risk starting point (zero signature change, zero other-repo coupling,
  direct 1:1 async equivalent already exists: `GetAsync(Guid, CancellationToken)`).
- **`DeferredSearchReindexService`'s media/member reindexing** — still on NPoco (`PageAndReindex`),
  since no `IAsyncMediaRepository`/`IAsyncMemberRepository` exists yet. Would need those built first;
  much bigger scope than anything done so far in this file's migration.
- Nothing else is currently a known small, self-contained gap.

## 4. Memory files (verbatim)

Everything below this point is the full content of every file under this session's
`~/.claude/projects/.../memory/` directory, in the same format Claude Code's memory system uses.
If Claude Code's memory system is available in the new session already (e.g. the memory directory
synced some other way), these may be redundant — otherwise, treat the content below as if it were
loaded from memory.

### MEMORY.md (index)

```markdown
# Memory Index

- [DocumentRepository retirement plan — READ FIRST](project_document_repository_retirement_plan.md) — scope/CancellationToken/blueprint decisions, DI-constructor bug fix, DocumentUrlService + DeferredSearchReindexService fully migrated, GetPagedOfContentTypesAsync added; only ContentService left
- [EF Core document repository implementation status](project_ef_core_document_repository_status.md) — read+write+tags+IsMoving+recycle-bin+permissions+scheduling+name-uniqueness+UpdateSortOrderAsync+DI registration done+tested (127/127); pessimistic locking documented-not-implemented; ContentService's sync-to-async conversion is the remaining phase
- [EF Core migration regeneration procedure](project_ef_migration_regeneration.md) — Step-by-step for removing+regenerating both provider migrations; covers the provider-switch dance and type-reference build errors
- [EF Core migration snapshot drift after merge](project_ef_migration_merge_drift.md) — merges can silently mangle UmbracoDbContextModelSnapshot.cs even when the build succeeds; use `has-pending-model-changes` + one new reconciliation migration to fix
- [EF Core document repository progress (historical)](project_ef_core_dto_phase1.md) — Phase 1 DTOs + Phase 2 PerformGet* complete, superseded by the status memory above
- [Provider-specific defaultValueSql: use SQLite customizer](feedback_no_efcore_sqlite_defaultvaluesql_in_shared_config.md) — Never use SQL Server SQL in shared config without a SQLite override customizer
- [Always use braces](feedback_always_use_braces.md) — Never omit braces for any conditional/loop body, even single-line returns
- [Use Enumerable.Empty](feedback_enumerable_empty.md) — Return Enumerable.Empty<T>() not cast-hacked empty literals for IEnumerable<T>
- [No NPoco class references from EF Core](feedback_no_npoco_class_references_from_efcore.md) — never call even a stateless static helper on a NPoco repository class from EF Core code; copy it instead
- [Don't depend on the repository being replaced](feedback_dont_depend_on_repository_being_replaced.md) — EF Core code delegating unported logic should depend on a narrower sub-repository, not the NPoco repo it's replacing
- [EF Core NoTracking requires .AsTracking()](feedback_efcore_notracking_requires_astracking.md) — UmbracoDbContext is globally NoTracking; read-then-mutate-then-save needs explicit `.AsTracking()`
- [No section-header comments](feedback_no_section_header_comments.md) — never add `// --- Section Name ---` banner comments; blank lines/ordering only
- [Self-contained code comments](feedback_self_contained_code_comments.md) — never reference ephemeral plan files or "this conversation" in TODOs/comments
- [Watch for stray file changes](feedback_watch_for_stray_file_changes.md) — always `git status --short` before committing; investigate recurring unexplained diffs rather than silently reverting twice
- [SQLite harness masks ordering tiebreak bugs](feedback_sqlite_harness_masks_ordering_tiebreak_bugs.md) — unit-test the ordering helper directly with a manipulated in-memory sequence instead of relying on SQLite integration tests
- [Stale build after subagent revert](feedback_stale_build_after_subagent_revert.md) — after a revert-rebuild-restore TDD check, force a real rebuild before trusting `dotnet test --no-build`
- [Large EF Core migration workflow](feedback_large_efcore_migration_workflow.md) — research first, delegate one phase per subagent with an exhaustive brief, independently re-verify every claim
- [Internal class needs public constructor for DI](feedback_internal_class_needs_public_constructor_for_di.md) — Microsoft.Extensions.DependencyInjection only reflects over public constructors, even on an internal class; manual-construction tests don't prove DI resolution works
- [Guid keys not int IDs on async repository](feedback_guid_keys_not_int_ids_on_async_repository.md) — every new IAsyncDocumentRepository method must take Guid keys even if the DTO column and first caller are int-based; resolve internally instead of leaking the int type into the interface
```

### project_document_repository_retirement_plan.md — **READ THIS ONE FIRST**

```markdown
---
name: document-repository-retirement-plan
description: "User's decided direction for retiring NPoco's IDocumentRepository/DocumentRepository in favor of IAsyncDocumentRepository/AsyncDocumentRepository — key decisions on scope pattern, CancellationToken, DocumentBlueprintRepository, ElementService, and sequencing. DocumentUrlService and DeferredSearchReindexService are now fully migrated; only ContentService remains. Read before doing any further work on this."
metadata:
  node_type: memory
  type: project
---

**Corrected understanding of the current dual-repository setup**: `IDocumentRepository`/`DocumentRepository` (NPoco) is NOT a permanent architectural sibling to `IAsyncDocumentRepository`/`AsyncDocumentRepository` (EF Core) — it exists only as a **temporary measure to give the new EF Core repository something to test against**, exactly the same transitional pattern already used for every other repository migrated in this codebase's NPoco→EF Core journey (Language, KeyValue, Dictionary, RedirectUrl, PublicAccess, Domain, ContentType, etc. — see the research done confirming ~11 repos already went through this). Once `DocumentRepository` is no longer referenced anywhere, **`AsyncDocumentRepository`/`IAsyncDocumentRepository` will be renamed to `DocumentRepository`/`IDocumentRepository`** — this is an in-place-conversion end state, not a permanent parallel-interfaces end state. Don't plan around a permanent two-interface future; plan around a rename-at-the-end future.

**Why:** User corrected an initial (wrong) framing that this branch had deviated from the codebase's dominant "in-place conversion" pattern by keeping `IAsyncDocumentRepository` as a separate, permanent interface. It hasn't deviated — the naming is just temporary scaffolding for the migration period, same as every other repo.

**How to apply:** Don't design anything (comments, architecture docs, naming) around `IDocumentRepository`/`IAsyncDocumentRepository` coexisting forever. Any new "AsyncX" sibling class created during this effort (e.g. `AsyncDocumentBlueprintRepository`, see below) is also temporary scaffolding under the same eventual-rename plan.

## User's decisions on the ContentService/IDocumentRepository retirement (2026-08-06)

Asked (by the assistant) 5 scoping questions after a 3-agent research pass (consumer map of `IDocumentRepository`, full `ContentService`/`PublishableContentServiceBase` API inventory, precedent search across the codebase's prior NPoco→EF Core service conversions). Answers:

1. **Scope strategy**: Accept the established "sync scope held across awaits" pattern already used everywhere else in this codebase (e.g. `ContentEditingServiceBase`/`ContentPublishingServiceBase` open a sync `ICoreScope`, await only genuinely-async leaf calls, call sync methods un-awaited within it). Do **not** build a first-class async `ICoreScopeProvider`/`ICoreScope` API — none exists anywhere in the codebase today (confirmed via grep, zero hits for `CreateCoreScopeAsync`/`IAsyncCoreScope`), and building one is out of scope. `IEFCoreScope<TDbContext>.ExecuteWithContextAsync(...)` is what actually enables writing real `async`/`await` methods against the EF Core repository within that sync-scope shell.
2. **`CancellationToken` adoption**: Yes — adopt it on the new async service surface, even though no prior service migration in this codebase has done so (the closest precedent, `IContentTypeService`'s async conversion via `IAsyncContentTypeBaseService<TItem>`, notably did NOT add `CancellationToken` anywhere). This migration will be the first to do so.
3. **`DocumentBlueprintRepository`**: `DocumentBlueprintRepository : DocumentRepository` (NPoco) is a **class-inheritance** dependency, not just a service-level consumer — confirmed the single sharpest blocker to deleting `DocumentRepository`. Resolve by creating `AsyncDocumentBlueprintRepository : AsyncDocumentRepository` (mirroring the exact same shape) — this was done, see below.
4. **`ElementService`**: Also inherits the shared `PublishableContentServiceBase<TContent>` base that `ContentService` uses — any base-class change ripples to it. **Defer `ElementService`'s conversion if at all possible** — don't force it into scope just because the base class changes; only touch it if truly unavoidable.
5. **`IAsyncDocumentRepository` vs `IDocumentRepository` method-surface parity check**: Confirmed as a real, valuable next step — this was done, see below.

**Explicit sequencing instruction from the user**: "Don't do a complete plan to fix all of this at once, but keep it in your memory. For now start make a plan to resolve DocumentBlueprintRepository and then after we've done that let's do 5 and check for parity." — i.e. work through this incrementally, one self-contained increment at a time, not as one giant upfront plan. The full `ContentService`-to-async conversion (the actual point of this whole effort) is NOT yet scoped into a concrete plan.

### Step 3 done (2026-08-06): `AsyncDocumentBlueprintRepository` created

Committed as `09341483ebc`. `AsyncDocumentRepository` un-sealed (`internal sealed class` → `internal class` — confirmed safe, no `is`/`typeof`/`as` exact-type checks anywhere in the codebase). New `IAsyncDocumentBlueprintRepository : IAsyncDocumentRepository` marker interface (`src/Umbraco.Core/Persistence/Repositories/`) and `AsyncDocumentBlueprintRepository : AsyncDocumentRepository, IAsyncDocumentBlueprintRepository` (`src/Umbraco.Infrastructure/Persistence/Repositories/Implement/EFCore/`) mirror NPoco's `DocumentBlueprintRepository` exactly: constructor forwards all parameters unchanged to `base(...)`, overrides only `EnsureUniqueNaming => false` and `NodeObjectTypeKey => Constants.ObjectTypes.DocumentBlueprint`. New test file `AsyncDocumentBlueprintRepositoryTest.cs` (3 tests: duplicate names not suffixed, correct `NodeObjectType` persisted, isolation from a plain `AsyncDocumentRepository`'s child queries). Independent review found zero issues; full suite (124/124 at that point) passes on a real rebuild (not `--no-build`).

Not DI-registered (consistent with `AsyncDocumentRepository`/`AsyncDocumentBlueprintRepository`'s current unregistered state) — this step only removes the class-inheritance blocker, doesn't wire anything in yet.

### Step 5 done (2026-08-06): `IAsyncDocumentRepository` vs `IDocumentRepository` parity check

Read the full `IAsyncDocumentRepository` interface chain (`IAsyncContentRepository<TEntity>`, `IAsyncPublishableContentRepository<TContent>`, `IAsyncReadRepository`/`IAsyncWriteRepository`, all in `src/Umbraco.Core/Persistence/` and `src/Umbraco.Core/Persistence/Repositories/`) and cross-referenced against `IDocumentRepository`'s full surface. Result:

- **Bulk of the interface matches**: every schedule method, version method, count/children/descendants/recycle-bin method, `CheckDataIntegrity`, and all 4 permission methods have a direct `Async`-suffixed, `CancellationToken`-accepting Guid-keyed counterpart. No action needed there.
- **Real gap 1 — `UpdateSortOrder(IReadOnlyList<int> orderedNodeIds)`**: no async equivalent anywhere in the chain. Backs backoffice tree drag-drop reordering. This was closed — see below.
- **Arbitrary `IQuery<IContent>`-based querying — RESOLVED, not a gap to fix (user decision, 2026-08-06)**: NPoco's `IQueryRepository<IContent>.Get(IQuery<IContent> query)` / `Count(IQuery<IContent>? query)`, plus the two `GetPage(IQuery<IContent>? query, ..., IQuery<IContent>? filter, ...)` overloads, have no EF Core equivalent, and **this is intentional and will stay that way** — the user confirmed the EF Core side deliberately only exposes purpose-built query shapes (`GetChildrenAsync`, `GetDescendantsAsync`, `GetRecycleBinAsync`, etc.), not a generic arbitrary-predicate mechanism. Do NOT build an EF Core equivalent of `IQuery<T>`. Every current `ContentService.cs` call site that uses `_documentRepository.Get(query)`/`GetPage(query, ...)` will need to be re-expressed against a specific purpose-built method (existing or, if genuinely needed, a new narrowly-scoped one) when that conversion happens — not ported through a generic query builder.
- **Int-keyed reads — RESOLVED (user decision, 2026-08-06)**: `Get(int)`/`GetMany(int[])`/`Exists(int id)` are absent on the async side because `IAsyncDocumentRepository` is deliberately Guid-first throughout. The user explicitly rejected bridging this at the `ContentService` boundary via `IIdKeyMap` — **`ContentService`'s own public signatures should change to accept only Guids**, pushing int→Guid resolution up to *callers*, not absorbing it internally. Concretely: methods like `IContentService.GetById(int)`/`GetByIds(IEnumerable<int>)` are themselves in scope to become Guid-only over the course of this conversion, not kept as int-accepting wrappers around an internal `IIdKeyMap` lookup. Every caller of those methods needs to be updated to pass a Guid instead. This is a more invasive decision than a typical internal bridge — it ripples to `ContentService`'s callers, not just its internals — and should be weighed accordingly when this part of the work is actually scoped/planned.

### Step: `UpdateSortOrderAsync` added (2026-08-06)

The one genuine missing-method gap from the parity check (not a design decision like the other two — an actual absent method) is now closed. Committed as `f6a138deb3d`. Added `UpdateSortOrderAsync(IReadOnlyList<Guid> orderedNodeKeys, CancellationToken)` to `IAsyncContentRepository<TEntity>` (generic tier, matching NPoco's own placement on `IContentRepository<TId, TEntity>` rather than Document-specific) and implemented on `AsyncContentRepositoryBase<TEntity, TRepository>` — `AsyncDocumentRepository`/`AsyncDocumentBlueprintRepository` inherit it with no override needed. Implementation reuses the established batched-fetch (`InGroupsOf(Constants.Sql.MaxParameterCount)`) + `.AsTracking()` mutate + single `SaveChangesAsync` pattern (same shape as `PersistContentScheduleAsync`), not NPoco's raw-SQL `CASE WHEN` batch update — confirmed via review this doesn't reintroduce the 2100-parameter-limit risk, since EF Core's `SaveChangesAsync` issues one `UPDATE` per changed row rather than one giant parameterized statement.

Review caught two real things, both fixed before commit: (1) a coincidental-pass test — `UpdateSortOrderAsync_UnknownKeyInList_SkipsItSilentlyAndStillReordersTheRest` originally reused `_textpage` as parent, whose pre-existing children (`_subpage`=0, `_subpage2`=1) meant one sibling's pre-call and expected post-call `SortOrder` were coincidentally identical, so a broken implementation could still have passed that assertion — fixed by using a fresh parent instead; (2) a local idiom deviation — used `ExecuteWithContextAsync<object?>(async db => {...; return null;})` where established precedent (`PersistContentScheduleAsync`) uses `ExecuteWithContextAsync<object>(async db => {...})` with no `return` statement — aligned to match.

**Status**: both immediate next steps the user asked for (blueprint repo, then parity check) are done, both parity design questions are resolved into decisions (no `IQuery<T>` equivalent; `ContentService` goes Guid-only, no `IIdKeyMap` bridge), and the one genuine missing-method gap (`UpdateSortOrderAsync`) is now implemented and tested. The actual `ContentService`-to-async conversion is still NOT scoped into a concrete plan — per the user's "one increment at a time" instruction, wait for explicit direction before planning the next increment.

## Research findings worth keeping (from the 3-agent pass, 2026-08-06)

- **`IDocumentRepository` production consumers** (4 total): `ContentService` (heaviest — nearly the whole interface), `DocumentBlueprintRepository` (class inheritance, see above), `DocumentUrlService` (one `GetMany` call, in `RebuildAllUrlsAsync`), `DeferredSearchReindexService` (one `GetPage` call for background reindex paging, one `GetMany` call for block-reference reindexing).
- **`ContentService`/`PublishableContentServiceBase<TContent>` current API state**: ~55 public members, only 1 (`EmptyRecycleBinAsync`) is `Task`-returning, zero accept `CancellationToken`, zero return a real typed `Attempt<TResult, TStatus>` (one trivial `Attempt<OperationResult?>` wrapper only). Nearly every method touches the document repository directly or via the shared `_contentRepository` field.
- **No async scope API exists anywhere in this codebase** — `ICoreScopeProvider.CreateCoreScope()`/`ICoreScope.Complete()` are fully synchronous, confirmed via grep (zero hits for `CreateCoreScopeAsync`/`IAsyncCoreScope`/`IAsyncScopeProvider`). The `IEFCoreScope<TDbContext>` "bridged scope" mechanism (`ExecuteWithContextAsync`) is the actual enabling mechanism, not a first-class async scope.
- **Controller-layer blast radius is small**: only 3-4 Management API files call `IContentService` directly; the rest go through `IContentEditingService`/`IContentPublishingService` (already-async wrapper services, ~19 and ~4 controller files respectively) which themselves call straight into sync `ContentService` methods un-awaited inside a sync scope (Pattern A). Converting `ContentService` to real async will require touching `ContentEditingServiceBase`/`ContentPublishingServiceBase`'s call sites (turn direct sync calls into awaited calls) more than it requires touching controllers directly.
- **Notification publishing**: `ContentService` only ever calls the sync `Publish`/`PublishCancelable` (never the existing async `PublishCancelableAsync`). The sync `EventAggregator.Publish` already internally does `Task.WaitAll(...)` to invoke async notification handlers — so async handlers already work today via a blocking bridge; converting to real `await PublishAsync(...)` is a comparatively contained, mechanical follow-on change, not a blocker.
- **`AsyncDocumentRepository` is currently `internal sealed class`** — must be un-sealed before `AsyncDocumentBlueprintRepository : AsyncDocumentRepository` can exist. `NodeObjectTypeKey` (`protected abstract` in `AsyncContentRepositoryBase`, `protected override` in `AsyncDocumentRepository`, not `sealed override`) can be re-overridden fine once un-sealed. `EnsureUniqueNaming` is `protected virtual bool ... => true;` on `AsyncPublishableContentRepositoryBase` — already overridable directly by a blueprint subclass without needing further changes on `AsyncDocumentRepository` itself.
- **NPoco's `DocumentBlueprintRepository`** (`src/Umbraco.Infrastructure/Persistence/Repositories/Implement/DocumentBlueprintRepository.cs`) is tiny: constructor purely forwards every parameter to `base(...)` (plus one `[Obsolete]` legacy constructor), and overrides exactly two members — `EnsureUniqueNaming => false` (duplicates allowed for blueprints) and `NodeObjectTypeId => Constants.ObjectTypes.DocumentBlueprint`. `IDocumentBlueprintRepository : IDocumentRepository` is a pure marker interface, adds no new members.

## Session of 2026-08-07: DI registration, and two of the four production consumers fully migrated off IDocumentRepository

Picking up from "DI registration done" above — this session did real consumer migration work, not just registration plumbing.

### DI registration bug found and fixed
The DI registration itself (`AddUnique<IAsyncDocumentRepository, AsyncDocumentRepository>()`) looked fine — full test suite passed — but that was misleading, since every existing test constructs the repository manually (`new AsyncDocumentRepository(...)`), never through the container. The first real DI-constructed consumer (`DocumentUrlService`, see below) failed at runtime: `AsyncDocumentRepository`'s constructor was `internal`, and `Microsoft.Extensions.DependencyInjection`'s default container only reflects over **public** constructors regardless of the containing class's own accessibility. Fixed by making both `AsyncDocumentRepository`'s and `AsyncDocumentBlueprintRepository`'s constructors `public` (classes stay `internal`) — confirmed this matches the established convention already used by every other `internal`-class repository in the codebase (e.g. `ContentTypeRepository`). See the dedicated feedback memory `feedback_internal_class_needs_public_constructor_for_di`. Committed as `fe2175eaffa` + `4ef772dd164`.

### `DocumentUrlService` — fully migrated off `IDocumentRepository`
`RebuildAllUrlsAsync()` now takes a `CancellationToken` (breaking signature change — no obsolete-overload dance, since the breaking change is already announced for this version), threaded from `InitAsync` down into `_documentRepository.GetAllAsync(cancellationToken)` on the new `IAsyncDocumentRepository`. Committed as `4ef772dd164`.

### `DeferredSearchReindexService` — fully migrated off `IDocumentRepository`
Both of its document-facing methods converted; `IDocumentRepository` is now entirely unused in this class and was removed (field + constructor param deleted).

- `ReindexDocumentsReferencingElements` → `ReindexDocumentsReferencingElementsAsync`: `FindDocumentIdsReferencingElements` renamed to `FindDocumentKeysReferencingElements`, now collects `document.Key` (Guid) instead of `document.Id` (int) — no `IIdKeyMap` bridge needed at all, since the entities the relations traversal (`IUmbracoEntity`) already returns carry a `.Key` alongside `.Id`. The element-to-element BFS traversal itself stays int-keyed (unrelated to this migration — it's `IRelationService`-keyed). Committed as `c9ef8179ecb`.
- `ReindexContentOfContentTypes` → `ReindexContentOfContentTypesAsync`: now reads through a brand-new purpose-built repository method, `IAsyncDocumentRepository.GetPagedOfContentTypesAsync` (see below). Committed as `764c6938f52` (the new repository method) and `6086fb4fe9d` (wiring + the `AsyncPageAndReindex`/`ResolveKeysAsync` refactor described below).

### New repository method: `GetPagedOfContentTypesAsync`
Added to `IAsyncDocumentRepository` — pages `IContent` filtered by a set of content-type keys, ordered by any existing field plus a new `"path"` case. This is **not** a violation of the "no generic `IQuery<T>` port" decision above (arbitrary IQuery<IContent> stays permanently blocked) — it's a narrow, purpose-built, already-reused shape: the same filter-by-content-type-ids-paged pattern already backs `IContentService.GetPagedOfType(s)` and 3 real NPoco call sites. Built via TDD (tests written and confirmed failing first) and verified by an independent review agent using the same 3-axis brief (behavioral correctness / conventions / cleanliness) as prior migration work, including a TDD-honesty revert check on the new `"path"` ordering case.

Two design decisions worth remembering for the next purpose-built method added to this interface:
1. **Takes `Guid[] contentTypeKeys`, not `int[] contentTypeIds`** — even though the EF Core DTO column (`ContentDto.ContentTypeId`) and every current caller are int-based, `IAsyncDocumentRepository` is Guid-first throughout and this should not be the one exception. Content types are themselves nodes, so their key lives on `NodeDto.UniqueId`; resolved to the underlying node ID via one batched `db.Nodes.Where(...).Select(node => node.NodeId)` query scoped to `Constants.ObjectTypes.DocumentType`, not per-key `IIdKeyMap` round-trips. See the dedicated feedback memory `feedback_guid_keys_not_int_ids_on_async_repository`.
2. **`ApplyDocumentOrdering`'s new `"path"` case** was added as an *optional, nullable* selector parameter with a guarded switch arm (`"path" when pathSelector is not null => ...`), specifically so the three existing callers (`GetChildrenCoreAsync`/`GetDescendantsCoreAsync`/`GetPagedRecycleBinAsync`, none of which pass it) and the existing ordering unit test file are completely unaffected — a failing `when` guard falls through to the `_` default arm. Preferred over either a required parameter (would've touched 3 unrelated call sites + broken the existing unit test) or a special-cased inline branch bolted onto just the new method (would've duplicated the tiebreak/descending logic outside the switch).

### `AsyncPageAndReindex<TEntity>` + `ResolveKeysAsync` extracted (`DeferredSearchReindexService.cs`)
After wiring `ReindexContentOfContentTypesAsync` inline, refactored into two reusable pieces:
- `AsyncPageAndReindex<TEntity>` — async counterpart of the existing `PageAndReindex<TEntity>`, mirroring its **exact** `page`/`page * pageSize < total` loop shape (a functionally-equivalent `skip`-accumulator version was tried first and rejected on review for looking inconsistent with its sync sibling, even though mathematically identical — worth matching established loop idioms exactly, not just achieving equivalent behavior, when a method is explicitly framed as another one's "counterpart"). Takes a `Func<int, int, Ordering?, CancellationToken, Task<PagedModel<TEntity>>>` fetch delegate rather than a repository interface directly, since document/media/member don't share a common async paged-query interface yet (only `IAsyncDocumentRepository.GetPagedOfContentTypesAsync` exists today) — media/member reindexing can adopt it once they get their own async purpose-built paged methods. Media/member reindexing itself was **not** touched — still on the sync `PageAndReindex`/NPoco, since no async repository exists for them yet.
- `ResolveKeysAsync` — the int-id→Guid-key bridge via `IIdKeyMap.GetKeyForIdAsync`, needed because the cache-refresher notifications feeding `QueueContentTypeReindex` are still sync and int-keyed. Carries a `TODO` noting it should be removed once that notification pipeline carries Guid keys directly instead of sync int IDs — this bridge is scaffolding for the same reason the whole `IDocumentRepository`→`IAsyncDocumentRepository` effort is scaffolding, not a permanent fixture.

### Current consumer status (of the original 4)
- `DocumentUrlService` — fully migrated (done this session).
- `DeferredSearchReindexService` — fully migrated (done this session).
- `DocumentBlueprintRepository` — resolved via class-inheritance (`AsyncDocumentBlueprintRepository`, done earlier).
- `ContentService` — **the only one left, and by far the largest** (nearly the whole interface). Still 100% synchronous. Still NOT scoped into a concrete plan — per the "one increment at a time" instruction, wait for explicit direction before planning it. See the `ef-core-document-repository-implementation-status` memory for the detailed method-by-method tier breakdown (an HTML ledger artifact was also built categorizing every `ContentService`/`PublishableContentServiceBase` method that touches the document repository into priority tiers — Tier A "start here," through Tier F "write/publish orchestration, save for last").
```

### project_ef_core_document_repository_status.md

```markdown
---
name: ef-core-document-repository-implementation-status
description: "Current state of AsyncDocumentRepository on v18/feature/ef-core-document-repository — read path, write path, tags, IsMoving, recycle bin, permissions, publish-scheduling/publish-status, sibling name-uniqueness, UpdateSortOrderAsync, and DI registration all done+tested (127/127); pessimistic locking deliberately documented-not-implemented; ContentService's sync-to-async conversion is the remaining phase"
metadata:
  node_type: memory
  type: project
---

Branch `v18/feature/ef-core-document-repository` (based on `v18/feature/ef-core-repositories`) now has both the **read path and the full write path** implemented and tested for `AsyncDocumentRepository`. This supersedes the previous version of this memory, which described the write path as entirely unimplemented — a full multi-phase implementation session closed that gap. See `project_ef_core_dto_phase1` (historical) for the DTO-only phase.

**Why:** Same investigation thread continued across multiple long sessions: fixed the two open TODOs first (custom property field ordering, obsolete language lookup), then planned and implemented the write path in four phases via delegated subagents, then fixed five issues a review agent found, then continued through recycle bin, permissions, scheduling, name-uniqueness, blueprint repo, and UpdateSortOrderAsync.

**How to apply:** When resuming this branch, the write path is real and tested — don't re-derive it from scratch. The remaining gaps below are the actual TODO list.

### Implemented and tested (read path — unchanged from before)
- `GetAsync`/`GetAllAsync`/`GetManyAsync`, `GetVersionAsync`/`GetAllVersionsAsync`/`GetVersionKeysAsync`/`DeleteVersionAsync`
- `GetChildrenAsync`/`GetChildrenWithoutTemplatesAsync`, `GetDescendantsAsync`/`GetDescendantsWithoutTemplatesAsync`, including **custom property field ordering** (`Ordering.IsCustomField`) — sorts in-memory with a native-typed comparer (int/decimal/date/string), not NPoco's zero-padded-string SQL trick
- `CheckDataIntegrityAsync`, count endpoints, culture-specific name ordering
- Culture-variation ISO-code resolution no longer uses the obsolete `ILanguageRepository.GetIsoCodeByIdAsync` bridge — `LoadVariationsAsync` loads the language table once into a dictionary instead

### Implemented and tested (write path)
`PersistNewItemAsync`/`PersistUpdatedItemAsync`/`BuildEntityDto`/`OnUowRefreshedEntityAsync`, covering:
- New: node/path/level/sortorder assignment (incl. sort-order collision handling, blueprint/import `GetReservedId` support), content/version/property-data rows, default template assignment
- Update: dirty-check short-circuit, current-version guard, parent-move path/level/sortorder recompute, PropertyData diff-reconcile (update-in-place/insert/delete, batched, uses tracked-entity mutation + one `SaveChangesAsync` rather than per-row `ExecuteUpdateAsync`)
- Culture variants for both New and Update (per-culture names, edited/available/published flags)
- Save-and-publish-in-one-call double-insert version dance for both New and Update (each new draft version gets a genuinely fresh `Guid` Key — EF Core has no DB-side default here, unlike NPoco)
- `SortableValue` population on write (ported `SetEntitySortableValues`, needed a new `IIdKeyMap` constructor dependency — safe to add, class is `internal`)
- `OnUowRefreshedEntityAsync` fires the `[Obsolete]` `ContentRefreshNotification` deliberately — it's still the only signal `CacheRefreshingNotificationHandler` (HybridCache) listens for; NPoco's own `DocumentRepository` does the same today

### Implemented and tested (tags + IsMoving fast path)
- **Tags**: `SetEntityTagsAsync`/`ClearEntityTags` ported from NPoco's `ContentRepositoryBase.SetEntityTags`/`ClearEntityTags`, with `ITagRepository`/`IJsonSerializer` added as new constructor dependencies (obsolete-constructor pattern not needed — class is `internal`, DI not yet wired anywhere). Called from `PersistUpdatedItemAsync` (when `publishing`) and from `ApplyPostPublishFlagFlipsAsync` (now async, on both the Publishing and Unpublishing branches), matching NPoco's two call sites exactly.
- **`IsMoving()` bulk-move fast path**: `PersistUpdatedItemAsync` now branches on it. Structure is an early-return, not a threaded `if (!isMoving)` through the whole method: the unpublish-old-version / `SanitizeNames` / `SanitizeEntityPropertiesForXmlStorage` / ParentId-dirty-recompute block is wrapped in `if (!isMoving)`; `BuildEntityDto` + the Node row `ExecuteUpdateAsync` always run; then `if (isMoving) { OnUowRefreshedEntityAsync; ResetDirtyProperties; IsolatedCache.Clear; return true; }`; everything from the ContentTypeId update onward is left untouched and now only executes on the non-moving path. Matches NPoco's `PublishableContentRepositoryBase.PersistUpdatedItem`.
- File: `src/Umbraco.Infrastructure/Persistence/Repositories/Implement/EFCore/AsyncDocumentRepository.cs`.

### Implemented and tested (recycle bin)
- `GetRecycleBinAsync` — mirrors NPoco's `ContentRepositoryBase.GetRecycleBin`: a flat `node.Trashed` filter with NO depth/path restriction.
- `GetPagedRecycleBinAsync` — same flat-trashed filter, but paged/ordered; built by structurally copy-adapting `GetChildrenCoreAsync`. Supports all three ordering branches (invariant/default, culture-variant name, custom-field).
- `RecycleBinSmellsAsync` — mirrors NPoco's `PublishableContentRepositoryBase.RecycleBinSmells`: checks for a DIRECT child of the recycle bin node (`Constants.System.RecycleBinContent = -20`) only — materially different semantic from `GetRecycleBinAsync`'s "any depth" filter. Deliberately omits NPoco's synchronous `IAppPolicyCache.Get(...)` wrapper — no async factory overload exists for it.
- `GetPagedRecycleBinAsync` migrated from pageIndex/pageSize to skip/take (matching the rest of the class) — no obsolete-constructor dance needed since `AsyncDocumentRepository` was never shipped/DI-registered.

### Bug fixed by review: missing pagination tiebreaker
The shared `ApplyDocumentOrdering` helper and the `FetchCultureNameOrdered` local functions ordered purely by the requested field with no secondary tiebreaker — NPoco's equivalent unconditionally appends `ORDER BY umbracoNode.id` (citing http://issues.umbraco.org/issue/U4-8831). Fixed by adding `.ThenBy(idSelector)`/`.ThenBy(joined => joined.node.NodeId)`. **Important testing lesson**: an integration-level regression test for this could NOT discriminate (SQLite's freshly-inserted-row scan order coincidentally matches NodeId order regardless of the fix) — see the `feedback_sqlite_harness_masks_ordering_tiebreak_bugs` memory below. Fixed by unit-testing the ordering helper directly (`AsyncDocumentRepositoryOrderingTests.cs`, `Umbraco.Tests.UnitTests`) with a manipulated in-memory sequence where the higher-id row comes first — LINQ-to-Objects' guaranteed-stable sort makes this genuinely discriminating.

### Permissions migrated natively to EF Core
All 4 `IAsyncDocumentRepository` permission methods (`ReplaceContentPermissionsAsync`, `AssignEntityPermissionAsync`, `GetPermissionsForEntityAsync`, `AddOrUpdatePermissionsAsync`) are natively EF-Core-backed — no NPoco delegation. Went through 3 iterations: (1) delegated to `IDocumentRepository` directly — rejected, that's the repository being replaced (see `feedback_dont_depend_on_repository_being_replaced` below); (2) delegated to NPoco's `PermissionRepository<IContent>` sub-repository instead — user then asked to just migrate this now since it's a sub-repository of the document repository; (3) final: ported to EF Core natively via a new `AsyncPermissionRepository<TEntity>` sub-repository class (manually constructed inside `AsyncDocumentRepository`'s constructor, not DI-registered, mirroring NPoco's own sub-repo pattern). Key finding: `UserGroupDto` did NOT need porting to EF Core to do this — the storage table's `userGroupKey` column is a plain `Guid`, and int↔Guid group-id translation is handled by the already-DI-registered `IUserGroupService`.

### Publish-scheduling / publish-status methods implemented
`AsyncPublishableContentRepositoryBase` had 11 `virtual` members throwing `NotImplementedException` (schedule CRUD, has/get-for-expiration/release, count-published, is-path-published, get-schedules-by-keys). All 11 implemented — 10 generically in the base class, plus `IsPathPublishedAsync` overridden concretely in `AsyncDocumentRepository` (the one method NPoco itself implements per-entity-type rather than sharing). Required widening the `TEntityDto` generic constraint to `IPublishableContentDto<TContentVersionDto>` — verified safe via git history (NPoco's own base class always required the analogous constraint) and via confirming the only two possible instantiators are `IContent`/`IElement`.

Found 3 real bugs only by running tests: (1) `ContentService.Publish` rejects publishing a node whose ancestor path isn't published — not documented anywhere obvious; (2) `UmbracoDbContext` is globally `NoTracking` — any read-then-mutate-then-save method needs explicit `.AsTracking()` (see the dedicated feedback memory below); (3) `ContentScheduleDto.NodeId` has a real FK to `ContentDto` — can't insert a schedule row for a node with no `umbracoContent` row (e.g. system nodes).

### Sibling name-uniqueness implemented
Replaced a blank-name-only stand-in with a real port of NPoco's `PublishableContentRepositoryBase.SanitizeNames`: literal duplicate-name suffixing, URL-segment collision detection (fixes umbraco/Umbraco-CMS#22070 for the EF Core path too), and per-culture uniqueness for variant content. Split to mirror the schedule/publish-status precedent: shared orchestration in `AsyncPublishableContentRepositoryBase` behind two virtual hooks, `AsyncDocumentRepository` overrides both to add the URL-segment check (needed a new `IShortStringHelper` dependency).

**Important architectural rule surfaced here** (see the dedicated feedback memory below): never reference an NPoco repository class from EF Core code, even a stateless `internal static` helper — those classes are slated for deletion. The URL-segment logic was originally implemented as a call to NPoco's `DocumentRepository.EnsureUniqueUrlSegment` — rejected by the user, fixed by copying the method's implementation directly into `AsyncDocumentRepository.cs`.

Also surfaced a stale-build gotcha during review (see the dedicated feedback memory below) — a review agent's temporary revert-rebuild-restore TDD-honesty check left a stale DLL that made 5 tests look like they'd regressed when the source was actually fine; diagnosed via temporary debug tracing, resolved by forcing a real rebuild instead of trusting `--no-build`.

### Explicitly NOT implemented — deliberate, documented gap
- **Pessimistic row locking** — NPoco's `ForUpdate()` in `ReplacePropertyValues` has no direct EF Core equivalent. A full EF-Core-native mirror (query-tagging + a SQL-Server-only `DbCommandInterceptor` injecting `WITH (UPDLOCK)`) was designed in detail but the user decided against building it — instead `PersistUpdatedPropertyDataAsync`'s comment was strengthened to explain why the gap is acceptable: `ContentService` always holds the global `Constants.Locks.ContentTree` write lock (a real cross-server DB lock) before reaching this path, and `EFCoreScope` shares the ambient NPoco scope's physical connection/transaction, so that lock already serializes the read-then-write sequence across servers. Documentation-only change.
- **Relations** — intentionally never touched by the repository (matches NPoco — reconciled by the `ContentRelationsUpdate` notification handler after `ContentService` saves). Not a gap, correct architecture.
- **`UserGroupDto`/`UserGroupRepository` EF Core port** — still NPoco-only, not a blocker for anything permission-related (decoupled via `IUserGroupService`), tracked as its own unrelated future task.
- **`ContentScheduleDto` has no indexes** — matches NPoco's existing behavior exactly, not a regression.

### DI registration done (2026-08-07)
`IAsyncDocumentRepository`/`AsyncDocumentRepository` and `IAsyncDocumentBlueprintRepository`/`AsyncDocumentBlueprintRepository` are now registered in `src/Umbraco.Infrastructure/DependencyInjection/UmbracoBuilder.Repositories.cs` via `AddUnique` (Singleton lifetime), placed right next to and mirroring the NPoco `IDocumentBlueprintRepository`/`IDocumentRepository` registrations (blueprint before document). No prior "Async" repository in the codebase had ever been DI-registered, so there was no existing precedent — this sets one, matching the NPoco sibling's lifetime exactly. All 18 constructor dependencies for both classes already resolved from the container with zero additional registrations needed (confirmed by a full rebuild + the full 127/127 `AsyncDocumentRepositoryTest`/`AsyncDocumentBlueprintRepositoryTest` suite passing unchanged). `AsyncPermissionRepository<TEntity>` stays manually `new`'d inside `AsyncDocumentRepository`'s constructor — it's a private sub-repository field, not itself DI-registered. Test files still use their own manual `CreateRepository()` helpers (deliberate test isolation via `AppCaches.Disabled`/`Mock.Of<...>()` for some deps) rather than resolving the interface from the container — that's unchanged and intentional, not something this step touched.

**Update (2026-08-07, later same day)**: that "0 additional registrations needed" conclusion was misleading — see `feedback_internal_class_needs_public_constructor_for_di` and the retirement-plan memory's "DI registration bug found and fixed" section. The repository test suite passing proved nothing about DI-constructibility, since it never actually asked the container to build the type. `AsyncDocumentRepository`/`AsyncDocumentBlueprintRepository`'s constructors had to be changed from `internal` to `public` before any real DI-resolved consumer (`DocumentUrlService`, `DeferredSearchReindexService`) could actually use them.
```

### feedback_internal_class_needs_public_constructor_for_di.md

```markdown
---
name: internal-class-needs-public-constructor-for-di
description: "An internal repository/service class registered with plain AddUnique<TInterface, TImpl>() needs a public constructor even though the class itself stays internal — Microsoft.Extensions.DependencyInjection's default container only reflects over public constructors"
metadata:
  node_type: memory
  type: feedback
---

`AsyncDocumentRepository`/`AsyncDocumentBlueprintRepository` were registered in DI (`builder.Services.AddUnique<IAsyncDocumentRepository, AsyncDocumentRepository>()`), and the repository's own integration tests (which construct it manually via `new AsyncDocumentRepository(...)`) all passed — but the first real DI-constructed consumer (`DocumentUrlService`, once refactored to depend on `IAsyncDocumentRepository`) failed at runtime with `System.InvalidOperationException: A suitable constructor for type 'AsyncDocumentRepository' could not be located`.

**Why:** the constructor was declared `internal`. `Microsoft.Extensions.DependencyInjection`'s default `ServiceProvider` only discovers **public** constructors via reflection (`Type.GetConstructors()` with default flags), regardless of the containing class's own accessibility. An `internal` constructor on an `internal` class is invisible to it. This bug was invisible for a long time because nothing had ever actually asked the DI container to construct the type — every existing test used manual `new(...)` construction, which bypasses the container's constructor-discovery logic entirely.

Confirmed this is the established, working convention already used by every OTHER `internal` repository in this codebase registered the same way — e.g. `ContentTypeRepository` is `internal sealed class ContentTypeRepository` with a `public ContentTypeRepository(...)` constructor. The class being `internal` already prevents external assemblies from doing `new ContentTypeRepository(...)`; the constructor itself being `public` is what lets the DI container inside the same process construct it.

**How to apply:** When adding DI registration (`AddUnique<TInterface, TImpl>()`, `AddSingleton<TInterface, TImpl>()`, etc., without an explicit factory lambda) for any `internal` class in this codebase, its constructor must be `public`, not `internal`. If a repository/service class only has manual-construction test coverage (no DI-registered consumer yet), that test coverage does NOT prove the class is DI-constructible — write or find at least one test that resolves the type via the real DI container (`GetRequiredService<T>()` against a composed `UmbracoBuilder`) before trusting that registration works.
```

### feedback_guid_keys_not_int_ids_on_async_repository.md

```markdown
---
name: guid-keys-not-int-ids-on-async-repository
description: "Every new method added to IAsyncDocumentRepository must take Guid keys, not int IDs, even when the underlying EF Core DTO column and every current caller are int-based — resolve ints to Guids internally instead"
metadata:
  node_type: memory
  type: feedback
---

`GetPagedOfContentTypesAsync` was first built taking `int[] contentTypeIds`, justified at the time by three real facts: `ContentDto.ContentTypeId` is `int` in the EF Core schema (same as NPoco), it matched `IContentService.GetPagedOfTypes(int[] contentTypeIds, ...)`'s existing signature, and it matched the caller's (`DeferredSearchReindexService`) fully int-based pipeline end to end. The user rejected this: "Everywhere else we use keys, we don't want IDs I think we should change this."

**Why:** `IAsyncDocumentRepository` is deliberately Guid-first throughout (an explicit standing decision from earlier in this migration, see `project_document_repository_retirement_plan`) — every other method on the interface takes a Guid key (`parentKey`, `ancestorKey`, `entityKey`, `groupKeys`). `int[] contentTypeIds` would have been the only exception, breaking that consistency for no reason other than "it was the smaller diff." That a caller currently happens to be int-based is not a good enough reason to leak that constraint into the new interface's shape — the caller should adapt to the repository's convention, not the other way around.

**The fix, concretely:** content types are themselves nodes (their own `umbracoNode` row), so their Guid key lives on `NodeDto.UniqueId`, not on `ContentTypeDto`. Changed the signature to `Guid[] contentTypeKeys`, and inside the method resolve them to the underlying node IDs that `ContentDto.ContentTypeId` actually stores via one batched query: `db.Nodes.Where(node => node.NodeObjectType == Constants.ObjectTypes.DocumentType && contentTypeKeysList.Contains(node.UniqueId)).Select(node => node.NodeId).ToListAsync(...)`. This is a single round-trip through the live EF Core context — not N calls through `IIdKeyMap.GetKeyForIdAsync`/`GetIdForKeyAsync` (which has no batch API and would mean one DB round-trip per key).

**When the CALLER is the thing that's int-based** (as with `DeferredSearchReindexService.ReindexContentOfContentTypes`, fed by int content-type IDs from cache-refresher notification payloads), the int→Guid bridge belongs in the caller, using `IIdKeyMap` per-id (acceptable there specifically because content-type ID batches are small/bounded, unlike a hypothetical large document batch) — not by making the repository method itself accept ints to avoid that translation step.

**How to apply:** Before adding any new method to `IAsyncDocumentRepository` (or extending its established Guid-first siblings), check what identifier type the *rest of the interface* uses, not just what's convenient for the underlying DTO column or the first caller. If the natural DTO/caller type is `int`, that's a signal the resolution step needs to happen somewhere (inside the method via a Node join, or in the caller via `IIdKeyMap`) — it's not a license to break the interface's Guid-first convention.
```

### feedback_no_npoco_class_references_from_efcore.md

```markdown
---
name: no-npoco-class-references-from-efcore
description: Never reference an NPoco repository class from EF Core code, even a static/internal helper method with no NPoco-specific dependency itself — copy it instead, since the class is slated for deletion
metadata:
  type: feedback
---

Don't call a method on an NPoco repository class (`DocumentRepository`, `ContentRepositoryBase`, etc.) from EF Core repository code, even when that specific method is a self-contained `internal static` helper with zero NPoco-specific dependencies (no `Sql<ISqlContext>`, no `Database.Fetch`, pure C# string/collection logic). The method itself being "safe" to call doesn't matter — the class it lives on is slated for deletion once the EF Core migration completes, so any reference to it, however small, blocks that deletion later.

**Concrete instance**: `AsyncDocumentRepository.EnsureUniqueNodeNameAsync` originally called NPoco's `DocumentRepository.EnsureUniqueUrlSegment` (an `internal static` method in the same assembly, no NPoco dependency of its own — just string/`IShortStringHelper` logic). The user rejected this immediately: "we cannot do this as the goal is to kill DocumentRepository." Fixed by copying the method's full implementation directly into `AsyncDocumentRepository.cs` as a private static method.

**Why this is a distinct trap from "don't depend on the repository being replaced"**: that rule is about *delegating unported business logic* to the NPoco repository at runtime (a real functional dependency). This is different — it's reusing a small, pure, already-correct utility method that happens to be *declared on* the doomed class, not calling back into the doomed class's actual repository behavior. It's tempting to treat "it's just a static helper, no real coupling" as safe, but the class-level reference is exactly what blocks deletion — the compiler doesn't care that the method is stateless.

**How to apply**: before referencing ANY member (static or instance, however small) on `DocumentRepository`, `ContentRepositoryBase`, `PublishableContentRepositoryBase`, or any other NPoco repository class from new EF Core code, stop and copy the implementation into the EF Core file instead — don't just check "does this method itself have a NPoco dependency," check "does this method live on a class that's going away." The one exception: genuinely standalone utility classes that are NOT repository classes themselves and are used by multiple NPoco repos too (e.g. `SimilarNodeName` — a plain data/algorithm class in the same namespace as the NPoco repos, but not itself a repository, and not tied to any one repository's deletion) are fine to keep depending on.
```

### feedback_stale_build_after_subagent_revert.md

```markdown
---
name: stale-build-after-subagent-revert
description: After a review subagent temporarily reverts+rebuilds+restores a file for a TDD-honesty check, the compiled DLL can be stale even though the source is correct — don't trust `dotnet test --no-build` right after, force a real rebuild
metadata:
  type: feedback
---

A review subagent's TDD-honesty check (temporarily revert a fix to old behavior, rebuild, confirm tests fail, restore the fix) rebuilds the DLL against the *reverted* source mid-check. If it restores the source afterward but doesn't rebuild again, the DLL on disk still reflects the reverted (old/broken) behavior even though `git diff` shows the correct, restored source.

**Why this is easy to miss**: running `dotnet test --no-build` right after such a check uses that stale DLL. The resulting failures look exactly like a real regression — in one case, 5 tests failed with symptoms consistent with the new logic simply not running at all — even though the source on disk was completely correct. Confirmed by adding temporary debug `Console.WriteLine` tracing to the method in question: the trace never appeared in test output despite clearly being present in the source, which was the tell that the *compiled binary* didn't match the *source*.

**How to apply**: after any subagent (or your own) revert-rebuild-restore cycle on a file — including ones done for a legitimate TDD-honesty check — do not run the next verification with `--no-build`. Force a full rebuild first (plain `dotnet test` without `--no-build`, or an explicit `dotnet build` beforehand) so the DLL is guaranteed to match the current source before trusting a pass/fail result. If a suite fails right after such a cycle and the failures don't match any code change you're aware of, suspect a stale build before suspecting a real bug — rebuild and rerun before spending time debugging.
```

### feedback_dont_depend_on_repository_being_replaced.md

```markdown
---
name: dont-depend-on-repository-being-replaced
description: "When an EF Core repository delegates to NPoco for unported functionality, never depend on the NPoco repository interface/class it is itself replacing — depend on a narrower sub-repository/service instead"
metadata:
  node_type: memory
  type: feedback
---

When a new EF Core repository (e.g. `AsyncDocumentRepository`) temporarily delegates some of its methods to existing NPoco logic (because that logic hasn't been ported to EF Core yet), do not inject the NPoco repository interface that the new class is *itself replacing* (e.g. `IDocumentRepository` for `AsyncDocumentRepository`). Depend on a narrower, longer-lived sub-repository or Core-level service that actually owns the logic instead (e.g. `PermissionRepository<TEntity>`, the sub-repo `DocumentRepository` itself delegates permission storage to).

**Why:** The user caught this directly — "we don't want to depend on Documentrepository at all, we're replacing it, inject permissionsrepository instead." Depending on the class being replaced is backwards: once the old NPoco repository is eventually deleted (the whole point of the migration), the new class would break or need untangling. A narrower sub-repository/service that isn't tied to the old repository's lifecycle survives the migration and is the correct long-term dependency.

**How to apply:** Any time an EF-Core-migration repository in this codebase needs to delegate to not-yet-ported NPoco functionality, check whether the logic lives in the "parent" NPoco repository being replaced, or in a separate sub-repository/service it merely calls into. Depend on the latter. If the logic is genuinely only reachable via the parent repository (no extractable sub-repository), flag this to the user rather than assuming a direct dependency is fine — it may be worth extracting a sub-repository first, exactly like `PermissionRepository<TEntity>` already was in NPoco's own design. If the sub-repository isn't DI-registered yet, self-register it (`AddSingleton<ConcreteType>()`, no interface needed — see `ExternalLoginRepository` for the existing pattern) rather than reaching for the parent's DI registration as a shortcut.

**Update (same session, immediately after):** the user changed direction again and asked for the permission *storage* itself to be migrated to EF Core now (not delegated to NPoco at all), since it's "a sub-repository, so part of the document repository." So the `AddSingleton<PermissionRepository<IContent>>()` DI registration described above was itself reverted — the final state is a brand-new native EF Core class (`AsyncPermissionRepository<TEntity>`), manually constructed (not DI-registered) by `AsyncDocumentRepository`, mirroring NPoco's sub-repo pattern exactly. Lesson still holds (don't depend on the repository being replaced) — it just turned out the better fix here was porting the sub-repository's logic itself rather than delegating to either NPoco version of it. When a user is actively migrating a repository to EF Core, don't assume delegation-to-NPoco is the final answer just because it's the smaller diff — ask whether the sub-concern should be ported now instead, especially when doing so turns out to be decoupled and low-risk once you look closely.
```

### feedback_efcore_notracking_requires_astracking.md

```markdown
---
name: efcore-notracking-requires-astracking
description: UmbracoDbContext sets QueryTrackingBehavior.NoTracking globally — any EF Core repository method that reads-then-mutates-then-saves an entity must add .AsTracking() explicitly or SaveChangesAsync silently does nothing (or throws a duplicate-tracking exception)
metadata:
  node_type: memory
  type: feedback
---

`UmbracoDbContext` configures `optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` globally (`src/Umbraco.Infrastructure/Persistence/EFCore/UmbracoDbContext.cs:198`). Every LINQ query against it returns fresh, untracked entity instances by default — mutating one and calling `SaveChangesAsync()` does nothing, since EF Core doesn't know the instance needs saving.

**Why this matters more than it looks:** the `IEFCoreScope<T>.ExecuteWithContextAsync` DbContext instance is cached per ambient scope (`EFCoreScope<T>` has a single `_dbContext` field, reused across every repository call within one `NewScopeProvider.CreateScope()`/`ICoreScope`). So if one repository call does `db.Foo.Add(...)` + `SaveChangesAsync()` (which DOES track the added entity, regardless of the global query default), and a *later* call in the same scope queries that same row again without `.AsTracking()`, EF Core returns a *new*, untracked clone — and if that clone is then passed to `.Remove()` or otherwise attached, you get `InvalidOperationException: cannot be tracked because another instance with the same key value ... is already being tracked`.

**How to apply:** any time an EF Core repository method needs to load an entity, mutate a property, and save the change (rather than doing a pure `ExecuteUpdateAsync`/`ExecuteDeleteAsync` bulk statement), add `.AsTracking()` to that specific query. The established, already-commented precedent for this is `PersistUpdatedPropertyDataAsync` in `AsyncDocumentRepository.cs` — copy its comment/reasoning rather than re-discovering this from scratch. Don't assume "I queried it, so EF Core is tracking it" — check whether the query has `.AsTracking()`; if not, it isn't.
```

### feedback_large_efcore_migration_workflow.md

```markdown
---
name: feedback-large-efcore-migration-workflow
description: "Validated workflow for large EF Core repository migration work — phased plan, delegate each phase with an exhaustive self-contained brief, independently re-verify every claim"
metadata:
  node_type: memory
  type: feedback
---

For large EF Core repository migration work in this codebase, the following workflow has been used across multiple long sessions without correction or pushback from the user — treat it as validated.

**Why:** The user explicitly asked for independent review agents repeatedly ("same parameters as last time") and consistently approved delegating implementation phases to subagents, then approved fixes found during review without re-litigating scope.

**How to apply:**

1. **Research before delegating.** Before handing an implementation phase to a subagent, personally read the actual NPoco reference method end-to-end (not a summary of it), the exact EF Core DTO shapes involved, and any existing EF Core precedent in the same codebase. Extract concrete gotchas yourself. A subagent brief built from this research, including verbatim code snippets and exact file:line references, produces far more faithful ports than a brief that just names the method and says "port this."

2. **One phase per subagent call**, sized so each phase is genuinely testable end-to-end on its own. Each phase's brief should explicitly list what NOT to implement yet (with a one-line reason).

3. **Require the delegate to follow this repo's TDD rule**: write the new tests first, run them against the still-unimplemented code, confirm they fail with the expected symptom (not a compile error), only then implement, then confirm green, then re-run the FULL existing test file (not just the new tests) to catch cross-phase regressions.

4. **After every delegated phase, independently re-verify — do not just read the agent's summary.** `git status --short`/`git diff --stat` to confirm only expected files changed, rebuild, rerun the full test filter, and read the actual new code for at least the highest-risk section per phase.

5. **When a review agent finds something, and it's a genuine bug (not a style nit), fix it immediately in the same turn** rather than only documenting it.

6. **Review agents should get the same 3-axis brief each time**: behavioral correctness, alignment with existing conventions, code cleanliness/duplication — and should independently rebuild/retest, not trust prior reports.

7. **The `Workflow` tool works well for this exact pattern when the user says "fan out"** — but still independently re-verify the final workflow result exactly like a single delegated agent's output.
```

### feedback_sqlite_harness_masks_ordering_tiebreak_bugs.md

```markdown
---
name: feedback-sqlite-harness-masks-ordering-tiebreak-bugs
description: SQLite integration test harness coincidentally preserves NodeId/insertion order for tied sort keys; resolved by unit-testing the pure ordering helper in isolation with a manipulated in-memory sequence instead
metadata:
  node_type: memory
  type: feedback
---

When adding or fixing ORDER BY / paging logic in EF Core repositories tested against this repo's default SQLite integration test harness (`tests/Umbraco.Tests.Integration`), a test asserting a specific tiebreak order for rows with equal sort-key values can pass **both before and after** the fix — proving nothing, in direct violation of this repo's own TDD rule (root `CLAUDE.md` → "Tests for a bug fix must fail before the fix").

**Why:** Freshly-inserted rows in a per-test SQLite schema get NodeId == insertion order, and for the join shapes used in this codebase's EF Core repositories (nested-loop joins over `Nodes` as the outer/driving table), the engine's default row-scan order for ties coincidentally matches NodeId-ascending order regardless of whether an explicit `.ThenBy(idSelector)` is present.

**Resolution that actually worked**: when the user asked for a regression test "by manipulating the NodeId," the right move was to stop trying to force nondeterminism through the database and instead unit-test the pure ordering logic directly, decoupled from any DB engine:
1. Changed the private ordering helper from `private static` to `internal static` — a safe, behavior-unchanged visibility bump — so a unit test in `Umbraco.Tests.UnitTests` can call it directly via `InternalsVisibleTo`. (This triggers a new StyleCop SA1600 warning, since `internal` members need XML doc comments but `private` ones don't — fix by adding a `<summary>`/`<remarks>` block.)
2. Built an in-memory `List<T>.AsQueryable()` with the row carrying the HIGHER id placed FIRST in the sequence — deliberately decoupling "sequence order" from "id order," which is impossible to do with real freshly-inserted SQLite rows.
3. Called the helper directly and asserted ascending-id output. TDD-verified by reverting the fix and rerunning: this time it genuinely failed, because LINQ-to-Objects' `OrderBy` is a **documented, guaranteed-stable** sort — unlike SQL engines, where tie-order stability is implementation-defined, not guaranteed.

**How to apply:**
1. If you add or change a `.ThenBy(...)`/secondary-ordering tiebreak fix and want to TDD-verify it at the INTEGRATION level, actually run the new test with the fix temporarily reverted first — don't assume a plausible-looking assertion is discriminating. If it passes both ways in this SQLite harness, don't keep it as a "regression test."
2. Instead, default to a UNIT test that manipulates the ordering key directly, using LINQ-to-Objects' guaranteed-stable sort to make the test reliably discriminating.
3. This is specific to ordering/tiebreak bugs in SQLite in this harness — it is not evidence that TDD verification is generally unreliable here.
```

### feedback_no_section_header_comments.md

```markdown
---
name: no-section-header-comments
description: Never add "// --- Section Name ---" banner comments to group members within a class in this codebase — not this project's style
metadata:
  type: feedback
---

Don't add banner/section-header comments like `// --- AsyncContentRepositoryBase abstract overrides ---`, `// --- Private helpers ---`, `// --- Group 20: Recycle bin ---`, etc. to visually divide a class or test file into regions. This applies to production code and test files alike.

**Why:** The user caught this directly — "You keep adding these section comments like `// --- AsyncContentRepositoryBase abstract overrides ---`, we don't want these, it's against our styling." This is a stricter, more specific rule than the root `CLAUDE.md` "Code Comment Policy" (which already says don't restate what code does) — banner comments are the same failure mode applied at the section/region level instead of the line level.

**How to apply:** When splitting a class file into logical groups, use blank lines and file ordering alone — never a `// --- X ---` (or similarly-styled banner) comment line. If a genuine non-obvious rationale needs stating for a group of members, write a real sentence explaining the *why*, not a restated label of *what the group is*. Applies repo-wide.
```

### feedback_self_contained_code_comments.md

```markdown
---
name: feedback-self-contained-code-comments
description: "Code comments and TODOs must never reference ephemeral session artifacts (plan files under ~/.claude/plans/, this conversation) — only real, durable repo content"
metadata:
  node_type: memory
  type: feedback
---

While implementing the `AsyncDocumentRepository` write path, several TODO/explanatory comments were initially written as "...see the write-path plan" — referencing a local `~/.claude/plans/` file. That file is a per-session planning artifact, not part of the repository; it won't exist for another developer, another session, or even this same session after the plan file is cleaned up.

**Why:** Caught this while doing a follow-up "add TODOs for deferred items" task — had to go back and rewrite every comment that referenced "the write-path plan" into something self-contained.

**How to apply:** When writing any code comment or TODO that explains *why* something is scoped down, deferred, or diverges from a reference implementation, point only at things that will still be resolvable by a future reader with just the repo checked out: other files/classes/methods in the same repo, external issue trackers, or a plain-English explanation inline. Never reference: a local plan file path, "this conversation," "the session," or anything under `~/.claude/`. This applies retroactively too.
```

### feedback_watch_for_stray_file_changes.md

```markdown
---
name: feedback-watch-for-stray-file-changes
description: Always check git status for unexpected file changes before committing — but a repeated .editorconfig mutation this session turned out to be a wanted fix, not an anomaly; resolved and applied properly
metadata:
  node_type: memory
  type: feedback
---

Twice in one session, `git status` showed `.editorconfig` as modified with a single added line that no Edit/Write tool call in the visible conversation had targeted. Both times this was caught before committing (`git status --short` right before staging) and reverted, flagged to the user as an unexplained anomaly.

**Resolution: it was not an anomaly.** The user had explicitly asked to suppress a specific StyleCop rule repo-wide; `.editorconfig` was missing the matching line, which is apparently what the user's IDE/linter reads for live warnings (MSBuild builds were already clean via `.globalconfig`, so this never showed up in `dotnet build` output). Something in the user's environment had already been trying to apply this exact fix twice, and it was discarded both times before the context to recognize it existed.

**Why this still matters:** The instinct to flag and revert an unexplained working-tree mutation before committing was still the *correct* action in the moment. The lesson isn't "trust unexplained changes," it's "an unexplained change that recurs identically across a session is a signal worth surfacing explicitly to the user rather than just silently discarding a second time."

**How to apply:**
1. Before every `git add`/`git commit`, run `git status --short` and scan for any file outside the set you intentionally touched.
2. If something unexpected recurs, don't just revert it silently a second time — say so explicitly and ask.
3. If it turns out to be wanted, apply it properly and deliberately once you have the context, rather than leaving it to keep reappearing and getting reverted.
```

### feedback_always_use_braces.md

```markdown
---
name: feedback-always-use-braces
description: "Always use braces for all conditional/loop bodies, even single-line — never omit braces"
metadata:
  node_type: memory
  type: feedback
---

Always use braces `{}` for every `if`, `else`, `for`, `foreach`, `while`, etc. body, even when the body is a single statement or return.

**Why:** User explicitly corrected bracketless single-liners (e.g. `if (x) return [];`). Consistent braces are the project style.

**How to apply:** Place the body on its own line inside braces in all generated code, plan examples, and code snippets:
```csharp
// CORRECT
if (rows.Count == 0)
{
    return [];
}

// WRONG
if (rows.Count == 0) return [];
```
```

### feedback_enumerable_empty.md

```markdown
---
name: feedback-enumerable-empty
description: Use Enumerable.Empty<T>() instead of cast-to-interface empty collection literals
metadata:
  node_type: memory
  type: feedback
---

Use `Enumerable.Empty<T>()` to return an empty `IEnumerable<T>`, not `(IEnumerable<T>)[]` or similar cast hacks.

**Why:** `Enumerable.Empty<T>()` is the idiomatic, allocation-free way to return an empty enumerable. Cast tricks like `(IEnumerable<IContent>)[]` are workarounds that obscure intent.

**How to apply:** Any time a method returns `IEnumerable<T>` and the result is empty, use `Enumerable.Empty<T>()`.
```

### feedback_no_efcore_sqlite_defaultvaluesql_in_shared_config.md

```markdown
---
name: Provider-specific defaultValueSql must use SQLite customizer, not shared config
description: HasDefaultValueSql in shared IEntityTypeConfiguration must not use SQL Server syntax; override in SQLiteContentVersionDtoModelCustomizer instead
type: feedback
---

Do not put SQL Server-specific SQL expressions (like `GETUTCDATE()`) in `HasDefaultValueSql` inside a shared `IEntityTypeConfiguration` file without a provider-specific override for SQLite.

**Why:** EF Core will use that exact SQL string when generating the SQLite migration/snapshot. `GETUTCDATE()` is not valid SQLite SQL — it would silently produce a wrong snapshot and fail if EF Core ever creates the table on SQLite. The correct SQLite equivalent is `datetime('now')`.

**How to apply:** When a DateTime column needs a DB-level default:
1. Put `HasDefaultValueSql("GETUTCDATE()")` in the shared configuration (documents intent, correct for SQL Server)
2. Create a `Sqlite{Entity}DtoModelCustomizer` in `Umbraco.Cms.Persistence.EFCore.Sqlite` that overrides it with `HasDefaultValueSql("datetime('now')")`
3. Register the customizer in `UmbracoBuilderExtensions.AddUmbracoEFCoreSqliteSupport()`
4. Regenerate both provider migrations so snapshots diverge correctly

Existing example: `SqliteContentVersionDtoModelCustomizer` for `ContentVersionDto.VersionDate`.
```

### project_ef_migration_regeneration.md

```markdown
---
name: EF Core migration regeneration procedure
description: Step-by-step procedure for removing and regenerating EF Core migrations when both SQL Server and SQLite providers exist
type: project
---

When model configurations change after a migration has already been generated, both provider migrations must be removed and regenerated. The procedure is error-prone due to the provider switch dance.

**Why:** The `appsettings.json` startup project controls which provider EF Core tools use. SQLite operations require `Microsoft.Data.Sqlite`; SQL Server operations require `Microsoft.Data.SqlClient`. Both provider `GetMigrationType()` switches reference the migration class by type — when `migrations remove` deletes the class file, the switch breaks the build, which prevents the next `migrations add` from running.

**How to apply:** Follow these steps in order every time a migration needs regenerating.

1. Remove SQL Server migration (`--force` skips DB check): `dotnet ef migrations remove --force -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext`
2. Comment out the migration case from `SqlServerMigrationProvider.GetMigrationType()` (class file was just deleted, build would fail otherwise).
3. Switch provider in `src/Umbraco.Web.UI/appsettings.json` to `"Microsoft.Data.Sqlite"`.
4. Remove SQLite migration: `dotnet ef migrations remove --force -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext` (`--force` needed because the SQLite connection string is rejected by `Microsoft.Data.SqlClient` when it's the active provider).
5. Comment out the migration case from `SqliteMigrationProvider.GetMigrationType()`.
6. Switch provider back to SQL Server in `appsettings.json`.
7. Regenerate SQL Server migration: `dotnet ef migrations add <Name> -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext`
8. Restore the SQL Server switch case.
9. Switch provider to SQLite.
10. Regenerate SQLite migration: `dotnet ef migrations add <Name> -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext`
11. Restore the SQLite switch case.
12. Restore provider to SQL Server.
13. Empty `Up()` and `Down()` in both generated migration `.cs` files (keep Designer files and snapshot unchanged).
```

### project_ef_migration_merge_drift.md

```markdown
---
name: ef-core-migration-snapshot-drift-after-merge
description: "How to detect and fix a botched UmbracoDbContextModelSnapshot.cs after resolving merge conflicts across two EF Core migration branches, WITHOUT touching a migration that's already merged/shared"
metadata:
  node_type: memory
  type: project
---

When two branches each add their own EF Core migrations (both touching `UmbracoDbContextModelSnapshot.cs`, `EFCoreMigration.cs`, `UmbracoPlan.cs`, and both `*MigrationProvider.cs` switches), a manual/git-resolved merge of the snapshot file is NOT reliably correct even if it compiles — EF Core's snapshot format isn't safely line-mergeable. In one real case, a merge silently dropped most of a newly-added entity's columns and gave it the wrong table name on SQLite, and dropped the entity's table entirely on SQL Server, yet the build still succeeded with 0 errors.

**Why:** `dotnet build` only proves the C# compiles — it says nothing about whether the snapshot matches the actual `DbContext` model. Verify with EF Core's own tool, not by eyeballing the diff:
```bash
dotnet ef migrations has-pending-model-changes -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext
dotnet ef migrations has-pending-model-changes -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext
```
(Match the global `dotnet-ef` tool version to the project's `Microsoft.EntityFrameworkCore` package version in `Directory.Packages.props` first — `dotnet tool update -g dotnet-ef --version <match>` — a mismatch only prints a warning but is worth eliminating before trusting the verdict.)

**Critical constraint — figure out which side's migration is safe to touch before fixing anything:** only regenerate a migration that is still local/unmerged/unreleased (e.g. your own feature branch's migration that hasn't been merged into the shared integration branch or shipped). A migration that came in from the OTHER branch (already merged/shared, possibly already applied to someone's real database) must NEVER be removed or regenerated — ask/check this explicitly; don't assume. My first attempt at this got corrected by the user for exactly this reason: I nearly ran `dotnet ef migrations remove` on a migration (`MemberPropertyTypeToEFCore`) that had already been merged elsewhere, purely because it happened to be chronologically last in the id-sorted chain.

**How to apply, when it's your own migration that needs fixing (call it `X`), and it happens to sit BEFORE an already-merged migration `Y` in timestamp order** (`dotnet ef migrations remove` only ever pops the last migration, so you cannot get back to "before X" without removing Y — which you must not do):
1. Do NOT use `dotnet ef migrations remove` for this. Instead, manually reset the snapshot to the state it should be in without X:
   - `Y`'s own `.Designer.cs` (untouched by the merge, since only the cumulative snapshot file conflicted, not individual per-migration files) contains a `BuildTargetModel(ModelBuilder modelBuilder)` method whose body is — by construction, this is how EF Core's tooling always generates it — byte-for-byte identical to what `UmbracoDbContextModelSnapshot.cs`'s `BuildModel(ModelBuilder modelBuilder)` body would be if Y were the last migration and X didn't exist yet.
   - Splice: keep the snapshot file's own header (namespace/class/`BuildModel` signature + opening brace) and footer (closing braces), but replace the body in between with Y's Designer.cs body. Do this per provider. Verify with `grep` that the new-entity's table name no longer appears (since Y's own migration, authored before X existed, never knew about it).
2. Delete X's old migration files (`.cs`/`.Designer.cs`, both providers) directly (`rm`) — safe since X is unmerged.
3. Temporarily comment out X's case in both `*MigrationProvider.GetMigrationType()` switches (needed only because the class file is briefly gone — `dotnet ef migrations add` needs a successful build). See [[project_ef_migration_regeneration]] for the provider-switch appsettings dance.
4. Run `dotnet ef migrations add X` for both providers against the now-correctly-reset snapshot — this generates a fresh, correct migration positioned after Y (new timestamp, same class name X, so the enum/plan/switch wiring elsewhere needs zero changes since they reference the type by name only).
5. Uncomment the switch cases, empty the new migration's `Up()`/`Down()` (same no-op convention as every EF Core migration in this repo — NPoco owns real schema creation).
6. Re-run `has-pending-model-changes` for both providers — must report clean before considering it done.
7. Restore `appsettings.json` to its original committed value (note: in this repo `src/Umbraco.Web.UI/appsettings.json` is actually gitignored/untracked — `git status` won't show drift either way, so diff its content against what it was before you started, not against git).

End state: no new enum value, no new `UmbracoPlan` entry, no new Umbraco migration class — X keeps its original name and position in the enum/plan, only its underlying generated migration files got a new timestamp and correct content. Only the already-unmerged side's artifacts changed.
```

### project_ef_core_dto_phase1.md (historical, superseded — kept for completeness)

```markdown
---
name: EF Core document repository progress
description: Progress on the document repository EF Core migration on v18/feature/ef-core-document-repository as of 2026-06-03
type: project
---

The `v18/feature/ef-core-document-repository` branch has completed Phase 1 (DTOs) and Phase 2 (PerformGet*).

**Why:** Incrementally migrating NPoco DocumentRepository to EF Core via parallel async class hierarchy.

**How to apply:** When continuing work on this branch, GetAsync/GetManyAsync/GetAllAsync are done with full property + culture variation loading. Next work: PersistNew/PersistUpdated, versioning methods, and Children/Descendants paging.

### Phase 1: DTOs complete
- 10 document repo DTOs + 6 extra DTOs added later (DataTypeDto, ContentTypeDto, PropertyTypeGroupDto, PropertyTypeDto, TagDto, TagRelationshipDto)
- EFCoreMigration.AddDocumentRepositoryDtos = 16, AddContentTypeDtos = 18
- All in `src/Umbraco.Infrastructure/Persistence/Dtos/EFCore/`

### Phase 2: PerformGet* complete (2026-06-03)
- `PropertyDataDto.Value` computed property added (matches NPoco version)
- `PropertyFactory.BuildEntities` EF Core overload added — accepts `IReadOnlyCollection<EFCoreDtos.PropertyDataDto>`
- `ContentBaseFactory.DocumentProjection` internal record + `BuildEntity(DocumentProjection, IContentType?)` overload added
- `AsyncPublishableContentRepositoryBase` — added `IContentTypeRepository` as 13th constructor param; stored as `protected IContentTypeRepository ContentTypeRepository`
- `AsyncDocumentRepository` — full implementation of `PerformGetAsync`, `PerformGetManyAsync`, `PerformGetAllAsync` with properties + culture variations
  - `PerformGetRangeAsync(Guid[]? keys)` private method (null = all) does 5 EF Core queries
  - `ApplyVariationsAsync` private method applies draft/published culture names + edited cultures
- Key pattern: 5 queries per batch: main join, published versions, property data, content version culture variations, document culture variations

### Key decisions
- `DocumentProjection` pattern mirrors NPoco's nested `DocumentDto` — keeps factory signature `(projection, contentType)` instead of 6 loose params
- `BuildEntity(DocumentDto, IContentType?)` throws `NotSupportedException` — flat DTO insufficient for entity construction
- Templates set directly in factory (TemplateId + PublishTemplateId from DocumentVersionDto) — no separate `AddAdditionalContentMapping` pass needed
- Properties include both draft and published version rows; `PropertyFactory.BuildEntities` uses `publishedVersionId` to distinguish
- TODO batching: allVersionIds/nodeIds IN queries not yet batched for > 2000 params
```

---

## 5. Cleanup (only relevant if this file was transferred via a git commit)

If you committed this file to pull it on another machine or share it via git, remove it from
history once pulled rather than leaving it as a permanent commit:

```bash
# Confirm this is genuinely the top commit and nothing else has landed on top of it
git log --oneline -3

# Drop the handover commit (rewrites history — only safe if no one else has based work on it)
git reset --hard HEAD~1

# Push the rewritten history (force, since this drops a commit that's already on origin)
git push --force-with-lease origin v18/feature/ef-core-document-repository
```

`--force-with-lease` (not a bare `--force`) so the push aborts if something unexpected landed
on the remote branch between the pull and the cleanup, rather than silently clobbering it.
Do this deliberately, not automatically — confirm the pull actually succeeded on the other
side first. If this file was just handed off locally (not committed/pulled), simply delete it.

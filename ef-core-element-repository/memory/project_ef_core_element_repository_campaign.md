---
name: project_ef_core_element_repository_campaign
description: "The Element NPoco-to-EF Core campaign (planned 08-10-2026) — where the plan, status file and progress artifact live, the PR shape, the hoist-first design decision, what PR 2 established about EF Core translation and hook shapes, and the PR 2b rule that EF Core DTOs are plain rows built from parts"
metadata:
  node_type: memory
  type: project
  originSessionId: 4f8da05c-fe36-4b55-8eed-9028f7cca30c
  modified: 2026-10-08T09:26:14.566Z
---

Planned 08-10-2026 on `v18/feature/ef-core-repositories` (post v19/dev merge, `f1583e70294`), as the
successor to the Document campaign ([[document-repository-retirement-plan]]).

**Where things live (personal orphan branch `campaign/ef-core-element-repository`, worktree at `.claude/campaign/` inside the main checkout, gitignored there; never on a PR branch or the shared integration branch):**
- `.claude/campaign/ef-core-element-repository/CAMPAIGN.md` — the agent-facing plan: goal, rules, inventory,
  six PR specs, per-PR checklist, artifact-update procedure (§7), decisions log (§8). Read it before any
  element work.
- `.claude/campaign/ef-core-element-repository/status.json` — the only source of truth for progress.
- `.claude/campaign/ef-core-element-repository/progress.html` — renders status.json; never holds status.
- Progress artifact: https://claude.ai/artifact/Q5Bttu57VfLyS1ajt7nKtU (title "Element Repository Campaign").
  Update = edit status.json, then Artifact publish with that `url`, `file_path` = progress.html,
  `files` = {"status.json": …}. From a new session, `Artifact read` the URL first.

**Shape:** six PRs into `v18/feature/ef-core-repositories`, branches
`v18/feature/ef-core-element-repository-<n>-<slug>`, title prefix `EF Core:`.
1 Element EF Core DTOs + no-op migration (independent) · 2 hoist DocumentRepository read path into
`AsyncPublishableContentRepositoryBase` · 3 hoist write path · 4 additive `AsyncElementRepository` +
ported tests · 5 `ElementService` onto `AsyncPublishableContentServiceBase`, consumers cut over, NPoco repo
deleted · 6 delete sync `PublishableContentServiceBase`/NPoco `PublishableContentRepositoryBase`/
`IPublishableContentRepository`, rename scaffolding.

**Key design decision (user to confirm):** hoist first, don't fork. The EF Core `DocumentRepository`
(2 530 lines) is written against concrete Document DTOs although the base is generic; copying it into an
`ElementRepository` would create a second fork that goes stale on merge-ups ([[project_async_fork_merge_hazard]]).
Concrete joins stay per repository (`BuildBaseQuery` hook); everything after the row is generic.

**Why:** the user asked for a multi-PR campaign document usable by agents plus a self-updating progress
artifact, after the 433-file Document PR.

**How to apply:** follow CAMPAIGN.md §6 checklist every PR, including the two status.json/artifact updates
(start and PR-opened/merged). Out of scope by decision: version, container and database-cache repositories.

**Known-failing baseline (08-10-2026):** `ElementIndexingNotificationHandlerTests`, `ExternalBlockElementIndexingTests`,
`ExternalBlockElementVarianceTests`, `ExternalElementReindexOnRepublishTests` (25 tests) fail on
`NotImplementedException` from three stubs in the EF Core `RelationRepository` (`GetParentEntitiesByChildIds` and
the two paged parent/child lookups), left by #22809 pending an `EntityRepository` migration. Not caused by, and
not fixed by, the element migration. CAMPAIGN.md §4a + optional PR 0 (EF Core ids + `IEntityRepository.GetAll`
hydration) cover it; PR 5 has a checkpoint to run the four fixtures.

**Cross-machine handover (08-10-2026):** the plan folder is the only content of orphan branch
`campaign/ef-core-element-repository` (worktree `.claude/campaign/`), carrying a snapshot of this memory
directory under `memory/`; setup on a new machine: `git fetch origin campaign/ef-core-element-repository &&
git worktree add .claude/campaign campaign/ef-core-element-repository`; `sync-memory.sh pull` at session start on a machine that is behind,
`sync-memory.sh push` + commit of the folder at session end. Branches use the `v20/` prefix (ships in
Umbraco 20); PR target stays `v18/feature/ef-core-repositories`; migrations stay in `V_19_0_0`. after PR 6: `git worktree remove .claude/campaign` and delete the branch. The first attempt committed the
folder on the shared integration branch; reverted (local only) because coworkers use that branch. First branch: `v20/feature/ef-core-element-repository-1-element-dtos`.

**PR 2 (read-path hoist, 12-10-2026, draft #24175, cut from PR 1's branch at the user's request, not from the integration branch; commits 4b93bb37671 spike + 8966625748b hoist):**
- EF Core translated everything generic without provider-specific work: `new ContentRow<TEntityDto, TContentVersionDto>(...)` built
  from a static expression on the generic base, and interface-member access on type parameters inside query lambdas
  (`joined.Entity.Published`, `variation.NodeId` on `db.Set<TContentCultureVariationDto>()`). Both providers green.
- Hook shapes that landed: `BuildBaseQuery(db, nodes, contentVersions)` (three-argument, the versions pre-filtered by the
  caller; a two-argument overload supplies "current") and `OnEntitiesAssembledAsync(db, entities, rows, loadTemplates, ct)`
  called after the build loop and before the dirty reset / ParentKey loop, so a hook may set members without the base
  needing to know. Templates live only in `DocumentRepository`'s override.
- Trap: adding a required trailing `CancellationToken` after existing optional parameters is CS1737. The fix on this
  branch is to make the optional parameters required (token stays last), not to move the token forward.
- Tests reach static base members through the derived class name (`DocumentRepository.ApplyOrdering(...)`), so the
  ordering unit fixture needed no closed-generic alias.
- A full `dotnet build umbraco.sln --no-incremental` rewrites `src/Umbraco.Web.UI.Client/src/libs/localization-api/known-keys.*.generated.ts`
  and `src/Umbraco.Web.UI.Login/public/mockServiceWorker.js` besides the package-locks; `git checkout --` all of them.
- Left for a follow-up by decision: the ordering-branch block (custom field / culture name / default) is still repeated in
  five paged methods (four on the base, `GetByLevelAsync` on DocumentRepository); consolidating it is a behaviour-neutral
  refactor but not a hoist, so it stayed out of the mechanically-verified PR.


**PR 2b (entities from row parts, 14-10-2026, branch `v20/feature/ef-core-element-repository-2b-entity-from-row-parts` cut from PR 2):**
- Design rule now fixed for the whole EF Core content tier: DTOs are plain table rows; nothing stitches navigation members after a query, and factories take the rows as parameters (`BuildEntity(DocumentDto, NodeDto, ContentVersionDto, ContentVersionDto?, IContentType?)`). `IPublishableContentDto` is non-generic with only `NodeId/Published/Edited`; `IContentVersionDto` has only `Id/Published`. PR 3 must make the write hook return parts and delete `DocumentDto.ContentDto/CurrentVersion`, `DocumentVersionDto.ContentVersionDto`, `ContentDto.NodeDto`; PR 4's element factory follows the same shape.
- Media/members checked: non-publishable, never touch `IPublishableContentDto`; NPoco version-cleanup base reads only `Id`/`Published`. Safe.
- Removing ignored members (`builder.Ignore`) does not touch the model, so no migration and no snapshot diff.
- Red-proof pattern that worked: point the factory at the draft version instead of the published one; `PublishName` is the unambiguous discriminator (`PublisherId` is the same writer on both rows, so it does not discriminate).

**PR 3 (write-path hoist, 14-10-2026, branch `v20/feature/ef-core-element-repository-3-hoist-write-path` cut from PR 2b):**
- Hooks: `BuildEntityRow(entity, published, edited)` (called after the node id and flags are decided), `BuildEntityVersionRow(entity, publishing)` (reads `entity.VersionId`, so assign the id first), `UpdateEntityVersionRowAsync(db, row)`. Base composes `ContentWriteRow` (node, content, content version only) from `ContentBaseFactory.BuildEFCoreNodeDto/ContentDto/ContentVersionDto` (internal). Rule the user set: never put setters on the EF Core DTO interfaces; build a row when its values are known instead of mutating it.
- Trap: `db.Set<T>().Update(row)` on a row whose id the scope's context already tracks (inserted earlier in the same scope) throws "already being tracked"; use an update statement. The scope's DbContext lives across the whole unit of work, so Adds stay tracked.
- Interface-member `SetProperty` on `db.Set<TEntityDto>()` translates (name binding to implicit implementations); no locals needed.
- Templates: `ITemplatedContent` cast in generic code is the NPoco precedent, not a hook.

**State on 19-10-2026:** PR 1 (#24134) merged as `c1a33ebee86`; PR 2 (#24175) merged as `a4e27d26e0c` (14-10); PR 3
(#24205) was merged on 15-10 into PR 2b's branch as `aec7ebf6e16`, so PR 2b (#24203, open against
`v18/feature/ef-core-repositories`) now carries both the parts-based entity assembly and the write-path hoist and is the
PR that lands them. Next after #24203 merges: PR 4 (`AsyncElementRepository`), cut from the integration branch. The
integration branch has had no v19/dev merge-up since `95111fe68ac`.


Update 16-11-2026: PR 4 (#24542) approved by Andy on 11-11; its two factory nits are applied (concrete `Element` return,
name from the version row). His bigger point became PR 4b on the board: the `BuildEntity(ContentRow, IContentType?)`
hook and both EF Core factory overloads accept null but cannot build from it, and the base hands an unresolved
`TryGetValue` result to the hook unchecked. PR 4b makes the base throw a named `InvalidOperationException` and the hook
and EF Core overloads non-nullable; it depends on PR 4 and runs alongside PR 5. The board now has PRs 0, 1, 2, 2b, 3, 4,
4b, 5, 6.

Update 16-11-2026 (later): PR 4 (#24542) merged as `62e2911cc6e` into `v18/feature/ef-core-repositories`. PRs 1, 2, 2b,
3 and 4 are in. Next are PR 5 (`ElementService` cut-over, notification hooks, NPoco `ElementRepository` deleted) and
PR 4b (required content type on the generic read path); both cut from the integration branch at or after `62e2911cc6e`.
The integration branch had no v19/dev merge-up between PR 4's cut and its merge, so no merge audit is pending.

Update 16-11-2026 (PR 4b): implemented and opened as draft #24624 (`12857ebae14`, branch
`v20/feature/ef-core-element-repository-4b-required-content-type`). Trap worth keeping: the test for a missing content
type is written against the public repository surface with a Moq `IContentTypeRepository` returning null, injected
through a new `CreateRepository` overload on `DocumentRepositoryTest`; the FK on `umbracoContent.contentTypeId` means an
orphan row cannot be produced through the public surface, so the mock is the only red-first route. Next: PR 5.

Update 17-11-2026: PR 4b (#24624) merged 16-11 as `a2a288de44b`. PR 5 implemented (uncommitted at the time of writing) on
`v20/feature/ef-core-element-repository-5-element-service-cutover`, cut from `a2a288de44b`. Traps worth keeping:
- `GetChildrenAsync` on the EF Core content repositories throws `ArgumentNullException` on a null `ordering`; pass `Ordering.By("sortOrder")`.
- `ElementService.DeleteOfTypesAsync` keeps two things `ContentService`'s version lacks, both confirmed by the user on 17-11: `NotFound` for
  an unknown type key, done as an existence check (`IContentTypeRepository.ExistsAsync`), never as an id lookup whose ids go unused (the user
  rejected that); and an early return when nothing matches (`ContentTypeService` calls it on every content type delete).
- Scheduled release of invariant content stays as the async engine had it (`includeInvariantForVariant: true`, no writer lookup). A writer
  lookup for NPoco parity was tried and reverted the same day at the user's request: it changes documents too, and
  `Perform_Scheduled_Publishing_Does_Not_Silently_Reattribute_A_Version_With_No_Known_Writer` forbids failing on an unknown writer.
- `DeleteVersionAsync`/`DeleteVersionsAsync` are declared on `IContentService`, not `IPublishableContentService`; for elements they are reachable
  only through the concrete `ElementService`.
- Known gaps left alone: the content integrity check reports every element inside a container (container nodes are not element nodes; NPoco did
  the same, nothing calls it for elements); `AsyncPublishableContentServiceBase.GetParentAsync` short-circuits on `RecycleBinContent` only.
- Board publish trap (17-11-2026): passing `status.json` as an absolute path in `files` is refused with "blocked by a Read permission rule".
  What worked: `root` = `.claude/campaign/ef-core-element-repository` and `files` = {"status.json": "status.json"}, after an Artifact `read` of the
  published `status.json` (`path`), with `Read(.claude/campaign/**)` allowed in `.claude/settings.local.json` (added the same day, with the user's
  approval; not established which of the two made the difference). Board republished as version 51.
Committed as `5edd50597e6`, PR #24653 opened 17-11-2026 and ready for review; follow-ups `2069d626756` (scheduled-release revert) and
`e528600414a` (multi-page delete-of-types test, from Copilot). Bot reviews answered; breaking-change comments are declined on this branch.
Next: PR 6 after #24653 merges; the user is also considering PR 0 (relation stubs) to turn the search jobs green.

Update 17-11-2026 (PR 0): implemented and opened as draft #24657 (`34672930fc0`, branch
`v20/feature/ef-core-element-repository-0-relation-entity-lookups`). Ids via EF Core, hydration via `IEntityRepository.GetAll(objectType, ids)`
per object type (never call it with an empty id list: it then returns every entity of the type). Paged lookups order by sortOrder then id to
match upstream. The four baseline search fixtures pass 28/28 locally, so the "known failing tests" caveat goes away once #24657 merges.
Two traps: a `Contains` on an array inside an EF Core lambda breaks the Infrastructure CLAUDE.md rule (copy to `List<T>` first), and Git Bash
heredocs with long Python bodies fail with "unexpected EOF" here; write scripts to the scratchpad with the Write tool instead.

Update 18-11-2026: PR 5 merged as `0b5b4c28bbe` (#24653). Its human review added `61ef39eb54f`: `protected abstract int RecycleBinId` on
`AsyncPublishableContentServiceBase`, and all twelve notification hooks there renamed with a `Create` prefix (`CreateSavingNotification`, ...);
other service bases keep the old names. PR 0 (#24657) is ready for review; TODOs on `EntityRepository` and
`RelationRepository.GetPagedRelatedEntitiesAsync` mark the node paging and entity building for the entity repository's EF Core migration.
User decisions on that: keep it EF Core, no new `IQuery` use, no new NPoco code. PR 6 is next and unblocked.

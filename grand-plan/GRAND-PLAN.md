# Grand plan: every repository still on NPoco

The campaign folders (`ef-core-element-repository/` and the ones that follow) each cover one migration.
This folder is the map above them: what is already on EF Core, what is still on NPoco, and the order in
which the remaining work is proposed to go. Read it before planning a new campaign, and update it when one
closes.

## Where things live

- `grand-plan.json` is the only source of truth: the inventory, the proposed campaigns with their
  repositories, the loose ends and a dated log. Edit it, never the page.
- `overview.html` renders `grand-plan.json`. It holds no data of its own.
- The published page ("EF Core Migration Map") is listed in `grand-plan.json` under `artifact.url`.
  To update it: edit the JSON, then publish `overview.html` with the Artifact tool to that URL, passing
  `root` = `.claude/campaign/grand-plan` and `files` = `{"grand-plan.json": "grand-plan.json"}`.
  From a new session, `Artifact read` the URL first.
- Everything here is on the orphan branch `campaign/ef-core-element-repository` (worktree `.claude/campaign/`),
  never on a PR branch or the integration branch. Commit and push the folder at the end of a session, as
  `LEARNINGS.md` describes.

## How the inventory was made

Inventory of `v18/feature/ef-core-repositories` at `450b951b298` (20-11-2026, right after main was merged in).
A repository counts as EF Core when it reads and writes through `UmbracoDbContext`; as NPoco when it builds
`Sql<ISqlContext>` or calls `Database.*`. Three repositories do both and are listed under loose ends. Line
counts are the file sizes that day and only indicate relative effort.

To refresh the inventory, run the same two greps over
`src/Umbraco.Infrastructure/Persistence/Repositories/Implement/` and the three files outside it
(`DatabaseCacheRepository`, `IndexDocumentRepository`, `UserIdKeyResolver`), then update the JSON.

## The proposed order

| Id | Campaign | Why this position |
|----|----------|-------------------|
| A | Media and members on the content tier | Cutover onto the existing publishable base; retires NPoco `ContentRepositoryBase` and frees the last `Async*` names. |
| B | Entity tier | TODOs from element PR 0 wait on it; navigation and URL repositories are hot paths. |
| C | Versions and containers | Deferred twice on purpose; small and shared by documents and elements. |
| D | Data types | On every property editor path; two tiny usage repositories go with it. |
| E | Users and security | Largest remaining surface; service contracts already mostly async. |
| F | Infrastructure and operations | Small independent repositories; fillers or newcomer work. |
| G | Outside the repositories folder | `DatabaseCacheRepository`, `IndexDocumentRepository`, `UserIdKeyResolver`. |
| H | Retire the NPoco bases | Only possible when the last subclass is gone. |

A to G are independent of each other; the order is a recommendation, not a dependency chain. H depends on all
of them. Status values in the JSON: `proposed-next`, `proposed`, `planned` (a campaign folder exists),
`in-progress`, `done`, `last`.

## Easy wins

Small repositories that can go one PR at a time, in any order, between or beside the campaigns. They are
listed in `grand-plan.json` under `easyWins` with the campaign each one also belongs to, so a finished easy
win is simply removed from its campaign's list.

- **No new DTO needed.** Every table these touch already has an EF Core DTO and configuration, so there is
  no migration or snapshot work: `DataTypeUsageRepository` and `PropertyTypeUsageRepository` (one PR),
  `NodeCountRepository`, `CacheInstructionRepository`, `ContentNavigationRepository`, `DocumentUrlRepository`,
  `DocumentUrlAliasRepository`, `UserIdKeyResolver`, `NotificationsRepository`, `ContentVersionRepositoryBase`
  with its two subclasses, and the leftover NPoco calls in `AuditEntryRepository`, `LanguageRepository` and
  `WebhookRepository`.
- **One new DTO plus a no-op snapshot migration each** (the `umb-ef-core-empty-migration` skill does the
  plumbing): `RepositoryCacheVersionRepository`, `WebhookRequestRepository`, `WebhookLogRepository`,
  `ServerRegistrationRepository`, `IndexDocumentRepository` (Search.Core), `UserDataRepository`,
  `LogViewerQueryRepository`, `TwoFactorLoginRepository`.

The usual rules still apply to each: Guid keys and a trailing `CancellationToken` on the new contract,
`Attempt` with a status enum on writes, the red-first test, an independent review.

## Measuring progress

The page shows two measures side by side, each with its own bar: lines of repository code, and the number
of repositories and bases. Lines say how much work is left (a 66-line repository and a 1,648-line one are not
the same); the count says how many separate cutovers remain. `done.lines` in the JSON is the EF Core side,
counted the same way as the NPoco side on the inventory day. Refresh both sides together.

## Rules carried over

Every campaign follows the working agreements in `LEARNINGS.md` at the branch root: small PRs in a fixed
order, hoist before you add, Guid keys and trailing `CancellationToken` on every new contract, `Attempt` with
a status enum on writes, a known-failing baseline recorded on day one and fixed first if a small PR can do it,
an independent reviewer after each PR, and the merge-up audit from the memory notes after every merge from
main (sync hunks land silently in the renamed async files).

## Decisions

| Date | Decision | Why |
|------|----------|-----|
| 20-11-2026 | The grand plan is a second area beside the campaign folders, with its own JSON and page, instead of a section inside a campaign plan. | A campaign plan closes with its campaign; the map outlives all of them. |
| 20-11-2026 | Media and members (A) is proposed as the next campaign. | Everything it needs exists; it is the only step that removes a whole NPoco base and the `Async*` scaffolding. |

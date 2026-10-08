---
name: project_ef_core_element_repository_campaign
description: "The Element NPoco-to-EF Core campaign (planned 08-10-2026) — where the plan, status file and progress artifact live, the six-PR shape, and the hoist-first design decision"
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

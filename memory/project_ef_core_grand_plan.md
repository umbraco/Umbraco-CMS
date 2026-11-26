---
name: project_ef_core_grand_plan
description: "The map of every repository still on NPoco and the proposed order of the remaining EF Core campaigns — where it lives on the campaign branch, its artifact, and how to update it"
metadata:
  node_type: memory
  type: project
  originSessionId: 1d62f47f-f4cd-4020-bc2d-5258c53ab109
  modified: 2026-11-26T12:03:15.006Z
---

Created 20-11-2026, right after main was merged into `v18/feature/ef-core-repositories` (merge commit `450b951b298`).

**Where:** `.claude/campaign/grand-plan/` on the orphan branch `campaign/ef-core-element-repository` (worktree at
`.claude/campaign/`, see [[project_ef_core_element_repository_campaign]] for the branch mechanics).
- `grand-plan.json` — the only source of truth: done list, campaigns A–H with repositories/lines/notes, loose ends, log.
- `GRAND-PLAN.md` — how the inventory was made, the proposed order and why, decisions table.
- `overview.html` — renders the JSON; artifact "EF Core Migration Map" at https://claude.ai/artifact/Dnds6W162dYKDBjiZoYZgJ.
  Update = edit the JSON, then Artifact publish with `url`, `file_path` = overview.html, `root` = `.claude/campaign/grand-plan`,
  `files` = {"grand-plan.json": "grand-plan.json"}. From a new session, `Artifact read` the URL first.

**State on 20-11-2026:** 25 concrete repositories on EF Core, 36 repositories/bases still on NPoco (~17k lines).
Proposed order (user has not yet chosen): A media+members on the content tier (retires NPoco `ContentRepositoryBase`,
frees the `Async*` names) · B entity tier · C versions+containers · D data types · E users+security ·
F infrastructure · G `DatabaseCacheRepository`/`IndexDocumentRepository`/`UserIdKeyResolver` · H retire NPoco bases (last).

**Why:** the user asked for a second area beside the campaign folders holding the "grand plan" of repositories, plus an artifact overview, so the next campaign can be picked and planned from it.

**How to apply:** when a campaign starts, set its status in the JSON to `planned`/`in-progress`, add a log row and republish; when it closes, move its repositories to `done` and add the campaign to `done.campaigns`. Refresh line counts from the two greps described in GRAND-PLAN.md rather than guessing.

**20-11-2026 (later):** campaign A split in the JSON: A = media (planned, plan in `ef-core-media-repository/`), A2 = members (proposed, depends on A, retires the NPoco bases and the `Async*` prefixes); H depends on A2 as well. Board for A: [[project_ef_core_media_repository_campaign]].

**20-11-2026 (later still):** campaign A set to `in-progress` on the map (PR 1 of the media campaign open as draft #24724, PR 1b added); the map's `why` text for A now says nine PRs.

**26-11-2026:** first easy win done outside a campaign plan: `DataTypeUsageRepository` and `PropertyTypeUsageRepository` moved to EF Core in #24780 (`186ab8c64de`); both are in `done.repositories`, removed from campaign D and the easy-wins list (D stays `proposed` with only `DataTypeRepository`). `done.lines` was bumped by the two new files (77) only, not recounted. The sibling-name loose end (#23447) is closed by media PR 3; the culture-aware published ordering TODO is still in `PublishableContentRepositoryBase`.

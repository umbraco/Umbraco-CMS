---
name: project_ef_core_media_repository_campaign
description: "The Media NPoco-to-EF Core campaign (planned 20-11-2026) — where the plan, status file and board live, the eight-PR shape, the hoist-down design in both tiers, and the decisions the user confirmed on 20-11-2026"
metadata:
  node_type: memory
  type: project
  originSessionId: 1d62f47f-f4cd-4020-bc2d-5258c53ab109
  modified: 2026-11-20T10:10:32.668Z
---

Planned 20-11-2026 on `v18/feature/ef-core-repositories` at `450b951b298` (right after main was merged in), as the successor to the element campaign ([[project_ef_core_element_repository_campaign]]).

**Where things live (same orphan branch `campaign/ef-core-element-repository`, worktree `.claude/campaign/`):**
- `.claude/campaign/ef-core-media-repository/CAMPAIGN.md` — the agent-facing plan (goal, rules, inventory, §4b parity list, eight PR specs, checklist, §7 board procedure, §8 decisions, backlog, handover). Read it before any media work.
- `.claude/campaign/ef-core-media-repository/status.json` — the only source of truth for progress; `progress.html` renders it.
- Board artifact: https://claude.ai/artifact/5rY17CDCK6o6AiY7q62bVd (title "Media Repository Campaign"). Update = edit status.json, then Artifact publish with that `url`, `file_path` = progress.html, `root` = the folder, `files` = {"status.json": "status.json"}.
- `sync-memory.sh/.ps1` and the `memory/` snapshot moved to the branch root on 20-11-2026 (shared by every campaign).

**Shape:** eight PRs into `v18/feature/ef-core-repositories`, branches `v20/feature/ef-core-media-repository-<n>-<slug>`, title prefix `EF Core:`.
1 EF Core `MediaVersionDto` + no-op migration (enum 27) · 2 hoist the generic read path from `PublishableContentRepositoryBase` down into `AsyncContentRepositoryBase` with two-tier rows (wide rows need no entity DTO and no `Published`) · 3 hoist write/delete/naming · 3b narrow the sibling-name fetch (#23447; required, NPoco media has it) · 4 additive `AsyncMediaRepository` + ported `MediaRepositoryTest` · 5 hoist the generic service engine (incl. ContentService's tree operations) into new `ContentServiceBase<TContent, TContentType>`; `PublishableContentServiceBase` becomes its subclass, `ElementService` implements the new move/recycle-bin hooks · 6 `MediaService` thin subclass, `IMediaService` reshaped like `IContentService` minus publishing, 30 src consumer files + ~60 test files ported, NPoco repository deleted · 7 drop the `AsyncMedia*` names.
Dependencies: 1 ∥ 2 ∥ 5; 3 after 2; 3b after 3; 4 after 1+3b; 6 after 4+5; 7 after 6.

**Key facts found while planning (verified 20-11-2026):**
- Media is non-publishable: tables node/content/contentVersion/mediaVersion(id, path); no entity table, no culture variations; media types are invariant.
- `MediaService` is the only consumer of `IMediaRepository`; `IMediaService` has ~75 src call sites in 30 files (Core, Infrastructure, Api.Management, Search.Core only) and ~287 integration call sites (156 are `Save`); 20 of its 49 members have no caller outside `MediaService`. HybridCache and the Delivery API do not use it.
- Media baseline is fully green (562 unit + 697 integration tests named Media), so no PR 0.
- Parity list (§4b): `IsSorting` on the sort saving notification (#24349 handlers skip metadata extraction), one `MediaDeletedNotification` per item deepest-first (deletes files), `MediaRefreshNotification` from the repository, `umbracoMediaVersion.path` from `umbracoFile` via `MediaUrlGeneratorCollection`, tags on every save, version delete is a no-op, Guid-v7 + duplicate-key checks on single save, `DeleteMediaOfTypes` trashes all non-matching descendants, no relation check when emptying the bin.
- Hazard once PR 6 merges: upstream fixes to the sync `MediaService`/`MediaRepository` must be walked by hand into the async twins on every merge from main ([[project_async_fork_merge_hazard]]).

**Decisions confirmed by the user on 20-11-2026 (§8; PR 1, PR 2 and PR 5 unblocked, none started):** media-only scope (members next), hoist-first in both tiers, tree operations on the wide service base (ElementService inherits them), engine semantics replace media's exceptions/ignored cancels, no relation check on empty-recycle-bin for media, the four file-system members of IMediaService stay sync, `GetMediaByPathAsync` kept though uncalled, `CountNotTrashed` dropped.

**Why:** the user asked for the media migration planned the same way as elements, with the branch's contract rules (no ids, no IQuery, async everywhere, Attempt pattern) and the content repository/service as the template.

**How to apply:** follow CAMPAIGN.md §6 every PR, including the two status.json/board updates; a hoist PR is verified by the document/element fixtures at unchanged counts plus a normalised method diff, not by red-first tests.

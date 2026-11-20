---
name: project_async_fork_merge_hazard
description: On every merge-up, upstream fixes to a sync base class land only in the sync twin — the branch's Async*/EF Core fork must be checked and patched by hand
metadata:
  type: project
---

Both EF Core branches fork base classes as `Async*` siblings that live **next to** their sync
original, both still live. An upstream fix to the sync file merges cleanly and silently leaves
the fork stale — the compiler never complains. The fork is never a mechanical translation, so a
difference found in one method says nothing about the rest.

Forks that have actually gone stale so far:
- `AsyncContentTypeServiceBase{TRepository,TItem}.cs` — missed the composition-change
  cache-refresh fix (v19/dev merge, 2026-09-22).
- `ContentTypeEditing/AsyncContentTypeEditingServiceBase.cs` — missed `ValidateDescendantPropertyAliases`
  and the composition re-validation change from #23103 (same merge).
- `Navigation/AsyncContentNavigationServiceBase.cs` — missed `TryGetHasChildren` (degraded silently
  to the interface default), plus four correctness bugs; see [[project_async_navigation_roots_not_threadsafe]].
- `AsyncPublishableContentServiceBase.cs` — missed the whole "GUID key alongside the integer id"
  logging pass (#23547), 20 messages, on the ef-core-repositories → ef-core-document-repository merge.
- `PropertyFactory.BuildEFCoreDtos` vs `BuildDtos` — missed the branch-publishing variance fix
  (#23850). This one is a *method* pair inside one file, not a file pair, so the file-level
  fork sweep does not find it: its doc comment claims it "mirrors BuildDtos exactly", which is
  the only marker.

**Why:** these are the changes a merge cannot flag. Everything git does conflict on gets looked
at; this class of loss is silent and only a deliberate sweep finds it.

**How to apply:** run both checks in [[project_merge_audit_procedure]] after every merge-up, and
treat any "mirrors X exactly" comment as a fork to diff.

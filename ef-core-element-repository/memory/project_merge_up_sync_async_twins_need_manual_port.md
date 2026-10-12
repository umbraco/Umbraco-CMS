---
name: merge-up-sync-async-twins-need-manual-port
description: "On every v19/dev merge-up into the ef-core-repositories branch, changes git lands in PublishableContentServiceBase (sync, used by ElementService) must be hand-ported into AsyncPublishableContentServiceBase (used by ContentService); git has no idea they are twins"
metadata:
  node_type: memory
  type: project
  originSessionId: 8c44f8f9-e415-4371-b468-f74b090ffba4
  modified: 2026-10-12T09:13:21.952Z
---

The branch has two publish engines: `PublishableContentServiceBase` (sync, still live for `ElementService`) and `AsyncPublishableContentServiceBase` (async, live for `ContentService`). Upstream only knows the sync one, so a merge-up applies fixes to the sync file and the async twin silently keeps the old behaviour. Same for `ContentService` sync methods upstream touches that exist here only as `*Async` (conflicts surface those, the base twin does not).

Seen 2026-10-12 merging v19/dev commit 043b6858906 (invariant-for-variant user-group permission): the whole `includeInvariantForVariant` / `GetRequiredUser` plumbing had to be ported by hand into the async base, and `ImpactExplicit(culture, bool)` kept compiling with the old `isDefault` bool meaning the new `includeInvariantForVariant` parameter.

**Why:** regressions here compile cleanly; only a diff of the two bases or a behavioural test catches them.

**How to apply:** after any merge-up, `git diff <merge-base> MERGE_HEAD -- src/Umbraco.Core/Services/PublishableContentServiceBase.cs src/Umbraco.Core/Services/ContentService.cs` and walk every hunk into the async twin. Also: an upstream NPoco column add (e.g. `UserGroupDto`) needs the EF Core DTO + config + a no-op snapshot migration here (pattern: `RemoveUserGroupDefaultPermissionsFromModel`), otherwise every integration test fails with PendingModelChangesWarning. See [[efcore-handover-premises-unreliable]] and [[no-obsolete-constructors-on-this-branch]] (upstream obsolete forwarding ctors were dropped on merge to match the branch).

---
name: content-version-meta-key-migration-idea
description: "Future-work idea — ContentVersionMeta.ContentId is int-only, forcing an int→Guid resolution (_entityService.GetKey) at every call site that needs the content's key; consider adding a Key property populated at the source instead."
metadata: 
  node_type: memory
  type: project
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-09-11T11:09:17.957Z
---

`ContentVersionMeta` (`src/Umbraco.Core/Models/ContentVersionMeta.cs`) only exposes `ContentId` as an
`int` — no Guid key. Every caller in `ContentVersionServiceBase` that needs the content's key (not just
its int id) has to do a separate resolution step: `_entityService.GetKey(version.ContentId,
ItemObjectType).Result` (used in the version-cleanup path, and now also in `RollBackAsync` — see
[[project_document_repository_retirement_plan]]'s Rollback retirement section, and
[[feedback_check_existing_deps_before_new_di_dependency]] for how that call site was found/fixed).

**Why this is worth revisiting**: this campaign's broader direction is converting the whole
`IContentService` surface to be Guid-key-first (matches [[feedback_guid_keys_not_int_ids_on_async_repository]]).
`ContentVersionMeta` staying int-only is a small but recurring friction point — every future async
version-related method that needs a Guid key pays the same extra `_entityService.GetKey(...)` round trip
(or `.Result`-blocking call, in the current sync-bridged form) that `RollBackAsync` just did. If
`ContentVersionMeta` carried its own `Key` (populated once, at the point where the underlying version/
node row is read — presumably already available there, since the row it's built from originates from a
`Nodes`-joined query that has the `UniqueId` right there), every downstream caller would get it for free
instead of resolving it themselves.

**How to apply**: next time `ContentVersionMeta` or its construction path is touched (e.g. while working
on `PerformScheduledPublish`, version cleanup, or any other version-related retirement), consider adding
a `Key` property and populating it at the source query instead of leaving every caller to resolve it
separately. Not urgent — purely a nice-to-have noticed in passing, not blocking anything today.

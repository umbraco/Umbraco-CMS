---
name: elementservice-wrong-version-notification-bug
description: "Known, unfixed pre-existing bug: PublishableContentServiceBase<TContent>.DeleteVersions/DeleteVersion hardcodes Content-flavored notifications even when TContent is IElement."
metadata: 
  node_type: memory
  type: project
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-25T08:29:32.008Z
---

`PublishableContentServiceBase<TContent>.DeleteVersions(int, DateTime, int)` and
`.DeleteVersion(int, int, bool, int)` (`src/Umbraco.Core/Services/PublishableContentServiceBase.cs`,
~lines 1857-1928) hardcode `new ContentDeletingVersionsNotification(...)` /
`new ContentDeletedVersionsNotification(...)` — the **Content**-specific notification types —
regardless of `TContent`. There is no virtual/abstract hook to select the right notification type per
entity, unlike [[project_document_repository_retirement_plan]]'s `ContentVersionServiceBase`, which
does have `protected abstract DeletingVersionsNotification(...)`/`DeletedVersionsNotification(...)`
overridden per-type in `ContentVersionService`/`ElementVersionService`.

**Why this matters**: `ContentService` moved off this base onto `AsyncPublishableContentServiceBase`
a while ago; `ElementService` is the only remaining consumer of `PublishableContentServiceBase<TContent>`.
So calling `DeleteVersions`/`DeleteVersion` on an `ElementService` instance today fires the *wrong*
notification type (`ContentDeletingVersionsNotification`/`ContentDeletedVersionsNotification`, not
`ElementDeletingVersionsNotification`/`ElementDeletedVersionsNotification`). Practical consequence:
`ElementDeletedVersionsWebhookEvent` never fires for Element version deletions (it listens for the
Element-flavored notification, which is never published), and any handler correctly scoped to
`ElementDeletingVersionsNotification` never sees Element version-delete events either.

**How to apply**: found while investigating the `DeleteVersion`/`DeleteVersions` int→Guid notification
migration (same lines were about to be touched anyway) — user explicitly said to leave it alone for
now and log it rather than fix it in that pass. Fix it in its own increment: add a
`protected virtual DeletingVersionsNotification<TContent> DeletingVersionsNotification(...)` /
`DeletedVersionsNotification<TContent>` hook to `PublishableContentServiceBase<TContent>` (mirroring
`ContentVersionServiceBase`'s existing pattern exactly), default it to the Content notification for
backward compat if needed, and override it in whatever `ElementService`'s current class hierarchy
looks like at fix time — check whether `DeleteVersion`/`DeleteVersions` have themselves been retired
from `IContentService` by then (per [[project_document_repository_retirement_plan]]), since if Element
never gets its own async equivalent, this bug may simply become moot/dead code instead of something to
fix forward.

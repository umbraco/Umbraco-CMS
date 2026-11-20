---
name: feedback-userid-guid-roundtrip-loses-unknown-writer
description: Converting a sync member taking int userId into an async one taking Guid userKey silently re-attributes content with WriterId 0 (unknown) to the super user (-1)
metadata:
  type: feedback
---

When retiring a sync member that took `int userId` in favour of an async one taking
`Guid userKey`, never round-trip an existing `content.WriterId` through
`IUserIdKeyResolver` (int -> Guid -> int). It is lossy for the unknown-writer case.

The chain: a NULL `umbracoContentVersion.userId` materialises as
`WriterId = Constants.Security.UnknownUserId` (== 0) in `ContentBaseFactory`.
`UserIdKeyResolver.TryGetAsync(int)` short-circuits only on `SuperUserId` (-1), so id 0
hits the database, finds no row, and fails. Any `SuperUserKey` fallback then resolves
back to -1, and the commit engine's `c.WriterId = userId` persists -1 — where the sync
path passed 0 straight through and `ContentBaseFactory` wrote it back as NULL.

**Why:** the audit trail silently changes from "unknown writer" to "the built-in admin
published this", and it diverges from sibling branches in the same method that still
pass the raw int.

**How to apply:** keep an existing `WriterId` as an int on the path that consumes it, or
give the async member an int-userId-preserving overload, until the whole method is
converted. If a fallback is genuinely needed, special-case `UnknownUserId`/0 rather than
letting it collapse onto the super user. Watch for this on the remaining
`PublishBranch`/`SaveAndPublish`/`Unpublish`/`PerformScheduledPublish` conversions, which
all still take `int userId`. See [[feedback_guid_keys_not_int_ids_on_async_repository]]
and [[project_document_repository_retirement_plan]].

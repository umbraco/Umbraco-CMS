---
name: feedback-cancellationtoken-first-with-params
description: "When a new async IContentService member takes a params array, put CancellationToken first in the parameter list instead of last"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-08-28T10:26:32.221Z
---

When a newly-added async `IContentService` member (or similar campaign method) uses `params T[] items` for
zero-or-more-argument convenience, put `CancellationToken cancellationToken` as the **first** parameter,
not last. C# requires `params` to be the final parameter, so the otherwise-standard "CancellationToken
last" convention (see [[project_document_repository_retirement_plan]]) is inverted specifically for this
case, keeping the whole signature consistent: `MethodAsync(CancellationToken cancellationToken, params Guid[] keys)`,
or `MethodAsync(Guid userKey, CancellationToken cancellationToken, params Guid[] keys)` when there are other
non-params arguments too.

**Why:** User correction on `GetBlueprintsForContentTypesAsync` — initially written as
`GetBlueprintsForContentTypesAsync(Guid[] contentTypeKeys, CancellationToken cancellationToken)` (dropping
`params` entirely to preserve "CancellationToken last"). The user asked to keep `params` for its original
call-site convenience and move `CancellationToken` to the front instead, establishing this as the pattern
to follow whenever `params` is used going forward in this campaign.

**How to apply:** Whenever a migration candidate's old sync signature used `params` AND there's no separate
singular overload alongside it, keep `params` on the new async signature and place `CancellationToken`
first, not last — e.g. `GetBlueprintsForContentTypesAsync(CancellationToken, params Guid[] contentTypeKeys)`.

**Follow-up correction, same session**: for `DeleteBlueprintsOfType`/`DeleteBlueprintsOfTypes`, the user
first asked for `params` here too, then explicitly reverted it once both a singular and plural overload
existed (`DeleteBlueprintsOfTypeAsync(Guid, Guid, CancellationToken)` +
`DeleteBlueprintsOfTypesAsync(IEnumerable<Guid>, Guid, CancellationToken)`), reasoning that once you have a
dedicated singular convenience overload, `params` on the plural one is redundant — plain `IEnumerable<Guid>`
reads better and matches the original sync API's shape more closely. In that case `CancellationToken`
reverted to last, matching every other sibling in the family (`SaveBlueprintAsync`/`MoveBlueprintAsync`/
`DeleteBlueprintAsync`). **Net rule**: `params` + CancellationToken-first only when there's genuinely no
singular overload; the moment a method gains a singular sibling, prefer singular+`IEnumerable<T>` pair with
CancellationToken last instead. See [[project_document_repository_retirement_plan]] for the concrete
example.

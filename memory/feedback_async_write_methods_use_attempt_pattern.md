---
name: async-write-methods-use-attempt-pattern
description: "Every new/retired async IContentService write method must return Task<Attempt<TStatus>> with a dedicated status enum — never Task<OperationResult>, Task<bool>, or a bare Task — even when there's currently no failure branch."
metadata:
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-27T15:48:25.933Z
---

Every async write-operation member added to `IContentService`/`IAsyncPublishableContentService<TContent>`
during the sync-surface retirement campaign must return `Task<Attempt<TStatus>>` with a dedicated,
per-operation status enum (e.g. `ContentSaveOperationStatus`, `ContentDeleteOperationStatus`) — never
the legacy `Task<OperationResult>`, a bare `Task<bool>`, or a plain `Task`. This applies even when the
operation currently has zero failure branches (e.g. `MoveBlueprintAsync`/`DeleteBlueprintAsync`, whose
notifications aren't cancelable) — use a single-value enum (`ContentBlueprintOperationStatus { Success }`)
rather than skipping Attempt, since it costs nothing today and avoids another breaking signature change
if a real failure mode is added later.

**Why:** `SaveAsync` (added by other work on this branch before any of this session's own increments)
already established `Task<Attempt<ContentSaveOperationStatus>>` as the pattern "every other
EF-Core-migrated service already uses" (see [[project_document_repository_retirement_plan]]). Despite
that precedent already existing, this session shipped four increments that didn't follow it:
`EmptyRecycleBinAsync` (`Task<OperationResult>`), `SendToPublicationAsync` (`Task<bool>`),
`MoveBlueprintAsync`/`DeleteBlueprintAsync` (plain `Task`) — plus a fifth, pre-existing case from
before this session, `SortChildrenAsync` (`Task<OperationResult>`), found only once this rule was
stated explicitly and an audit was done. The user caught `MoveBlueprintAsync` specifically and asked
for a full audit of "any other already migrated methods... missing the attempt pattern" — a
newly-stated instruction category (broad self-audit against an established pattern), not just a
single-instance fix.

**How to apply:**
- Before shipping ANY new async write method (or when asked to audit), grep the touched interface file
  for `Task<OperationResult>`/`Task<bool>`/a bare `Task` on write operations and check each one against
  the Attempt convention.
- A "real cancel path" (a `PublishCancelableAsync`/`PublishCancelable` call) always needs at least
  `Success`/`CancelledByNotification` in the status enum.
- Design the enum around every distinct outcome the method body actually has — e.g.
  `ContentSendToPublicationOperationStatus` needed `NotFound` (null content) and `SaveFailed` (wraps
  the inner `SaveAsync` failure) alongside `Success`/`CancelledByNotification`, since collapsing those
  into fewer values would lose information the old `bool`/`OperationResult` return already didn't have
  either — don't under-model just because the legacy type was coarser.
- When a caller elsewhere (an internal editing-service override, a Management API controller) still
  expects the old shape via a shared helper (e.g. `OperationResultToOperationStatus(OperationResult?)`),
  translate the new `Attempt<TStatus>` back to a throwaway `OperationResult` at that one boundary rather
  than rewriting the shared helper — this was already the established pattern from `DeleteAsync`'s
  `ContentEditingService`/`ElementEditingService` overrides. Preserve any existing (even buggy) mapping
  behavior in that shared helper faithfully rather than silently fixing it as a side effect — see
  `AsyncContentEditingServiceBase.OperationResultToOperationStatus`'s pre-existing `NoOperation → Unknown`
  gap (has a literal `// TODO: shouldn't it be a success?` on `OperationResultType.NoOperation`'s
  definition), left untouched during the `SortChildrenAsync` retrofit — flag it, don't fix it inline.
- `Attempt<TResult>` has a public `implicit operator bool` (returns `.Success`) and a `.Success`
  property — existing test assertions like `Assert.IsTrue(result)` or `Assert.That(result.Success, ...)`
  against an old `bool`/`OperationResult`-typed local usually keep compiling unchanged after switching
  to `Attempt<TStatus>`, since `var result = await ...` just re-infers the new type. Don't assume test
  rewrites are needed — verify with a build first.
- **Exception, confirmed with the user earlier**: `CreateAndSaveAsync`/`CreateBlueprintFromContentAsync`
  (and any other `Create`-shaped method that returns the created entity, throwing on validation failure)
  are deliberately NOT part of this convention — that shape was an explicit prior decision ("faithful
  translation... not a new Attempt-based result shape"), not an oversight. Don't "fix" those.

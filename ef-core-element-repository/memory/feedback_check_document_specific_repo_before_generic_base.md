---
name: check-document-specific-repo-before-generic-base
description: "Before implementing a new async IContentService member in the shared AsyncPublishableContentServiceBase, check IAsyncDocumentRepository for an already-built, purpose-built method first."
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-25T09:26:31.664Z
---

Before writing a new async service-tier method by composing existing generic methods on
`AsyncPublishableContentServiceBase<TContent>` (e.g. `CountChildrenAsync`), check whether
`IAsyncDocumentRepository` (Document-specific, accessible via `ContentService`'s own
`_asyncDocumentRepository` field) already has a purpose-built method for exactly this operation.

**Why:** caught by an independent review agent on the `RecycleBinSmells` → `RecycleBinSmellsAsync`
retirement. `IAsyncDocumentRepository.RecycleBinSmellsAsync(CancellationToken)` already existed,
fully implemented (`db.Nodes.AnyAsync(node => node.NodeObjectType == ... && node.ParentId ==
Constants.System.RecycleBinContent, ct)`) and covered by two passing integration tests — but nothing
called it. Composing a working-but-worse version via `CountChildrenAsync(RecycleBinContentKey, null,
ct) > 0` instead: added an avoidable Guid→node-id resolution query, used `COUNT` instead of `EXISTS`,
and left the tested repository method dead. Not a correctness bug (results agreed, tests passed) but
a real efficiency/cleanliness miss that would have shipped if not caught.

**How to apply:** when the sync method being retired lived in `ContentService.cs` and called a
Document-specific repository field (`_documentRepository`/`_asyncDocumentRepository`, not the generic
`_contentRepository`/`_asyncContentRepository`), the faithful translation is almost always: implement
the new async method directly in `ContentService.cs` too, calling the equivalent
`_asyncDocumentRepository.XxxAsync(...)` method under the same `CreateCoreScope()`/`ReadLock`/`Complete()`
pattern as its neighbors (e.g. `GetPagedContentInRecycleBinAsync`, `GetRootContentAsync`) — not in the
shared `AsyncPublishableContentServiceBase<TContent>` via generic composition. Only reach for the
generic base's existing methods when no Document-specific repository equivalent exists (that was the
case for `HasChildrenAsync`/`CountChildrenAsync` themselves — no Document-specific counterpart existed,
so composing generic methods there was the only sensible option). Grep `IAsyncDocumentRepository.cs`
for the operation name before deciding.

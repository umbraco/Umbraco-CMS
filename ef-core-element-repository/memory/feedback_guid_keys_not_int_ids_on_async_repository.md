---
name: guid-keys-not-int-ids-on-async-repository
description: "Every new method added to IAsyncDocumentRepository must take Guid keys, not int IDs, even when the underlying EF Core DTO column and every current caller are int-based — resolve ints to Guids internally instead"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-10T06:55:31.887Z
---

`GetPagedOfContentTypesAsync` was first built taking `int[] contentTypeIds`, justified at the time by three real facts: `ContentDto.ContentTypeId` is `int` in the EF Core schema (same as NPoco), it matched `IContentService.GetPagedOfTypes(int[] contentTypeIds, ...)`'s existing signature, and it matched the caller's (`DeferredSearchReindexService`) fully int-based pipeline end to end. The user rejected this: "Everywhere else we use keys, we don't want IDs I think we should change this."

**Why:** `IAsyncDocumentRepository` is deliberately Guid-first throughout (an explicit standing decision from earlier in this migration, see [[project_document_repository_retirement_plan]]) — every other method on the interface takes a Guid key (`parentKey`, `ancestorKey`, `entityKey`, `groupKeys`). `int[] contentTypeIds` would have been the only exception, breaking that consistency for no reason other than "it was the smaller diff." That a caller currently happens to be int-based is not a good enough reason to leak that constraint into the new interface's shape — the caller should adapt to the repository's convention, not the other way around.

**The fix, concretely:** content types are themselves nodes (their own `umbracoNode` row), so their Guid key lives on `NodeDto.UniqueId`, not on `ContentTypeDto`. Changed the signature to `Guid[] contentTypeKeys`, and inside the method resolve them to the underlying node IDs that `ContentDto.ContentTypeId` actually stores via one batched query: `db.Nodes.Where(node => node.NodeObjectType == Constants.ObjectTypes.DocumentType && contentTypeKeysList.Contains(node.UniqueId)).Select(node => node.NodeId).ToListAsync(...)`. This is a single round-trip through the live EF Core context — not N calls through `IIdKeyMap.GetKeyForIdAsync`/`GetIdForKeyAsync` (which has no batch API and would mean one DB round-trip per key).

**When the CALLER is the thing that's int-based** (as with `DeferredSearchReindexService.ReindexContentOfContentTypes`, fed by int content-type IDs from cache-refresher notification payloads), the int→Guid bridge belongs in the caller, using `IIdKeyMap` per-id (acceptable there specifically because content-type ID batches are small/bounded, unlike a hypothetical large document batch) — not by making the repository method itself accept ints to avoid that translation step.

**How to apply:** Before adding any new method to `IAsyncDocumentRepository` (or extending its established Guid-first siblings), check what identifier type the *rest of the interface* uses, not just what's convenient for the underlying DTO column or the first caller. If the natural DTO/caller type is `int`, that's a signal the resolution step needs to happen somewhere (inside the method via a Node join, or in the caller via `IIdKeyMap`) — it's not a license to break the interface's Guid-first convention.

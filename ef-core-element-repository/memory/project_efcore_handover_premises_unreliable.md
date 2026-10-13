---
name: efcore-handover-premises-unreliable
description: "EFCORE_DOCUMENT_REPOSITORY_HANDOVER.md's per-item premises are leads, not facts - test each against existing tests/NPoco before implementing; several were wrong."
metadata: 
  node_type: memory
  type: project
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-09-23T10:50:10.967Z
---

`EFCORE_DOCUMENT_REPOSITORY_HANDOVER.md` (untracked, repo root) drives the PR-readiness work on
`v18/feature/ef-core-document-repository`. Its *plan* is sound, but several per-item **premises are
wrong**, and implementing them as written introduces bugs. The user's standing instruction from
2026-09-23: **test the premise before implementing each item.**

**Confirmed wrong so far:**
- **5.7** ("gate published property hydration on `row.Document.Published`") — breaks a *deliberate*
  contract. `ContentServiceTests.Can_SaveRead_Variations` and `PublishingTest` document it: an
  unpublished document keeps its published property values and names so it can be republished in its
  exact previous state. Reverted; a comment at that line now records why it is not gated.
- **Phase 4** ("the Management API create path uses the int constructor, so saved entities throw") —
  `ContentEditingService` resolves the parent entity and `SetParent` populates `ParentKey` already. A
  test written against that path passes with the fix disabled. Test the *repository* path instead
  (`ContentBuilder.CreateSimpleContent(type, name, parentId)` + `ContentService.SaveAsync`).
- **5.3** (ordering) — claimed `"published"`/`"owner"` were unhandled; both were handled, just mapped to
  the wrong column. Its prescribed `throw` on the default arm would have broken **two** live paths:
  `Ordering.By("Path")` reached the default whenever no `pathSelector` was passed (the paged recycle bin
  does exactly this), and a **null** ordering legitimately means sort order.
- **5.4** (`ChangeTracker.Clear()` at the end of persist) — the cited precedent
  (`AsyncContentTypeRepositoryBase`) clears at the **start** of an operation as a correctness guard, and
  its comment states the DbContext is *shared across repositories within the ambient scope*. Clearing at
  the end could discard another repository's tracked state. Skipped deliberately.

**How to test a premise cheaply:** grep the NPoco counterpart for parity (`ContentRepositoryBase`,
`PublishableContentRepositoryBase` are the references), and grep existing tests for the behaviour before
changing it. See [[filter-by-touched-test-not-touched-file]] for keeping those runs fast.

Related: [[document-repository-retirement-plan]], [[efcore-document-repository-status]].

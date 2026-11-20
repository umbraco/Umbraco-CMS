---
name: verify-code-path-actually-executes
description: "Before debugging a failing test by reading/editing code that looks relevant, confirm that code path is actually reached (e.g. via a quick Console.WriteLine trace) — in a codebase mid-migration from sync to async, the old sync implementation can be fully dead code while looking exactly like what the test should be exercising"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-11T07:37:50.485Z
---

Spent real time chasing a `Can_Renormalize_Edited_Flag_When_Property_Becomes_Invariant(True)` test failure inside NPoco's `ContentTypeRepositoryBase.RenormalizeEditedFlags` — a plausible-looking method that does exactly what the test's name and failure message describe. Two hypotheses about that method were tried and both failed to change the test's behavior at all (swapping a `LanguageRepository.GetDefaultIdAsync().GetAwaiter().GetResult()` bridge for an existing, proven sibling method; then adding `Console.WriteLine` tracing inside the method). The tracing produced **zero output**, even after a full rebuild — the tell that this method wasn't running at all.

**Why:** `IContentTypeService` on this branch had already been fully swapped to an async, EF-Core-backed implementation (`AsyncContentTypeServiceBase`/`AsyncContentTypeRepositoryBase`) in earlier, unrelated work. The old NPoco `ContentTypeRepositoryBase` still compiles, still looks complete, and even has the exact same method name pattern (`RenormalizeDocumentEditedFlags`/`RenormalizeElementEditedFlags`) — but it's dead code for this call path. The real, currently-executing implementation lived in a completely different file (`AsyncContentTypeRepositoryBase.cs`), with a different bug (no Element-specific variant of the method existed there at all).

**How to apply:** Before spending time reading or editing a repository/service method to debug a failing test, add a cheap, immediate confirmation that it's the code actually running — a temporary `Console.WriteLine`/`Debug.WriteLine` at the top of the method, rebuilt and rerun once, before investigating further. In a codebase with an in-progress sync-to-async migration (a recurring pattern in this repo — see [[project_document_repository_retirement_plan]]), always check whether a fully-async sibling class already exists and has become the sole live path before trusting that the sync original is what's executing. Two failed hypotheses in the same method without the test's behavior changing at all is itself a strong signal to stop and verify the code path is even reached, rather than trying a third hypothesis in the same place.

---
name: no-npoco-class-references-from-efcore
description: "Never reference an NPoco repository class from EF Core code, even a static/internal helper method with no NPoco-specific dependency itself — copy it instead, since the whole class is slated for deletion"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-06T10:30:32.186Z
---

Don't call a method on an NPoco repository class (`DocumentRepository`, `ContentRepositoryBase`, etc.) from EF Core repository code, even when that specific method is a self-contained `internal static` helper with zero NPoco-specific dependencies (no `Sql<ISqlContext>`, no `Database.Fetch`, pure C# string/collection logic). The method itself being "safe" to call doesn't matter — the class it lives on is slated for deletion once the EF Core migration completes, so any reference to it, however small, blocks that deletion later.

**Concrete instance**: `AsyncDocumentRepository.EnsureUniqueNodeNameAsync` originally called NPoco's `DocumentRepository.EnsureUniqueUrlSegment` (an `internal static` method in the same assembly, no NPoco dependency of its own — just string/`IShortStringHelper` logic). The user rejected this immediately: "we cannot do this as the goal is to kill DocumentRepository." Fixed by copying the method's full implementation directly into `AsyncDocumentRepository.cs` as a private static method.

**Why this is a distinct trap from [[dont-depend-on-repository-being-replaced]]**: that memory is about *delegating unported business logic* to the NPoco repository at runtime (a real functional dependency). This is different — it's reusing a small, pure, already-correct utility method that happens to be *declared on* the doomed class, not calling back into the doomed class's actual repository behavior. It's tempting to treat "it's just a static helper, no real coupling" as safe, but the class-level reference is exactly what blocks deletion — the compiler doesn't care that the method is stateless.

**How to apply**: before referencing ANY member (static or instance, however small) on `DocumentRepository`, `ContentRepositoryBase`, `PublishableContentRepositoryBase`, or any other NPoco repository class from new EF Core code, stop and copy the implementation into the EF Core file instead — don't just check "does this method itself have a NPoco dependency," check "does this method live on a class that's going away." The one exception: genuinely standalone utility classes that are NOT repository classes themselves and are used by multiple NPoco repos too (e.g. `SimilarNodeName` — a plain data/algorithm class in the same namespace as the NPoco repos, but not itself a repository, and not tied to any one repository's deletion) are fine to keep depending on.

---
name: spy-the-actual-fallback-seam-not-the-injected-one
description: "When TDD-verifying that a fast path avoids a fallback, spy on the exact seam the fallback actually resolves through (e.g. StaticServiceProvider.Instance) - not just the constructor-injected instance of the same interface, which the fallback may bypass entirely"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-20T07:41:06.855Z
---

While TDD-verifying that `AsyncDocumentRepository`'s new batched `ParentKey` population actually ran (rather than the entity silently falling back to a per-item `IIdKeyMap` lookup), a spy wrapping the `IIdKeyMap` instance passed into the repository's constructor recorded zero calls even with the real population code temporarily reverted — a false green. The entity's fallback (`ContentBase.ParentKey`'s getter) resolves its own `IIdKeyMap` via `StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>()` — a completely different resolution path from the repository's constructor-injected instance. The spy was watching the wrong door.

**Why:** Default-interface-implementation fallbacks in this codebase (root `CLAUDE.md` §6.3) resolve their dependencies via the ambient `StaticServiceProvider`, not via whatever was constructor-injected into the class under test. A test can pass a mock/spy into the class's constructor and still see the fallback silently succeed through the real, unmodified `StaticServiceProvider.Instance` — producing a coincidentally-correct value with the spy showing zero activity, exactly mimicking what a real fix would look like.

**How to apply:** Before trusting a spy/mock-based "the fast path ran, not the fallback" assertion, identify the *exact* resolution mechanism the fallback actually uses — grep the fallback's implementation, don't assume it goes through the same DI seam as the class under test. If the fallback uses `StaticServiceProvider.Instance`, intercept that instead (temporarily replace `StaticServiceProvider.Instance` with a decorating `IServiceProvider` that serves the spy for the one interface in question and delegates everything else to the original, restoring it in a `finally`/`TearDown`). Always run the revert-rebuild-confirm-fails check (per root `CLAUDE.md`'s TDD rule) on the *exact* test as it will ship — a first attempt at this kind of test can look correct and still be a coincidental pass for a completely different reason than the first one you find; check again after fixing the first flaw before trusting it.

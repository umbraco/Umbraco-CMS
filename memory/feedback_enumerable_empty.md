---
name: feedback-enumerable-empty
description: Use Enumerable.Empty<T>() instead of cast-to-interface empty collection literals
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-10T06:55:42.273Z
---

Use `Enumerable.Empty<T>()` to return an empty `IEnumerable<T>`, not `(IEnumerable<T>)[]` or similar cast hacks.

**Why:** `Enumerable.Empty<T>()` is the idiomatic, allocation-free way to return an empty enumerable. Cast tricks like `(IEnumerable<IContent>)[]` are workarounds that obscure intent.

**How to apply:** Any time a method returns `IEnumerable<T>` and the result is empty, use `Enumerable.Empty<T>()`.

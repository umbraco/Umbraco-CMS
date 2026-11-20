---
name: feedback-no-autocomplete-on-scopes
description: Never use CreateCoreScope(autoComplete:true); complete scopes explicitly with scope.Complete() instead
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-08-24T07:28:30.342Z
---

Don't use `ScopeProvider.CreateCoreScope(autoComplete: true)`. Create the scope without `autoComplete`, do the work, then call `scope.Complete()` explicitly before returning.

**Why**: User explicitly asked for this, correcting a `GetPagedOfTypeAsync` method in `ContentService.cs` that used `autoComplete: true`. Applies even to simple read-only methods with a single `return await ...` statement — restructure to capture the result in a local variable, call `scope.Complete()`, then return the variable, rather than relying on `autoComplete: true` to complete the scope implicitly.

**How to apply**: Whenever writing or editing a method that creates a scope via `CreateCoreScope(...)`/`CreateScope(...)`, always call `.Complete()` explicitly on it rather than passing `autoComplete: true`. This was a fix applied to code newly written in the session, not a retroactive sweep of pre-existing sibling methods that already use `autoComplete: true` (e.g. `GetChildrenAsync`, `GetDescendantsAsync` in `ContentService.cs`) — those were left as-is unless the user asks for a broader cleanup. Apply this preference to all new scope-creating code going forward.

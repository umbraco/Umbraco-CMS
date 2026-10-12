---
name: prefer-explicit-scope-complete
description: In new code, call scope.Complete() explicitly rather than using CreateCoreScope(autoComplete: true), even when neighboring/template code uses the autoComplete shortcut.
metadata:
  type: feedback
  originSessionId: acc3673d-daf5-4c6f-9c2e-4a53931f01bd
  modified: 2026-08-21T12:55:10.307Z
---

While planning the `GetRootContentAsync` service-tier method (modeled on the existing
`GetAncestorsAsync`/`GetChildrenAsync`/etc. templates in `ContentService.cs`, all of which use
`ScopeProvider.CreateCoreScope(autoComplete: true)`), the user rejected the plan with: "Don't use
autocomplete, explicitly complete the scope."

**Why:** `Umbraco.Infrastructure/CLAUDE.md` §5's own documented canonical scope-usage example uses
the explicit form:
```csharp
using (ICoreScope scope = ScopeProvider.CreateCoreScope())
{
    try
    {
        // Operations
        scope.Complete(); // MUST be called
    }
    catch
    {
        throw; // rolls back automatically if not completed
    }
}
```
`autoComplete: true` is a real, used-elsewhere shortcut in this codebase (many existing
`ContentService.cs` read methods use it) — but it's a shortcut, not the documented convention, and
the user prefers new code make the commit point explicit and visible at the call site rather than
implicit via a constructor flag.

**How to apply:** When writing a *new* method that opens an `ICoreScope`, default to
`ScopeProvider.CreateCoreScope()` (no `autoComplete: true`) plus an explicit `scope.Complete();`
before returning — even if the immediately-neighboring methods you're using as a structural template
use the `autoComplete: true` shortcut. Don't retrofit this onto existing, already-committed sibling
methods just for consistency — that would be unrequested scope creep on unrelated code; the rule
applies to code you are newly writing, not a mandate to "fix" the rest of the file.

---
name: feedback-always-use-braces
description: "Always use braces for all conditional/loop bodies, even single-line — never omit braces"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-10T06:55:37.273Z
---

Always use braces `{}` for every `if`, `else`, `for`, `foreach`, `while`, etc. body, even when the body is a single statement or return.

**Why:** User explicitly corrected bracketless single-liners (e.g. `if (x) return [];`). Consistent braces are the project style. Distinct from [[feedback_brace_style]] (Allman brace placement) — that one is about where `{` goes, this one is about never omitting the braces at all.

**How to apply:** Place the body on its own line inside braces in all generated code, plan examples, and code snippets:
```csharp
// CORRECT
if (rows.Count == 0)
{
    return [];
}

// WRONG
if (rows.Count == 0) return [];
```

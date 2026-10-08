---
name: Brace style preference
description: Always put opening braces on a new line (Allman style), not same line as statement
type: feedback
originSessionId: 2af83b02-3c9e-4cea-90ce-c963f5b032bb
---
Always use Allman-style braces — opening `{` on its own line, not at the end of the statement.

**Why:** User corrected this preference explicitly.

**How to apply:** In all C# code written or suggested, use:
```csharp
if (condition)
{
    // body
}
```
Not:
```csharp
if (condition) {
    // body
}
```

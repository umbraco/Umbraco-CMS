---
name: feedback-expression-bodied-members-break-brace-matching
description: "A scripted member deletion that finds \"the first { after the signature\" silently eats the next method when the target is expression-bodied"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-09-17T08:53:32.056Z
---

When scripting the deletion of a C# member, do **not** locate its body as "the first `{` after the
declaration". An expression-bodied member (`=> Something(...);`) has no brace at all, so that search jumps
into the **next** member and the brace-matcher deletes everything up to *its* closing brace.

**Why:** deleting the sync publish engine, my helper reported `70 lines` for
`protected virtual PublishResult CommitContentChanges(...)` — a method whose body is one `=>` line. It had
swallowed the following method whole. The code still looked plausible and would have compiled if the eaten
member had been unused.

**How to apply:** paren-match the parameter list first, skip whitespace past the closing `)`, then branch:
`=>` means terminate at the next `;`; `{` means brace-match. And always verify the result with a
**member-declaration set-diff** rather than trusting the script's own line counts:

```bash
decl() { grep -oP "^\s+(public|protected|private|internal)[^;{]*\b\w+\(" "$1" | sed 's/^\s*//' | sort -u; }
diff <(git show HEAD:path/File.cs > /tmp/old.cs; decl /tmp/old.cs) <(decl path/File.cs)
```

That diff must list exactly the members you meant to remove and nothing else. It also tells you when a
scary-looking diffstat (mine read `130 insertions, 792 deletions` for a pure deletion) is only alignment
noise. Same family of lesson as [[feedback_regex_bulk_edit_overmatch]] — diff the whole file, not the
pattern's match count.

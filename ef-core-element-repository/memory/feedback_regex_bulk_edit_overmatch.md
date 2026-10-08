---
name: feedback-regex-bulk-edit-overmatch
description: "After a scripted regex-based bulk edit across a file, diff the whole file — not just the count of the intended pattern — to catch accidental matches on structurally similar but unrelated code"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-09-14T09:23:52.047Z
---

A regex targeting one call pattern inside a shared structural shape (e.g. "a lambda passed to
`WaitForIndexing(alias, () => {...})`") can accidentally match *other*, unrelated call sites that
happen to sit inside the same structural shape in the same file — not just the ones actually intended.

**What happened**: while converting `ContentService.MoveToRecycleBin(x)` calls to `await
ContentService.MoveToRecycleBinAsync(...)` across ~25 test files
([[project_document_repository_retirement_plan]], MoveToRecycleBin retirement session), a regex pass
scoped to "a `WaitForIndexing` lambda containing `return Task.CompletedTask;`" correctly converted 3
intended lambdas to `async () => {...}`, but also matched and converted 3 *other*, unrelated lambdas
in the same file that called `ContentService.Unpublish(...)` instead — nothing to do with the change
being made. Caught only by reading the full diff, not by trusting the regex's reported match count for
the intended pattern.

**Why**: the regex matched on the surrounding structural shape (`WaitForIndexing(..., () => { ...
return Task.CompletedTask; })`), not on the actual content that motivated the change
(`MoveToRecycleBin`). Any other call sharing that same wrapper shape was equally eligible to match.

**How to apply**: after any regex/scripted bulk edit — even one that reports a plausible-looking match
count — read the full diff of every touched file before moving on, not just grep for the intended
pattern's occurrences. A silently-included unrelated change is easy to miss if you only check "did my
N expected replacements happen," since that check can pass at N while still including extras.

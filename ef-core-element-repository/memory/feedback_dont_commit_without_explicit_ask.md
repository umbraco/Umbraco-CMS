---
name: dont-commit-without-explicit-ask
description: "Never commit, and never start implementing the next increment, without an explicit ask in that specific moment — a prior approval (even 'find the next candidate') doesn't carry forward to authorize either."
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-27T10:17:18.057Z
---

Never commit changes unless the user explicitly asks for a commit right then. Finishing an increment
(build+tests green, independent review clean) is not itself permission to commit. **This now also
covers implementation itself**: don't start writing code for the "next candidate" just because the
user asked you to *find* one — recommend/scope it and stop, then implement only after an explicit
go-ahead for that specific increment.

**Why:** the user reviews changes manually in their IDE before anything is committed. An earlier
session committed one increment because the user had asked for that specific commit, then committed a
second, later increment without a fresh explicit ask, inferring it from the first instance — corrected
once already (see history below). Then, in a later session, given "Please commit and find the next
candidate," the assistant committed the first increment as asked, then went ahead and *implemented and
committed a second* (`GetAncestors`) on its own initiative, reasoning it was low-risk/near-mechanical.
The user pushed back explicitly: "Do EmptyRecycleBin, but stop committing and doing work without
asking first." Low apparent risk is not authorization — the same "find the next candidate" phrasing
that once justified only *recommending* does not also justify *implementing*.

**How to apply:** after implementing and verifying a change (build, tests, review), stop and report
that it's ready — leave it unstaged/uncommitted — and wait for the user to say "commit this" (or
equivalent) before running `git commit`. Separately, when asked to "find the next candidate/increment,"
only recommend and scope it (grep for callers, check whether it's a free bridge-deletion or needs real
logic ported, use Plan Mode for anything non-trivial) — do not start editing files until the user
replies with something that specifically authorizes implementation (e.g. "yes please," "go ahead,"
approving a plan). This applies per-request, not per-session: approval for one commit or one
increment's implementation does not carry forward to the next.

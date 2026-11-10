---
name: could-we-questions-are-not-implementation-requests
description: "A \"could we change X to do Y?\" question from the user asks for a feasibility answer, not for code edits"
metadata:
  node_type: memory
  type: feedback
  originSessionId: ab230cf9-f392-405b-95e3-184e4176e824
  modified: 2026-11-10T11:20:37.518Z
---

When the user asks "Could we change this to make it possible to…?" (or similar feasibility phrasing), answer the question — investigate read-only and explain whether and how — and do not edit any files.

**Why:** On 10-11-2026 the user asked whether `EFCoreMigrationExecutor` could be opened up to package developers (context: feedback on the EF Core breaking-change announcement). I implemented it; the user replied "This was just a question, not a request for you to implement this, please roll back the changes you made."

**How to apply:** Treat "could we / can we / would it be possible" as a question unless the user explicitly says to implement. Reading code and running nothing-changing commands to answer is fine. Related: [[feedback_dont_commit_without_explicit_ask]].

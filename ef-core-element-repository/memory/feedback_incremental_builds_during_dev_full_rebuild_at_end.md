---
name: incremental-builds-during-dev-full-rebuild-at-end
description: Use plain incremental dotnet build for intermediate checkpoints during implementation; reserve --no-incremental full rebuilds for the final verification/test/review pass only — non-incremental rebuilds are too slow to run at every step
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-19T14:08:33.473Z
---

Throughout this branch's work, every intermediate build checkpoint during implementation was run as a full `dotnet build ... --no-incremental` rebuild, matching the root `CLAUDE.md` "fresh build before trusting a green" rule. The user corrected this: "switch away from using non-incremental rebuilds until the final pass/test run/review, it takes way too long."

**Why:** `--no-incremental` rebuilds the entire dependency graph from scratch every time, which is very slow on this large solution (many minutes). Most intermediate checkpoints during active implementation (e.g. "does this one file still compile after this edit") don't need that guarantee — a stale-build false green at an intermediate step gets caught by the next real checkpoint anyway, and the cost of re-running a full rebuild after every small edit vastly outweighs the risk.

**How to apply:** During active implementation (writing code, iterating on a fix, checking one file compiles), use a plain `dotnet build <project> -p:UmbracoBuild=true` (incremental, fast). Reserve `--no-incremental` full rebuilds for the checkpoints that actually matter for trusting a result: the final pass before considering an increment done, before running the real test suite, and before handing off to an independent review agent. This doesn't relax the underlying "don't trust a stale build" principle — it just narrows *where* the expensive full-rebuild guarantee is actually needed, per the user's explicit steer. The related, narrower gotcha in [[feedback_stale_build_after_subagent_revert]] (a subagent's revert-rebuild-restore cycle leaving a stale DLL) still applies on top of this — after that specific scenario, force a real rebuild regardless of where you are in the increment.

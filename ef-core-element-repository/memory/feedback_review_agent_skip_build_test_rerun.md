---
name: review-agent-skip-build-test-rerun
description: "Independent review agents should not re-run build/tests when the main session already ran and reported them — that's redundant, review agent should focus on reading/reasoning about the diff."
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-25T08:50:06.307Z
---

Don't ask an independent code-review subagent to independently re-run `dotnet build`/`dotnet test` when
the main session already ran those same commands and reported concrete pass/fail counts in the
conversation.

**Why:** the user pointed out this is redundant — build/test results are deterministic, reproducible,
and already visible (exact counts, 0 errors) from the main session's own run. Re-running them in the
review agent duplicates work and burns tokens/time for no additional confidence. This is different from
"trust but verify" on *subjective* claims (did the code actually do what the session claims, is the
translation faithful, are edge cases handled) — that's exactly what the review agent should spend its
budget on. Objective, already-demonstrated build/test state doesn't need a second run.

**How to apply:** when briefing an independent review agent, don't include a "re-verify the build and
run test X, don't trust the implementing session's numbers" instruction for build/test commands the
main session already ran and reported in-conversation. Point the review agent at correctness,
convention alignment, and cleanliness instead — reading the diff and reasoning about it, not
re-executing what's already been executed. If the review agent needs to confirm a specific *new* test
scenario that wasn't already run (e.g. a fresh test it wants to write to probe an edge case), that's
fine — the restriction is on redundantly re-running the exact same build/test commands.

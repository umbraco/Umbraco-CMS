---
name: split-test-running-from-review-use-cheap-model
description: "Don't let a single agent/turn both run builds+tests and do code analysis sequentially — split mechanical test execution onto a cheap model (Haiku) running in the background, concurrent with capable-model analysis/review, so waiting on tests doesn't block forward progress"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-19T13:49:19.103Z
---

After several long increments on the `v18/feature/ef-core-document-repository` branch where a lot of turns were spent just waiting for `dotnet build`/`dotnet test` runs to finish (sometimes several minutes each) before continuing, the user asked: "could we split this out? and use a cheaper model to run the tests and validate feedback? so we can work, analyze and review while waiting for builds/tests?"

**Why:** Running a build or test suite and parsing pass/fail counts is a mechanical, low-reasoning-effort task — it doesn't need the same model that's doing architectural review or code analysis. Bundling both into one sequential flow (or one agent that does "read code, run tests, read more code") wastes the capable model's availability on idle waiting, when it could be doing useful analysis work concurrently.

**How to apply:**
- For build/test execution: use a background `Agent` call with `model: "haiku"` (or a plain background `Bash` call, which has no model cost at all) that runs the command(s), parses the output, and reports pass/fail counts + error details. Don't ask it to do judgment calls beyond "did it pass, and what failed."
- For analysis/review (reading diffs, checking correctness, verifying conventions): keep this on the standard/capable model, and launch it to run *concurrently* with the test-runner agent, not after it. Don't gate the start of review work on test results finishing first, unless the review genuinely needs the test outcome to proceed (e.g. verifying a specific bug fix's regression test).
- When a single review task needs both "read the code" and "run the tests," split it into two agents launched together rather than one agent doing both sequentially — a cheap-model test-runner and a capable-model reviewer.
- Continue working (reading code, planning, responding to the user) in the same turn instead of idling on a `ScheduleWakeup`/notification wait purely to babysit a build — background tasks/agents notify automatically on completion; there's no need to block the turn on them.

**Failure handoff (user follow-up, same conversation):** "If there's failing tests, we should also ensure that those get returned to a capable agent which can then analyze and plan and execute a fix." The cheap-model test-runner's job stops at reporting *what* failed (test name, assertion message, stack trace) — it must not attempt to diagnose or fix anything itself. Any failure it reports gets routed to a capable-model agent (or back to the main loop) whose job is to analyze the root cause, plan a fix, and implement it — the same rigor as any other bug investigation in this codebase (read the actual failing assertion and the code under test, don't guess). Only trust a suite as "green" when the cheap-model runner reports 0 failures; a single failure anywhere in the output routes the whole thing to the capable agent, not just a note-and-move-on.

**Recurring failure mode observed (twice, same session): a subagent's own background-process monitor doesn't reliably detect when its spawned test run has actually finished**, and it reports "still waiting" / re-arms another wait window while burning 150-250k+ tokens per idle cycle. Both times, checking `tasklist` (Windows) for `dotnet.exe`/`testhost.exe` directly from the main loop showed **zero processes running** — the test run had already exited, but the subagent hadn't registered completion and kept idling. **How to apply:** when a subagent reports it's waiting on a background test/build run for longer than seems reasonable, check process status yourself from the main loop (`tasklist //FI "IMAGENAME eq testhost.exe" //FI "IMAGENAME eq dotnet.exe"` on Windows) before letting it re-arm another wait. If no matching process is running, send it a message telling it plainly that the process has already exited and to retrieve/report the actual result immediately rather than waiting again — don't let it silently loop.

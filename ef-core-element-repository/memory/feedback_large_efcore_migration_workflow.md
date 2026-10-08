---
name: feedback-large-efcore-migration-workflow
description: "Validated workflow for large EF Core repository migration work — phased plan, delegate each phase with an exhaustive self-contained brief, independently re-verify every claim"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-05T07:36:33.650Z
---

For the `AsyncDocumentRepository` write-path implementation (four phases: New/Update × invariant/culture-variant × plain-save/publish-on-save, plus a bug-fix pass and a 5-item review-fix pass), the following workflow was used across an entire long session without correction or pushback from the user — treat it as validated for continuing this EF Core migration (and likely similar large migrations of other NPoco repositories to EF Core).

**Why:** The user explicitly asked for independent review agents twice ("same parameters as last time") and consistently approved delegating implementation phases to subagents, then approved fixes found during review without re-litigating scope. No message in the session pushed back on this approach — it was implicitly endorsed by repeated reuse.

**How to apply:**

1. **Research before delegating.** Before handing an implementation phase to a subagent, personally read the actual NPoco reference method end-to-end (not a summary of it), the exact EF Core DTO shapes involved, and any existing EF Core precedent in the same codebase (e.g. `AsyncContentTypeRepositoryBase` for insert/update idioms). Extract concrete gotchas yourself (e.g. "`ContentVersionDto.Key` has no EF-Core-side default, must explicitly `Guid.NewGuid()`", "EF Core writes DTO backing fields directly, bypassing 0→null coalescing getters"). A subagent brief built from this research, including verbatim code snippets and exact file:line references, produces far more faithful ports than a brief that just names the method and says "port this."

2. **One phase per subagent call**, sized so each phase is genuinely testable end-to-end on its own (e.g. "New, invariant, unpublished" before "New, culture-variant" before "publish-on-save"). Each phase's brief should explicitly list what NOT to implement yet (with a one-line reason), so the delegate doesn't scope-creep into a later phase.

3. **Require the delegate to follow this repo's TDD rule**: write the new tests first, run them against the still-unimplemented code, confirm they fail with the expected symptom (not a compile error), only then implement, then confirm green, then re-run the FULL existing test file (not just the new tests) to catch cross-phase regressions.

4. **After every delegated phase, independently re-verify — do not just read the agent's summary.** Concretely: `git status --short`/`git diff --stat` to confirm only expected files changed (this caught an unrelated stray `.editorconfig` mutation twice — see [[feedback_watch_for_stray_file_changes]]), rebuild, rerun the full test filter, and read the actual new code for at least the highest-risk section per phase. When a phase claims a specific bug is fixed (e.g. "the version Key is now unique, not duplicated"), don't just trust it — read the code and confirm the concrete mechanism.

5. **When a review agent finds something, and it's a genuine bug (not a style nit), fix it immediately in the same turn** rather than only documenting it. This surfaced a real bug once (a missing in-memory flag-flip in `PersistNewItemAsync` that `PersistUpdatedItemAsync` already had) while doing an unrelated "add TODOs" task — the right move was to fix it and add a regression test proving it fails without the fix, not just note it.

6. **Review agents should get the same 3-axis brief each time**: behavioral correctness, alignment with existing conventions, code cleanliness/duplication — and should independently rebuild/retest, not trust prior reports. Re-running review after a fix round should re-verify ALL of the axes fresh, not just the specific fixed items (a second review round can surface things a first round missed, e.g. `SortableValue` never being populated on write was only caught in a later review pass).

7. **The `Workflow` tool works well for this exact pattern when the user says "fan out"** — a 5-phase script (parallel Plan → sequential Implement → Review → Fix Feedback) successfully shipped tag support + the `IsMoving()` fast path in one background run. Two things worth remembering: (a) a mid-run failure ("agent abandoned: user requested retry on all N attempts" — despite the wording, this was an infrastructure interruption, not an actual user action) is usually safe to just resume with `Workflow({scriptPath, resumeFromRunId})`, which replays completed phases from cache and only retries the failed one — check `git status`/build/test on the partial state first to confirm it's a sane, resumable checkpoint (it was: the interrupted phase had already gotten as far as writing its TDD-red test before being cut off, and everything before it was clean); (b) still independently re-verify the final workflow result exactly like a single delegated agent's output — the review phase inside this workflow caught something (a test-coverage gap where two "tag" tests both accidentally routed through `PersistNewItemAsync` instead of `PersistUpdatedItemAsync`) that a less careful pass would have missed, and the fix-feedback phase then empirically proved its own new test via a notification-callback DB snapshot rather than just asserting end-state.

See [[project_ef_core_document_repository_status]] for what this workflow actually produced.

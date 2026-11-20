---
name: feedback-parallel-review-and-test-agents
description: "After implementing a migration/retirement, spawn two separate agents in parallel — one independent code reviewer, one simple test-runner — rather than one agent doing both or sequential runs"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-08-24T11:11:25.937Z
---

After finishing an implementation (especially a sync→async / int-keyed→Guid-keyed retirement migration on this branch), spawn two agents **in a single message, in parallel**:

1. **Independent review agent** (`general-purpose`, thorough brief) — reads the full diff, checks the migration against the established retirement pattern (interface/service/repository layering, default-ordering restoration, out-of-scope files correctly left untouched, no remaining references to the retired member, doc comment accuracy). Explicitly told it does NOT need to run tests itself — a separate agent is doing that.
2. **Simple test-runner agent** (`general-purpose`, short mechanical brief) — just runs `dotnet build` (full solution, fresh) + `dotnet build` on any specifically-touched project + the relevant `dotnet test --filter` command(s), and reports pass/fail counts and full failure text if anything fails. No code review, no analysis — just execute and report.

**Why**: Confirmed working well on the `GetPagedOfType` → `GetPagedOfTypeAsync` migration (this session). Splitting review and test-running into separate agents lets them run concurrently rather than one agent doing both serially, and keeps each agent's brief focused (a reviewer distracted by running long test suites reviews less carefully; a test-runner given review responsibilities adds noise). The user explicitly asked for this exact two-agent split and asked to remember it for future migrations.

**How to apply**: After completing a migration (sync/int-keyed retirement → async/Guid-keyed, or similar mechanical multi-file change) in this repo, launch both agents together in one message with two `Agent` tool calls, rather than one call for review followed by another for tests. Give the review agent a comprehensive self-contained brief (files touched, what changed, what was deliberately left out of scope and why, what's already verified so it doesn't repeat work). Give the test-runner agent just the exact commands to run and what to report — no need for it to understand the migration's design rationale. This mirrors [[feedback_large_efcore_migration_workflow]]'s guidance to delegate with exhaustive briefs and independently re-verify claims, applied specifically to the post-implementation verification step.

**Unconditional, not just for large migrations**: the user explicitly confirmed (2026-08-24) this applies to *every* method migration on this branch, regardless of size or blast radius — do not skip the independent-review step for a "small"/single-caller migration (e.g. `GetVersionsSlim`, `GetVersionIds`) just because it seems low-risk. Always spawn at least the independent review agent after migrating a method; pair it with the test-runner agent when a fresh test run hasn't already been done inline.

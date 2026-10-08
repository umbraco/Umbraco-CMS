---
name: feedback-watch-for-stray-file-changes
description: Always check git status for unexpected file changes before committing — but a repeated .editorconfig mutation this session turned out to be a wanted fix, not an anomaly; resolved and applied properly
metadata:
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-05T07:42:39.034Z
---

Twice in one session, `git status` showed `.editorconfig` as modified with a single added line (`dotnet_diagnostic.SA1101.severity = silent` the first time, `= none` the second time) that no Edit/Write tool call in the visible conversation had targeted. Both times this was caught before committing (`git status --short` right before staging) and reverted via `git checkout -- .editorconfig`, flagged to the user as an unexplained anomaly.

**Resolution (same session, later): it was not an anomaly.** The user explicitly asked to suppress SA1101 ("we do not, anywhere in our codebase, prefix with this., please make it go away"). Investigation showed `.globalconfig` already suppresses SA1101 with the exact reasoning "`stylecop appears to be ignoring dotnet_style_qualification_for_*`" and `.editorconfig` already sets all four `dotnet_style_qualification_for_*` options to `false` — but `.editorconfig` itself was missing the matching `dotnet_diagnostic.SA1101.severity = none` line, which is apparently what the user's IDE/linter reads for live warnings (MSBuild builds were already clean via `.globalconfig`, so this never showed up in `dotnet build` output during the session — only in whatever surfaced it to the user directly). Something in the user's environment had already been trying to apply this exact fix twice, and it was discarded both times before the context to recognize it existed.

**Why this still matters:** The instinct to flag and revert an unexplained working-tree mutation before committing was still the *correct* action in the moment — reverting silently or committing silently would both have been worse than pausing and asking. The lesson isn't "trust unexplained changes," it's "an unexplained change that recurs identically (or near-identically) across a session is a signal worth surfacing explicitly to the user rather than just silently discarding a second time" — recurrence is itself information.

**How to apply:**
1. Before every `git add`/`git commit`, run `git status --short` and scan for any file outside the set you intentionally touched — `git diff --stat` on specific paths won't surface a stray file at all.
2. If something unexpected recurs (the same file, especially with a plausible-looking config value), don't just revert it silently a second time — say so explicitly and ask, since a recurring identical mutation is more likely to be a real signal (a tool trying to apply a fix, a linter suggestion, environment drift) than random noise.
3. If it turns out to be wanted (as here), apply it properly and deliberately once you have the context, rather than leaving it to keep reappearing and getting reverted.

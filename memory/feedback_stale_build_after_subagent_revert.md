---
name: stale-build-after-subagent-revert
description: "dotnet test --no-build silently runs a stale DLL whenever the referenced project was rebuilt independently of the test project (e.g. `dotnet build src/Foo` without rebuilding the test project), or after a subagent's revert-rebuild-restore TDD check — don't trust --no-build, force a real rebuild"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-11T08:03:22.205Z
---

Two confirmed triggers for the same underlying trap, both in this session:

1. **Subagent revert-rebuild-restore.** A review subagent's TDD-honesty check (temporarily revert a fix to old behavior, rebuild, confirm tests fail, restore the fix) rebuilds the DLL against the *reverted* source mid-check. If it restores the source afterward but doesn't rebuild again, the DLL on disk still reflects the reverted (old/broken) behavior even though `git diff` shows the correct, restored source.
2. **Building only the library project, not the test project.** Running `dotnet build src/Umbraco.Infrastructure/Umbraco.Infrastructure.csproj` updates that project's own `bin/` output, but `tests/Umbraco.Tests.Integration/bin/Debug/net10.0/Umbraco.Infrastructure.dll` is a separate copy that only gets refreshed when the *test project* itself is rebuilt (project references copy the referenced DLL on their own build, not the referenced project's). If the test project's copy is older than the source change, `dotnet test --no-build` silently runs the old copy.

**Why this is easy to miss**: running `dotnet test --no-build` right after either trigger uses the stale DLL. The resulting failures (or false passes) look exactly like a real regression or a real fix — in one case, a test kept failing with a symptom consistent with new logic simply not running at all, even though the source on disk was completely correct. Confirmed both times by adding temporary debug tracing to the method in question (a `Console.WriteLine` is not reliably captured by the NUnit/VSTest console redirect — write to a file with `File.AppendAllText` instead for a trustworthy signal) and finding zero trace output despite the code clearly being on the execution path — the tell that the *compiled binary* didn't match the *source*. In trigger #2 specifically, compare `ls -la` timestamps on the library's own `bin/` DLL vs. the test project's copy of the same DLL to confirm staleness directly, before adding trace instrumentation.

**How to apply**: don't run `dotnet test --no-build` unless you just built *that exact test project* (not just a project it references). After any subagent revert-rebuild-restore cycle, or after rebuilding only a referenced library project, rebuild the test project itself before trusting a pass/fail result (`dotnet build tests/Foo.Tests.csproj`, or plain `dotnet test` without `--no-build`). If a suite fails or passes in a way that doesn't match any code change you're aware of, suspect a stale build before suspecting a real bug or a real fix — check DLL timestamps or rebuild and rerun before spending time debugging.

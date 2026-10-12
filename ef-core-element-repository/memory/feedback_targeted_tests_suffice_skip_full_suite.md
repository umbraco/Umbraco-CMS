---
name: targeted-tests-suffice-skip-full-suite
description: For a contained, well-precedented change, a targeted --filter covering every touched file is sufficient verification — don't default to a full, untargeted test-suite run afterward.
metadata:
  type: feedback
  originSessionId: acc3673d-daf5-4c6f-9c2e-4a53931f01bd
  modified: 2026-08-21T12:53:26.394Z
---

During the `IContentService.GetRootContent()` → `GetRootContentAsync()` migration, after a targeted
`dotnet test --filter` covering every file touched by the change (repository tier, service tier, 3
production callers, 14 test files) passed 764/764, a full untargeted `dotnet test` was kicked off as
a final sanity pass — matching what the approved plan's verification section called for. The user
stopped it: "Running the full test suite is way overkill, just run a targeted suite."

**Why:** A full suite takes far longer (a prior full run on this same branch took ~1.5 hours) for no
additional signal when the change is a contained, mechanical migration following an established
pattern and the targeted filter already enumerates every touched area. The full suite only earns its
cost when a change's blast radius is genuinely unclear — e.g. it touches a widely-shared base class,
changes a default, or could plausibly affect callers outside the files already identified.

**How to apply:** Default to a targeted `--filter` built from the actual touched-file list (e.g.
`FullyQualifiedName~FileA|FullyQualifiedName~FileB|...`) as the *final* verification step, not just
an intermediate one — don't automatically follow it with a full-suite run "just to be safe." Only
reach for an untargeted `dotnet test` when there's a specific, articulable reason the blast radius
might extend beyond the enumerated files. This narrows (but doesn't replace) the final-pass guidance
in [[feedback_incremental_builds_during_dev_full_rebuild_at_end]]: a full, non-incremental *build* at
the end is still correct (compilation errors can hide anywhere in the dependency graph), but the final
*test run* itself should stay scoped to a targeted filter unless the change's reach is genuinely
uncertain.

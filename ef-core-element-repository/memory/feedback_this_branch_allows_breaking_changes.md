---
name: this-branch-allows-breaking-changes
description: "On v18/feature/ef-core-document-repository, the user has said breaking changes to public API (constructors, method signatures) are fine — don't default to CLAUDE.md's obsolete-constructor/obsolete-overload dance here"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-10T09:17:35.133Z
---

When adding `IAsyncDocumentRepository` as a new `ContentService` constructor dependency, the obsolete-constructor-plus-new-constructor pattern (root `CLAUDE.md` §5.1) was applied by default — obsoleting the current constructor, adding a new one, threading `StaticServiceProvider` through the old one. The user rejected this: "You did a bunch of stuff to avoid breaking changes with the ContentService constructor, but you don't need to in this branch we're allowed to make breaking changes."

**Why:** This matches prior precedent already set earlier in the same effort — `DocumentUrlService.RebuildAllUrlsAsync()` got a directly breaking `CancellationToken` parameter added with no obsolete-overload dance, justified at the time as "the breaking change is already announced for this version." The NPoco→EF Core retirement effort on this branch is pre-release, in-progress restructuring of internal service/repository surfaces — not yet-shipped public API that external consumers depend on today. Defaulting to the CLAUDE.md obsolete pattern for *every* signature change on this branch is the wrong default; it adds real complexity (as seen here: an obsolete-constructor chain that accidentally tied in parameter count with an already-obsolete legacy constructor, forcing a `[ActivatorUtilitiesConstructor]` + custom DI factory-registration workaround) for no benefit, since nothing needs the old signature preserved.

**How to apply:** On `v18/feature/ef-core-document-repository` (and likely similar in-progress NPoco→EF Core migration branches), when a constructor or method signature needs a new parameter as part of this migration work, just change it directly — don't reach for the obsolete-constructor/obsolete-overload pattern by default. If genuinely unsure whether a specific change should be breaking (e.g. touching a class with many known external consumers, or something already shipped in a prior stable release), ask rather than assume either way. The general CLAUDE.md obsolete pattern still applies to the rest of the codebase's already-shipped public API — this is a branch/effort-specific carve-out, not a repo-wide policy change.

**Side effect of following this correctly**: two pre-existing legacy constructors on `ContentService` (marked `"Scheduled for removal in Umbraco 19"`) were also deleted in the same pass, since `version.json` confirmed the current version is already `19.0.0-beta1` — past their own declared removal point. Breaking-changes-allowed also means expired obsolete members blocking current work can just be removed rather than worked around, when the removal is already independently due.

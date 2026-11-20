---
name: public-api-survives-even-with-zero-internal-callers
description: "Zero internal callers is not sufficient reason to delete a public, non-obsolete method — check for external/package-author-facing API surface separately from an internal-caller grep"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 3629769a-7eca-4d89-9fd7-e4c33337b0f8
  modified: 2026-08-17T06:54:29.874Z
---

While retiring `IContentService.GetById(int)`, deleted `ContentServiceExtensions.GetAnchorValuesFromRTEs`/`GetAnchorValuesFromRTEContent` as "dead code" because a grep of `src/` and `tests/` showed zero internal callers. The user caught this: "I don't think we can remove the GetAnchorValuesFromRTEs and GetAnchorValuesFromRTEContent outright, we still need to support RTE, and other people might need this code." Both methods were `public static` extension methods on `IContentService` with no `[Obsolete]` attribute — genuine public API surface, just not exercised anywhere *inside* this repository. Had to restore both with their original signatures unchanged, re-plumbed internally via `IIdKeyMap`/`StaticServiceProvider` (this repo's established pattern for a public static method that needs a new dependency but can't take a constructor parameter).

**Why:** root `CLAUDE.md` §5's whole no-binary-breaking-changes policy exists because package authors and site implementors call this codebase's public API from outside the repository — a repo-wide grep can only ever prove "nothing *inside this repo* calls it," never "nothing anywhere calls it." Deleting based on the weaker claim silently breaks the stronger one.

**How to apply:** Before deleting any method/class that is `public` (not `internal`) and lacks an `[Obsolete]` attribute, treat "zero internal callers" as necessary but not sufficient. Ask instead: is this declared `[Obsolete]` already (then removal-after-deprecation-window is fine, see [[feedback_remove_dont_fix_v19_obsolete]])? Is it on an `internal` type or does it live only in a test/tooling assembly (then internal-caller-zero really is sufficient)? If neither, the safe default is to keep it — or, if it's collateral to an unrelated change (like a method it calls being removed), re-plumb it to keep working rather than deleting it. Only delete un-obsoleted public API when explicitly asked to, or when it's provably not part of the public contract (e.g. it's on an internal-only type).

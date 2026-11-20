---
name: no-int-stepping-stone-when-obsoleting
description: "When retiring an int-keyed IContentService member on the ef-core-document-repository branch, delete the int overload outright — don't add a new obsolete Guid-keyed stepping-stone overload"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 3629769a-7eca-4d89-9fd7-e4c33337b0f8
  modified: 2026-08-18T07:09:46.843Z
---

When a public `IContentService` (or similar) member takes an `int id`, don't introduce a brand-new
`[Obsolete]` Guid-keyed overload as an intermediate migration step for it and have the `int` overload
delegate to that. Delete the `int` overload outright and add the real replacement (either a Guid-keyed
sync method or the async method directly, per what was actually asked for).

**Why:** On `GetAncestors(int id)`, I first added `GetAncestors(Guid key)` as a *new*
`[Obsolete(...)]` method and retargeted `GetAncestors(int id)`'s obsolete message to point at it
(`int` → `Guid` → `GetAncestorsAsync`), reasoning that this gave int-keyed callers a smaller migration
hop. User rejected this: "we should remove `GetAncestors(int id)` entirely, we don't want int id
signatures anymore at all." The distinction that matters: keeping an *already-existing* obsolete method
in place (e.g. `GetAncestors(IContent content)`, kept obsolete by explicit prior instruction to give
consumers time) is different from *creating a new method that is obsolete on arrival* just to soften an
int→Guid transition — the latter adds permanent surface area for something that was never going to be
kept. See [[feedback_this_branch_allows_breaking_changes]] (this branch allows breaking changes) and
[[feedback_remove_dont_fix_v19_obsolete]] (delete rather than patch already-obsolete members) — this is
the same "just delete it" instinct applied one layer earlier, before an obsoletion is even introduced.

**How to apply:** Before adding a new `[Obsolete]` overload as a "gentler" replacement for an int-keyed
method, check whether the user actually asked for a migration grace period on *that specific member*.
If not, default to outright deletion + a single direct replacement (Guid-keyed sync, or async, per the
task), matching every other Tier-A `IContentService` retirement on this branch.

# Code Review: PR #21672 — Block Editor: Implements hints for Tabs

**PR Title:** Block Editor: Implements hints for Tabs (fixes #21178)
**Branch:** `v17/bugfix/21178` → `main`
**Files changed:** 4 (frontend only)
**Lines:** +285 / -64

---

## Summary

This PR adds validation badge indicators ("hints") to tabs in the block editor workspace. When a block has multiple tabs and a validation error exists on a non-active tab, a badge is displayed on that tab.

The core changes: `UmbBlockElementManager` now uses `UmbViewContext` instead of the lower-level `UmbHintContext`; `block-workspace.context.ts` wires `content.view` and `settings.view` to inherit from the workspace-level view; both the routed and non-routed edit views create per-tab `UmbViewController` instances that observe hint state and render badges.

---

## Findings

### Bug (High) — Routed view never creates a root view context

`#createRoutes` adds a `root` path route when root groups/properties exist but `#createViewContext(null, ...)` is never called — only inside the tab forEach loop. `#provideViewContext` will throw crashing the block editor.

### Bug (High) — Root route setup does not call `#provideViewContext`

Even if the context were created, the root route setup never calls `#provideViewContext(null, component)`.

### Minor — `_hasRootGroups` may be `undefined` when `#setupViewContexts` first runs

Fragile coupling between async observers.

### Minor — Type mismatch in `#renderTab` call

`tab.name` is `string | undefined | null` but parameter typed `string`.

### Minor — `getViewAliasForTab` helper not used in render method

Inline alias construction should use the helper consistently.

### Minor — `#provideViewContext` hard-throws when view not found

For a non-critical display feature, graceful degradation might be preferable.

---

## Recommendation

The two high-severity bugs affect blocks with root-level properties in the routed view. Fix before merging.

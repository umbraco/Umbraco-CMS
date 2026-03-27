## PR Review

**Target:** `origin/main` · **Based on commit:** `6a8f6ee944f5b0cc64173df7bc88c5cc216bfa7e` · **Skipped:** 0 noise files out of 4 total

Adds validation hint badges to block workspace tabs (both routed and inline/no-router variants) by introducing `UmbViewController` per tab and wiring each to the block manager's view context, so tabs with validation errors surface a colored badge on the inactive state.

- **Modified public API:** `UmbBlockElementManager.hints` renamed to `UmbBlockElementManager.view` (type changed from `UmbHintContext` to `UmbViewContext`)
- **Affected implementations (outside this PR):** None — `UmbBlockElementManager` is not exported via the `./block` package exports (workspace `index.ts` only re-exports tokens and constants), so this is an internal rename only.
- **Breaking changes:** None detected.

---

### Important

- **`block-workspace-view-edit.element.ts:#createRoutes`**: The `root` route's `setup` callback never calls `#createViewContext(null, ...)` or `#provideViewContext(null, ...)`, so no `UmbViewController` with `viewAlias === null` is ever created for the routed workspace view. `#renderTab(null, '#general_generic')` reads `this._hintMap.get(null)`, which is always `undefined`. Root/Generic tab badges are silently absent in the routed (modal) workspace view even when root properties have validation errors. The no-router variant handles this correctly. Fix: add `this.#createViewContext(null, '#general_generic')` alongside the `root` route push, and call `this.#provideViewContext(null, component)` inside its `setup` callback — same as what's done for tab routes.

- **`block-workspace-view-edit.element.ts` and `block-workspace-view-edit-content-no-router.element.ts`**: `#tabViewContexts` accumulates indefinitely — view contexts for removed or renamed tabs are never destroyed. If a user renames a tab, the old `UmbViewController` (with its observer and provider) is orphaned. The `.find()` guard prevents duplicate creation, so renames produce a new controller and leave the old one active. Fix: clear `#tabViewContexts` (calling `destroy()` on each) at the start of `#createRoutes` / `#setupViewContexts` before rebuilding.

### Suggestions

- **`block-workspace-view-edit-content-no-router.element.ts:236`**: `#renderTab` signature is `name: string` but receives `tab.name` which is `string | null | undefined`. The template handles this with `?? '#general_unnamed'`, but the type mismatch is technically incorrect — TypeScript's strict mode would flag this. Widen the parameter type to `name: string | null | undefined` to match call sites.

- **`block-workspace-view-edit-content-no-router.element.ts:#createViewContext`**: `throw new Error('Block Manager not found')` is unreachable in practice since the method is guarded by `if (!this._tabs || !this.#blockManager) return` in `#setupViewContexts`. Consider replacing with an early return or an assertion comment to avoid the false impression of a recoverable error path.

---

## Request Changes

Critical and important issues must be addressed first.

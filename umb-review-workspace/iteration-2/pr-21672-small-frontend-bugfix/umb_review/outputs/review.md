## PR Review

**Target:** `origin/main` · **Based on commit:** `6a8f6ee944f5b0cc64173df7bc88c5cc216bfa7e` · **Files:** 4 changed, 0 skipped, 4 reviewed (4 full, 0 diff + header-only)

Adds per-tab validation badge indicators to the block workspace edit views by replacing the `UmbHintContext`/`hints` infrastructure with `UmbViewContext`/`UmbViewController`, enabling tab-scoped hint filtering and badge rendering on inactive tabs in both the routed (`umb-block-workspace-view-edit`) and no-router (`umb-block-workspace-view-edit-content-no-router`) variants.

- **Modified public API:** `UmbBlockElementManager.hints` property renamed to `view` (type changed from `UmbHintContext<UmbVariantHint>` to `UmbViewContext`); `UmbContentValidationToHintsManager` constructor call updated to pass `this.view.hints` instead of `this.hints`, with an empty `[]` passed for the path segments parameter (previously `[workspaceViewAlias]`).
- **Other changes:** The routing strategy in `block-workspace-view-edit.element.ts` now uses a named `root` path as the canonical path for root groups/properties, with an empty-string alias cloned from `routes[0]`. Tab rendering updated to use `data-mark` attributes and `umb-badge` on inactive tabs.

---

### Important

- **`src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit.element.ts:117–155`**: The root-tab view context (`viewAlias === null`) is never created in `#createRoutes()`. In the no-router variant, `#setupViewContexts()` explicitly calls `this.#createViewContext(null, '#general_generic')` when `_hasRootGroups || _hasRootProperties`. In the routed variant, `#createViewContext` is only called inside the `_tabs.forEach` loop (line 141 in the new file), so a block with root groups but no tabs — or with both root groups and tabs — will never have a view context for the "Generic" tab. As a result, `_hintMap.get(null)` in `#renderTab(null, '#general_generic')` will always return `undefined` and the badge on the Generic/root tab will never appear in the routed mode. The fix is to call `this.#createViewContext(null, ...)` (and correspondingly `#provideViewContext(null, component)`) for the root route, mirroring the no-router implementation.

- **`src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit.element.ts:155–161`**: `#provideViewContext` is never called for the root route. The `setup` callback on the root route (path `'root'`) does not call `this.#provideViewContext(null, component)`, and neither does the alias empty-path route (which is a spread copy of the same root route setup). This means even if a root view context were created, it would never be `provideAt`-ed when the user is on the root/Generic tab in routed mode.

- **`src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/block-element-manager.ts:83`**: The `workspaceViewAlias` argument that was previously passed to `UmbContentValidationToHintsManager` as `[workspaceViewAlias]` is now replaced with `[]`. This means the path-segment filtering that previously scoped hints to the workspace view is removed. Depending on what `UmbContentValidationToHintsManager` does with those path segments, removing the filter could cause all validation hints to be propagated without the view-alias scoping. Verify that `UmbViewContext` (now used as the parent) provides equivalent scoping so that hints aren't incorrectly leaked across multiple block workspace instances on the same page.

- **`src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit-content-no-router.element.ts:97–130`**: The `#tabViewContexts` array is populated additively (contexts are only added, never removed or destroyed explicitly). If `#setupViewContexts` is called multiple times (e.g., tabs change because the block type structure is updated at runtime), the array grows but old contexts remain because the `find`-based deduplication key is `viewAlias`. If a tab is renamed, the old `viewAlias` remains in the array with its observer still active. This is a potential memory/observer leak. Consider either clearing and rebuilding the array on each `#setupViewContexts` call, or tracking and destroying removed contexts.

- **No tests are included** for the new badge logic or the updated tab-active detection logic in either element. The active-state computation in the routed version (`#renderTab`) has multi-branch logic (root with/without alias path, first-tab alias when no root items) that is particularly prone to regressions and would benefit from unit tests.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit-content-no-router.element.ts:191`**: `#provideViewContext` throws `new Error(...)` when the view is not found, but since it can be called during the `#checkDefaultTabName` flow (which runs inside observers set up in the constructor), an uncaught exception here would surface as an unhandled error. Consider returning early instead of throwing if the view context is not yet available, since the missing-context case can occur during the async setup race between `_tabs` and `_hasRootGroups`.

- **`src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit.element.ts:200–218`**: `#createViewContext` throws `new Error('Block Manager not found')` at the top, but it is called from `#createRoutes` which itself is called from `#setStructureManager` only after `this.#blockManager` is assigned. The guard is correct in principle but could be replaced with a defensive early return (same as the no-router version) to avoid unexpected throws if the call order ever changes.

- **`src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit-content-no-router.element.ts:110`**: `view.hints.setPathFilter(...)` is applied only when `viewAlias === null`, but the comment says "Treat empty paths as 'not in a tab'". The same filter logic is duplicated between the no-router and routed versions. Consider extracting a shared `createRootTabPathFilter` helper to reduce duplication and ensure the filter semantics stay in sync.

---

## Request Changes

Critical and important issues must be addressed first.

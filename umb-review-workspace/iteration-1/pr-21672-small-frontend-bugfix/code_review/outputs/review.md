### Code review

Found 2 issues:

1. **Missing root tab view context creation in router-based view means hints/badges never appear on the root "Generic" tab** (bug)

   In `block-workspace-view-edit.element.ts`, the `#createRoutes()` method creates a route for the root tab (lines 120-128) but never calls `#createViewContext(null, ...)` for it. This means `_hintMap.get(null)` will always return `undefined` when `#renderTab(null, '#general_generic')` is called at line 230, so validation badges will never appear on the root tab in the router-based block workspace view.

   Compare with `block-workspace-view-edit-content-no-router.element.ts` where `#setupViewContexts()` correctly calls `this.#createViewContext(null, '#general_generic')` at line 99.

   Additionally, the root tab route's `setup` callback does not call `#provideViewContext(null, component)`, so even if a view context were created for `null`, it would never be provided to the root tab's component. Named tabs correctly call both `#createViewContext` and `#provideViewContext` in their route setup.

   `src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit.element.ts:120-128`

2. **`#tabViewContexts` array grows without cleanup when tabs change dynamically** (resource leak)

   In both `block-workspace-view-edit.element.ts` and `block-workspace-view-edit-content-no-router.element.ts`, the `#tabViewContexts` array is only ever appended to (via `push` in `#createViewContext`). The deduplication check (`!this.#tabViewContexts.find(...)`) prevents duplicates but never removes stale entries. If tabs are renamed or removed, the old `UmbViewController` instances and their associated observers remain alive. Each `UmbViewController` creates observers on `firstHintOfVariant` and calls `inheritFrom` on the block manager's view, so stale controllers will continue consuming resources.

   `src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit.element.ts:58,164-199`
   `src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit-content-no-router.element.ts:51,111-147`

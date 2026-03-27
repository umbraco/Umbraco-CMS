### Code review
Found 1 issue:

1. Root tab hint badge never appears in the routed editor view (missing `#createViewContext` call for root path)

   In `block-workspace-view-edit.element.ts`, the `#createRoutes()` method creates a `UmbViewController` via `#createViewContext(path, tabName)` for each named tab (line 144), and `#provideViewContext(path, component)` is called in each tab's route `setup` callback (line 141). However, the root route (for blocks with root groups/properties) never calls either `#createViewContext(null, ...)` or `#provideViewContext(null, component)` in its `setup` callback (lines 120–128).

   As a result, `_hintMap.get(null)` is always `undefined`, so the hint badge for the root/"Generic" tab is silently omitted in the routed view — even when there are validation errors scoped to root properties. This is asymmetric with the non-router element (`block-workspace-view-edit-content-no-router.element.ts`) which correctly creates and provides a view context for the root tab (lines 98–100 and 161–165).

   `src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit.element.ts:120`

# PR Review

**Target:** `origin/main` · **Based on commit:** `6a8f6ee944f5b0cc64173df7bc88c5cc216bfa7e` · **Files:** 4 changed, 0 skipped, 4 reviewed

This PR adds validation error badges to block workspace tabs — both in the routed (`umb-block-workspace-view-edit`) and the no-router inline-edit (`umb-block-workspace-view-edit-content-no-router`) variants — by replacing the previous `UmbHintContext`-based approach with `UmbViewContext`/`UmbViewController` per-tab contexts that observe and surface per-tab hint state.

- **Modified public API:** `UmbBlockElementManager.hints` renamed to `UmbBlockElementManager.view` (type changed from `UmbHintContext<UmbVariantHint>` to `UmbViewContext`)
- **Affected implementations (outside this PR):** None — `UmbBlockElementManager` and its `.hints`/`.view` property are internal and not reachable via the `@umbraco-cms/backoffice` package exports

---

### Critical

- **`block-workspace-view-edit.element.ts:120–128`**: The root route (`path: 'root'`) is added to `routes` without a corresponding call to `#createViewContext(null, ...)` or `#provideViewContext(null, ...)`. Only tab routes get a view context created and provided in their `setup` callbacks. As a result, `this._hintMap.get(null)` in `#renderTab(null, '#general_generic')` always returns `undefined`, so the Generic (root) tab can never display a validation error badge in the routed workspace view — even though the no-router variant does support it. To fix, add a view context for the root path alongside the root route:

  ```ts
  // After the root route push:
  this.#createViewContext(null, '');  // or whatever title the root tab uses

  // And in the root route's setup callback:
  setup: (component) => {
    (component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
    (component as UmbBlockWorkspaceViewEditTabElement).containerId = null;
    this.#provideViewContext(null, component);
  },
  ```

  Additionally, the path filter for `viewAlias === null` inside `#createViewContext` of this file would need to be applied to the root context, same as in the no-router counterpart.

---

### Important

- **`block-workspace-view-edit.element.ts:148–153`**: The empty-path alias route is always built as a spread of `routes[0]`. When `_hasRootGroups` or `_hasRootProperties` is true, `routes[0]` is the `'root'` route and its `setup` does not call `#provideViewContext`. When only tabs exist (no root groups/properties), `routes[0]` is the first tab route whose `setup` does call `#provideViewContext`. This asymmetry means that in the root-has-content scenario, navigating via the alias `''` path activates a route component whose `setup` never provides a view context for the active tab. The `'root'` route setup should call `#provideViewContext` to make both the canonical and alias paths consistent.

- **`block-workspace-view-edit-content-no-router.element.ts:86–104`**: `#setupViewContexts` is called on every update to `_tabs`, `_hasRootProperties`, and `_hasRootGroups`. These three observables can all emit during a single mount cycle, so `#setupViewContexts` (and therefore `#checkDefaultTabName`) may be called multiple times before `_activeTabKey` is resolved. The guard `if (_activeTabKey === undefined)` inside `#checkDefaultTabName` prevents double-initialization, but `#setupViewContexts` still calls `#createViewContext` repeatedly; the duplicate guard inside `#createViewContext` is correct, but the overall structure is fragile. A minor but observable consequence: if `_tabs` arrives before `_hasRootGroups`, the root view context is not created in the first `#setupViewContexts` call, so the default tab might be set to the first tab even when root groups exist. Consider making `#setupViewContexts` only run when both `_tabs` is non-null and the groups observation has completed.

- **`block-workspace-view-edit-content-no-router.element.ts:57–80`**: `this.#blockManager` is assigned inside `consumeContext` but `#observeRootGroups()` (called immediately after) accesses `this.#blockManager` via an `await` on `this.#blockManager?.structure.hasRootContainers('Group')`. Because `consumeContext` callbacks fire asynchronously, there is a window where `#observeRootGroups` is called with `this.#blockManager` potentially still `undefined` (before the context has been consumed). This was a pre-existing risk but the refactor makes `#blockManager` a direct field now (instead of going through `this.#blockWorkspace?.content`). The `?` optional chain on `this.#blockManager?.structure` means it silently produces `undefined`, and the observation is never set up — root groups would never be observed. Add a guard: `if (!this.#blockManager) return;` at the top of `#observeRootGroups`.

---

### Suggestions

- **`block-workspace-view-edit.element.ts:197`**: The observer alias is `'umbObserveState_' + viewAlias`, and `viewAlias` can theoretically be `null`, producing the string `'umbObserveState_null'`. This works today but is surprising. Using a more explicit sentinel (e.g., `viewAlias ?? '__root__'`) would make the intent clearer.

- **`block-workspace-view-edit-content-no-router.element.ts:109`**: `getViewAliasForTab` encodes `tab.name ?? ''` for tabs without a name, producing the alias `'tab/'`. Two unnamed tabs would therefore share the same alias, and the duplicate check in `#createViewContext` would silently skip the second one. This is likely an edge-case not encountered in practice (unnamed tabs), but a comment noting the assumption or using `tab.ids[0]` as a fallback in the alias would make the code more robust.

- **`block-workspace-view-edit.element.ts:204`**: `view.provideAt(component as any)` uses a type cast to `any`. `PageComponent` is already typed as `UmbClassInterface` per the router module, so the cast should not be needed if the type of `component` in the parameter is narrowed correctly. If `PageComponent` does not satisfy `UmbClassInterface`, it is worth understanding why and possibly fixing the type definition rather than casting away the type error.

---

## Request Changes

The missing view context for the root tab in the routed element (`block-workspace-view-edit.element.ts`) means the Generic tab will silently never show validation error badges when blocks have root-level properties, which is the primary scenario this PR aims to address. The empty-path alias route issue compounds this by making the root tab inconsistent regardless of navigation path. These must be addressed before merge.

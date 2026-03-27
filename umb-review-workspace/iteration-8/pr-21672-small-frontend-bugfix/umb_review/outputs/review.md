## PR Review

**Target:** `origin/main` · **Based on commit:** `v17/bugfix/21178` · **Skipped:** 0 files out of 4 total

Implements validation hint badges on tabs in the Block Editor workspace — both in the routed modal view (`block-workspace-view-edit.element.ts`) and the inline no-router view (`block-workspace-view-edit-content-no-router.element.ts`). In `block-element-manager.ts`, the `hints` property is replaced by a `view` property (`UmbViewContext`), and `block-workspace.context.ts` wires up view inheritance for content/settings managers.

- **Modified public API:** `UmbBlockElementManager.hints` (public `readonly` field) renamed to `UmbBlockElementManager.view`
- **Affected implementations (outside this PR):** `UmbBlockElementManager` is not reachable via `package.json` exports — confirmed by checking `src/packages/block/block/workspace/index.ts` and no root export entry. Not a public breaking change.
- **Breaking changes:** None — `hints` was not part of the exported public API surface.
- **Other changes:** Tab headers in block workspaces now show `<umb-badge>` with validation hint color/text when a tab contains invalid properties (and the tab itself is not active). The `data-mark` attribute is added to tab elements for test automation.

---

### Important

- **`block-workspace-view-edit.element.ts:120-144`**: When `_hasRootGroups || _hasRootProperties` is true and the block also has named tabs, `#createViewContext(null, ...)` is never called for the root/generic tab. Only tab-path routes (lines 131-145) call `#createViewContext`. As a result, `this._hintMap.get(null)` at render time (line 257) is always `undefined`, so the generic tab badge never shows validation hints in the routed variant — even though the no-router variant handles this correctly. Fix: call `this.#createViewContext(null, tabName)` in the root route block (lines 120-128), with the appropriate tab name (e.g. `'#general_generic'` or `undefined`).

  ```ts
  if (this._hasRootGroups || this._hasRootProperties) {
      routes.push({
          path: 'root',
          component: () => import('./block-workspace-view-edit-tab.element.js'),
          setup: (component) => {
              (component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
              (component as UmbBlockWorkspaceViewEditTabElement).containerId = null;
              this.#provideViewContext(null, component);
          },
      });
      this.#createViewContext(null, '#general_generic');
  }
  ```

- **`block-workspace-view-edit.element.ts:219`**: `view.provideAt(component as any)` — `component` is typed as `PageComponent` (from `@umbraco-cms/backoffice/router`) but `provideAt` likely expects `UmbControllerHost`. The `as any` cast silences a legitimate type incompatibility. If `PageComponent` is always a `UmbControllerHost` in practice, a narrower cast or a type guard would make the contract explicit and avoid silent failures if the assumption changes.

### Suggestions

- **`block-workspace-view-edit-content-no-router.element.ts:111-113`**: `#createViewContext` throws when `#blockManager` is null, but the only caller (`#setupViewContexts`) already guards `if (!this._tabs || !this.#blockManager) return` at line 95. The throw is unreachable. Either remove the guard (rely on the caller's invariant) or convert it to a no-op early return for consistency with the rest of the file's defensive style.

- **`block-workspace-view-edit-content-no-router.element.ts:221` / `block-workspace-view-edit.element.ts:237`**: `tab.name` (`string | undefined`) is passed to `#renderTab`'s `name: string` parameter in both files. TypeScript would flag this; the fallback `name ?? '#general_unnamed'` inside `#renderTab` handles it correctly at runtime, but the parameter type should be widened to `string | undefined` to match callers.

- **`block-workspace-view-edit-content-no-router.element.ts:94-109`**: `#setupViewContexts` only adds view contexts, never removes stale ones. If a tab is removed from the content type while the block editor is open, orphaned `UmbViewController` instances stay in `#tabViewContexts` and their `firstHintOfVariant` observations keep running. In practice this is a minor concern since the entire component is usually remounted on structural changes, but worth noting if live-editing of content types becomes a scenario.

---

## Request Changes

Critical and important issues must be addressed first.

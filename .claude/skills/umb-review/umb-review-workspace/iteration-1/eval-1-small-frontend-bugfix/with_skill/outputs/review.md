## PR Review

**Target:** `origin/main` · **Based on commit:** `a1bcabf44e7`

Adds validation badge indicators to block workspace tabs so that validation errors on non-active tabs are visible, mirroring the pattern already used in the content workspace editor.

- **Other changes:** Block workspace tabs now display `umb-badge` elements showing validation state (e.g., invalid color) on inactive tabs. The `UmbBlockElementManager.hints` property was renamed to `view` (type changed from `UmbHintContext` to `UmbViewContext`), and view context inheritance is wired through `block-workspace.context.ts`.

---

### Important

- **`block-workspace-view-edit.element.ts:120-128`**: Missing `#createViewContext(null, '#general_generic')` call and `#provideViewContext(null, component)` in the root route's `setup`. The sibling `content-editor.element.ts:128-131` calls both for the root tab. Without these, the root tab in the router-based block workspace will never display a validation badge, and the root route's child elements won't receive a view context. The no-router variant (`block-workspace-view-edit-content-no-router.element.ts`) handles this correctly via `#setupViewContexts()` at line 98-99. → Add `this.#createViewContext(null, '#general_generic');` after the root route push (around line 129), and add `this.#provideViewContext(null, component);` inside the root route's `setup` callback.

---

### Suggestions

- **`block-workspace-view-edit-content-no-router.element.ts:236`**: The `#renderTab` method's first parameter `tabKey` is `string | null` while `name` is `string` (non-nullable), yet the template applies `name ?? '#general_unnamed'` fallback. The caller at line 213 passes `'#general_generic'` which is always truthy, so the fallback is unreachable for the root tab. Same pattern in `block-workspace-view-edit.element.ts:270`. Minor inconsistency with no runtime impact, but the types could be tighter to match intent.

- **`block-workspace-view-edit-content-no-router.element.ts:94-109`**: `#setupViewContexts()` is called from three observers (tabs change, root properties change, root groups change) and creates view contexts incrementally but never removes stale ones if tabs are removed. The `find`-guard prevents duplicates but orphaned contexts for removed tabs persist. Same pattern exists in the content-editor sibling, so this is pre-existing, but worth noting for future cleanup.

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions.

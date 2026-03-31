# Code Review: PR #21672 -- Tab Validation Badges in Block Workspace

**PR:** #21672
**Scope:** 4 files, +285/-64 lines -- block workspace tab validation badge support
**Target:** `origin/main`

---

## Summary

This PR adds validation error badges to block workspace tabs, so users can see which tab contains invalid fields without switching to it. The approach replaces `UmbHintContext` with a new `UmbViewContext`/`UmbViewController` pattern in `block-element-manager.ts`, wires up inheritance in `block-workspace.context.ts`, and adds per-tab `UmbViewController` instances plus badge rendering in both the routed (`block-workspace-view-edit.element.ts`) and non-routed (`block-workspace-view-edit-content-no-router.element.ts`) block workspace view edit elements.

The general approach follows the existing pattern established in `content-editor.element.ts` and is structurally sound. However, there is one significant bug and several lesser issues.

---

## Issues

### 1. [HIGH] Root tab missing view context creation and provision in routed element

**File:** `src/Umbraco.Web.UI.Client/src/packages/block/block/workspace/views/edit/block-workspace-view-edit.element.ts`
**Lines:** 120-129 (in `#createRoutes`)

In the routed variant, the root route (for root groups/properties) does not call `#createViewContext(null, ...)` or `#provideViewContext(null, component)` in its `setup` callback. Compare with the reference implementation in `content-editor.element.ts` lines 122-132, which correctly does both:

```typescript
// content-editor.element.ts (correct):
if (this._hasRootGroups || this._hasRootProperties) {
    routes.push({
        path: 'root',
        component: () => import('./content-editor-tab.element.js'),
        setup: (component) => {
            (component as ...).containerId = null;
            this.#provideViewContext(null, component);   // <-- PRESENT
        },
    });
    this.#createViewContext(null, '#general_generic');    // <-- PRESENT
}
```

```typescript
// block-workspace-view-edit.element.ts (missing):
if (this._hasRootGroups || this._hasRootProperties) {
    routes.push({
        path: 'root',
        component: () => import('./block-workspace-view-edit-tab.element.js'),
        setup: (component) => {
            (component as ...).managerName = this.#managerName;
            (component as ...).containerId = null;
            // #provideViewContext(null, component) is MISSING
        },
    });
    // #createViewContext(null, '#general_generic') is MISSING
}
```

**Consequence:** Validation errors on root-level properties (those not in any named tab) will never produce a badge on the "Generic" root tab in the routed block workspace. The badge feature works for named tabs only.

The non-routed element (`block-workspace-view-edit-content-no-router.element.ts`) handles this correctly via `#setupViewContexts()` which creates the root view context at line 98-99 and provides it via `#checkDefaultTabName` -> `#provideViewContext(null)`.

---

### 2. [MEDIUM] View contexts are only ever appended, never cleaned up on tab changes

**Files:**
- `block-workspace-view-edit.element.ts`, line 58 / line 168
- `block-workspace-view-edit-content-no-router.element.ts`, line 51 / line 115

Both elements use `#tabViewContexts: Array<UmbViewController> = []` and only ever push to it, guarded by a deduplication check (`if (!this.#tabViewContexts.find(...))`). When the content type structure changes at runtime (e.g., composition changes cause tabs to be removed), stale `UmbViewController` instances remain in the array indefinitely. Their observers continue running and they continue to hold references.

In practice this is mitigated by the fact that block workspace lifetime is generally short-lived (editing a single block), but it is still a leak if compositions change during editing. The content-editor has the same pattern, so this is a pre-existing issue replicated here rather than introduced.

---

### 3. [LOW] Duplicated logic between routed and non-routed elements

**Files:**
- `block-workspace-view-edit.element.ts` lines 164-219
- `block-workspace-view-edit-content-no-router.element.ts` lines 111-203

The methods `#createViewContext`, `#provideViewContext`, and `#renderTab` are nearly identical between the two elements. The path filter logic, hint observation pattern, and badge rendering template are copy-pasted. This makes them prone to divergence (as demonstrated by issue #1 where one has a bug the other does not). Consider extracting the shared view-context management logic into a mixin or shared controller.

---

### 4. [LOW] `name` parameter typed as `string` but `tab.name` could be nullish in template

**File:** `block-workspace-view-edit-content-no-router.element.ts`, line 236
**File:** `block-workspace-view-edit.element.ts`, line 256

The `#renderTab` methods accept `name: string` but the template uses `name ?? '#general_unnamed'` with a nullish coalescing operator. Since the parameter is typed as `string` (not `string | undefined | null`), the `??` fallback can never trigger. This is cosmetically harmless (it mirrors existing code in other parts of the codebase), but the type and the guard are inconsistent with each other. Either widen the type or remove the fallback.

---

### 5. [LOW] Minor: observer key could collide when `viewAlias` is `null`

**File:** `block-workspace-view-edit-content-no-router.element.ts`, line 145
**File:** `block-workspace-view-edit.element.ts`, line 198

The observer alias is constructed as `'umbObserveState_' + viewAlias`. When `viewAlias` is `null`, this produces `'umbObserveState_null'` (string coercion). This works, but is fragile -- if a tab were ever literally named `"null"` the aliases would collide. Using something like `'umbObserveState_' + (viewAlias ?? '__root__')` would be more robust.

---

### 6. [LOW] `this._tabs?.length` double-optional-chaining in `#provideViewContext`

**File:** `block-workspace-view-edit-content-no-router.element.ts`, line 201
**File:** `block-workspace-view-edit.element.ts`, line 217

```typescript
view.setTitle(this._tabs && this._tabs?.length > 0 ? '#general_generic' : undefined);
```

The `&&` guard already ensures `this._tabs` is truthy, so `this._tabs?.length` does not need the optional chaining operator. Minor style nit.

---

### 7. [INFO] Positive: Improved defensiveness in path filter

Both block workspace elements add a guard for empty/undefined `paths[0]` in the root tab path filter:

```typescript
view.hints.setPathFilter((paths) => {
    const firstPath = paths[0];
    if (!firstPath) { return true; }
    return firstPath.includes('tab/') === false;
});
```

The content-editor's equivalent at line 173 does `paths[0].includes('tab/') === false` without this guard. The block workspace's version is more defensive and would survive edge cases where validation hints have empty paths. This is a good improvement.

---

### 8. [INFO] Good: `UmbHintContext` replaced by `UmbViewContext`

The migration from `UmbHintContext` (which provided hints directly) to `UmbViewContext` (which encapsulates hints, shortcuts, and title management in a coherent view abstraction) is the right direction. The `UmbViewContext` auto-provides at construction while `UmbViewController` gives manual control -- the usage here appropriately picks `UmbViewContext` for the element manager (always provided) and `UmbViewController` for per-tab contexts (provided on demand).

---

## Verdict

**Request changes.** Issue #1 is a functional bug: the routed block workspace element is missing `#createViewContext` and `#provideViewContext` calls for the root tab route, meaning validation badges will not appear on the "Generic" tab when editing blocks via the modal/routed workspace. This should be fixed before merge. The remaining issues are low severity and could be addressed in follow-up work.

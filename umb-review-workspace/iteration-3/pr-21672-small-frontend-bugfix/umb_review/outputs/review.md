## PR Review

**Target:** `origin/main` · **Based on commit:** `6a8f6ee944f5b0cc64173df7bc88c5cc216bfa7e` · **Files:** 4 changed, 0 skipped, 4 reviewed

This PR adds validation badge indicators to block workspace tabs (both routed and no-router variants), so users can see which tab contains validation errors without switching to it. It refactors the `UmbBlockElementManager` to expose a `view` property (replacing `hints`) and wires per-tab `UmbViewController` instances to propagate hint state into the tab bar UI.

- **Modified public API:** `UmbBlockElementManager.hints` renamed to `UmbBlockElementManager.view` (type changed from `UmbHintContext` to `UmbViewContext`). However, `UmbBlockElementManager` is not exported through the `@umbraco-cms/backoffice/block` package exports, so this is an internal-only change — not a breaking change for external consumers.

---

> **Note on scope:** The three-dot diff (`origin/main...origin/pr/21672`) shows a large set of changes because the PR branch diverged from the `release/17.2` merge base and `origin/main` has since incorporated most of this work. The actual unique changes between this PR's HEAD and current `origin/main` are confined to:
> - `block-workspace-view-edit-content-no-router.element.ts`: Explicit `= false` initialization for `_hasRootProperties` and `_hasRootGroups`, and a simplified `#checkDefaultTabName` guard.
> - `block-workspace.context.ts`: Removal of `propertyWriteGuard` rules and a wrong method call (`setOneContent` instead of `setOneSettings`).
>
> The review below evaluates the full diff as presented.

---

### Critical

- **`block-workspace.context.ts:657`**: `this.#blockManager?.setOneContent(this.#initialSettings)` calls `setOneContent` instead of `setOneSettings` for the initial settings data. When the block workspace restores initial data on discard/cancel, the settings data will be written into the content slot, corrupting the block state.

  Fix: Change to `this.#blockManager?.setOneSettings(this.#initialSettings)`.

- **`block-workspace.context.ts` (observeIsReadOnly)**:  The PR removes the four lines that apply `propertyWriteGuard` rules when the block manager marks the block as read-only:
  ```typescript
  this.content.propertyWriteGuard.addRule({ unique, permitted: false });
  this.settings.propertyWriteGuard.addRule({ unique, permitted: false });
  // and
  this.content.propertyWriteGuard.removeRule(unique);
  this.settings.propertyWriteGuard.removeRule(unique);
  ```
  Without these, read-only enforcement no longer propagates to the individual property write guards on the content and settings managers. Users may be able to edit properties on blocks that should be read-only (e.g., blocks inside published content in preview mode or blocks with a read-only block manager context).

  Fix: Restore all four lines. These are distinct responsibilities — `readOnlyGuard` controls document-level read-only, while `propertyWriteGuard` controls per-property write access.

### Important

- **`block-workspace-view-edit.element.ts` (`#provideViewContext`)**: `view.provideAt(component as any)` uses an `as any` cast to pass a `PageComponent` (`HTMLElement | undefined`) where `UmbClassInterface` is expected. If the component passed is `undefined` (possible according to the `PageComponent` type), this will throw at runtime inside `provideAt`. The `setup` callback in the route config is documented to receive a resolved component, but the type contract allows `undefined`.

  Consider adding a guard: `if (!component) return;` before calling `#provideViewContext`, and changing the signature to accept `HTMLElement` rather than `PageComponent` if undefined is not a realistic input here.

- **`block-workspace-view-edit.element.ts` / `block-workspace-view-edit-content-no-router.element.ts` (`#tabViewContexts`)**:  `#createViewContext` deduplicates by alias, but if `#createRoutes` or `#setupViewContexts` is called multiple times after tabs change (tabs added, removed, or renamed), stale `UmbViewController` instances accumulate in `#tabViewContexts` because old contexts are never removed. Since they are registered as controllers on `this`, they will be garbage-collected when the element is destroyed, so this is not a memory leak in the traditional sense. However, stale contexts observing `view.firstHintOfVariant` will continue firing and calling `requestUpdate('_hintMap')` unnecessarily.

  Consider adding cleanup logic: when `#createRoutes` or `#setupViewContexts` is called, destroy and remove any existing `UmbViewController` instances that no longer correspond to current tabs before creating new ones.

### Suggestions

- **`block-workspace-view-edit-content-no-router.element.ts:140`**: The comment `// block manager does not need to be setup this in file as that it being done by the implementation of this element.` has a grammatical issue and is somewhat unclear. It could say: `// The block manager is set up by the outer implementation; no setup call needed here.`

- **`block-workspace-view-edit.element.ts` and `block-workspace-view-edit-content-no-router.element.ts`** both duplicate the `#createViewContext` and `#provideViewContext` methods with near-identical logic. The pattern is complex enough that a shared utility or base class helper might reduce the maintenance surface — though this can wait for a follow-up if the two components diverge over time.

- **`block-workspace-view-edit-content-no-router.element.ts` (`#provideViewContext`, line with `therefor`)**: Typo in comment — `therefor` should be `therefore`. Same typo appears in `block-workspace-view-edit.element.ts`.

---

## Request Changes

Critical issues must be addressed before merge: the `setOneContent(this.#initialSettings)` bug will silently corrupt block state on cancel/discard, and the removal of `propertyWriteGuard` read-only rules creates a behavioral regression that allows edits in read-only blocks.

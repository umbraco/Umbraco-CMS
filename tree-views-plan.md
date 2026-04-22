# Tree Views — Implementation Plan

Introduce "views" for `<umb-tree>`, analogous to collection views. v1 ships two views (Classic + Card) and the surrounding infrastructure to register more later. Scope is the Umbraco backoffice client (`src/Umbraco.Web.UI.Client/`).

---

## 1. Design decisions (locked)

### 1.1 Scope

- **v1 views**: `Umb.TreeView.Classic` (the current expandable tree) + `Umb.TreeView.Card` (new, shows one level of children at a time with double-click to navigate into a child). Table view is explicitly deferred.
- **Consumers**: workspace-embedded trees, section sidebars, `<umb-tree-picker-modal>`.
- **No URL reflection.** Tree-view selection and drill position live in context state only. Persistence is runtime via `UmbInteractionMemoryManager`. Deep-linking for classic view is via expansion state; card view navigation is driven by the picker's `_currentLocation` prop.

### 1.2 Extension model

- New extension type **`treeView`**, mirroring the shape of `collectionView`. Manifest: `{ type: 'treeView', alias, name, kind?, element, meta: { label, icon } }`. No `pathName` (no routing).
- New extension type **`treeItemCard`** for per-entity card-item rendering. Follows the `treeItem` pattern (not prefixed with `entity`, keeping consistency with existing tree manifests).
- **Default view = highest `weight`** registered entry. No dedicated default-config.
- **View selection persistence is deferred to Phase 8.** The approach (dedicated interaction manager on the tree context vs. consuming `UMB_INTERACTION_MEMORY_CONTEXT`) needs further design before implementation.

### 1.3 Architecture

- **`<umb-tree>`** (`src/packages/core/tree/tree.element.ts`) unchanged. Still routes `ManifestTree` by entity type.
- **`<umb-default-tree>`** (`src/packages/core/tree/default/default-tree.element.ts`) becomes a **shell**. Registered via `Umb.Kind.Tree.Default`. Renders `<umb-tree-toolbar>` (conditionally) and the active view element below it.
- **`<umb-tree-toolbar>`** new element. Mirrors `<umb-collection-toolbar>`. Contains `<umb-tree-view-bundle>` in top-right.
- **`<umb-tree-view-bundle>`** new element. Popover menu for picking the active view. Divergence from `<umb-collection-view-bundle>`: **no URL routing** — clicking a view updates context state only.
- **Classic view**: existing rendering extracted from `<umb-default-tree>` into a new element, registered as `Umb.TreeView.Classic`.
- **Card view**: new element, registered as `Umb.TreeView.Card`. Renders items via `<umb-tree-item-card>` (entity-routed) with `<umb-default-tree-item-card>` fallback.

### 1.4 State

- **Expansion state is classic-view only.** `UmbTreeExpansionManager` tracks which nodes are expanded/collapsed in the classic list view (a multi-set). Card view does not use expansion for navigation.
- **`expandTo()` removed.** The previously planned `expansion.expandTo()` method has been removed — its use case (normalising expansion as a navigation chain for card view) is gone with the new architecture.
- **Card view navigation is driven by `startNode`.** The picker owns two values: `_initialStartNode` (the caller-configured ceiling, immutable) and `_currentLocation` (the current navigation position, changes as the user drills in). The picker always passes `_currentLocation` as the `startNode` prop to `<umb-tree>`. From the tree's perspective there is no distinction between ceiling and navigation depth — it simply renders children of whatever `startNode` it receives.
- **`startNode` prop is dynamic.** `setStartNode()` in `UmbDefaultTreeContext` already calls `loadTree()`, so the tree reloads automatically whenever the picker updates `_currentLocation`.

### 1.5 Interaction

- Single-click: existing behavior (picker → toggle selection; workspace → open workspace).
- **Double-click** on a card: navigate into that item. The card element calls `treeContext.open({ unique, entityType })` — a new method on `UmbTreeContext`. The context dispatches `UmbTreeItemOpenEvent` on its host element via `getHostElement().dispatchEvent(...)` so the event bubbles up to the picker.
- **Classic view has no drill gesture.** Classic view is scoped to whatever `startNode` the picker passes; the user navigates back up only via the breadcrumb. No "enter this node" action exists on classic tree items.
- Keyboard: **Enter** = single-click action, **Right Arrow** = navigate into (card view only).
- No explicit drill affordance on cards in v1. Discoverability via behavior.

### 1.6 Breadcrumb

- **Lives in the picker modal**, not in a tree view. It is always visible regardless of which view (Classic or Card) is active, and is rendered directly in `<umb-tree-picker-modal>` above `<umb-tree>`.
- **Both views are scoped by the breadcrumb.** The picker passes `_currentLocation` as `startNode` to `<umb-tree>`. Classic view shows an expandable subtree from that node; card view shows that node's children as cards.
- **Picker state:**
  - `_initialStartNode`: the caller-configured ceiling (`UmbTreeStartNode | undefined`). Immutable during the session. Determines the topmost (non-clickable) breadcrumb entry.
  - `_currentLocation`: the current navigation position (`UmbTreeStartNode | undefined`). Always passed as `startNode` prop. Starts equal to `_initialStartNode`.
  - `_breadcrumb`: `Array<{ unique: string | null, entityType: string, name: string }>`. Built from the ancestors response. First entry = ceiling, last entry = current location.
- **Initial breadcrumb population:**
  - No ceiling configured → call `repository.requestTreeRoot()` to get the root item name. Single entry, non-clickable until the user navigates deeper.
  - Ceiling configured → call `repository.requestTreeItemAncestors({ treeItem: _initialStartNode })`. The backend returns ancestors including the item itself (`includeSelf = true` is the default). Slice the chain at the ceiling unique. Single entry on first open.
- **Repository access:** The picker looks up the `ManifestTree` by `treeAlias` from `umbExtensionsRegistry` → reads `meta.repositoryAlias` → instantiates via `UmbExtensionApiInitializer`. The breadcrumb is outside `<umb-tree>` and cannot consume the tree context.
- **Forward navigation (card view double-click):** `UmbTreeItemOpenEvent` bubbles from the card element through `<umb-tree>` to the picker. The picker:
  1. Immediately sets `_currentLocation = { unique, entityType }` from the event (optimistic — tree starts reloading at once).
  2. Asynchronously calls `requestTreeItemAncestors({ treeItem: { unique, entityType } })` to get the full chain with names. Slices at the ceiling unique. Updates `_breadcrumb`.
- **Backward navigation (breadcrumb click):** Clicking a breadcrumb entry sets `_currentLocation` to that entry's `{ unique, entityType }` and truncates `_breadcrumb` to that index. The `startNode` prop on `<umb-tree>` updates, triggering a tree reload.
- **Ceiling entry** is always non-clickable (first item). All other entries are clickable.
- **Variant-aware name resolution** (e.g. `DocumentItemDataResolver` for culture-specific names) is deferred to v2. v1 uses raw `name` from the tree item response.

### 1.7 Opt-out

- New `hideToolbar` boolean attribute on `<umb-default-tree>`. **Default: `true`** (backwards-compat: every existing tree stays toolbar-less until explicitly opted in). Consumers that want the view switcher set `hide-toolbar="false"`.
- **Documented deviation**: `hideTreeRoot` and `hideTreeItemActions` both default to `false`. `hideToolbar = true` is the odd one out for backwards-compat; documented on the JSDoc.
- No `treeHostContext` condition in v1 (explicitly rejected).

### 1.8 Context interface additions

- `hideTreeItemActions` is added to `UmbTreeContext` as an **optional** observable member (`hideTreeItemActions?: Observable<boolean>`), with optional `setHideTreeItemActions` / `getHideTreeItemActions`. Non-breaking for external implementors. Mirrors how `hideTreeRoot` already lives on the context.
- Shell pushes into the context from its `updated()` method. Classic view consumes from context.
- Add `isMenu` similarly if the Classic view needs it during the extraction.

---

## 2. Implementation phases

Each phase lists dependencies, files to create/edit, and parallelizability. Phases are PR-sized units unless noted.

### Phase 0 — Pre-flight (read-only)

**Deps**: none.

- Skim `.claude/skills/general-create-extension-type/SKILL.md` for `treeView` + `treeItemCard` type registration.
- Skim `.claude/skills/general-create-kind/SKILL.md` for the default kind manifest.
- Skim `src/Umbraco.Web.UI.Client/docs/manifests.md` for alias conventions.
- Confirm subclass consumers of `UmbDefaultTreeElement`: `UmbDocumentTreeElement`, `UmbMediaTreeElement` are empty-body subclasses (safe).

### Phase 1 — New extension types (pure-type, non-breaking)

**Deps**: Phase 0. **Parallelizable with Phase 2.**

**Create**:
- `src/packages/core/tree/view/tree-view.extension.ts` — `ManifestTreeView` mirroring `ManifestCollectionView` without `pathName`. Adds `UmbExtensionManifestMap.umbTreeView`.
- `src/packages/core/tree/tree-item-card/tree-item-card.extension.ts` — `ManifestTreeItemCard` with `forEntityTypes: string[]`. Adds `UmbExtensionManifestMap.umbTreeItemCard`.
- `src/packages/core/tree/view/index.ts` — barrel, type re-exports.
- `src/packages/core/tree/view/types.ts` — `export type * from './tree-view.extension.js'`.
- `src/packages/core/tree/tree-item-card/index.ts` — barrel.

**Edit**:
- `src/packages/core/tree/index.ts` — re-export the two new barrels.

**Acceptance**: repo compiles; new manifest types appear in `UmbExtensionManifestMap`; no runtime changes.

### Phase 2 — Remove `expandTo()` from expansion manager

**Deps**: none (pure deletion). **Can be done at any time.**

The `expandTo()` method was added in anticipation of card-view navigation but is now superseded by the `_currentLocation`/`startNode` approach in the picker. It was never called outside its own test file.

**Edit**:
- `src/packages/core/tree/expansion-manager/tree-expansion-manager.ts` — delete the `expandTo()` method and any imports it introduced.
- `src/packages/core/tree/expansion-manager/tree-expansion-manager.test.ts` — delete the `expandTo` describe block.

**Acceptance**: repo compiles; no remaining references to `expandTo`.

### Phase 3 — Extract Classic render + shell refactor (the invasive step)

**Deps**: Phase 1.

#### 3.1 Add optional config to `UmbTreeContext`

**Edit**:
- `src/packages/core/tree/tree.context.interface.ts` — add optional `hideTreeItemActions?: Observable<boolean>`, optional `setHideTreeItemActions?(value: boolean): void`, optional `getHideTreeItemActions?(): boolean`. Add `isMenu?: Observable<boolean>` + setter/getter if Classic view needs it.
- `src/packages/core/tree/default/default-tree.context.ts` — implement the new optional members (private `UmbBooleanState`, public observable, setter/getter) mirroring existing `hideTreeRoot`.

#### 3.2 Create the Classic view element

**Create**:
- `src/packages/core/tree/view/classic/classic-tree-view.element.ts` — `umb-classic-tree-view`, extends `UmbLitElement`. Consumes `UMB_TREE_CONTEXT`. Observes `treeRoot`, `rootItems`, pagination, load-prev/next loading flags, `hideTreeRoot`, `hideTreeItemActions`, `isMenu` from the context. Replicates `render()`, `#renderTreeRoot`, `#renderRootItems`, `#renderLoadPrevButton`, `#renderLoadNextButton` from current `default-tree.element.ts:164-214`.
- `src/packages/core/tree/view/classic/manifests.ts` — `{ type: 'treeView', alias: 'Umb.TreeView.Classic', element: () => import('./classic-tree-view.element.js'), weight: 1000, meta: { label: '#treeView_classic', icon: 'icon-list' } }`.
- `src/packages/core/tree/view/classic/index.ts` — barrel.

#### 3.3 View manager + context token

**Create**:
- `src/packages/core/tree/view/tree-view.manager.ts` — `UmbTreeViewManager extends UmbContextBase`. Shaped like `UmbCollectionViewManager` minus route code. Holds `#views: UmbArrayState<ManifestTreeView>`, `#currentView: UmbObjectState<ManifestTreeView | undefined>`, methods `setCurrentView`, `getCurrentView`. Observes `treeView` manifests via `UmbExtensionsManifestInitializer`. Default selection = highest-weight manifest. **No interaction memory in this phase** — view selection persistence is deferred to Phase 8.
- `src/packages/core/tree/view/tree-view.context.token.ts` — `UMB_TREE_VIEW_CONTEXT`.

#### 3.4 Convert `<umb-default-tree>` into a shell

**Edit** `src/packages/core/tree/default/default-tree.element.ts`:
- **Remove** `#renderTreeRoot`, `#renderRootItems`, `#renderLoadPrevButton`, `#renderLoadNextButton`, all related `@state` (`_rootItems`, `_treeRoot`, `_currentPage`, `_hasPreviousItems`, `_hasNextItems`, `_isLoadingPrevChildren`, `_isLoadingNextChildren`), and the observers that populate them.
- **Keep** all existing public `@property` declarations and `api` property.
- **Add** `@property({ type: Boolean, attribute: 'hide-toolbar' }) hideToolbar: boolean = true;` with JSDoc explaining the default-`true` backwards-compat deviation.
- **Extend** `updated()` to push `hideTreeItemActions` (and `isMenu` if added) into the context via the new setters.
- **Render**: when `hideToolbar` is false, render `<umb-tree-toolbar>`. Always render the active view element, mounted from the `UmbTreeViewManager`'s `currentView` observable.
- View mounting: attach `UmbTreeViewManager` as a controller on the shell. Observe `currentView`; use `createExtensionElement(manifest)` to build the element; stamp into render output.

#### 3.5 Register Classic view manifests

**Edit** `src/packages/core/tree/manifests.ts` — import and spread the Classic view's manifests array into the exported manifests list.

**Acceptance**:
- Shell renders a Classic tree that looks and behaves identically to the pre-refactor default tree.
- `hideTreeItemActions`, `hideTreeRoot`, `startNode`, `selection`, `expansion` props all flow correctly.
- Subclasses (`UmbDocumentTreeElement`, `UmbMediaTreeElement`) still work unchanged.
- Sidebar tree usages show no toolbar (because `hideToolbar = true` is default).

### Phase 4 — Toolbar + view-bundle elements

**Deps**: Phase 3.

**Create**:
- `src/packages/core/tree/components/tree-toolbar.element.ts` — `umb-tree-toolbar`. Simple flex layout: `<div id="slot"><slot></slot></div>` + `<umb-tree-view-bundle></umb-tree-view-bundle>`. No action bundle or filter extensions in v1.
- `src/packages/core/tree/components/tree-view-bundle.element.ts` — `umb-tree-view-bundle`. Consumes `UMB_TREE_VIEW_CONTEXT`. Renders nothing when `views.length <= 1`. Otherwise renders a popover with a menu item per view; click sets current view via `viewContext.setCurrentView(view)`. No `href`, no router.
- `src/packages/core/tree/components/index.ts` — barrel, side-imports both elements.

**Edit**:
- `src/packages/core/tree/index.ts` — re-export the components barrel.

**Acceptance**: toolbar appears in workspace/picker context when multiple views are registered; clicking a menu item changes the active view; choice persists via interaction memory.

### Phase 5 — Card view + card-item routing

**Deps**: Phases 1, 3, 4.

#### 5.1 Add `open()` to tree context

**Edit**:
- `src/packages/core/tree/tree.context.interface.ts` — add `open(item: { unique: string; entityType: string }): void` to the interface.
- `src/packages/core/tree/default/default-tree.context.ts` — implement `open(item)`: calls `this.getHostElement().dispatchEvent(new UmbTreeItemOpenEvent(item))`. The event is `composed: true` + `bubbles: true` so it crosses shadow DOM and reaches the picker.

#### 5.2 Default card item element

**Edit**:
- `src/packages/core/tree/tree-item-card/default/default-tree-item-card.element.ts` — on double-click / Right Arrow, call `treeContext.open({ unique: item.unique, entityType: item.entityType })` via the consumed `UMB_TREE_CONTEXT`. Do **not** dispatch a DOM event directly. `UmbTreeItemCardNavigateEvent` is not needed and should not be created.
- `src/packages/core/tree/tree-item-card/default/index.ts` — barrel (no events subfolder).

#### 5.3 Card item routing element

Already implemented. No changes needed beyond removing any reference to `UmbTreeItemCardNavigateEvent` if it exists.

#### 5.4 Card view element (slim)

**Edit** `src/packages/core/tree/view/card/card-tree-view.element.ts` — strip down to a slim grid renderer:
- **Remove**: breadcrumb render (`#renderBreadcrumb`, `<uui-breadcrumbs>`), `#navigateTo`, `#navigateToRoot`, `_expansion` state, `#loadItemsForCurrentLocation`, `#nameCache`, `_observeExpansion` observer, `e.stopPropagation()` in `#onOpen`.
- **Keep**: observe `rootItems` from context, render grid of `<umb-tree-item-card>`, wire `@selected`/`@deselected` to context selection, wire `@umb-tree-item-open` to call `treeContext.open(item)` (which dispatches the event upward — **do not call `stopPropagation()`**).
- Keyboard on card: `Enter` = single-click action, `Right Arrow` = call `treeContext.open(item)`.
- Pagination: `<umb-tree-load-more-button>` at grid bottom.
- Empty state: `#treeView_cardEmptyRoot`.

**Edit**:
- `src/packages/core/tree/manifests.ts` — spread Card manifests in (already done; verify only).

**Acceptance**:
- Card view renders `rootItems` (scoped by whatever `startNode` the picker passes).
- Double-click fires `UmbTreeItemOpenEvent` upward to the picker; picker drives breadcrumb and `startNode` update.
- Single-click in picker toggles selection; single-click in workspace opens workspace.
- Keyboard: Enter and Right Arrow behave as specified.
- No breadcrumb or navigation state inside the card view element itself.

### Phase 6 — Breadcrumb in the picker modal

**Deps**: Phase 5 (requires `treeContext.open()` and the slimmed card view).

**Edit** `src/packages/core/tree/tree-picker-modal/tree-picker-modal.element.ts`:

#### State
Add to the picker element:
- `_initialStartNode: UmbTreeStartNode | undefined` — set from `this.data?.startNode` on init, immutable.
- `_currentLocation: UmbTreeStartNode | undefined` — starts equal to `_initialStartNode`, updated on navigation.
- `_breadcrumb: Array<{ unique: string | null; entityType: string; name: string }>` — always at least one entry.

#### Repository initialisation
On `connectedCallback` / first render: use `UmbExtensionApiInitializer` to look up the `ManifestTree` by `this.data.treeAlias` from `umbExtensionsRegistry` → read `meta.repositoryAlias` → instantiate the tree repository. Store as `_treeRepository`.

#### Initial breadcrumb
After repository is ready:
- No `_initialStartNode`: call `_treeRepository.requestTreeRoot()` → use the returned item's `name` as the single ceiling entry (`unique: null`).
- `_initialStartNode` set: call `_treeRepository.requestTreeItemAncestors({ treeItem: _initialStartNode })` → response includes self (backend default). Slice to the entry matching `_initialStartNode.unique` (discard any ancestors above it). The last item gives the name.

#### Forward navigation
Listen to `UmbTreeItemOpenEvent` on the picker element (it bubbles up from the card view through `<umb-tree>`):
1. Immediately set `_currentLocation = { unique: e.unique, entityType: e.entityType }`. The `startNode` prop on `<umb-tree>` updates at once — tree starts reloading optimistically.
2. Asynchronously call `_treeRepository.requestTreeItemAncestors({ treeItem: _currentLocation })`. Slice chain at `_initialStartNode.unique` (or keep all if no ceiling). Map to `_breadcrumb` array with names from response.

#### Backward navigation
Each non-last breadcrumb entry is clickable. On click:
- Set `_currentLocation = { unique: entry.unique, entityType: entry.entityType }` (or `undefined` if entry is the root ceiling with `unique: null`).
- Truncate `_breadcrumb` to that index.
- The `startNode` prop update triggers tree reload automatically.

#### Template
- Render `<uui-breadcrumbs>` above `<umb-tree>`. Always visible.
- Pass `_currentLocation` as `startNode` prop to `<umb-tree>`.
- Set `hide-toolbar="false"` on `<umb-tree>` so the view switcher appears.

**Acceptance**:
- Breadcrumb is always visible when the picker opens, showing at minimum the root or configured ceiling.
- Switching between Classic and Card views does not reset the breadcrumb or navigation position.
- Card view double-click updates the breadcrumb and rescopes the tree; names fill in asynchronously.
- Clicking a breadcrumb entry navigates back; both views show the correct subtree.
- Classic view with a drilled-in startNode shows that subtree in expanded form (no drill gesture, only breadcrumb for going back).

### Phase 8 — View selection persistence (UX enhancement)

**Deps**: Phase 3.

This phase is deferred — it is a UX enhancement that does not affect correctness. The approach needs further design: rather than consuming `UMB_INTERACTION_MEMORY_CONTEXT` directly in `UmbTreeViewManager`, the preferred direction is to introduce a dedicated interaction manager on the tree context itself (similar to how expansion state is owned by `UmbTreeExpansionManager`), keeping view persistence local to the tree without coupling `UmbTreeViewManager` to a specific memory provider.

**Open questions to resolve before implementing**:
- Should the tree context expose an `UmbInteractionManager`-like object, or should `UmbTreeViewManager` accept an optional persistence adapter?
- Which host isolation model ensures that picker and workspace trees do not share the same persisted view selection?
- How should this interact with the fallback (CLASSIC_FALLBACK) when no treeView manifests are registered?

**Acceptance**: switching view in a picker persists the selection for that picker session; switching back to Classic and re-opening the picker restores the Card view. Workspace trees do not share picker view selection.

### Phase 7 — Localization, tests, polish

**Deps**: Phases 3–6.

#### 7.1 Localization

Add to the appropriate dictionary file:
- `treeView_classic` = "Classic"
- `treeView_card` = "Cards"
- `treeView_cardEmptyRoot` = empty-state copy at root
- `treeView_cardEmptyAtLocation` = empty-state copy at drilled-in node

Reference keys via `#treeView_*` in manifests.

#### 7.2 Unit tests

- `tree-view.manager.test.ts` — views populate, default = highest weight, graceful when no manifests registered.
- `tree-view-bundle.element.test.ts` — hidden with 0/1 views; popover and click-to-change with 2+.
- `classic-tree-view.element.test.ts` — parity tests for classic render (coverage gap worth filling in this pass).
- `card-tree-view.element.test.ts`:
  - Renders `rootItems` from context (scoped by whatever `startNode` is set).
  - Double-click calls `treeContext.open({ unique, entityType })`.
  - Single-click toggles selection via context.
  - Keyboard: `Enter` dispatches selection/open; `Right Arrow` calls `treeContext.open()`.
  - Empty state when `rootItems` is empty.
  - No breadcrumb rendered inside the card view element.
- `tree-picker-modal` breadcrumb tests:
  - Breadcrumb shows root entry on open (no ceiling configured).
  - Breadcrumb shows ceiling entry on open (ceiling configured).
  - `UmbTreeItemOpenEvent` triggers optimistic `_currentLocation` update and async name fetch.
  - Breadcrumb click truncates array and updates `_currentLocation`.
  - Switching views does not reset `_currentLocation` or `_breadcrumb`.

#### 7.3 Manual smoke tests

- Content section sidebar renders identically to before (Classic view, no toolbar).
- Media section sidebar same.
- Open a document in the workspace; sidebar auto-expands to the document (existing behavior from `menu-tree-structure-workspace-context-base.ts`).
- Document picker modal: opens in Classic by default; switch to Card; drill into a node; breadcrumb appears under the header; navigate up via breadcrumb; pick an item; confirm.
- Verify `UmbDocumentTreeElement` and `UmbMediaTreeElement` subclasses continue to work without source changes.

---

## 3. Backwards compatibility

### 3.1 Public API surface

- **`<umb-default-tree>` attributes**: all existing attributes preserved. New attribute `hideToolbar` added with default `true` (no visual change to existing usages).
- **`UmbDefaultTreeElement` class**: `api`, `getSelection()`, `getExpansion()` preserved. Private render methods removed — safe, not public API.
- **`UmbTreeContext` interface**: new members (`hideTreeItemActions?`, optional setters/getters) added as **optional**. Non-breaking for external implementors.
- **Subclasses**: `UmbDocumentTreeElement`, `UmbMediaTreeElement` are empty-body `extends UmbDefaultTreeElement`. No source edits required. Verified.
- **`UmbTreeExpansionManager`**: `expandTo` was added in Phase 2 and is now removed (Phase 2 revised). It was never called outside its own test file — no external consumers to break.
- **Kind manifest `Umb.Kind.Tree.Default`**: element name + api class unchanged.

### 3.2 Visual regressions

- **None expected** for existing consumers. `hideToolbar = true` default means every existing `<umb-tree>` renders without a toolbar, preserving the current sidebar / menu appearance.
- Any consumer that wants the new view switcher explicitly sets `hide-toolbar="false"` (initially only the tree picker modal in Phase 6).

### 3.3 Deprecation

- No public API is being removed in v1. No `@deprecated` JSDoc or `UmbDeprecation` runtime warnings needed.

---

## 4. Risks and open items

### 4.1 Decided, but worth monitoring

- **`hideToolbar` default inconsistency**: `hideTreeRoot` and `hideTreeItemActions` default to `false`; `hideToolbar` defaults to `true`. Documented in JSDoc. Accepted trade-off for backwards compat.
- **Optimistic `startNode` update**: picker updates `_currentLocation` immediately on open event; breadcrumb names arrive async. There is a brief window where the tree is reloading but breadcrumb entries show `name` from the previous state. Accepted for responsiveness.
- **`includeSelf` on ancestors endpoint**: the backend `GetAncestors` method defaults to `includeSelf = true`. The picker relies on this to get the opened item's own name. If a custom tree repository overrides this behaviour, breadcrumb names may be missing. Documented assumption.

### 4.2 Implementation-time decisions

- **Empty-state copy**: final wording for `treeView_cardEmpty*` keys needs UX sign-off.
- **Card grid layout specifics**: column count, responsive breakpoints, card size — follow `<umb-collection-item-card>` styling where possible; otherwise inherit from uui-card defaults.
- **Interaction memory unique-key collisions** with multiple `<umb-tree>` instances on the same page: if two trees with the same `treeAlias` appear in the same memory context (rare), they share a view-selection key. If this becomes a problem, extend the key with an instance discriminator later. Not blocking v1.

### 4.3 Deferred (explicit v2+ scope)

- Table view.
- URL reflection / deep-linking via URL.
- Shared `<umb-tree-breadcrumb>` element (extract if/when workspace footer needs one).
- Variant-aware breadcrumb names via `DocumentItemDataResolver` (v1 uses raw `name` from tree item response).
- Discoverability affordance on cards (chevron on hover, tooltip, etc.).
- Filtering / search within a tree view.
- `treeHostContext` condition.
- Per-entity card items for documents/media/users (routing infrastructure ships in v1; specific implementations come later).
- Drag-and-drop reordering within card view.

---

## 5. Dependency graph

```
Phase 0 (read)
  └─► Phase 1 (extension types)
        ▼
      Phase 2 (remove expandTo — independent cleanup, any time)
      Phase 3 (shell + Classic)
        ▼
      Phase 4 (toolbar + bundle)
        ▼
      Phase 5 (open() on context + slim card view)
        ▼
      Phase 6 (picker breadcrumb)
        ▼
      Phase 7 (localization + tests + smoke)
        ▼
      Phase 8 (view selection persistence — UX enhancement)
```

Phase 2 is independent cleanup with no dependents. Phases 3 through 7 are sequential. Phase 8 can be tackled independently after Phase 3.

---

## 7. Progress tracker

Each phase requires review and sign-off before the next begins.

- [x] **Phase 0** — Pre-flight (read-only checks)
- [x] **Phase 1** — New extension types (`ManifestTreeView`, `ManifestTreeItemCard`)
- [ ] **Phase 2** — Remove `expandTo()` from expansion manager + tests (independent cleanup)
- [x] **Phase 3** — Extract Classic view + shell refactor (invasive)
  - [x] 3.1 — Optional `hideTreeItemActions` / `isMenu` on `UmbTreeContext`
  - [x] 3.2 — `<umb-classic-tree-view>` element + manifest
  - [x] 3.3 — `UmbTreeViewManager` + `UMB_TREE_VIEW_CONTEXT` token
  - [x] 3.4 — Convert `<umb-default-tree>` into a shell
  - [x] 3.5 — Register Classic view manifests
- [x] **Phase 4** — `<umb-tree-toolbar>` + `<umb-tree-view-bundle>` elements
- [ ] **Phase 5** — `open()` on tree context + card view refactor (slim)
  - [ ] 5.1 — Add `open()` to `UmbTreeContext` interface + `UmbDefaultTreeContext` implementation
  - [ ] 5.2 — Update `<umb-default-tree-item-card>` to call `treeContext.open()` (remove `UmbTreeItemCardNavigateEvent`)
  - [ ] 5.3 — `<umb-tree-item-card>` routing element — verify no `UmbTreeItemCardNavigateEvent` references
  - [ ] 5.4 — Slim down `<umb-card-tree-view>`: remove breadcrumb, expansion-based navigation, name cache
- [ ] **Phase 6** — Breadcrumb in `<umb-tree-picker-modal>`
- [ ] **Phase 7** — Localization, unit tests, manual smoke tests
- [ ] **Phase 8** — View selection persistence (UX enhancement — design TBD)

---

## 6. File inventory

### New files

```
src/packages/core/tree/view/tree-view.extension.ts
src/packages/core/tree/view/tree-view.manager.ts
src/packages/core/tree/view/tree-view.context.token.ts
src/packages/core/tree/view/types.ts
src/packages/core/tree/view/index.ts
src/packages/core/tree/view/classic/classic-tree-view.element.ts
src/packages/core/tree/view/classic/manifests.ts
src/packages/core/tree/view/classic/index.ts
src/packages/core/tree/view/classic/classic-tree-view.element.test.ts
src/packages/core/tree/view/card/card-tree-view.element.ts
src/packages/core/tree/view/card/manifests.ts
src/packages/core/tree/view/card/index.ts
src/packages/core/tree/view/card/card-tree-view.element.test.ts

src/packages/core/tree/tree-item-card/tree-item-card.extension.ts
src/packages/core/tree/tree-item-card/tree-item-card.element.ts
src/packages/core/tree/tree-item-card/index.ts
src/packages/core/tree/tree-item-card/default/default-tree-item-card.element.ts
src/packages/core/tree/tree-item-card/default/index.ts
(no events subfolder — UmbTreeItemCardNavigateEvent is not needed)

src/packages/core/tree/components/tree-toolbar.element.ts
src/packages/core/tree/components/tree-view-bundle.element.ts
src/packages/core/tree/components/tree-view-bundle.element.test.ts
src/packages/core/tree/components/index.ts

src/packages/core/tree/view/tree-view.manager.test.ts
```

### Edited files

```
src/packages/core/tree/index.ts                                   (re-exports)
src/packages/core/tree/manifests.ts                               (register Classic + Card view manifests)
src/packages/core/tree/tree.context.interface.ts                  (optional hideTreeItemActions, isMenu)
src/packages/core/tree/default/default-tree.context.ts            (implement new optional members)
src/packages/core/tree/default/default-tree.element.ts            (shell refactor — remove render methods, add hideToolbar, mount view manager)
src/packages/core/tree/tree.context.interface.ts                          (add open() method)
src/packages/core/tree/default/default-tree.context.ts                    (implement open())
src/packages/core/tree/expansion-manager/tree-expansion-manager.ts        (remove expandTo)
src/packages/core/tree/expansion-manager/tree-expansion-manager.test.ts   (remove expandTo tests)
src/packages/core/tree/tree-picker-modal/tree-picker-modal.element.ts    (breadcrumb render, hide-toolbar=false)
```

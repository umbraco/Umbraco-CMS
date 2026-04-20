# Tree Views — Implementation Plan

Introduce "views" for `<umb-tree>`, analogous to collection views. v1 ships two views (Classic + Card) and the surrounding infrastructure to register more later. Scope is the Umbraco backoffice client (`src/Umbraco.Web.UI.Client/`).

---

## 1. Design decisions (locked)

### 1.1 Scope

- **v1 views**: `Umb.TreeView.Classic` (the current expandable tree) + `Umb.TreeView.Card` (new, shows one level of children at a time with double-click to navigate into a child). Table view is explicitly deferred.
- **Consumers**: workspace-embedded trees, section sidebars, `<umb-tree-picker-modal>`.
- **No URL reflection.** Tree-view selection and drill position live in context state only. Persistence is runtime via `UmbInteractionMemoryManager`. Deep-linking is imperative (`expansion.expandTo(entity)`).

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

- **Reuse `UmbTreeExpansionManager`** (`src/packages/core/tree/expansion-manager/tree-expansion-manager.ts`). Convention: **last entry in expansion = current card-view location**.
- Classic view treats expansion as a multi-set; card view treats it as a linked chain. Same underlying array.
- New convenience method **`expansion.expandTo(entity, { repository, startNode? })`**: calls `requestTreeItemAncestors`, links entries via `linkEntityExpansionEntries`, calls `setExpansion(chain)`. Server-authoritative ancestry — no client-side chain inference.
- **On card view mount**, always call `expandTo(lastEntry)` when expansion is non-empty. This normalizes any classic-view expansion state (which may contain unrelated sibling expansions) into a valid breadcrumb chain. Cost: one `requestTreeItemAncestors` call per view switch. Benefit: zero stale-`target` edge cases.
- **`startNode`** unchanged. Remains mount-time scope. `expandTo` must respect it (chain starts at start-node, not above).

### 1.5 Interaction

- Single-click: existing behavior (picker → toggle selection; workspace → open workspace).
- **Double-click** on a card: navigate into that item.
- Keyboard: **Enter** = single-click action, **Right Arrow** = navigate into.
- No explicit drill affordance on cards in v1. Discoverability via behavior.

### 1.6 Breadcrumb

- **Not a shared element** in v1. Each consumer renders its own breadcrumb from tree context state.
  - `<umb-tree-picker-modal>` renders it below its header.
  - Workspace integration (later) renders it in the workspace footer.
- Breadcrumb walks the linked chain in the expansion array and renders clickable anchors that truncate the expansion to the clicked depth.

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

### Phase 2 — `expansion.expandTo(entity)` convenience method

**Deps**: Phase 0. **Parallelizable with Phase 1.**

**Edit**:
- `src/packages/core/tree/expansion-manager/tree-expansion-manager.ts` — add `async expandTo(entity, options: { repository, startNode? }): Promise<void>`. Fetches ancestors via `repository.requestTreeItemAncestors({ treeItem: entity })`, prepends `startNode` to the chain (if provided, chain starts there, not root), links via `linkEntityExpansionEntries` from `@umbraco-cms/backoffice/utils`, calls `setExpansion(chain)`. On repository error, leaves expansion unchanged.

**Edit**:
- `src/packages/core/tree/expansion-manager/tree-expansion-manager.test.ts` — add tests: happy path (ancestors `[A, B, C]`, target `D` → chain `[A→B→C→D]`), with `startNode`, target is root, repository error.

**Acceptance**: unit tests pass.

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

**Deps**: Phases 1, 2, 3, 4.

#### 5.1 Default card item element

**Create**:
- `src/packages/core/tree/tree-item-card/default/default-tree-item-card.element.ts` — `umb-default-tree-item-card`. Renders `<uui-card-content-node>` (or equivalent) with icon + name + `hasChildren` chevron. Fires `UmbSelectedEvent` on single-click (selection context) and a new `UmbTreeItemCardNavigateEvent` on double-click / Right Arrow / Enter-in-navigation-mode.
- `src/packages/core/tree/tree-item-card/default/index.ts` — barrel.
- `src/packages/core/tree/tree-item-card/events/tree-item-card-navigate.event.ts` — `UmbTreeItemCardNavigateEvent` carrying `{ entity: UmbEntityExpansionEntryModel }`.

#### 5.2 Card item routing element

**Create**:
- `src/packages/core/tree/tree-item-card/tree-item-card.element.ts` — `umb-tree-item-card`. Routes to the `entityType`-appropriate `treeItemCard` manifest. Fallback: `umb-default-tree-item-card`. Mirror the structure of `tree-item/tree-item.element.ts`. `getExtensionType()` returns `'treeItemCard'`; `getDefaultElementName()` returns `'umb-default-tree-item-card'`.

**Edit**:
- `src/packages/core/tree/tree-item-card/index.ts` — side-import and export the elements.

#### 5.3 Card view element

**Create**:
- `src/packages/core/tree/view/card/card-tree-view.element.ts` — `umb-card-tree-view`, extends `UmbLitElement`. Consumes `UMB_TREE_CONTEXT`. Observes `expansion`, `startNode`, `selection`. Derives current location = last entry in expansion.
  - **On mount**: if expansion is non-empty, call `treeContext.expansion.expandTo(lastEntry, { repository, startNode })` to normalize the chain (see §1.4). Then fetch children of the current location.
  - Children fetch: `undefined` location + no startNode → `repository.requestTreeRootItems({ paging })`. Otherwise `repository.requestTreeItemsOf({ parent: currentLocation, paging })`.
  - Render: grid of `<umb-tree-item-card>`, passing item and wiring handlers:
    - `@selected` / `@deselected` → `treeContext.selection.select / deselect`.
    - `@umb-tree-item-card-navigate` → `treeContext.expansion.expandTo(entity, { repository, startNode })`.
    - Keyboard on card: `Enter` dispatches selection (picker) or opens workspace (workspace), `Right Arrow` dispatches navigate.
  - Pagination: reuse existing `<umb-tree-load-more-button>` at the bottom of the grid, wired to `treeContext.targetPagination` (same as Classic view).
  - Empty state: `#treeView_cardEmptyAtLocation` / `#treeView_cardEmptyRoot`.
- `src/packages/core/tree/view/card/manifests.ts` — `{ type: 'treeView', alias: 'Umb.TreeView.Card', element: () => import('./card-tree-view.element.js'), weight: 900, meta: { label: '#treeView_card', icon: 'icon-grid' } }`.
- `src/packages/core/tree/view/card/index.ts` — barrel.

**Edit**:
- `src/packages/core/tree/manifests.ts` — spread Card manifests in.

**Acceptance**:
- Card view renders root items when no drill state.
- Double-click on a card navigates into the child; children render; previous level is reachable via breadcrumb (Phase 6).
- Single-click in picker toggles selection; single-click in workspace opens workspace.
- Keyboard: Enter and Right Arrow behave as specified.
- Empty state appears at drilled-in node with no children.

### Phase 6 — Breadcrumb in the picker modal

**Deps**: Phases 2, 5.

**Edit**:
- `src/packages/core/tree/tree-picker-modal/tree-picker-modal.element.ts` — below the header, render a breadcrumb row:
  - Reads `this._treeExpansion` (already observed, line 39).
  - Walks the linked chain (entries with `target` pointing forward).
  - Each entry (except the final) is a clickable anchor; on click, truncates expansion to-and-including that entry via `this._pickerContext.expansion.setExpansion(chainUpTo)`.
  - Final entry = current location, non-clickable.
  - Also set `hide-toolbar="false"` on the `<umb-tree>` inside the picker so the view switcher appears (line 199-218).

**Acceptance**: opening a picker in Card view shows a breadcrumb under the header; clicking a breadcrumb entry jumps back up the tree; opening in Classic shows no breadcrumb (expansion may be empty).

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

- `tree-expansion-manager.test.ts` — Phase 2 additions.
- `tree-view.manager.test.ts` — views populate, default = highest weight, memory read/write, graceful when no memory context.
- `tree-view-bundle.element.test.ts` — hidden with 0/1 views; popover and click-to-change with 2+.
- `classic-tree-view.element.test.ts` — parity tests copied/adapted from anything currently testing `<umb-default-tree>`'s render (none currently exists — this is a coverage gap worth filling in this pass).
- `card-tree-view.element.test.ts`:
  - Renders root items when expansion empty and no `startNode`.
  - Renders `startNode`'s children when `startNode` set and expansion empty.
  - Navigating into a card appends the entity to expansion and fetches its children.
  - Navigating up (breadcrumb click truncates expansion) refetches parent's children.
  - Single-click vs double-click disambiguation uses the DOM native `click` / `dblclick` distinction.
  - Keyboard: `Enter` dispatches selection/open; `Right Arrow` dispatches navigate.
  - Empty state at drilled-in node with no children.
  - On mount with existing expansion, calls `expandTo(lastEntry)` once to normalize.

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
- **`UmbTreeExpansionManager`**: `expandTo` is a new public method. Additive.
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
- **One extra `requestTreeItemAncestors` call per view switch**: accepted cost for server-authoritative breadcrumbs. Alternative (client-side chain inference) rejected as "magical".

### 4.2 Implementation-time decisions

- **Empty-state copy**: final wording for `treeView_cardEmpty*` keys needs UX sign-off.
- **Card grid layout specifics**: column count, responsive breakpoints, card size — follow `<umb-collection-item-card>` styling where possible; otherwise inherit from uui-card defaults.
- **Interaction memory unique-key collisions** with multiple `<umb-tree>` instances on the same page: if two trees with the same `treeAlias` appear in the same memory context (rare), they share a view-selection key. If this becomes a problem, extend the key with an instance discriminator later. Not blocking v1.

### 4.3 Deferred (explicit v2+ scope)

- Table view.
- URL reflection / deep-linking via URL.
- Shared `<umb-tree-breadcrumb>` element (extract if/when workspace footer needs one).
- Discoverability affordance on cards (chevron on hover, tooltip, etc.).
- Filtering / search within a tree view.
- `treeHostContext` condition.
- Per-entity card items for documents/media/users (routing infrastructure ships in v1; specific implementations come later).
- Drag-and-drop reordering within card view.

---

## 5. Dependency graph

```
Phase 0 (read)
  ├─► Phase 1 (extension types) ──┐
  └─► Phase 2 (expandTo)  ────────┤
                                   ▼
                                 Phase 3 (shell + Classic)
                                   ▼
                                 Phase 4 (toolbar + bundle)
                                   ▼
                                 Phase 5 (Card view + card-item routing)
                                   ▼
                                 Phase 6 (picker breadcrumb)
                                   ▼
                                 Phase 7 (localization + tests + smoke)
                                   ▼
                                 Phase 8 (view selection persistence — UX enhancement)
```

Phases 1 and 2 can run in parallel. Phases 3 through 7 are sequential. Phase 8 can be tackled independently after Phase 3.

---

## 7. Progress tracker

Each phase requires review and sign-off before the next begins.

- [x] **Phase 0** — Pre-flight (read-only checks)
- [x] **Phase 1** — New extension types (`ManifestTreeView`, `ManifestTreeItemCard`)
- [x] **Phase 2** — `expansion.expandTo()` convenience method + tests
  - ⚠️ **Needs revisit** — current implementation is not satisfactory. Revisit before Phase 5 and figure out an alternative approach before the card view depends on it.
- [x] **Phase 3** — Extract Classic view + shell refactor (invasive)
  - [x] 3.1 — Optional `hideTreeItemActions` / `isMenu` on `UmbTreeContext`
  - [x] 3.2 — `<umb-classic-tree-view>` element + manifest
  - [x] 3.3 — `UmbTreeViewManager` + `UMB_TREE_VIEW_CONTEXT` token
  - [x] 3.4 — Convert `<umb-default-tree>` into a shell
  - [x] 3.5 — Register Classic view manifests
- [ ] **Phase 4** — `<umb-tree-toolbar>` + `<umb-tree-view-bundle>` elements
- [ ] **Phase 5** — Card view + card-item routing
  - [ ] 5.1 — `<umb-default-tree-item-card>` + `UmbTreeItemCardNavigateEvent`
  - [ ] 5.2 — `<umb-tree-item-card>` routing element
  - [ ] 5.3 — `<umb-card-tree-view>` element + manifest
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
src/packages/core/tree/tree-item-card/events/tree-item-card-navigate.event.ts

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
src/packages/core/tree/expansion-manager/tree-expansion-manager.ts       (add expandTo)
src/packages/core/tree/expansion-manager/tree-expansion-manager.test.ts  (expandTo tests)
src/packages/core/tree/tree-picker-modal/tree-picker-modal.element.ts    (breadcrumb render, hide-toolbar=false)
```

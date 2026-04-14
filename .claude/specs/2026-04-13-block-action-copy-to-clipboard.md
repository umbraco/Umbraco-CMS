# Plan: Migrate Block Clipboard Copy to blockAction Extension

## Context

The `blockAction` extension type is now implemented and applied across all four block editors (List, Grid, RTE, Single). Delete is the first action registered via the extension registry. The next action to migrate is Copy to Clipboard.

Currently, each block entry element has a hardcoded `#renderCopyToClipboardAction()` button slotted into `<umb-block-action-list>`, with `#copyToClipboard()` logic inline in the element (except Block Grid, which already delegates to its entry context).

**Goal:** Move Copy to Clipboard from a slotted button to a registered `blockAction` extension, following the same pattern as Delete.

**Design spec:** `.claude/specs/2026-04-13-block-action-extension-type.md`

## Approach

**Move copy logic to each entry context, register a single blockAction.**

The Delete action works because `requestDelete()` exists on the base `UmbBlockEntryContext`. For clipboard copy, Block Grid already has `copyToClipboard()` on its context — extend that pattern to List, RTE, and Single. Then register a single `blockAction` that calls `context.copyToClipboard()`.

Each sub-context keeps its editor-specific logic (value model types, schema aliases, Grid's nested block recursion, RTE's dynamic editor UI alias).

## Key Differences Between Editors

| Aspect | List | Grid | RTE | Single |
|--------|------|------|-----|--------|
| Copy logic location | Element | Context (done) | Element | Element |
| Nested block collection | No | Yes (recursive) | No | No |
| Conditionally shown | No | No | Yes (clipboard ctx) | No |
| EditorUI alias | Hardcoded const | Hardcoded const | Dynamic from manifest | Hardcoded const |

## Steps

### 1. Add `copyToClipboard()` to Block List entry context

**Modify:** `src/packages/block/block-list/context/block-list-entry.context.ts`

Move the `#copyToClipboard()` logic from `block-list-entry.element.ts` into the context as a public `async copyToClipboard()` method. The context already has access to `getContent()`, `getLayout()`, `getSettings()`, `getExpose()`, `getName()` from the base class. It needs to consume the three additional contexts (`UMB_PROPERTY_DATASET_CONTEXT`, `UMB_PROPERTY_CONTEXT`, `UMB_CLIPBOARD_PROPERTY_CONTEXT`) via `this.getContext()`.

Use the existing Block Grid context method (`block-grid-entry.context.ts:300-362`) as the reference pattern.

### 2. Add `copyToClipboard()` to Block RTE entry context

**Modify:** `src/packages/block/block-rte/context/block-rte-entry.context.ts`

Same pattern as Step 1. Note the RTE-specific difference: it gets the `propertyEditorUiAlias` dynamically from `propertyContext.getEditorManifest()?.alias` instead of a hardcoded constant.

### 3. Add `copyToClipboard()` to Block Single entry context

**Modify:** `src/packages/block/block-single/context/block-single-entry.context.ts`

Same pattern as Step 1, using `UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS` and `UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS`.

### 4. Create `readOnlyVisible` kind

**New files** in `src/packages/block/block/action/read-only-visible/`:

- `read-only-visible.action.kind.ts` — kind manifest: `alias: 'Umb.Kind.BlockAction.ReadOnlyVisible'`, `matchKind: 'readOnlyVisible'`, `matchType: 'blockAction'`. Provides element `() => import('./block-action-read-only-visible.element.js')`, same meta shape as default kind.
- `block-action-read-only-visible.element.ts` — identical to `default/block-action.element.ts` but **without** the `UMB_BLOCK_ENTRY_CONTEXT` read-only guard. Always renders the button regardless of read-only state.
- `types.ts` — `ManifestBlockActionReadOnlyVisibleKind` extending `ManifestBlockAction<MetaBlockActionDefaultKind>` with `kind: 'readOnlyVisible'`
- `manifests.ts` — exports the kind manifest

**Pattern source:** `src/packages/block/block/action/default/`

### 5. Create Copy to Clipboard blockAction

**New files** in `src/packages/block/block/action/common/copy-to-clipboard/`:

- `copy-to-clipboard-block.action.ts` — `UmbCopyToClipboardBlockAction` extending `UmbBlockActionBase<MetaBlockActionDefaultKind>`. Consumes `UMB_BLOCK_ENTRY_CONTEXT`, calls `context.copyToClipboard()` in `execute()`. Pattern identical to `delete-block.action.ts`.
- `manifests.ts` — manifest with `type: 'blockAction'`, `kind: 'readOnlyVisible'`, `alias: 'Umb.BlockAction.CopyToClipboard'`, `weight: 200` (higher than Delete's 100 so it renders before Delete), `meta: { icon: 'icon-clipboard-copy', label: '#clipboard_labelForCopyToClipboard' }`
- `constants.ts` — `UMB_BLOCK_ACTION_COPY_TO_CLIPBOARD_ALIAS`

**Note:** No `forBlockEditor` filter — applies to all block editors.

### 6. Wire up manifests and exports

**Modify:** `src/packages/block/block/action/manifests.ts` — add read-only-visible kind + copy-to-clipboard manifests
**Modify:** `src/packages/block/block/action/index.ts` — add copy-to-clipboard constants export + read-only-visible types export

### 7. Remove slotted copy action from all four entry elements

**Modify each entry element** — remove `#renderCopyToClipboardAction()` method and its call from the `<umb-block-action-list>` slot:

- `src/packages/block/block-list/components/block-list-entry/block-list-entry.element.ts`
  - Remove `#renderCopyToClipboardAction()` method
  - Remove `#copyToClipboard()` method
  - Remove `${this.#renderCopyToClipboardAction()}` from `#renderActionBar()`
  - Remove now-unused imports (`UMB_CLIPBOARD_PROPERTY_CONTEXT`, `UMB_PROPERTY_CONTEXT`, `UMB_PROPERTY_DATASET_CONTEXT`, `UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS`, `UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS` — verify each is not used elsewhere first)

- `src/packages/block/block-grid/components/block-grid-entry/block-grid-entry.element.ts`
  - Remove `#renderCopyToClipboardAction()` method
  - Remove `${this.#renderCopyToClipboardAction()}` from `#renderActionBar()`
  - Note: `#copyToClipboard()` doesn't exist on the element (already on context) — nothing else to remove

- `src/packages/block/block-rte/components/block-rte-entry/block-rte-entry.element.ts`
  - Remove `#renderCopyToClipboardAction()` method
  - Remove `#copyToClipboard()` method
  - Remove `${this.#renderCopyToClipboardAction()}` from `#renderActionBar()`
  - Remove `#clipboardContext` member and its `consumeContext` in constructor
  - Remove now-unused imports

- `src/packages/block/block-single/components/block-single-entry/block-single-entry.element.ts`
  - Remove `#renderCopyToClipboardAction()` method
  - Remove `#copyToClipboard()` method
  - Remove `${this.#renderCopyToClipboardAction()}` from `#renderActionBar()`
  - Remove now-unused imports

### 8. Handle RTE conditional visibility

The RTE currently hides the copy button when `#clipboardContext` is unavailable. With the action on the extension registry, this needs a different approach.

**Option:** The `copyToClipboard()` method on the RTE context should gracefully handle a missing clipboard context (early return with console.warn, matching its current behavior). The button will always render but the execute will no-op. This matches Block List/Single/Grid behavior where the button always renders.

If the clipboard context is truly never available in certain RTE scenarios, we could add a block condition for this in future — but for now, a graceful no-op is sufficient.

## Verification

1. `npm run build` — no build errors
2. `npm run check:circular` — no new circular dependencies
3. Const export test — `npm run generate:check-const-test && npx web-test-runner src/export-consts.test.ts`
4. Visual: Copy to Clipboard button appears in same position (before Delete) across all four editors
5. Functional: Clicking copy writes to clipboard correctly in all four editors
6. Block Grid: Copying a block with nested blocks includes all nested block data
7. Sort mode: Action bar (including copy) hidden during sort
8. Read-only: Copy button is still visible in read-only mode (uses `readOnlyVisible` kind), Delete button is hidden (uses `default` kind)

## Decision: Read-Only Behavior

**Decided:** Create a new kind `readOnlyVisible` that does not hide the button when read-only. The Copy action uses this kind; Delete keeps using `default` (which hides on read-only).

## Critical Files

| File | Role |
|------|------|
| `src/packages/block/block-list/context/block-list-entry.context.ts` | Add `copyToClipboard()` |
| `src/packages/block/block-rte/context/block-rte-entry.context.ts` | Add `copyToClipboard()` |
| `src/packages/block/block-single/context/block-single-entry.context.ts` | Add `copyToClipboard()` |
| `src/packages/block/block-grid/components/block-grid-entry/block-grid-entry.context.ts` | Reference — already has `copyToClipboard()` |
| `src/packages/block/block/action/common/copy-to-clipboard/` (new) | New blockAction files |
| `src/packages/block/block/action/read-only-visible/` (new) | New kind — button visible in read-only |
| `src/packages/block/block/action/manifests.ts` | Wire up new manifests |
| All four entry elements | Remove slotted copy button + inline copy logic |

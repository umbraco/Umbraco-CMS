# Plan: Stage 2 — Migrate Edit Content to blockAction Extension

## Context

Stage 1 (Edit Settings) is complete. The pattern is proven: use the `default` kind with an API class that provides `getHref()` and `getValidationDataPath()`, controlled by manifest conditions.

Edit Content is more complex because it has two mutually exclusive visual states:
1. **Edit button** — navigates to content workspace (when `showContentEdit && workspaceEditContentPath`)
2. **Expose button** — calls `context.expose()` with "Create this for..." label (when `showContentEdit === false && exposed === false`)

## Approach: Two Separate blockAction Extensions

Rather than a custom kind that handles both states, register two `blockAction` extensions with mutually exclusive conditions:

**`Umb.BlockAction.EditContent`** — uses `default` kind:
- Condition: `Umb.Condition.BlockEntryShowContentEdit` + `Umb.Condition.BlockEntryIsReadOnly` match: false
- API: `getHref()` returns `workspaceEditContentPath`, `getValidationDataPath()` returns content validation path
- Icon: `icon-edit` (or `icon-add` when not exposed — needs the API or element to handle this)

**`Umb.BlockAction.ExposeContent`** — uses `default` kind:
- Condition: `Umb.Condition.BlockEntryShowContentEdit` match: false + `Umb.Condition.BlockEntryIsExposed` match: false + `Umb.Condition.BlockEntryIsReadOnly` match: false
- API: `execute()` calls `context.expose()`
- Meta: `icon: 'icon-add'`, label: dynamic "Create this for {contentTypeName}"

### Decisions

- **Edit Content icon**: Always `icon-edit`. The Expose action handles the `icon-add` case.
- **Expose label**: Use a static localization key (`#blockEditor_createThisFor`). Loses the dynamic content type name suffix but keeps things simple.
- **`BlockEntryShowContentEdit` condition**: Needs `match?: boolean` added so the Expose action can use `match: false`.

### Existing Conditions

- `Umb.Condition.BlockEntryShowContentEdit` — already exists, uses `UMB_BLOCK_ENTRY_CONTEXT.showContentEdit`. **Needs `match` support** for the expose action to use `match: false`.
- `Umb.Condition.BlockEntryIsReadOnly` — already exists (we created it)
- `Umb.Condition.BlockWorkspaceIsExposed` — uses `UMB_BLOCK_WORKSPACE_CONTEXT` (wrong level). We need an **entry-level** version: `Umb.Condition.BlockEntryIsExposed`

## Steps

### 1. Add `match` support to BlockEntryShowContentEdit condition

**Modify:** `src/packages/block/block/conditions/block-entry-show-content-edit.condition.ts`
- Add `match` comparison: `this.permitted = showContentEdit === (this.config.match !== undefined ? this.config.match : true)`

**Modify:** `src/packages/block/block/conditions/types.ts`
- Change `BlockEntryShowContentEditConditionConfig` from `type` to `interface` with `match?: boolean`

### 2. Create Block Entry Is Exposed condition

**New file:** `src/packages/block/block/conditions/block-entry-is-exposed.condition.ts`
- Consumes `UMB_BLOCK_ENTRY_CONTEXT`, observes `hasExpose` (the exposed state)
- Supports `match?: boolean` (default true)

**Modify:** `conditions/types.ts`, `conditions/manifests.ts`, `conditions/constants.ts`

### 3. Create Edit Content blockAction API class

**New files** in `src/packages/block/block/action/common/edit-content/`:
- `edit-content-block.action.ts` — extends `UmbBlockActionBase`:
  - Consumes `UMB_BLOCK_ENTRY_CONTEXT`
  - `getHref()`: awaits context, returns `workspaceEditContentPath` via `observe().asPromise()`
  - `getValidationDataPath()`: returns `$.contentData[${UmbDataPathBlockElementDataQuery({ key: contentKey })}]`
- `manifests.ts`:
  ```
  type: 'blockAction', kind: 'default',
  alias: 'Umb.BlockAction.EditContent', weight: 400,
  meta: { icon: 'icon-edit', label: '#general_edit' },
  conditions: [
    { alias: 'Umb.Condition.BlockEntryShowContentEdit' },
    { alias: 'Umb.Condition.BlockEntryIsReadOnly', match: false },
  ],
  ```
- `constants.ts`

### 4. Create Expose Content blockAction API class

**New files** in `src/packages/block/block/action/common/expose-content/`:
- `expose-content-block.action.ts` — extends `UmbBlockActionBase`:
  - Consumes `UMB_BLOCK_ENTRY_CONTEXT`
  - `execute()`: calls `context.expose()`
- `manifests.ts`:
  ```
  type: 'blockAction', kind: 'default',
  alias: 'Umb.BlockAction.ExposeContent', weight: 400,
  meta: { icon: 'icon-add', label: '#blockEditor_createThisFor' },
  conditions: [
    { alias: 'Umb.Condition.BlockEntryShowContentEdit', match: false },
    { alias: 'Umb.Condition.BlockEntryIsExposed', match: false },
    { alias: 'Umb.Condition.BlockEntryIsReadOnly', match: false },
  ],
  ```
- `constants.ts`

Note: `#blockEditor_createThisFor` will render without the content type name parameter. This is a minor UX simplification.

### 5. Wire up manifests and exports

**Modify:** `action/manifests.ts` — add edit-content + expose-content manifests
**Modify:** `action/index.ts` — add new constants exports

### 6. Remove slotted Edit Content from all four entry elements

Remove `#renderEditContentAction()` / `#renderEditAction()` and `#expose()` from all four elements. Remove their calls from `<umb-block-action-list>` slot.

After this step the slot is empty — remove `<slot></slot>` from `<umb-block-action-list>`.

### 7. Clean up

Remove unused state/observers from entry elements. Verify which are still needed for blockViewProps and reflected attributes before removing.

## Verification

1. `npm run build` — no errors
2. `npm run check:circular` — clean
3. Const export test passes
4. Edit Content button: navigates to content workspace, shows validation badge
5. Expose button: "Create this for..." appears when not exposed, clicking exposes block
6. Read-only: Both buttons hidden
7. Inline editing mode: Edit Content hidden when `showContentEdit` is false
8. `<slot>` removed from `<umb-block-action-list>`

## Critical Files

| File | Role |
|------|------|
| `src/packages/block/block/conditions/` | New BlockEntryIsExposed condition |
| `src/packages/block/block/action/common/edit-content/` (new) | Edit Content API + manifest |
| `src/packages/block/block/action/common/expose-content/` (new) | Expose Content API + manifest |
| `src/packages/block/block/action/block-action-list.element.ts` | Remove `<slot>` |
| All four entry elements | Remove edit content render + expose method |

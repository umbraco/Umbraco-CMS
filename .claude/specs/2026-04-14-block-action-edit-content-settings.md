# Plan: Migrate Edit Content & Edit Settings to blockAction Extensions

## Context

Following the successful migration of Delete and Copy to Clipboard, this plan migrates the remaining two hardcoded action buttons — Edit Content and Edit Settings — to `blockAction` extensions. This will complete the migration, allowing the `<slot>` in `<umb-block-action-list>` to be removed entirely.

**Design spec:** `.claude/specs/2026-04-13-block-action-extension-type.md`

## Analysis: Current Implementations

### Edit Content Button — 3 Visual States

The Edit Content button has complex conditional rendering across all four editors:

| State | Condition | Renders |
|-------|-----------|---------|
| **Edit** | `showContentEdit && workspaceEditContentPath` | `<uui-button href=...>` with edit/add icon + optional invalid badge |
| **Expose** | `showContentEdit === false && exposed === false` | `<uui-button @click=expose>` "Create this for..." |
| **Hidden** | `isReadOnly` OR none of above | `nothing` |

### Edit Settings Button — Simpler

| State | Condition | Renders |
|-------|-----------|---------|
| **Edit** | `hasSettings && workspaceEditSettingsPath` | `<uui-button href=...>` with settings icon + optional invalid badge |
| **Hidden** | `isReadOnly` OR no settings | `nothing` |

### Differences Between Editors

| Aspect | List | Grid | RTE | Single |
|--------|------|------|-----|--------|
| `showContentEdit` checks `inlineEditingMode` | Yes | No | No | Yes |
| Content invalid color | `invalid` | `danger` | `invalid` | `invalid` |
| Icon logic checks `isReadOnly` | Yes (redundant) | No | No | Yes (redundant) |
| Has `title` attribute on buttons | Yes | Yes | No | No |

The `danger` vs `invalid` color difference in Block Grid appears to be an inconsistency rather than intentional. The `isReadOnly` check in the icon logic is redundant since the method already early-returns when read-only.

### Key Observables (all on base UmbBlockEntryContext)

- `showContentEdit` — computed per-editor subclass (merged observable)
- `workspaceEditContentPath` / `workspaceEditSettingsPath` — from base context
- `contentInvalid` / `settingsInvalid` — from validation controllers on the element
- `hasExpose` (exposed) — from base context
- `hasSettings` — derived from `settingsElementTypeKey` observable (truthy check) on base context
- `readOnlyGuard.permitted` — from base context

## Challenge: Element-Level State

Unlike Delete and Copy to Clipboard (which delegate to context methods), Edit Content and Edit Settings use:

1. **`href` navigation** — the button's `href` links to the workspace edit path
2. **Validation state** (`contentInvalid`, `settingsInvalid`) — observed from `UmbObserveValidationStateController` on the **element**, not the context
3. **Expose click handler** — calls `context.expose()`

The `blockAction` API has `getHref()` and `execute()` — href navigation maps directly to `getHref()`. But the validation badge and expose alternate button don't fit the current default kind element, which renders a simple `<uui-button>` with an icon.

## Approach

### New Kinds for Complex Rendering

Create two new kinds that handle the specific rendering needs:

**`editSettings` kind** — A custom element that:
- Observes `settingsElementTypeKey`, `workspaceEditSettingsPath`, `readOnlyGuard.permitted` from `UMB_BLOCK_ENTRY_CONTEXT`
- Creates its own `UmbObserveValidationStateController` for settings validation
- Renders the settings button with `href`, icon, invalid badge
- OR renders nothing (when no settings or hidden)
- Hides when read-only

**`editContent` kind** — A custom element that:
- Observes `showContentEdit`, `workspaceEditContentPath`, `hasExpose` (exposed), `readOnlyGuard.permitted`, `contentElementTypeName` from `UMB_BLOCK_ENTRY_CONTEXT`
- Creates its own `UmbObserveValidationStateController` for content validation
- Renders the edit button with `href`, dynamic icon (`icon-add`/`icon-edit`), invalid badge
- OR renders the expose "Create this for..." button
- OR renders nothing (when hidden)
- Hides when read-only

These kinds don't need an API class — all the logic is in the element, consuming the block entry context directly. The manifest can omit the `api` property.

### Normalise Inconsistencies

- Use `invalid` color consistently (not `danger` for Grid)
- Add `title` attribute consistently across all editors
- Remove the redundant `isReadOnly` check in the icon logic

---

## Stage 1: Edit Settings

The simpler action — tackle first to prove the kind-based approach works.

### 1.1 Create `editSettings` kind

**New files** in `src/packages/block/block/action/edit-settings/`:

- `edit-settings.action.kind.ts` — kind manifest: `alias: 'Umb.Kind.BlockAction.EditSettings'`, `matchKind: 'editSettings'`, `matchType: 'blockAction'`
- `block-action-edit-settings.element.ts` — custom element that consumes `UMB_BLOCK_ENTRY_CONTEXT`. Observes: `settingsElementTypeKey` (truthy = has settings), `workspaceEditSettingsPath`, `readOnlyGuard.permitted`. Creates `UmbObserveValidationStateController` for `settingsInvalid`. Renders settings button with href, `icon-settings`, invalid badge — or nothing.
- `types.ts` — `ManifestBlockActionEditSettingsKind`
- `manifests.ts`

### 1.2 Register Edit Settings blockAction

**New files** in `src/packages/block/block/action/common/edit-settings/`:
- `manifests.ts` — `type: 'blockAction'`, `kind: 'editSettings'`, `alias: 'Umb.BlockAction.EditSettings'`, `weight: 300`
- `constants.ts` — `UMB_BLOCK_ACTION_EDIT_SETTINGS_ALIAS`

### 1.3 Wire up manifests and exports

**Modify:** `src/packages/block/block/action/manifests.ts` — add editSettings kind + action manifests
**Modify:** `src/packages/block/block/action/index.ts` — add new exports

### 1.4 Remove slotted Edit Settings from all four entry elements

Remove `#renderEditSettingsAction()` from all four entry elements and its call from the `<umb-block-action-list>` slot. Clean up now-unused state/observers:
- `_hasSettings` state and `settingsElementTypeKey` observer
- `_settingsInvalid` state and its `UmbObserveValidationStateController`
- `_workspaceEditSettingsPath` state and observer (verify not used elsewhere — e.g., custom view props)

**Note:** `_settingsInvalid` may still be used for the `settings-invalid` reflected attribute on the host element (for CSS `:host([settings-invalid])` styling). If so, keep the validation observer but remove the render method.

### 1.5 Stage 1 verification

1. `npm run build` — no errors
2. `npm run check:circular` — clean
3. Const export test passes
4. Visual: Edit Settings button appears in same position with settings icon
5. Functional: Clicking navigates to workspace settings view
6. Validation: Invalid badge appears when settings fail validation
7. Read-only: Button hidden when read-only
8. No settings: Button hidden when block has no settings type

---

## Stage 2: Edit Content

The complex action — the expose button alternate state adds significant rendering logic.

### 2.1 Create `editContent` kind

**New files** in `src/packages/block/block/action/edit-content/`:

- `edit-content.action.kind.ts` — kind manifest: `alias: 'Umb.Kind.BlockAction.EditContent'`, `matchKind: 'editContent'`, `matchType: 'blockAction'`
- `block-action-edit-content.element.ts` — custom element that consumes `UMB_BLOCK_ENTRY_CONTEXT`. Observes: `showContentEdit`, `workspaceEditContentPath`, `hasExpose` (exposed), `readOnlyGuard.permitted`, `contentElementTypeName`. Creates `UmbObserveValidationStateController` for `contentInvalid`. Renders:
  - Edit button (href, icon-add/icon-edit, invalid badge) when `showContentEdit && workspaceEditContentPath`
  - Expose button ("Create this for...", calls `context.expose()`) when `showContentEdit === false && exposed === false`
  - Nothing when read-only or none of above
- `types.ts` — `ManifestBlockActionEditContentKind`
- `manifests.ts`

### 2.2 Register Edit Content blockAction

**New files** in `src/packages/block/block/action/common/edit-content/`:
- `manifests.ts` — `type: 'blockAction'`, `kind: 'editContent'`, `alias: 'Umb.BlockAction.EditContent'`, `weight: 400` (highest — leftmost)
- `constants.ts` — `UMB_BLOCK_ACTION_EDIT_CONTENT_ALIAS`

### 2.3 Wire up manifests and exports

**Modify:** `src/packages/block/block/action/manifests.ts` — add editContent kind + action manifests
**Modify:** `src/packages/block/block/action/index.ts` — add new exports

### 2.4 Remove slotted Edit Content from all four entry elements

Remove `#renderEditContentAction()` / `#renderEditAction()` from all four entry elements and its call from the `<umb-block-action-list>` slot. Remove the `#expose` method. Clean up now-unused state/observers:
- `_showContentEdit` state and observer
- `_workspaceEditContentPath` state and observer (verify not used elsewhere)
- `_exposed` state and observer
- `_contentTypeName` state and observer
- `_contentInvalid` state and its `UmbObserveValidationStateController`

**Note:** Same caution as Stage 1 — `_contentInvalid` may be used for the `content-invalid` reflected attribute and for the badge rendered outside the action bar. Verify before removing.

### 2.5 Remove `<slot>` from `<umb-block-action-list>`

After both stages, the slot is empty across all editors. Remove `<slot></slot>` from the list element template.

### 2.6 Stage 2 verification

1. `npm run build` — no errors
2. `npm run check:circular` — clean
3. Const export test passes
4. Visual: Edit Content button appears in same position with correct icon
5. Functional: Clicking navigates to workspace content view
6. Expose: "Create this for..." button appears when block not exposed
7. Validation: Invalid badge appears when content fails validation
8. Icon: Shows `icon-add` when not exposed, `icon-edit` when exposed
9. Read-only: Button hidden when read-only
10. Slot removed: `<umb-block-action-list>` no longer renders `<slot>`

---

## Resolved Questions

1. **Validation state**: The kind elements create their own `UmbObserveValidationStateController` instances, using `contentKey` from the entry context to build the data path query. Same pattern as the current entry elements.

2. **`hasSettings`**: Available on the base context via `settingsElementTypeKey` observable (truthy = has settings). No need to add a new method.

3. **`showContentEdit` differences**: List/Single check `inlineEditingMode`, Grid/RTE don't. This is already handled by each subclass's own `showContentEdit` merged observable — the kind element just observes it from the context. No special logic needed.

4. **Block Grid `danger` color**: **Decided: normalise to `invalid`** across all editors. The `danger` in Grid was an inconsistency.

## Critical Files

| File | Role |
|------|------|
| `src/packages/block/block/action/edit-settings/` (new) | editSettings kind (Stage 1) |
| `src/packages/block/block/action/common/edit-settings/` (new) | Edit Settings action manifest (Stage 1) |
| `src/packages/block/block/action/edit-content/` (new) | editContent kind (Stage 2) |
| `src/packages/block/block/action/common/edit-content/` (new) | Edit Content action manifest (Stage 2) |
| `src/packages/block/block/action/block-action-list.element.ts` | Remove `<slot>` (Stage 2) |
| All four entry elements | Remove edit/settings render methods + cleanup |
| `src/packages/block/block/context/block-entry.context.ts` | Reference — has all needed observables |

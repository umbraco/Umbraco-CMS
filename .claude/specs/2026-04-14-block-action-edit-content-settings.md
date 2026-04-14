# Revised Plan: Migrate Edit Settings to blockAction (using default kind)

## Context

Edit Settings uses the `default` kind with an API class that returns the workspace path via `getHref()` and a validation data path via `getValidationDataPath()`. Visibility is controlled by manifest conditions. The default kind element is extended to support a validation badge driven by the API.

## Decisions

- **Kind**: Use `default` kind, not a custom `editSettings` kind
- **Validation badge**: API class provides `getValidationDataPath()`, default kind element creates the validation controller and renders the badge
- **Visibility**: Manifest conditions control when the action is shown (has settings, not read-only)

## Steps

### 1. Remove the `editSettings` kind

Delete `src/packages/block/block/action/edit-settings/` directory. Remove references from `action/manifests.ts` and `action/index.ts`.

### 2. Extend UmbBlockAction interface with getValidationDataPath()

**Modify:** `src/packages/block/block/action/block-action.interface.ts`
- Add `getValidationDataPath(): Promise<string | undefined>` to the `UmbBlockAction` interface

**Modify:** `src/packages/block/block/action/block-action-base.ts`
- Add default implementation returning `Promise.resolve(undefined)`

### 3. Add validation badge support to default kind element

**Modify:** `src/packages/block/block/action/default/block-action.element.ts`

After the API is set (in the `api` setter), call `api.getValidationDataPath()`. If it returns a path:
- Create a `UmbObserveValidationStateController` with that path
- Track `_invalid` state
- Render the button `color` as `'invalid'` when invalid
- Render `<uui-badge attention color="invalid" label="...">!</uui-badge>` inside the button when invalid

This requires importing `UmbObserveValidationStateController` and `UmbDataPathBlockElementDataQuery` (if needed — but the API returns the full path, so no query construction in the element).

### 4. Create Block Entry Has Settings condition

**New file:** `src/packages/block/block/conditions/block-entry-has-settings.condition.ts`
- Consumes `UMB_BLOCK_ENTRY_CONTEXT`, observes `settingsElementTypeKey`
- `this.permitted = !!settingsElementTypeKey`

**Modify:** `src/packages/block/block/conditions/types.ts` — add config type
**Modify:** `src/packages/block/block/conditions/manifests.ts` — register condition
**Modify:** `src/packages/block/block/conditions/constants.ts` — add alias constant

### 5. Create Edit Settings blockAction API class

**New files** in `src/packages/block/block/action/common/edit-settings/`:

- `edit-settings-block.action.ts` — extends `UmbBlockActionBase<MetaBlockActionDefaultKind>`:
  - Consumes `UMB_BLOCK_ENTRY_CONTEXT`
  - `getHref()`: returns `context.workspaceEditSettingsPath` value (observed reactively or fetched)
  - `getValidationDataPath()`: returns `$.settingsData[${UmbDataPathBlockElementDataQuery({ key: settingsKey })}]` using `context.settingsKey`
- `manifests.ts`:
  ```
  type: 'blockAction',
  kind: 'default',
  alias: 'Umb.BlockAction.EditSettings',
  weight: 300,
  api: () => import('./edit-settings-block.action.js'),
  meta: { icon: 'icon-settings', label: '#general_settings' },
  conditions: [
    { alias: 'Umb.Condition.BlockEntryIsReadOnly', match: false },
    { alias: 'Umb.Condition.BlockEntryHasSettings' },
  ],
  ```
- `constants.ts` — `UMB_BLOCK_ACTION_EDIT_SETTINGS_ALIAS`

### 6. Wire up manifests and exports

**Modify:** `src/packages/block/block/action/manifests.ts` — add edit-settings action manifests
**Modify:** `src/packages/block/block/action/index.ts` — add edit-settings constants export

### 7. Remove slotted Edit Settings from all four entry elements

Remove `#renderEditSettingsAction()` and its call from each entry element's `#renderActionBar()`. Keep `_settingsInvalid`, `_hasSettings`, and settings path observers where they're used for blockViewProps or reflected attributes.

## Note on getHref() reactivity

The current default kind element calls `api.getHref()` once in the `api` setter. For Edit Settings, the workspace path is resolved asynchronously from the context. The API class needs to either:
- Return the path once it's available (the element calls getHref() after API construction, which may be before the context resolves)
- Or the element needs to re-call getHref() when the API signals a change

Looking at how the entity action default element handles this — it also calls `getHref()` once. If the path isn't available yet, it returns undefined and the button renders without href. This may need the API to notify when the href changes, or the element to poll/observe. For the initial implementation, the context should resolve quickly enough that the path is available by the time the user sees the button.

If this proves unreliable, we can add an observable `href` property to the API later.

## Verification

1. `npm run build` — no errors
2. `npm run check:circular` — clean
3. Const export test passes
4. Visual: Edit Settings button in same position with settings icon
5. Functional: Clicking navigates to workspace settings view
6. Validation: Invalid badge appears when settings fail validation
7. Read-only: Button hidden when read-only (via condition)
8. No settings: Button hidden when block has no settings type (via condition)

## Critical Files

| File | Role |
|------|------|
| `src/packages/block/block/action/edit-settings/` | DELETE (replacing with default kind approach) |
| `src/packages/block/block/action/block-action.interface.ts` | Add getValidationDataPath() |
| `src/packages/block/block/action/block-action-base.ts` | Add default getValidationDataPath() |
| `src/packages/block/block/action/default/block-action.element.ts` | Add validation badge support |
| `src/packages/block/block/conditions/` | New BlockEntryHasSettings condition |
| `src/packages/block/block/action/common/edit-settings/` | New API class + manifests |
| All four entry elements | Remove #renderEditSettingsAction() |

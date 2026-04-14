# Plan: Replace showOnReadOnly with Block Entry Read-Only Condition

## Context

The default block action kind element currently has an inline read-only guard that hides the button when `readOnlyGuard.permitted` is true. A `showOnReadOnly` meta flag was added so Copy to Clipboard could opt out. 

This approach is non-standard — conditions are the established Umbraco pattern for controlling extension visibility. The `entityAction` extensions use conditions for state checks (e.g., `Umb.Condition.EntityIsNotTrashed`). We should follow the same pattern.

An existing `Umb.Condition.BlockWorkspaceIsReadOnly` condition exists but uses `UMB_BLOCK_WORKSPACE_CONTEXT` (only available inside the block editor modal). We need a new condition that uses `UMB_BLOCK_ENTRY_CONTEXT` (where block actions render).

## What Changes

1. Create a new condition: `Umb.Condition.BlockEntryIsReadOnly`
2. Remove the read-only guard from the default kind element
3. Remove `showOnReadOnly` from `MetaBlockActionDefaultKind`
4. Add `{ alias: 'Umb.Condition.BlockEntryIsReadOnly', match: false }` to the Delete manifest
5. Copy to Clipboard manifest stays as-is (no condition = always visible)

**After this change:**
- Delete: has `match: false` condition → hidden when read-only
- Copy to Clipboard: no read-only condition → always visible
- 3rd-party actions: explicitly add the condition if they need read-only gating

## Steps

### 1. Create Block Entry Is Read-Only condition

**New files** in `src/packages/block/block/conditions/block-entry-is-read-only/`:

- `constants.ts`:
  ```typescript
  export const UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS = 'Umb.Condition.BlockEntryIsReadOnly';
  ```

- `types.ts`:
  ```typescript
  export interface BlockEntryIsReadOnlyConditionConfig
    extends UmbConditionConfigBase<typeof UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS> {
    match?: boolean;
  }
  ```
  Plus `UmbExtensionConditionConfigMap` declaration.

- `block-entry-is-read-only.condition.ts`:
  ```typescript
  export class UmbBlockEntryIsReadOnlyCondition
    extends UmbConditionBase<BlockEntryIsReadOnlyConditionConfig>
    implements UmbExtensionCondition {
    constructor(host, args) {
      super(host, args);
      this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
        this.observe(context.readOnlyGuard.permitted, (isReadOnly) => {
          if (isReadOnly !== undefined) {
            this.permitted = isReadOnly === (this.config.match !== undefined ? this.config.match : true);
          }
        }, 'observeIsReadOnly');
      });
    }
  }
  ```
  Pattern matches existing `UmbBlockWorkspaceIsReadOnlyCondition` but uses `UMB_BLOCK_ENTRY_CONTEXT`.

**Register in:** `src/packages/block/block/conditions/manifests.ts` — add to existing conditions array.
**Add types to:** `src/packages/block/block/conditions/types.ts` — add to existing `UmbExtensionConditionConfigMap`.

### 2. Remove read-only guard from default kind element

**Modify:** `src/packages/block/block/action/default/block-action.element.ts`

- Remove `_isReadOnly` state property
- Remove `UMB_BLOCK_ENTRY_CONTEXT` consumption and `readOnlyGuard.permitted` observer
- Remove `if (this._isReadOnly && !this.manifest.meta.showOnReadOnly) return nothing;` from render

### 3. Remove `showOnReadOnly` from meta

**Modify:** `src/packages/block/block/action/default/types.ts`

- Remove `showOnReadOnly?: boolean` from `MetaBlockActionDefaultKind`

### 4. Add condition to Delete manifest

**Modify:** `src/packages/block/block/action/common/delete/manifests.ts`

- Add conditions array:
  ```typescript
  conditions: [
    { alias: 'Umb.Condition.BlockEntryIsReadOnly', match: false },
  ],
  ```

### 5. Remove `showOnReadOnly` from Copy to Clipboard manifest

**Modify:** `src/packages/block/block/action/common/copy-to-clipboard/manifests.ts`

- Remove `showOnReadOnly: true` from meta (no longer needed — the default is now "always visible")

### 6. Export condition constant

**Modify:** `src/packages/block/block/conditions/` index or ensure the constant is exported through the block package for 3rd-party use.

## Verification

1. `npm run build` — no errors
2. `npm run check:circular` — clean
3. Const export test passes
4. Delete button hidden when block is read-only
5. Copy to Clipboard button visible when block is read-only
6. Both buttons visible in normal (non-read-only) mode

## Critical Files

| File | Role |
|------|------|
| `src/packages/block/block/conditions/block-entry-is-read-only/` (new) | New condition |
| `src/packages/block/block/conditions/manifests.ts` | Register condition |
| `src/packages/block/block/conditions/types.ts` | Add config type |
| `src/packages/block/block/action/default/block-action.element.ts` | Remove read-only guard |
| `src/packages/block/block/action/default/types.ts` | Remove `showOnReadOnly` |
| `src/packages/block/block/action/common/delete/manifests.ts` | Add condition |
| `src/packages/block/block/action/common/copy-to-clipboard/manifests.ts` | Remove `showOnReadOnly` |

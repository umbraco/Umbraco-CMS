# Block Action Extension Type — Design Spec

## Context

The block entry elements (Block List, Block Grid, Block RTE, Block Single) each render a hardcoded `<uui-action-bar>` with 4 action buttons: Edit Content, Edit Settings, Copy to Clipboard, and Delete. There is no way for 3rd-party extensions to add custom actions to block items.

This design introduces a new `blockAction` extension type that allows both internal and 3rd-party extensions to register actions on block items. The pattern follows the established `entityAction` architecture but with block-specific filtering and rendering.

### Scope (Proof of Concept)

- **Convert**: Delete action becomes a `blockAction` extension
- **Keep hardcoded**: Edit Content, Edit Settings, Copy to Clipboard
- **Focus**: Block List property editor (integrate into all four block editors, but Delete action only targets Block List initially)
- **Future**: Migrate remaining actions, add overflow dropdown, extend to all block editors

## Manifest Type

Independent type (does not extend `ManifestEntityAction`). Follows the same structural patterns.

```typescript
export interface ManifestBlockAction<MetaType extends MetaBlockAction = MetaBlockAction>
  extends ManifestElementAndApi<UmbBlockActionElement, UmbBlockAction<MetaType>>,
          ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
  type: 'blockAction';
  forContentTypeAlias?: string | Array<string>;
  forBlockEditor?: string | Array<string>;
  meta: MetaType;
}

export interface MetaBlockAction {}

declare global {
  interface UmbExtensionManifestMap {
    umbBlockAction: ManifestBlockAction;
  }
}
```

- `forContentTypeAlias` / `forBlockEditor` — optional filters, consistent with `ManifestBlockEditorCustomView`
- Both support `string | Array<string>`, filtered via `stringOrStringArrayContains()`
- Omitting a filter = applies to all

## API Base Class

```typescript
export interface UmbBlockActionArgs<MetaArgsType> {
  unique: string;      // contentKey today, layout key in future
  meta: MetaArgsType;
}

export interface UmbBlockAction<ArgsMetaType> extends UmbAction<UmbBlockActionArgs<ArgsMetaType>> {
  getHref(): Promise<string | undefined>;
  execute(): Promise<void>;
}

export abstract class UmbBlockActionBase<ArgsMetaType>
  extends UmbActionBase<UmbBlockActionArgs<ArgsMetaType>>
  implements UmbBlockAction<ArgsMetaType> {

  public getHref(): Promise<string | undefined> {
    return Promise.resolve(undefined);
  }

  public execute(): Promise<void> {
    return Promise.resolve();
  }
}
```

Action API classes access block data via context consumption:
```typescript
this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => { ... });
```

## Default Kind

Renders as `<uui-button>` (not `<uui-menu-item>` like entity actions) to match the existing action bar UI.

```typescript
export const UMB_BLOCK_ACTION_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
  type: 'kind',
  alias: 'Umb.Kind.BlockAction.Default',
  matchKind: 'default',
  matchType: 'blockAction',
  manifest: {
    type: 'blockAction',
    kind: 'default',
    weight: 1000,
    element: () => import('./block-action.element.js'),
    meta: {
      icon: '',
      label: '',
    },
  },
};

export interface MetaBlockActionDefaultKind extends MetaBlockAction {
  icon: string;
  label: string;
}
```

### Default Element (`<umb-block-action>`)

```html
<!-- Renders as: -->
<uui-button label="..." look="secondary" @click=${execute} title="...">
  <uui-icon name="${icon}"></uui-icon>
</uui-button>
```

- Calls `api.getHref()` — if href exists, wraps as link
- Otherwise calls `api.execute()` on click
- Dispatches `UmbActionExecutedEvent` after execution

## List Element (`<umb-block-action-list>`)

Owns the `<uui-action-bar>` and renders both slotted (hardcoded) content and extension-registered `blockAction` extensions. This design allows actions to be migrated from slot content to extensions one at a time.

```typescript
@customElement('umb-block-action-list')
export class UmbBlockActionListElement extends UmbLitElement {
  // Property: blockEditor (attribute 'block-editor')

  // Consumes UMB_BLOCK_ENTRY_CONTEXT to get:
  //   - contentKey (as unique)
  //   - contentTypeAlias (for filtering)
  //   - showActions (to hide when not applicable)
  //   - isSortMode (to hide during sort — Block List specific, observed if available)

  // Filter method matches forContentTypeAlias and forBlockEditor
  // using stringOrStringArrayContains()

  // Renders:
  // <uui-action-bar>
  //   <slot></slot>  <!-- hardcoded actions from parent -->
  //   <umb-extension-with-api-slot type="blockAction" .filter .apiArgs>
  //   </umb-extension-with-api-slot>
  // </uui-action-bar>
}
```

### Integration Point

The `blockEditor` value for filtering is provided via a property. Each block entry element passes its editor type constant.

```typescript
@property({ type: String, attribute: 'block-editor' })
public blockEditor?: string;
```

Usage in block-list-entry — hardcoded actions are slotted, Delete is an extension:
```html
<umb-block-action-list block-editor=${UMB_BLOCK_LIST}>
  ${this.#renderEditContentAction()}
  ${this.#renderEditSettingsAction()}
  ${this.#renderCopyToClipboardAction()}
</umb-block-action-list>
```

The `<uui-action-bar>` styling (positioning, opacity transitions, hover/focus reveal) moves from the block entry element into the list element.

## Delete Action (Proof of Concept)

```typescript
// Manifest
{
  type: 'blockAction',
  kind: 'default',
  alias: 'Umb.BlockAction.Delete',
  name: 'Delete Block Action',
  weight: 100,  // low weight = renders last (rightmost)
  api: () => import('./delete-block.action.js'),
  forBlockEditor: UMB_BLOCK_LIST,
  meta: {
    icon: 'icon-remove',
    label: '#general_delete',
  },
}
```

```typescript
// API class
export class UmbDeleteBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
  #blockEntryContext?: UmbBlockEntryContext;

  constructor(host, args) {
    super(host, args);
    this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
      this.#blockEntryContext = context;
    });
  }

  async execute() {
    await this.#blockEntryContext?.requestDelete();
  }
}
```

### Read-Only Handling

The existing `Umb.Condition.BlockWorkspaceIsReadOnly` condition consumes `UMB_BLOCK_WORKSPACE_CONTEXT`, which is only available inside the block workspace modal — not at the block entry level where actions render. It cannot be used here.

Instead, the default kind element (`<umb-block-action>`) consumes `UMB_BLOCK_ENTRY_CONTEXT` and observes `readOnlyGuard.permitted`. When read-only, the button is hidden (renders `nothing`). This matches how the current hardcoded Delete button works — it checks `this._isReadOnly` inline.

The Delete action manifest does NOT need a read-only condition. The element handles it.

## File Structure

All new files live in the block package:

```
src/packages/block/block/action/
  block-action.extension.ts          -- ManifestBlockAction, MetaBlockAction, UmbExtensionManifestMap
  block-action.interface.ts          -- UmbBlockAction interface
  block-action-base.ts               -- UmbBlockActionBase class
  block-action-element.interface.ts  -- UmbBlockActionElement (extends UmbControllerHostElement)
  block-action-list.element.ts       -- <umb-block-action-list>
  types.ts                           -- UmbBlockActionArgs, re-exports
  constants.ts                       -- any constants
  index.ts                           -- public exports
  default/
    default.action.kind.ts           -- default kind manifest
    block-action.element.ts          -- <umb-block-action> (renders <uui-button>)
    types.ts                         -- MetaBlockActionDefaultKind
    manifests.ts
  common/
    delete/
      delete-block.action.ts         -- UmbDeleteBlockAction
      manifests.ts
      constants.ts
  manifests.ts                       -- aggregates kind + delete manifests
```

### Modified Files

- `src/packages/block/block/index.ts` — add `export * from './action/index.js'`
- `src/packages/block/block/manifests.ts` — add action manifests
- `src/packages/block/block-list/components/block-list-entry/block-list-entry.element.ts` — replace `#renderDeleteAction()` with `<umb-block-action-list>` in `#renderActionBar()`

### Reference Files (read-only, pattern source)

- `src/packages/core/entity-action/entity-action.extension.ts`
- `src/packages/core/entity-action/entity-action-base.ts`
- `src/packages/core/entity-action/entity-action-list.element.ts`
- `src/packages/core/entity-action/default/default.action.kind.ts`
- `src/packages/core/entity-action/default/entity-action.element.ts`
- `src/packages/core/entity-action/default/types.ts`
- `src/packages/block/block-custom-view/block-editor-custom-view.extension.ts`
- `src/packages/block/block/context/block-entry.context.ts`
- `src/packages/block/block/context/block-entry.context-token.ts`

## Rendering Integration

### Before (block-list-entry.element.ts)

```typescript
#renderActionBar() {
  if (this._isSortMode) return nothing;
  if (!this._showActions) return nothing;
  return html`
    <uui-action-bar>
      ${this.#renderEditContentAction()} ${this.#renderEditSettingsAction()} ${this.#renderCopyToClipboardAction()}
      ${this.#renderDeleteAction()}
    </uui-action-bar>
  `;
}
```

### After

```typescript
#renderActionBar() {
  return html`
    <umb-block-action-list block-editor=${UMB_BLOCK_LIST}>
      ${this.#renderEditContentAction()}
      ${this.#renderEditSettingsAction()}
      ${this.#renderCopyToClipboardAction()}
    </umb-block-action-list>
  `;
}
```

- The `<uui-action-bar>` moves into the list element
- Sort mode / showActions guards move into the list element (observed from entry context)
- `uui-action-bar` CSS (positioning, opacity, hover reveal) moves into the list element
- `#renderDeleteAction()` method is removed

## Future Enhancements (Out of Scope)

1. **Migrate Edit Content, Edit Settings, Copy to Clipboard** to `blockAction` extensions (Option A)
2. **Overflow dropdown** — when too many actions, render excess in a `...` popover
3. **Integrate into Block Grid, Block RTE, Block Single** — same pattern, different `block-editor` value
4. **Entity convergence** — when blocks get unique layout keys, consider extending `ManifestEntityAction` or providing `UMB_ENTITY_CONTEXT` per block entry

## Verification

1. **Build**: `npm run build` passes with no errors
2. **Type check**: No TypeScript errors in new or modified files
3. **Visual**: Block List entry shows Delete button in the same position, same icon, same behavior
4. **Delete works**: Clicking Delete shows confirmation modal, then removes the block
5. **Read-only**: Delete button is hidden when the block is read-only
6. **Sort mode**: Action bar (including block actions) is hidden during sort mode
7. **3rd-party test**: Register a custom `blockAction` manifest and verify it appears in the action bar
8. **Filtering**: A `blockAction` with `forBlockEditor: 'block-grid'` does NOT appear in the Block List
9. **Circular deps**: `npm run check:circular` passes

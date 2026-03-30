# Workspaces

Workspaces are the primary editing surfaces in the backoffice. Each workspace holds a **context** (state + behavior), **views** (tabs), and **actions** (toolbar buttons) for a specific entity or feature. All workspace infrastructure lives in `src/packages/core/workspace/`; each entity implements its own workspace in its domain package.

Workspaces are registered and composed through the **extension system** — the same mechanism used for all backoffice UI. This means any workspace can be extended, overridden, or replaced by any package.

**Prerequisites**: [Entities](./entities.md), [Core Primitives](./core-primitives.md), [Data Flow](./data-flow.md)

---

## Extension Types for Workspaces

All workspace-related extensions use **conditions** to scope to a specific workspace.

| Extension Type | Purpose | Key Config |
|---|---|---|
| `workspace` | The workspace itself | `kind` (`default`/`routable`), `api` (context class), `meta.entityType` |
| `workspaceView` | Tabs/views within workspace | `element`/`js` (lazy), `meta.label`, `meta.pathname`, `meta.icon`, `weight` |
| `workspaceAction` | Toolbar actions (save, publish) | `kind`, `api` (action class), `meta.look`, `meta.color`, `meta.label` |
| `workspaceActionMenuItem` | Dropdown items on actions | `forWorkspaceActions` (target action alias) |
| `workspaceContext` | Additional composable contexts | `api` (lazy), `conditions` |
| `workspaceFooterApp` | Footer components | `kind` (`menuBreadcrumb`, `variantMenuBreadcrumb`) |

### How extensions are discovered

1. `umb-workspace` element finds the `workspace` extension matching the entity type
2. Once the workspace context (API) is created, `UmbExtensionsApiInitializer` loads all matching `workspaceContext` extensions
3. `umb-workspace-editor` discovers `workspaceView` extensions via `UmbExtensionsManifestInitializer`
4. `umb-workspace-footer` discovers `workspaceAction` extensions via `umb-extension-with-api-slot`

Key file: `src/packages/core/workspace/workspace.element.ts`

---

## Workspace Kinds

Two built-in kinds provide the workspace shell and context. Start with `default`; use `routable` when you need create/edit URL routing.

### `default` — The Foundation

The simplest workspace. Provides a headline, renders `<umb-workspace-editor>`, and supports workspace views, actions, workspace contexts, and footer apps — all with **zero custom code**. Just register a manifest.

```typescript
{
  type: 'workspace',
  kind: 'default',
  alias: 'Umb.Workspace.WebhookRoot',
  name: 'Webhook Root Workspace',
  meta: {
    entityType: UMB_WEBHOOK_ROOT_ENTITY_TYPE,
    headline: '#treeHeaders_webhooks',
  },
}
```

**What you get out of the box:**
- `UmbDefaultWorkspaceContext` — implements `UmbWorkspaceContext`, sets up `UmbEntityContext` and `UmbViewContext`
- `<umb-default-workspace>` element — renders `<umb-workspace-editor>` with the localized headline
- Full support for `workspaceView`, `workspaceAction`, `workspaceContext`, and `workspaceFooterApp` extensions scoped via conditions
- Entity context propagation (`entityType` + `unique`)

**No custom context class or element needed.** The kind provides both.

**`meta` config:**
- `entityType` (required) — the entity type constant
- `headline` (required) — workspace title, supports localization keys (e.g., `'#treeHeaders_webhooks'`)

**Common pattern:** Default workspaces often pair with a collection view to list child entities:

```typescript
// Webhook root workspace + collection view
export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'workspace',
    kind: 'default',
    alias: UMB_WEBHOOK_ROOT_WORKSPACE_ALIAS,
    name: 'Webhook Root Workspace',
    meta: {
      entityType: UMB_WEBHOOK_ROOT_ENTITY_TYPE,
      headline: '#treeHeaders_webhooks',
    },
  },
  {
    type: 'workspaceView',
    kind: 'collection',
    alias: 'Umb.WorkspaceView.WebhookRoot.Collection',
    name: 'Webhook Root Collection Workspace View',
    meta: {
      label: 'Collection',
      pathname: 'collection',
      icon: 'icon-layers',
      collectionAlias: UMB_WEBHOOK_COLLECTION_ALIAS,
    },
    conditions: [
      { alias: UMB_WORKSPACE_CONDITION_ALIAS, match: UMB_WEBHOOK_ROOT_WORKSPACE_ALIAS },
    ],
  },
];
```

**Kind implementation:** `src/packages/core/workspace/kinds/default/`

**Base interface** (`UmbWorkspaceContext`):
```typescript
interface UmbWorkspaceContext extends UmbApi, UmbContextMinimal {
  readonly workspaceAlias: string;
  getEntityType(): string;
}
```

This is the minimal contract all workspace contexts satisfy. The default kind implements it automatically.

### `routable` — Entity Detail Editing

Used when the workspace needs internal URL routing (create/edit flows). The workspace element renders an `umb-router-slot` that delegates to routes defined by the workspace context. **Requires a custom context class** that sets up routes.

```typescript
{
  type: 'workspace',
  kind: 'routable',
  alias: 'Umb.Workspace.Webhook',
  name: 'Webhook Workspace',
  api: () => import('./webhook-workspace.context.js'),
  meta: { entityType: UMB_WEBHOOK_ENTITY_TYPE },
}
```

**What differs from default:**
- You must provide `api` — a context class that implements `UmbRoutableWorkspaceContext` and calls `this.routes.setRoutes(...)`
- You must provide a custom editor element (rendered by the routes)
- The kind only provides the router shell (`<umb-router-slot>`), not the workspace editor UI

**Kind implementation:** `src/packages/core/workspace/kinds/routable/routable-workspace.kind.ts`

---

## Base Class Hierarchy (for Routable Workspaces)

When building a routable workspace, you typically extend one of these base classes. Each layer adds functionality:

```
UmbContextBase
  └── UmbSubmittableWorkspaceContextBase<WorkspaceDataModelType>
       └── UmbEntityDetailWorkspaceContextBase<DetailModelType, DetailRepositoryType, CreateArgsType>
            └── UmbEntityNamedDetailWorkspaceContextBase<NamedDetailModelType, ...>
```

### UmbSubmittableWorkspaceContextBase

**File**: `src/packages/core/workspace/submittable/submittable-workspace-context-base.ts`

Foundation for any workspace that supports form submission:
- `workspaceAlias` — identifier matching the manifest alias
- `routes` — `UmbWorkspaceRouteManager` for internal routing
- `isNew` observable — tracks whether the entity is being created
- `requestSubmit()` → `validate()` → abstract `submit()` pipeline
- Multiple `UmbValidationContext` support for composable validation
- Keyboard shortcut: Ctrl+S triggers submit
- Modal integration (workspace can be opened in a modal dialog)

### UmbEntityDetailWorkspaceContextBase

**File**: `src/packages/core/workspace/entity-detail/entity-detail-workspace-base.ts`

Adds entity detail CRUD lifecycle on top of submittable:
- `UmbEntityWorkspaceDataManager` — dual-state data management (persisted vs. current) with change detection
- `load(unique)` — fetch entity from repository, observe for real-time updates
- `createScaffold(args)` — create new entity with optional preset data and parent
- `submit()` — routes to `_create()` or `_update()` based on `isNew`
- `delete(unique)` — delete entity via repository
- `validationContext` — base validation context (always present)
- `loading` / `forbidden` state managers
- Navigation-away guard (unsaved changes modal)
- Event dispatching for tree refresh after create/update

**Constructor args** (`UmbEntityDetailWorkspaceContextArgs`):
```typescript
{ workspaceAlias: string, entityType: string, detailRepositoryAlias: string }
```

### UmbEntityNamedDetailWorkspaceContextBase

**File**: `src/packages/core/workspace/entity-detail/entity-named-detail-workspace-base.ts`

Adds name management for entities with a `name` property:
- `name` observable — derived from current data
- `getName()` / `setName()` convenience methods
- `nameWriteGuard` — `UmbNameWriteGuardManager` for permission checking
- Auto-syncs view title from name changes

**This is the base class most entity detail workspaces extend.**

---

## Workspace Context Extensions (Modularity & Reuse)

**This is the most important pattern for maintaining package boundaries and enabling cross-workspace reuse.**

`workspaceContext` extensions let any package add capabilities to a workspace without modifying the workspace's own code. The owning package registers the extension; it activates only when the target workspace is open. This works for both `default` and `routable` workspaces.

Workspace contexts serve two purposes:
1. **Modularity** — Keep features in their own package, not in the workspace's package
2. **Reuse** — The same capability (publishing, menu structure, permissions) can be added to multiple workspaces via separate registrations or shared base classes

### Registration pattern

```typescript
// In: src/packages/documents/documents/publishing/workspace-context/manifests.ts
{
  type: 'workspaceContext',
  alias: 'Umb.WorkspaceContext.Document.Publishing',
  api: () => import('./document-publishing.workspace-context.js'),
  conditions: [
    { alias: UMB_WORKSPACE_CONDITION_ALIAS, match: UMB_DOCUMENT_WORKSPACE_ALIAS },
  ],
}
```

### Implementation pattern

The context class extends `UmbContextBase`, consumes the parent workspace context, and provides its own context token:

```typescript
export class UmbDocumentPublishingWorkspaceContext extends UmbContextBase
  implements UmbPublishableWorkspaceContext {

  constructor(host: UmbControllerHost) {
    super(host, UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT.toString());

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
      // Access workspace data and add publishing capabilities
    });
  }

  async saveAndPublish() { /* ... */ }
  async publish() { /* ... */ }
  async unpublish() { /* ... */ }
}
```

Other components consume this via its token:
```typescript
this.consumeContext(UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT, (ctx) => { /* ... */ });
```

### Deep example: Document Publishing

The document publishing system is the best example of a workspace context extension with a full stack. It demonstrates how an entire feature — with its own repository, actions, and UI — layers on top of a workspace.

**Architecture:**

```
documents/publishing/                          # Feature package
├── workspace-context/
│   ├── document-publishing.workspace-context.ts    # Context: adds publish/unpublish/schedule
│   ├── document-publishing.workspace-context.token.ts
│   └── manifests.ts                                # Registers as workspaceContext
├── repository/
│   ├── document-publishing.repository.ts           # Own repository (separate from document detail)
│   └── document-publishing.server.data-source.ts   # Own API endpoints
├── pending-changes/
│   └── document-published-pending-changes.manager.ts  # Tracks published vs. current diffs
├── publish/workspace-action/
│   └── save-and-publish.action.ts                  # Workspace action consuming publishing context
├── schedule-publish/workspace-action/
│   └── save-and-schedule.action.ts                 # Schedule workspace action
├── publish-with-descendants/workspace-action/
│   └── publish-with-descendants.action.ts          # Bulk publish workspace action
└── unpublish/entity-action/
    └── unpublish.action.ts                         # Entity action for unpublish
```

**Key design principles:**

1. **Own repository & endpoints** — The publishing context has its own `UmbDocumentPublishingRepository` with dedicated API endpoints (publish, unpublish, publishWithDescendants, getPublished). This is completely separate from the document detail repository.

2. **Consumes parent workspace context** — The publishing context uses `consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT)` to access the document's data, variant options, validation, and save flow. It calls methods like `getUnique()`, `variantOptions`, `constructSaveData()`, and `performCreateOrUpdate()` on the parent.

3. **Implements a core interface** — `UmbPublishableWorkspaceContext` is defined in core (`src/packages/core/workspace/contexts/tokens/publishable-workspace-context.interface.ts`). Any entity type could implement this interface if it gains publishing support in the future.

4. **Own workspace actions** — Save & Publish, Schedule, and Publish with Descendants are each separate workspace actions that consume `UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT` and delegate to its methods.

5. **Adds keyboard shortcuts** — The publishing context registers Ctrl+P for save-and-publish via the parent workspace's `view.shortcuts`.

**Core interface for reuse:**

```typescript
// src/packages/core/workspace/contexts/tokens/publishable-workspace-context.interface.ts
export interface UmbPublishableWorkspaceContext extends UmbWorkspaceContext {
  saveAndPublish(): Promise<void>;
  publish(): Promise<void>;
  unpublish(): Promise<void>;
}
```

If media were to support publishing, a similar `UmbMediaPublishingWorkspaceContext` could be created in `src/packages/media/` implementing this same interface, with its own repository and endpoints, registered as a `workspaceContext` conditioned on the media workspace alias.

### Reuse patterns

Workspace contexts achieve reuse across workspaces in two ways:

**1. Shared base classes** — A base class provides common logic; entity-specific subclasses configure it:

```typescript
// Base in core — provides breadcrumb structure from any tree repository
export class UmbMenuTreeStructureWorkspaceContextBase extends UmbContextBase { /* ... */ }

// Document-specific — just passes its tree repository alias
export class UmbDocumentMenuStructureContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
  constructor(host: UmbControllerHost) {
    super(host, { treeRepositoryAlias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS });
  }
}

// Media-specific — same base, different repo
export class UmbMediaMenuStructureContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
  constructor(host: UmbControllerHost) {
    super(host, { treeRepositoryAlias: UMB_MEDIA_TREE_REPOSITORY_ALIAS });
  }
}
```

**2. Multiple registrations** — The same context class (or base class) registered for different workspaces:

```typescript
// Property permissions registered for both document and block workspaces
{
  type: 'workspaceContext',
  alias: 'Umb.WorkspaceContext.Document.PropertyValueUserPermission',
  api: () => import('./document-property-value-user-permission.workspace-context.js'),
  conditions: [{ alias: UMB_WORKSPACE_CONDITION_ALIAS, match: UMB_DOCUMENT_WORKSPACE_ALIAS }],
},
{
  type: 'workspaceContext',
  alias: 'Umb.WorkspaceContext.Block.PropertyValueUserPermission',
  api: () => import('./block-property-value-user-permission.workspace-context.js'),
  conditions: [{ alias: UMB_WORKSPACE_CONDITION_ALIAS, match: UMB_BLOCK_WORKSPACE_ALIAS }],
}
```

### Examples

| Package | Context | Extends Workspace | Reuse Pattern |
|---|---|---|---|
| `documents/publishing/` | `UmbDocumentPublishingWorkspaceContext` | Document | Core `UmbPublishableWorkspaceContext` interface |
| `documents/menu/` | Document menu structure context | Document | `UmbMenuVariantTreeStructureWorkspaceContextBase` base class |
| `media/menu/` | Media menu structure context | Media | Same base class as documents |
| `language/` | Language access workspace context | Document | Single workspace |
| `documents/user-permissions/` | Property value permission context | Document, Block | `UmbPropertyValueUserPermissionWorkspaceContextBase` base class |

### Package boundary rules

- **The package that owns the feature registers the workspaceContext**, not the workspace's package
- Publishing features live in `documents/publishing/`, not `documents/workspace/`
- Menu structure contexts live in the entity's `menu/` directory
- Views and actions from other packages use conditions to target the workspace alias
- Never import internal files from another package — use public `index.ts` exports
- A workspace context can have its own repository, data source, and actions — it's a full feature stack

### Kinds for workspace contexts

Workspace contexts can use kinds for shared patterns:

```typescript
{
  type: 'workspaceContext',
  kind: 'menuStructure',  // Provides pre-built breadcrumb/menu behavior
  alias: 'Umb.Context.Media.Menu.Structure',
  api: () => import('./media-menu-structure.context.js'),
  meta: { menuItemAlias: UMB_MEDIA_MENU_ITEM_ALIAS },
  conditions: [{ alias: UMB_WORKSPACE_CONDITION_ALIAS, match: UMB_MEDIA_WORKSPACE_ALIAS }],
}
```

---

## Save/Submit Flow

### Simple save (most entities)

1. User clicks **Save** button (or Ctrl+S)
2. `UmbSubmitWorkspaceAction.execute()` calls `workspaceContext.requestSubmit()`
3. `requestSubmit()` runs all registered `UmbValidationContext` validators
4. If valid → calls abstract `submit()`
5. `submit()` in `UmbEntityDetailWorkspaceContextBase`:
   - If `isNew` → `_create(data, parent)` → `repository.create(data, parentUnique)`
   - If existing → `_update(data)` → `repository.save(data)`
6. Updates persisted + current data from response
7. Dispatches events: `UmbRequestReloadChildrenOfEntityEvent` (create), `UmbRequestReloadStructureForEntityEvent`, `UmbEntityUpdatedEvent` (update)
8. If opened in modal → resolves modal value and closes

### Built-in action classes

| Class | File | Consumes | Calls |
|---|---|---|---|
| `UmbSubmitWorkspaceAction` | `core/workspace/components/workspace-action/common/submit/submit.action.ts` | `UMB_SUBMITTABLE_WORKSPACE_CONTEXT` | `requestSubmit()` |
| `UmbSaveWorkspaceAction` | `core/workspace/components/workspace-action/common/save/save.action.ts` | `UMB_SAVEABLE_WORKSPACE_CONTEXT` | `requestSave()` |

Most simple entities use `UmbSubmitWorkspaceAction` directly (no custom action class needed). Complex entities like documents define custom action classes for variant dialogs, permission checks, etc.

### Action manifest example

```typescript
{
  type: 'workspaceAction',
  kind: 'default',
  alias: 'Umb.WorkspaceAction.Webhook.Save',
  api: UmbSubmitWorkspaceAction,
  meta: { label: '#buttons_save', look: 'primary', color: 'positive' },
  conditions: [{ alias: UMB_WORKSPACE_CONDITION_ALIAS, match: UMB_WEBHOOK_WORKSPACE_ALIAS }],
}
```

---

## Route Patterns

Routable workspaces define internal routes in the context constructor via `this.routes.setRoutes()`.

### Simple (no parent hierarchy)

```typescript
this.routes.setRoutes([
  {
    path: 'create',
    component: MyEditorElement,
    setup: async () => {
      await this.createScaffold({ parent: { entityType: ROOT_ENTITY_TYPE, unique: null } });
      new UmbWorkspaceIsNewRedirectController(this, this, this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!);
    },
  },
  {
    path: 'edit/:unique',
    component: MyEditorElement,
    setup: (_component, info) => {
      this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
      this.load(info.match.params.unique);
    },
  },
]);
```

### Parent-aware (tree entities)

```typescript
{
  path: 'create/parent/:entityType/:parentUnique',
  component: MyEditorElement,
  setup: async (_component, info) => {
    const parentEntityType = info.match.params.entityType;
    const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
    await this.createScaffold({ parent: { entityType: parentEntityType, unique: parentUnique } });
    new UmbWorkspaceIsNewRedirectController(this, this, this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!);
  },
}
```

**`UmbWorkspaceIsNewRedirectController`** observes the workspace's `isNew` state and replaces the URL from `/create` to `/edit/:unique` after the first save. File: `src/packages/core/workspace/controllers/workspace-is-new-redirect.controller.ts`

---

## Workspace Conditions

Built-in conditions for scoping extensions to specific workspaces:

| Condition Alias | Config | Purpose |
|---|---|---|
| `Umb.Condition.WorkspaceAlias` | `match: string` or `oneOf: string[]` | Match workspace by alias |
| `Umb.Condition.WorkspaceEntityType` | `match: string` or `oneOf: string[]` | Match entity type |
| `Umb.Condition.WorkspaceEntityIsNew` | — | True when entity is being created |
| `Umb.Condition.WorkspaceIsLoaded` | — | True when workspace data has loaded |

The most common is `Umb.Condition.WorkspaceAlias` (constant: `UMB_WORKSPACE_CONDITION_ALIAS`), used by virtually all workspace views and actions.

Files: `src/packages/core/workspace/conditions/`

---

## Context Token Pattern

Every routable workspace should expose a typed context token so views, actions, and workspace context extensions can consume it with type safety.

```typescript
// {entity}-workspace.context-token.ts
import type { UmbMyEntityWorkspaceContext } from './{entity}-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_MY_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<
  UmbSubmittableWorkspaceContext,       // Base type (what consumers minimally expect)
  UmbMyEntityWorkspaceContext           // Concrete type (what they actually get)
>(
  'UmbWorkspaceContext',                // Context string — always 'UmbWorkspaceContext'
  undefined,
  (context): context is UmbMyEntityWorkspaceContext =>
    context.getEntityType?.() === 'my-entity',  // Discriminator function
);
```

**Rules:**
- First generic = base interface (`UmbSubmittableWorkspaceContext`)
- Second generic = concrete context class
- Context string is always `'UmbWorkspaceContext'` (shared across all workspace contexts)
- Discriminator checks `getEntityType()` to find the right workspace context in the DOM tree
- Re-export from `constants.ts` for public API

Default workspaces don't need a custom context token — they use `UMB_WORKSPACE_CONTEXT` from core.

---

## Data Management

The `UmbEntityWorkspaceDataManager` (`src/packages/core/workspace/entity/entity-workspace-data-manager.ts`) provides dual-state data tracking for routable entity detail workspaces:

- **`persisted`** — last saved state from the server
- **`current`** — working copy with user edits

Key methods on workspace context (via `this._data`):
```typescript
// Create observables for specific properties
readonly myProp = this._data.createObservablePartOfCurrent((data) => data?.myProp);

// Read/write current data
this._data.getCurrent()?.myProp
this._data.updateCurrent({ myProp: newValue })

// Change detection
this._data.getHasUnpersistedChanges()  // JSON comparison
this._data.resetCurrent()              // Revert to persisted
```

---

## Reference Examples

### Default workspaces (kind: `default`)

| Entity | Package Path | Notes |
|---|---|---|
| Webhook Root | `src/packages/webhook/webhook-root/workspace/` | Root + collection view |
| User Root | `src/packages/user/user/workspace/user-root/` | Root + collection view |
| Extension Insights Root | `src/packages/extension-insights/workspace/` | Root + collection view |
| Clipboard Root | `src/packages/clipboard/clipboard-root/` | Root workspace |
| Data Type Root | `src/packages/data-type/data-type-root/` | Root + collection view |

### Routable workspaces (kind: `routable`)

| Complexity | Entity | Package Path | Notes |
|---|---|---|---|
| Simple | Webhook | `src/packages/webhook/webhook/workspace/` | Single view, flat data, `UmbSubmitWorkspaceAction` |
| Simple | Dictionary | `src/packages/dictionary/workspace/` | Single view, translation array management |
| Medium | Data Type | `src/packages/data-type/workspace/` | Two views, invariant dataset, property editor management |
| Complex | Document | `src/packages/documents/documents/workspace/` | Variants, publishing contexts, permissions, content workspace base |
| Complex | Media | `src/packages/media/media/workspace/` | Content workspace base, menu structure context |

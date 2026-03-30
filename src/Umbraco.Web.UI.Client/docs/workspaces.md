# Workspaces

Workspaces are the primary editing surfaces in the backoffice. Each workspace holds a **context** (state + behavior), **views** (tabs), and **actions** (toolbar buttons) for a specific entity or feature. All workspace infrastructure lives in `src/packages/core/workspace/`; each entity implements its own workspace in its domain package.

Workspaces are registered and composed through the **extension system** — the same mechanism used for all backoffice UI. This means any workspace can be extended, overridden, or replaced by any package.

**Prerequisites**: [Entities](./entities.md), [Core Primitives](./core-primitives.md), [Data Flow](./data-flow.md)

---

## Extension Types for Workspaces

Workspace *views, actions, contexts, and footer apps* typically use **conditions** to scope to a specific workspace, while the `workspace` manifest itself is matched by `meta.entityType`.

| Extension Type | Purpose | Key Config |
|---|---|---|
| `workspace` | The workspace itself | `kind` (`default`/`routable`), `api` (context class), `meta.entityType` |
| `workspaceView` | Tabs/views within workspace | `element`/`js` (lazy), `meta.label`, `meta.pathname`, `meta.icon`, `weight`, `conditions` |
| `workspaceAction` | Toolbar actions (save, publish) | `kind`, `api` (action class), `meta.look`, `meta.color`, `meta.label`, `conditions` |
| `workspaceActionMenuItem` | Dropdown items on actions | `forWorkspaceActions` (target action alias), `conditions` |
| `workspaceContext` | Additional composable contexts | `api` (lazy), `conditions` |
| `workspaceFooterApp` | Footer components | `kind` (`menuBreadcrumb`, `variantMenuBreadcrumb`), `conditions` |

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

**Common pattern:** Default workspaces often pair with a `workspaceView` of `kind: 'collection'` to list child entities. See `src/packages/webhook/webhook-root/workspace/manifests.ts` for a complete example.

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

**Editor element pattern**: Use `<umb-workspace-header-name-editable slot="header">` inside `<umb-entity-detail-workspace-editor>`. This built-in element auto-consumes the workspace context for name binding, validation (`umbBindToValidation`), write guard permissions, auto-focus, and localization — no manual name wiring needed. See `src/packages/core/workspace/components/workspace-header-name-editable/`.

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

The best example of a full-stack workspace context extension. The publishing feature has its own repository, data source, workspace actions, and keyboard shortcuts — all layered on top of the document workspace via a `workspaceContext` registration.

**Key principles:** own repository & endpoints (separate from document detail), consumes parent workspace context for entity data, implements a core interface (`UmbPublishableWorkspaceContext` in `src/packages/core/workspace/contexts/tokens/`), and registers its own workspace actions that delegate to the publishing context.

**Reference:** `src/packages/documents/documents/publishing/`

### Reuse patterns

1. **Shared base classes** — A base class in core provides common logic; entity-specific subclasses just pass configuration (e.g., `UmbMenuVariantTreeStructureWorkspaceContextBase` used by both document and media menu structure contexts, each passing their own tree repository alias).

2. **Multiple registrations** — The same context class registered for different workspaces with different conditions (e.g., property value permissions registered for both document and block workspaces).

### Package boundary rules

- **The package that owns the feature registers the workspaceContext**, not the workspace's package
- Views and actions from other packages use conditions to target the workspace alias
- Never import internal files from another package — use public `index.ts` exports

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
| `UmbSubmitWorkspaceAction` | `src/packages/core/workspace/components/workspace-action/common/submit/submit.action.ts` | `UMB_SUBMITTABLE_WORKSPACE_CONTEXT` | `requestSubmit()` |
| `UmbSaveWorkspaceAction` | `src/packages/core/workspace/components/workspace-action/common/save/save.action.ts` | `UMB_SAVEABLE_WORKSPACE_CONTEXT` | `requestSave()` |

Most simple entities use `UmbSubmitWorkspaceAction` directly (no custom action class needed). Complex entities like documents define custom action classes for variant dialogs, permission checks, etc.

---

## Route Patterns

Routable workspaces define internal routes in the context constructor via `this.routes.setRoutes()`. Two patterns exist:

- **Simple** (flat entities) — `create` route with fixed parent, `edit/:unique` route for existing entities
- **Parent-aware** (tree entities) — `create/parent/:entityType/:parentUnique` route that extracts parent from URL params

Both patterns use **`UmbWorkspaceIsNewRedirectController`** in the create route — it observes the workspace's `isNew` state and replaces the URL from `/create` to `/edit/:unique` after the first save. The edit route must call `removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias)` to clean up.

File: `src/packages/core/workspace/controllers/workspace-is-new-redirect.controller.ts`

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

Every routable workspace exposes a typed context token so views, actions, and workspace context extensions can consume it with type safety. The token uses `UmbContextToken` with two generics: the base interface (`UmbSubmittableWorkspaceContext`) and the concrete context class. The context string is always `'UmbWorkspaceContext'`, and a discriminator function checks `getEntityType()` to find the right workspace context in the DOM tree.

Re-export the token from `constants.ts` for the public API. Default workspaces don't need a custom context token — they use `UMB_WORKSPACE_CONTEXT` from core.

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

| Kind | Complexity | Path | Notes |
|---|---|---|---|
| `default` | — | `src/packages/webhook/webhook-root/workspace/` | Root + collection view |
| `routable` | Simple | `src/packages/webhook/webhook/workspace/` | Single view, flat data |
| `routable` | Medium | `src/packages/data-type/workspace/` | Two views, invariant dataset |
| `routable` | Complex | `src/packages/documents/documents/workspace/` | Variants, publishing, permissions |

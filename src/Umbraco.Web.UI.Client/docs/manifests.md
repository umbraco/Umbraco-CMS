# Manifests & Aliases

Every extension in the Backoffice is described by a **manifest** — a plain object that declares what the extension is, how it behaves, and when it should be active. Manifests are the fundamental building blocks of the extension-first architecture: all UI (sections, dashboards, property editors, workspaces, trees, actions) are registered via manifests. Any default behavior can be replaced, overridden, or removed.

**Prerequisites**: [Architecture](./architecture.md)

---

## Manifest Shape

All manifests extend `ManifestBase`:

```typescript
interface ManifestBase {
  type: string;       // Extension type (e.g., 'dashboard', 'workspace', 'entityAction')
  alias: string;      // Unique identifier — the extension's address in the registry
  name: string;       // Human-readable label (metadata, not used for lookup)
  kind?: string;      // Optional kind — inherits a manifest base from a kind-manifest
  weight?: number;    // Ordering — higher numbers appear first (default: 0)
}
```

Most extension types add further properties on top of `ManifestBase`:

- **`element`** / **`js`** — lazy-loaded UI component: `element: () => import('./my-element.js')`
- **`api`** — lazy-loaded API class (contexts, actions, conditions): `api: () => import('./my-api.js')`
- **`meta`** — type-specific configuration object (label, icon, pathname, repository aliases, etc.)
- **`conditions`** — declarative rules for when the extension is active
- **`overwrites`** — alias(es) of extensions this one replaces

### Minimal Example

```typescript
const manifest: UmbExtensionManifest = {
  type: 'dashboard',
  alias: 'My.Dashboard.Analytics',
  name: 'Analytics Dashboard',
  element: () => import('./analytics-dashboard.element.js'),
  weight: 100,
  meta: {
    label: 'Analytics',
    pathname: 'analytics',
  },
  conditions: [
    { alias: 'Umb.Condition.SectionAlias', match: 'Umb.Section.Content' },
  ],
};
```

---

## The Alias

The **alias** is the single most important property on a manifest. It is:

- **Required** — registration is rejected without it (console error, no exception thrown)
- **Unique** — duplicate aliases are rejected; the second registration is ignored (console error, no exception thrown)
- **The reference handle** — other manifests point to an extension by its alias (conditions, `overwrites`, `forWorkspaceActions`, repository aliases in `meta`)
- **Stable** — changing an alias is a breaking change because external consumers may reference it

### Alias vs. Name

| Property | Purpose | Used for lookup | Example |
|----------|---------|-----------------|---------|
| `alias` | Unique identifier | Yes | `'Umb.Workspace.Document'` |
| `name` | Human-readable description | No | `'Document Workspace'` |

The `name` is metadata for debugging and documentation. The `alias` is the functional identity.

### Naming Convention

Aliases are **dot-separated PascalCase segments**:

```
{Domain}.{ExtensionType}.{Subject}[.{Discriminator}]
```

- **Domain** — `Umb` for all core extensions. Third-party packages use their own prefix (company name, product name). **Exception**: `propertyEditorSchema` aliases use `Umbraco` as the domain (e.g., `Umbraco.Plain.String`) — these mirror server-side schema aliases.
- **ExtensionType** — the extension type in PascalCase (e.g., `Workspace`, `EntityAction`, `WorkspaceView`). For kind definitions, this becomes two segments: `Kind.{ExtensionType}` (e.g., `Kind.EntityAction`, `Kind.WorkspaceAction`).
- **Subject** — the entity type (e.g., `Document`, `Media`, `User`) or feature name (e.g., `HealthCheck`, `WorkspaceAlias`, `Clipboard`) depending on the extension type.
- **Discriminator** (optional) — distinguishes multiple extensions of the same type for the same subject (e.g., `Delete`, `Detail`, `Save`).

**Examples**:

| Alias | Domain | ExtensionType | Subject | Discriminator |
|-------|--------|---------------|---------|---------------|
| `Umb.Workspace.Document` | `Umb` | `Workspace` | `Document` | — |
| `Umb.EntityAction.Document.Delete` | `Umb` | `EntityAction` | `Document` | `Delete` |
| `Umb.Repository.Document.Detail` | `Umb` | `Repository` | `Document` | `Detail` |
| `Umb.Kind.EntityAction.Delete` | `Umb` | `Kind.EntityAction` | `Delete` | — |
| `Umb.Kind.WorkspaceAction.Default` | `Umb` | `Kind.WorkspaceAction` | `Default` | — |
| `Umb.Condition.WorkspaceAlias` | `Umb` | `Condition` | `WorkspaceAlias` | — |
| `Umb.Dashboard.HealthCheck` | `Umb` | `Dashboard` | `HealthCheck` | — |
| `Umb.Modal.BlockCatalogue` | `Umb` | `Modal` | `BlockCatalogue` | — |
| `Umb.GlobalContext.Clipboard` | `Umb` | `GlobalContext` | `Clipboard` | — |
| `Umb.Bundle.Documents` | `Umb` | `Bundle` | `Documents` | — |

### Alias Constants

Aliases that are referenced by other code are defined as exported constants in a `constants.ts` file:

```typescript
// src/packages/documents/documents/workspace/constants.ts
export const UMB_DOCUMENT_WORKSPACE_ALIAS = 'Umb.Workspace.Document';
```

**Naming**: `UMB_{DOMAIN}_{TYPE}_ALIAS` — all caps, `UMB_` prefix, matches the alias structure.

Constants are imported wherever the alias is referenced — in manifest definitions, conditions, workspace context constructors, and action configurations:

```typescript
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from './constants.js';

// In a manifest
{ alias: UMB_DOCUMENT_WORKSPACE_ALIAS, ... }

// In a condition
{ alias: UMB_WORKSPACE_CONDITION_ALIAS, match: UMB_DOCUMENT_WORKSPACE_ALIAS }

// In a workspace context constructor
super(host, { workspaceAlias: UMB_DOCUMENT_WORKSPACE_ALIAS, ... });
```

When an alias is only used within its own manifest declaration and never referenced elsewhere, a constant is not necessary — the string literal in the manifest is sufficient.

---

## How Aliases Connect Extensions

Aliases are the glue between independently-registered extensions. One extension references another by alias to create relationships at runtime.

### Conditions — Scoping to a Context

Conditions use aliases in two ways: the condition's own `alias` identifies the condition type, and its config properties (`match`, `oneOf`) reference the alias of the extension being matched:

```typescript
conditions: [
  // "Only active when the current workspace alias is Umb.Workspace.Document"
  { alias: 'Umb.Condition.WorkspaceAlias', match: 'Umb.Workspace.Document' },

  // "Only active in the Content or Media section"
  { alias: 'Umb.Condition.SectionAlias', oneOf: ['Umb.Section.Content', 'Umb.Section.Media'] },
]
```

Common conditions and their alias references:

| Condition | Config | References |
|-----------|--------|------------|
| `Umb.Condition.WorkspaceAlias` | `match` / `oneOf` | Workspace alias |
| `Umb.Condition.SectionAlias` | `match` / `oneOf` | Section alias |
| `Umb.Condition.WorkspaceEntityType` | `match` / `oneOf` | Entity type string |
| `Umb.Condition.WorkspaceContentTypeAlias` | `match` / `oneOf` | Content type alias |

### Repository Aliases in Meta

Actions and kinds reference repositories by alias in their `meta` configuration:

```typescript
{
  type: 'entityAction',
  kind: 'delete',
  alias: 'Umb.EntityAction.Document.Delete',
  forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
  meta: {
    detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,  // 'Umb.Repository.Document.Detail'
    itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,      // 'Umb.Repository.Document.Item'
  },
}
```

The action's implementation resolves the repository from the extension registry at runtime using these aliases. This decouples the action from specific repository implementations.

### forWorkspaceActions — Linking Menu Items to Actions

`workspaceActionMenuItem` extensions attach to workspace actions by alias:

```typescript
{
  type: 'workspaceActionMenuItem',
  alias: 'Umb.Document.WorkspaceActionMenuItem.Unpublish',
  forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPublish',  // target action alias
  meta: { label: '#actions_unpublish' },
}
```

The workspace action element filters menu items by matching `forWorkspaceActions` against its own alias.

### Overwrites — Replacing Extensions

The `overwrites` property references one or more aliases to replace:

```typescript
{
  type: 'entitySign',
  alias: 'Umb.EntitySign.Document.HasScheduledPublish',
  overwrites: 'Umb.EntitySign.Document.HasPendingChanges',  // replaces this extension
  // ...
}
```

When an extension with `overwrites` is permitted (conditions pass), the overwritten extension is removed from the active set. If the overwriting extension's conditions become false, the original reappears. Accepts a single alias string or an array of aliases.

---

## Registration

Three mechanisms exist for registering manifests, appropriate for different scenarios.

### Package Bundles (Internal Packages)

The standard pattern for packages within the monorepo. A `umbraco-package.ts` exports a bundle manifest that lazy-loads the package's manifests:

```typescript
// umbraco-package.ts
export const extensions = [
  {
    type: 'bundle',
    alias: 'Umb.Bundle.Documents',
    name: 'Documents Bundle',
    js: () => import('./manifests.js'),
  },
];
```

#### Manifest Bundling

Each sub-feature exports its own `manifests` array. The package-level `manifests.ts` aggregates them:

```typescript
import { manifests as sectionManifests } from './section/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';
export const manifests = [...sectionManifests, ...dashboardManifests];
```

### Static Package Manifest (External Packages)

External packages use a `umbraco-package.json` file — a static JSON manifest discovered by the server.

### Runtime Registration

For dynamic or programmatic registration:

```typescript
umbExtensionsRegistry.register(manifest);
umbExtensionsRegistry.registerMany(manifests);
```

---

## Kinds

Kinds are **reusable base manifests** for an extension type. A kind provides pre-built `element`, `api`, and/or `meta` that extensions inherit — extensions only specify what differs.

Kinds are documented in detail in [Architecture — Kinds](./architecture.md#kinds).

**How kind matching uses aliases**: A kind manifest has its own `alias` (e.g., `'Umb.Kind.EntityAction.Delete'`), plus `matchType` and `matchKind` that tell the registry which extensions to merge it with. The extension references the kind by its `kind` property (e.g., `kind: 'delete'`), not by the kind's alias.

```typescript
// Kind definition — has its own alias, matches by type+kind
{
  type: 'kind',
  alias: 'Umb.Kind.EntityAction.Delete',
  matchType: 'entityAction',
  matchKind: 'delete',
  manifest: { /* defaults */ },
}

// Extension using the kind — references 'delete', not the kind alias
{
  type: 'entityAction',
  kind: 'delete',
  alias: 'Umb.EntityAction.Document.Delete',
  meta: { /* overrides */ },
}
```

The registry merges them: kind properties are the base, extension properties override, and `meta` is shallow-merged (extension meta extends kind meta).

---

## Key Extension Types

For reference, the most common extension types and their typical alias patterns:

| Extension Type | Alias Pattern | Key Properties |
|----------------|---------------|----------------|
| `workspace` | `Umb.Workspace.{Domain}` | `kind`, `api`, `meta.entityType` |
| `workspaceView` | `Umb.WorkspaceView.{Domain}.{View}` | `element`, `meta.label`, `meta.pathname`, conditions |
| `workspaceAction` | `Umb.WorkspaceAction.{Domain}.{Action}` | `kind`, `api`, `meta.label`, conditions |
| `workspaceActionMenuItem` | `Umb.{Domain}.WorkspaceActionMenuItem.{Item}` | `forWorkspaceActions`, `api`, `meta.label` |
| `workspaceContext` | `Umb.WorkspaceContext.{Domain}.{Feature}` | `api`, conditions |
| `dashboard` | `Umb.Dashboard.{Name}` | `element`, `meta.label`, `meta.pathname`, conditions |
| `section` | `Umb.Section.{Name}` | `meta.label`, `meta.pathname` |
| `sectionView` | `Umb.SectionView.{Section}.{View}` | `element`, `meta.label`, conditions |
| `entityAction` | `Umb.EntityAction.{Domain}.{Action}` | `kind`, `forEntityTypes`, `meta` |
| `entityBulkAction` | `Umb.EntityBulkAction.{Domain}.{Action}` | `forEntityTypes`, `meta` |
| `tree` | `Umb.Tree.{Domain}` | `meta.repositoryAlias` |
| `treeItem` | `Umb.TreeItem.{Domain}` | `forEntityTypes`, `element` |
| `propertyEditorUi` | `Umb.PropertyEditorUi.{Name}` | `element`, `meta.label`, `meta.icon` |
| `repository` | `Umb.Repository.{Domain}.{Category}` | `api` |
| `store` | `Umb.Store.{Domain}.{Category}` | `api` |
| `condition` | `Umb.Condition.{Name}` | `api` |
| `kind` | `Umb.Kind.{Type}.{Name}` | `matchType`, `matchKind`, `manifest` |
| `bundle` | `Umb.Bundle.{Package}` | `js` (lazy import) |
| `globalContext` | `Umb.GlobalContext.{Name}` | `api` |
| `modal` | `Umb.Modal.{Name}` | `element` |

---

## Files & Source

| File | Purpose |
|------|---------|
| `src/libs/extension-api/types/manifest-base.interface.ts` | `ManifestBase` — defines `alias`, `type`, `name`, `kind`, `weight` |
| `src/libs/extension-api/types/manifest-kind.interface.ts` | `ManifestKind` — defines kind structure with `matchType`, `matchKind` |
| `src/libs/extension-api/types/condition.types.ts` | `ManifestWithDynamicConditions` — adds `conditions` and `overwrites` |
| `src/libs/extension-api/registry/extension.registry.ts` | `UmbExtensionRegistry` — registration, validation, lookup, kind merging |
| `src/libs/extension-api/controller/base-extension-initializer.controller.ts` | Single extension lifecycle, condition evaluation, `overwrites` handling |
| `src/libs/extension-api/controller/base-extensions-initializer.controller.ts` | Multi-extension lifecycle, overwrite resolution across extensions |

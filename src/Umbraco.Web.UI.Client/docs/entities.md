# Entities & Entity Types

The entity system is the **primary discriminator** that ties together workspaces, trees, actions, pickers, and routing. Most extension types in the backoffice are scoped to one or more entity types.

---

## Core Concept

An **entity** is anything in the backoffice that needs to be identified and have extensions scoped to it. This includes obvious things like documents, media, and users, but also structural concepts like tree roots (`document-root`), folders (`data-type-folder`), and virtual groupings (`clipboard-root`, `extension-root`). Every entity is identified by two values:

```typescript
interface UmbEntityModel {
  entityType: string;       // Type discriminator (e.g., 'document', 'media-root', 'data-type-folder')
  unique: string | null;    // Instance identifier (GUID string, or null for roots/singletons)
}
```

**entityType** is a string constant that classifies what something is. The extension system uses it to determine which actions, tree items, workspaces, and other extensions apply. It is the glue between independently-registered extensions.

---

## Defining Entity Types

Each domain package defines its entity type constants in an `entity.ts` file:

```typescript
// src/packages/documents/documents/entity.ts
export const UMB_DOCUMENT_ENTITY_TYPE = 'document';
export const UMB_DOCUMENT_ROOT_ENTITY_TYPE = 'document-root';

export type UmbDocumentEntityType = typeof UMB_DOCUMENT_ENTITY_TYPE;
export type UmbDocumentRootEntityType = typeof UMB_DOCUMENT_ROOT_ENTITY_TYPE;
```

**Conventions:**
- Constant: `UMB_{DOMAIN}_ENTITY_TYPE` (e.g., `UMB_MEDIA_ENTITY_TYPE`, `UMB_USER_ENTITY_TYPE`)
- Root type: `UMB_{DOMAIN}_ROOT_ENTITY_TYPE` — represents the tree root (unique is `null`)
- TypeScript type: `Umb{Domain}EntityType` — literal type derived via `typeof` for type-safe filtering
- Union type: `Umb{Domain}EntityTypeUnion` — combines all entity types for a domain

---

## Entity Context

`UmbEntityContext` (provided via `UMB_ENTITY_CONTEXT`) propagates the current entity's type and unique through the component tree. It is set by workspaces, tree items, and other entity-aware containers.

```typescript
// Consuming entity context in an element
this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
  this.observe(context.entityType, (entityType) => { /* react to type */ });
  this.observe(context.unique, (unique) => { /* react to identity */ });
});
```

Key file: `src/packages/core/entity/entity.context.ts`

---

## How Entity Types Connect Extensions

Entity types are the **primary filter** for several extension types. Extensions declare which entity types they support via `forEntityTypes: string[]`:

### Entity Actions

Actions that appear in context menus and action bars for specific entity types:

```typescript
{
  type: 'entityAction',
  kind: 'delete',
  alias: 'Umb.EntityAction.Document.Delete',
  forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
  meta: { /* ... */ },
}
```

### Tree Items

Custom tree item renderers for specific entity types:

```typescript
{
  type: 'treeItem',
  alias: 'Umb.TreeItem.Document',
  forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
  element: () => import('./document-tree-item.element.js'),
}
```

### Entity Bulk Actions

Bulk operations on multiple selected entities of a type:

```typescript
{
  type: 'entityBulkAction',
  alias: 'Umb.EntityBulkAction.Document.Delete',
  forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
}
```

### Entity Item Refs

Custom renderers for entity references (used in pickers, relation displays):

```typescript
{
  type: 'entityItemRef',
  alias: 'Umb.EntityItemRef.Member',
  forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
  element: () => import('./member-item-ref.element.js'),
}
```

At runtime, the system filters extensions by checking `extension.forEntityTypes.includes(currentEntityType)`.

---

## Workspaces & Routing

Workspaces register with a specific entity type in their manifest `meta`:

```typescript
{
  type: 'workspace',
  kind: 'routable',
  alias: 'Umb.Workspace.Document',
  meta: {
    entityType: UMB_DOCUMENT_ENTITY_TYPE,   // Routes to this workspace
  },
}
```

Entity type is part of the URL path pattern:

```
/section/{sectionName}/workspace/{entityType}/edit/{unique}
```

For example: `/section/content/workspace/document/edit/abc-123`

The workspace context sets the entity type on `UMB_ENTITY_CONTEXT`, making it available to all child extensions (views, actions, sidebar apps).

---

## Entity Type Conditions

Two built-in conditions control extension visibility based on entity state:

**EntityTypeCondition** — matches the current entity context's type:
```typescript
conditions: [
  { alias: 'Umb.Condition.EntityType', match: 'document' }
]
// or match multiple:
conditions: [
  { alias: 'Umb.Condition.EntityType', oneOf: ['document', 'media'] }
]
```

**WorkspaceEntityTypeCondition** — matches the workspace's entity type (useful for workspace views/actions that should only appear for specific entity types):
```typescript
conditions: [
  { alias: 'Umb.Condition.WorkspaceEntityType', match: 'document' }
]
```

---

## Adding a New Entity Type

When creating a new domain feature, define its entity type first — everything else builds on it:

1. **Define constants** in `entity.ts` (type constant + root type constant + TypeScript types)
2. **Register extensions** with `forEntityTypes` pointing to your constant (tree items, actions, item refs)
3. **Register workspace** with `meta.entityType` pointing to your constant
4. **Define paths** using `UMB_WORKSPACE_PATH_PATTERN` with your entity type
5. **Use entity context** in workspace contexts and elements to propagate identity

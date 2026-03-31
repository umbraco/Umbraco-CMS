# Repositories

How data access is organized across features. Repositories separate **where data comes from** from the UI that consumes it — the UI never knows whether data comes from a server API, a manifest, or a local cache.

For the full data flow chain (element → context → repository → data source → API client), see [Data Flow](./data-Flow.md). This document focuses on how repositories are structured, categorized, and organized within the codebase.

---

## Core Concept

A repository is a **domain-specific, feature-scoped** data access layer. It provides a clean interface for a specific operation (CRUD, listing, tree navigation, or a specialized action) without exposing transport details.

**Key principles:**

- **Feature-scoped** — A repository lives with the feature that uses it, both in naming and file location. A publishing repository lives inside the publishing feature folder, not in a generic repository folder.
- **Extension-registered** — Repositories are registered as `type: 'repository'` extensions with lazy-loaded `api` imports. Any extension can override a repository by registering the same alias with a higher weight.
- **Data source delegation** — The repository orchestrates but doesn't call APIs directly. It delegates to a data source that handles mapping between server types and domain models. See [Data Flow](./data-Flow.md) for the full delegation pattern.
- **One concern per repository** — A detail repository handles CRUD. A publishing repository handles publish/unpublish. Don't mix concerns.

---

## Repository Categories

Repositories fall into two groups: **standard** (backed by base classes) and **action-specific** (custom, feature-driven).

### Standard Repositories

These have base classes in `src/packages/core/` that handle the boilerplate.

| Category | Base Class | Purpose | Interface |
|----------|-----------|---------|-----------|
| **Detail** | `UmbDetailRepositoryBase<T>` | CRUD for a single entity | `UmbDetailRepository<T>` |
| **Item** | `UmbItemRepositoryBase<T>` | Batch fetch multiple items by unique IDs | `UmbItemRepository<T>` |
| **Tree** | `UmbTreeRepositoryBase<T, R>` | Hierarchical navigation (root, children, ancestors) | `UmbTreeRepository<T, R>` |
| **Collection** | *(no base class — extend `UmbRepositoryBase`)* | Paginated/filtered lists | `UmbCollectionRepository<T, F>` |

**Detail** is the most common — every entity that can be created, read, updated, or deleted needs one.

**Item** is for lightweight lookups — fetching display info (name, icon, entity type) for a set of known IDs. Used by pickers, references, and breadcrumbs.

**Tree** is for entities with hierarchical navigation in the sidebar — documents, media, document types.

**Collection** is for flat or filtered lists — the grid/table view of entities.

### Action-Specific Repositories

These are custom repositories for operations that don't fit CRUD. They extend `UmbRepositoryBase` or `UmbControllerBase` directly and define their own methods.

Examples from the document entity:

| Repository | Purpose | Methods |
|-----------|---------|---------|
| `UmbDocumentPublishingRepository` | Publish/unpublish variants | `publish()`, `unpublish()`, `publishWithDescendants()`, `published()` |
| `UmbDuplicateDocumentRepository` | Duplicate a document | `requestDuplicate()` |
| `UmbDocumentCultureAndHostnamesRepository` | Domain/culture assignments | `readCultureAndHostnames()`, `updateCultureAndHostnames()` |
| `UmbBulkMoveToDocumentRepository` | Bulk move action | `requestBulkMoveTo()` |

Action-specific repositories follow the same data source delegation pattern but define domain-appropriate methods instead of generic CRUD.

---

## File Structure & Naming

Repositories live **with their feature**, not in a separate data-access layer. The folder structure mirrors the feature hierarchy.

### Standard repositories (detail, item)

```
{package}/{entity}/
├── detail/
│   └── data/
│       ├── {entity}-detail.repository.ts
│       ├── {entity}-detail.server.data-source.ts
│       ├── {entity}-detail.store.ts
│       ├── {entity}-detail.store.context-token.ts
│       ├── manifests.ts
│       └── constants.ts
├── item/                                  # Optional — only if entity needs item lookups
│   └── data/
│       ├── {entity}-item.repository.ts
│       ├── {entity}-item.server.data-source.ts
│       ├── {entity}-item.store.ts
│       ├── {entity}-item.store.context-token.ts
│       └── manifests.ts
```

### Tree repository

```
{package}/{entity}/
├── tree/
│   └── data/
│       ├── {entity}-tree.repository.ts
│       ├── {entity}-tree.server.data-source.ts
│       ├── {entity}-tree.store.context-token.ts
│       └── manifests.ts
```

### Collection repository

```
{package}/{entity}/
├── collection/
│   └── data/
│       ├── {entity}-collection.repository.ts
│       ├── {entity}-collection.server.data-source.ts
│       └── manifests.ts
```

### Action-specific repositories

```
{package}/{entity}/
├── entity-actions/
│   ├── duplicate/
│   │   └── data/
│   │       ├── {entity}-duplicate.repository.ts
│   │       ├── {entity}-duplicate.server.data-source.ts
│   │       └── types.ts
│   ├── move-to/
│   │   └── data/
│   │       └── ...
│   └── culture-and-hostnames/
│       └── data/
│           └── ...
├── entity-bulk-actions/
│   ├── move-to/
│   │   └── data/
│   │       └── ...
│   └── duplicate-to/
│       └── data/
│           └── ...
├── publishing/
│   └── data/
│       ├── {entity}-publishing.repository.ts
│       └── {entity}-publishing.server.data-source.ts
```

### Naming conventions

| Item | Pattern | Example |
|------|---------|---------|
| Repository class | `Umb{EntityName}{Category}Repository` | `UmbDocumentDetailRepository` |
| Data source class | `Umb{EntityName}{Category}ServerDataSource` | `UmbDocumentDetailServerDataSource` |
| Repository file | `{entity}-{category}.repository.ts` | `document-detail.repository.ts` |
| Data source file | `{entity}-{category}.server.data-source.ts` | `document-detail.server.data-source.ts` |
| Manifest alias | `Umb.Repository.{EntityName}.{Category}` | `Umb.Repository.Document.Detail` |
| Store alias | `Umb.Store.{EntityName}.{Category}` | `Umb.Store.Document.Detail` |

---

## When to Create Which Repository Type

| Scenario | Repository type | Base class |
|----------|----------------|------------|
| Entity with create/read/update/delete | Detail | `UmbDetailRepositoryBase<T>` |
| Fetching display info for known IDs (pickers, breadcrumbs) | Item | `UmbItemRepositoryBase<T>` |
| Sidebar tree navigation | Tree | `UmbTreeRepositoryBase<T, R>` |
| Paginated/filtered listing | Collection | `UmbRepositoryBase` + `UmbCollectionRepository<T>` |
| Domain action (publish, duplicate, move, sort) | Action-specific | `UmbRepositoryBase` or `UmbControllerBase` |

**Rule of thumb:** If there's a base class for it, use it. If the operation is unique to the domain, create a custom repository with just the methods needed.

---

## Reference Examples

Study these when implementing repositories:

| Pattern | Example path | What it shows |
|---------|-------------|---------------|
| Simple detail + item | `src/packages/webhook/webhook/repository/` | Minimal CRUD entity |
| Full entity (detail + item + tree + collection) | `src/packages/documents/documents/` | All repository categories |
| Action-specific (simple) | `src/packages/documents/documents/entity-actions/duplicate/repository/` | Single-action with notification |
| Action-specific (domain-rich) | `src/packages/documents/documents/publishing/repository/` | Multiple domain methods |
| Action-specific (read/write) | `src/packages/documents/documents/entity-actions/culture-and-hostnames/repository/` | Custom read + update |
| Data type as reference | `src/packages/data-type/` | Detail + item + tree + collection |

---

## Key Rules

1. **Repositories live with their feature** — colocated in the feature's directory, not in a shared `repositories/` folder
2. **One concern per repository** — detail CRUD, publishing, duplication, and tree navigation are separate repositories
3. **Use base classes when they exist** — only create custom repositories for operations without a base class
4. **Always delegate to a data source** — see [Data Flow](./data-Flow.md) for the delegation pattern, `tryExecute`, and `{ data, error }` tuple conventions

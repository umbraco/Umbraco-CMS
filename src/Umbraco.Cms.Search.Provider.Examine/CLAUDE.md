# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with the Examine provider client code in this project.

**See also:** [Umbraco Search CLAUDE.md](../Umbraco.Cms.Search.Core/CLAUDE.md) for server-side architecture and build commands.

## Overview

The Examine provider client adds a **"Show Fields"** feature to the Search backoffice UI. When a user searches for documents in the Core Client's search results, each row shows an entity action that opens a **routable sidebar modal** showing all indexed fields for that document.

The modal is **deep-linkable** — its URL includes the `documentUnique` and `culture`, so users can share or bookmark direct links to a document's fields view.

This is a simpler client than the Core Client — it uses a **single-bundle pattern** (no importmap, no code-splitting across multiple entry points).

## Build Commands

This client is a **standalone npm project**. All config (`package.json`, `tsconfig.json` extending the local `tsconfig.base.json`, `.prettierrc.json`, `.nvmrc`) lives in the `Client/` folder.

```bash
# From this project's Client/ directory
cd Client

# Install
npm install

# Build
npm run build

# Watch
npm run watch

# Generate OpenAPI client (requires a running site at https://localhost:44324)
npm run generate-client
```

**Requirements:** Node.js 24 (see `Client/.nvmrc`)

## Architecture

### Bundle Pattern

Uses Umbraco's **bundle extension type** — a single `umbraco-package.json` declares a bundle, and the bundle JS file exports a `manifests` array:

```
Client/
├── src/
│   ├── api/                              (Generated OpenAPI client)
│   ├── lang/                             (Localization files)
│   ├── examine-bundle.ts                 (Entry point - re-exports manifests)
│   ├── examine-bundle.manifests.ts       (Extension manifests array + type augmentation)
│   ├── examine-provider.repository.ts    (API calls to Examine endpoints)
│   ├── fields-route-provider.element.ts  (Non-visual route provider for routable modal)
│   ├── show-fields.entity-action.ts      (Entity action with getHref() for routable links)
│   ├── show-fields.modal.ts              (Sidebar modal orchestrating field display)
│   ├── document-fields.element.ts        (Field table with filtering and expand/collapse)
│   └── types.ts                          (Shared type definitions)
├── public/
│   └── umbraco-package.json              (Bundle declaration)
├── package.json
├── tsconfig.json
├── vite.config.ts
├── eslint.config.js
└── .prettierignore
```

### How It Works

1. **`umbraco-package.json`** declares a bundle extension pointing to `examine-bundle.js`
2. **`examine-bundle.ts`** re-exports the `manifests` array from `examine-bundle.manifests.ts`
3. **`examine-bundle.manifests.ts`** registers three extensions:
   - An **entity action** (`entityAction`) for entity type `search-document` with lazy-loaded `api`
   - A **modal** (`modal`) with lazy-loaded `element`
   - A **searchIndexDetailBox** (`searchIndexDetailBox`) — non-visual route provider, lazy-loaded
4. Vite code-splits: the bundle is small, with lazy chunks for the action, modal, and route provider loaded on demand

### Routable Modal Architecture

The modal uses `UmbModalRouteRegistrationController` for URL-based routing. This is split across two files:

**`fields-route-provider.element.ts`** — Non-visual `searchIndexDetailBox` rendered inside the workspace detail view:
- Registers `UmbModalRouteRegistrationController` with the modal alias and path params `:documentUnique/:culture`
- Gets the index alias from `UMB_ENTITY_WORKSPACE_CONTEXT` (the workspace's entity unique = index alias)
- Exports `fieldsRouteBuilder` at **module level** so the entity action can import it
- Renders `nothing` — purely a route registration host
- Cleans up `fieldsRouteBuilder` on `disconnectedCallback()`

**`show-fields.entity-action.ts`** — Entity action that provides a routable URL:
- Imports `fieldsRouteBuilder` from the route provider module
- Imports `UMB_SEARCH_WORKSPACE_CONTEXT` from `@umbraco-cms/search/settings` (via tsconfig path mapping)
- Implements `getHref()` (not `execute()`) to return a URL built from `documentUnique` and `culture`
- Reads culture from the workspace context via `getSelectedCulture()` (not from URL params)
- The Core Client renders this as a navigable link via `umb-entity-actions-table-column-view`

**Flow (click-triggered):**
1. User clicks "Show Fields" link → browser navigates to the route builder URL
2. Route provider's controller detects URL change, calls `onSetup` with params
3. Modal opens as sidebar with `documentUnique`, `indexAlias`, and `culture` from URL

**Flow (deep-linked):**
1. User enters URL directly → workspace loads → detail view renders route provider
2. Controller matches URL immediately → modal opens

### Key Files

**`examine-provider.repository.ts`** — Repository for Examine-specific API calls:
- `requestSearchDocument(unique, indexAlias)` — Fetches all indexed fields for a specific document
- Returns culture-variant documents (multiple `ExamineIndexDocument` entries per content item)
- Uses `tryExecute()` for error handling

**`show-fields.modal.ts`** — Sidebar modal orchestrating the field display:
- Extends `UmbModalBaseElement<ShowFieldsModalData>` — receives `documentUnique`, `indexAlias`, and `culture` from route params
- Loads field data from repository, groups by culture, shows culture tabs when multiple cultures exist
- Sets the active tab to the preferred culture without reordering tabs (stable tab order)
- Delegates field rendering to `document-fields.element.ts`

**`document-fields.element.ts`** — Reusable field table component:
- Accepts `fields: Array<ExamineField>` property
- Provides search/filter functionality across field names and values
- Expand/collapse for long field values (>100 chars)
- Copy field values to clipboard via `uui-button-copy-text`
- Shows field type info icons
- Multi-value fields display with indexed value labels

**`types.ts`** — Shared type definitions:

```typescript
interface ExamineField {
  name: string;
  type: string;
  values: Array<string>;
}

interface ExamineIndexDocument {
  fields: Array<ExamineField>;
}

interface ExamineDocument {
  documents: Array<ExamineIndexDocument>;
}

interface ShowFieldsModalData {
  documentUnique: string;
  indexAlias: string;
  culture?: string;
}
```

**`examine-bundle.manifests.ts`** — Extension manifests with global type augmentation:
- Declares `searchIndexDetailBox` type via `declare global { interface UmbExtensionManifestMap { ... } }` since this extension type is defined in the Core Client, not in the backoffice SDK

## Configuration Files

### vite.config.ts

Single entry point, outputs to provider's wwwroot:

```typescript
export default defineConfig({
  build: {
    lib: {
      entry: 'src/examine-bundle.ts',
      formats: ['es'],
      fileName: () => 'examine-bundle.js',
    },
    outDir: '../wwwroot/App_Plugins/UmbracoSearchExamine',
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },
});
```

### tsconfig.json

Extends the shared base config. Path mappings allow importing from the Core Client's `@umbraco-cms/search/settings` and `@umbraco-cms/search/global` modules (e.g., `UMB_SEARCH_WORKSPACE_CONTEXT`). Vite externalizes these at build time; the importmap resolves them at runtime:

```json
{
  "extends": "../../tsconfig.json",
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@umbraco-cms/search/settings": ["../../Umbraco.Cms.Search.Core.Client/Client/src/settings/index.ts"],
      "@umbraco-cms/search/global": ["../../Umbraco.Cms.Search.Core.Client/Client/src/global/index.ts"]
    },
    "types": ["@umbraco-cms/backoffice/extension-types"]
  },
  "include": ["src"]
}
```

### public/umbraco-package.json

Simple bundle declaration:

```json
{
  "id": "Umbraco.Cms.Search.Provider.Examine",
  "name": "@umbraco-cms/search/examine",
  "extensions": [
    {
      "type": "bundle",
      "alias": "Umbraco.Cms.Search.Provider.Examine.Bundle",
      "name": "Umbraco Search Examine Bundle",
      "js": "/App_Plugins/UmbracoSearchExamine/examine-bundle.js"
    }
  ]
}
```

## Key Differences from Core Client

| Aspect | Core Client | Examine Client |
|--------|-------------|----------------|
| Bundle strategy | 3 bundles (importmap) | 1 bundle (simple) |
| Entry points | 3 (bundle, global, settings) | 1 (examine-bundle) |
| Code-splitting | Via importmap + lazy imports | Via Vite's built-in chunking |
| tsconfig paths | Yes (logical imports) | Yes (for `@umbraco-cms/search/settings` and `global`) |
| API generation | Yes (OpenAPI) | Yes (OpenAPI, separate Examine swagger) |
| Output directory | `UmbracoSearch/` | `UmbracoSearchExamine/` |

## Common Gotchas

1. **`addAdditionalPath()` Is Not Additive**: `UmbModalRouteRegistrationController.addAdditionalPath()` stores a **single string**. Calling it twice overwrites the first value. Combine multiple params in one call: `.addAdditionalPath(':documentUnique/:culture')`.

2. **`onSetup` With String Alias Requires `value: undefined`**: When using a string modal alias (not a `UmbModalToken`), the `onSetup` return object must include `value: undefined`.

3. **Global Type Augmentation for Cross-Package Extension Types**: The `searchIndexDetailBox` type is defined in the Core Client, not in the backoffice SDK. The Examine Client uses `declare global { interface UmbExtensionManifestMap { ... } }` to make TypeScript recognize it.

4. **Module-Level Route Builder Export**: `fieldsRouteBuilder` is exported as a mutable module-level variable, not a class property. This allows the entity action to import it directly without needing a shared context or event bus.

5. **`getHref()` vs `execute()`**: The entity action uses `getHref()` to provide a routable URL rather than `execute()` with `history.pushState()`. This allows `umb-entity-actions-table-column-view` to render it as a standard navigable link.

6. **Entity Action Culture via Workspace Context**: The entity action reads culture from `UMB_SEARCH_WORKSPACE_CONTEXT.getSelectedCulture()`, not from URL params. The Core Client includes culture in the table item `id` (e.g., `id: \`${doc.unique}_${culture}\``) to force fresh entity action instances when culture changes.

11. **Cross-Package Context Import**: Importing `UMB_SEARCH_WORKSPACE_CONTEXT` from `@umbraco-cms/search/settings` requires tsconfig path mappings for both `settings` and `global` (settings depends on global transitively). ESLint may flag `@typescript-eslint/no-unsafe-argument` and `@typescript-eslint/no-unsafe-call` on the context usage — use eslint-disable comments.

12. **Invariant Culture Value**: The Examine index uses `"none"` as the `Sys_Culture` field for invariant documents. When the entity action has no culture from the workspace context, it falls back to `'none'` to match this convention.

7. **Auth Token**: Use `UMB_AUTH_CONTEXT.getLatestToken()` for authentication, not raw `config.auth()`.

8. **Lit Decorators**: Import `@state()` from `@umbraco-cms/backoffice/external/lit`, not from a separate decorators module.

9. **Bundle Output**: Built files go to `../wwwroot/App_Plugins/UmbracoSearchExamine/` which is gitignored. The `.csproj` serves these as static web assets.

10. **Entity Type**: The entity action registers for `search-document` entity type (set by Core Client when rendering search results).

## Testing

Test through the test site:

1. Run: `dotnet run --project src/Umbraco.Web.TestSite.V17`
2. Navigate to Settings > Search
3. Click on an index, then search for documents
4. Click "Show Fields" on a search result row — modal opens, **URL changes** to include document unique and culture
5. Verify fields load, filtering works, expand/collapse works, copy works
6. Switch culture (e.g., to Danish) → click "Show Fields" → modal shows the selected culture tab first
7. Copy URL while modal is open → open in new tab → **modal opens directly** (deep-link verification)
8. Close modal → URL reverts to search workspace, search results still visible
9. Browser back button closes the modal and returns to search view

For development with watch mode:

```bash
cd src && npm run watch --workspace=Umbraco.Cms.Search.Provider.Examine/Client
```

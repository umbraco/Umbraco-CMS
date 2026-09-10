# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Umbraco Search is a new search abstraction for Umbraco CMS v16+ that will eventually replace the current search implementation. It provides three main capabilities:
1. Frontend search via the `ISearcher` interface
2. Backoffice search
3. Delivery API querying

The project uses a **provider-based architecture** where search technology implementations (currently Examine/Lucene) plug into core abstractions.

## Development Philosophy

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

### 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

### 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

### 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

## Build & Test Commands

These projects are part of the Umbraco-CMS repository and build with the main solution (`umbraco.sln` at the repository root).

### Building

```bash
# Build entire solution (from the repository root)
dotnet build umbraco.sln

# Build specific project
dotnet build src/Umbraco.Cms.Search.Core/Umbraco.Cms.Search.Core.csproj
```

### Running Tests

The search tests live in the main Umbraco test projects: unit tests under
`tests/Umbraco.Tests.UnitTests` (namespace `...Umbraco.Cms.Search.Core`) and integration tests
under `tests/Umbraco.Tests.Integration` (namespaces `...Umbraco.Search.Core`,
`...Umbraco.Search.BackOffice`, `...Umbraco.Search.Provider.Examine`).

```bash
# Run the search unit tests (filtered out of the full unit-test project)
dotnet test tests/Umbraco.Tests.UnitTests/Umbraco.Tests.UnitTests.csproj --filter "FullyQualifiedName~Umbraco.Cms.Search"

# Run the search integration tests (SQLite by default, see appsettings.Tests.json)
dotnet test tests/Umbraco.Tests.Integration/Umbraco.Tests.Integration.csproj --filter "FullyQualifiedName~Umbraco.Search"

# Run a specific test by filter
dotnet test tests/Umbraco.Tests.Integration/Umbraco.Tests.Integration.csproj --filter "FullyQualifiedName~ContentExtensionsTests"
```

### Client Development (Backoffice UI)

The search index management UI is part of the main backoffice client — `src/Umbraco.Web.UI.Client/src/packages/search-management/` — built and versioned together with the rest of the backoffice. It consumes the Management API's generated `SearchService` client (`@umbraco-cms/backoffice/external/backend-api`) like any other package; there is no separate OpenAPI document or generated client for it.

```bash
# From src/Umbraco.Web.UI.Client
npm install
npm run build
npm run dev
```

The Examine provider keeps its own **standalone npm project** at `src/Umbraco.Cms.Search.Provider.Examine/Client/` (its own `package.json`, `tsconfig.json`, `.nvmrc`, and OpenAPI document) — see [Examine Client CLAUDE.md](../Umbraco.Cms.Search.Provider.Examine/CLAUDE.md). That client resolves imports into the backoffice (including `@umbraco-cms/backoffice/search-management`) via generated tsconfig path aliases, same as any other in-repo consumer of the backoffice client's sources.

### Test Site

Use the main development site of this repository to manually test integration:

```bash
cd src/Umbraco.Web.UI
dotnet run
```

The full search stack is registered in the default install via composers: `SearchCoreComposer` (core engine, including the backoffice search services — `IContentSearchService`, `IMediaSearchService`, `IIndexedEntitySearchService` — which now live in `Umbraco.Core/Services`) + `ExamineSearchProviderComposer` (Examine provider). Delivery API content querying (running on the new indexes) is wired up directly by `AddDeliveryApi()` in `Umbraco.Cms.Api.Delivery`. The legacy Examine-based search stack has been fully removed from the codebase — there is no side-by-side legacy indexing and no legacy fallback setting.

## Architecture

### Core Abstractions (Umbraco.Cms.Search.Core)

The core provides **provider-agnostic abstractions**:

**Key Interfaces:**
- `ISearcher` - Search operations (filtering, faceting, sorting, pagination)
- `IIndexer` - Index management (add/update/delete documents, reset indexes)
- `ISearcherResolver` - Resolves the correct searcher implementation for an index alias
- `IContentIndexingService` - Orchestrates content indexing workflows
- `IContentIndexer` - Indexes system fields and property values into search documents

**System Architecture:**
```
ContentIndexingService (orchestration)
    ├─> IContentIndexingDataCollectionService (gathers data from Umbraco)
    ├─> ISystemFieldsContentIndexer (indexes system fields like Id, Name, Path)
    ├─> PropertyValueFieldsContentIndexer (indexes property values)
    │       └─> IPropertyValueHandler collection (type-specific value handlers)
    └─> IIndexer (writes to underlying provider)
```

**Index Aliases** (see `Umbraco.Cms.Core.Constants.IndexAliases` — lives in Core so `IPublishedContentQuery` can reference it without a circular dependency):
- `PublishedContent` = `"Umb_PublishedContent"` - Published content index
- `DraftContent` = `"Umb_Content"` - Draft content index
- `DraftMedia` = `"Umb_Media"` - Media index
- `DraftMembers` = `"Umb_Members"` - Members index

**System Field Names** (see `Constants.FieldNames`):
- All system fields are prefixed with `Umb_`
- Examples: `Umb_Id`, `Umb_Name`, `Umb_ContentTypeId`, `Umb_PathIds`, `Umb_Level`, `Umb_CreateDate`, `Umb_UpdateDate`

### Provider Pattern (Umbraco.Cms.Search.Provider.Examine)

The Examine provider implements the core abstractions using Examine/Lucene:

**Key Classes:**
- `Searcher` - Implements `IExamineSearcher : ISearcher`
- `Indexer` - Implements `IExamineIndexer : IIndexer`
- `ConfigureIndexOptions` - Configures Lucene index settings (field options for faceting/sorting)
- `SearcherOptions` - Configures search behavior (boost factors, facet limits)
- `FieldOptions` - Maps property aliases to index field types (Keywords, Integers, Decimals, DateTimeOffsets)

**Important:** Fields used for faceting/sorting must be configured in `FieldOptions` **before** indexing. Changes require a full index rebuild.

### Property Value Handlers (Umbraco.Cms.Search.Core/PropertyValueHandlers)

Property values are indexed based on property editor type. Each handler implements `IPropertyValueHandler` with a `CanHandle(string propertyEditorAlias)` method that determines which property editors it supports. Handlers are auto-discovered via `TypeLoader.GetTypes<IPropertyValueHandler>()`.

**Key handlers:**
- `ContentPickerPropertyValueHandler` - Extracts content IDs (Keywords)
- `DateTimeOffsetPropertyValueHandler` - Indexes dates as DateTimeOffset
- `IntegerPropertyValueHandler` / `DecimalPropertyValueHandler` - Indexes numeric values
- `RichTextPropertyValueHandler` - Extracts text with relevance levels (H1=R1, H2=R2, H3=R3, body=R4)
- `TagsPropertyValueHandler` - Accumulates tags into `Umb_Tags` system field
- `BlockListPropertyValueHandler` / `BlockGridPropertyValueHandler` - Recursively indexes nested block content (extend `BlockEditorPropertyValueHandler`)
- `KeywordStringPropertyValueHandler` - Exact-match string fields (dropdowns, radio buttons, etc.)
- `PlainStringPropertyValueHandler` - Full-text searchable strings (textbox, textarea)
- `MarkdownPropertyValueHandler` - Strips markdown, indexes as text
- `BooleanPropertyValueHandler`, `LabelPropertyValueHandler`, `SliderPropertyValueHandler`, `MultiNodeTreePickerPropertyValueHandler`, `MultiUrlPickerPropertyValueHandler`, `MultipleTextstringPropertyValueHandler`
- `NoopPropertyValueHandler` - Fallback for unsupported property editors (indexes nothing)

### Change Tracking Strategies

Content changes are tracked via notification handlers that trigger indexing:

- `IContentChangeStrategy` - Base interface for tracking content state changes
- `IPublishedContentChangeStrategy` - Tracks published content changes (for `Umb_PublishedContent` index)
- `IDraftContentChangeStrategy` - Tracks draft content changes (for `Umb_Content` index)

Index documents are persisted via `IndexDocumentRepository` using **MessagePack serialization** (with Lz4 compression) for efficient change detection — only actual field changes trigger re-indexing, not every save.

### Backoffice Integration (Umbraco.Core/Services)

`ContentSearchService`, `MediaSearchService` and `IndexedEntitySearchService` live in `Umbraco.Core/Services` and provide backoffice search using the Search API, querying the `Umb_Content` index (registered by `SearchCoreComposer`/`AddSearchCore()`).

### Delivery API Integration (Umbraco.Cms.Api.Delivery)

`DeliveryApiContentQueryProvider` and `DeliveryApiContentIndexer` live in `Umbraco.Cms.Api.Delivery/Services` and `/Indexing`, registered by that project's own `AddDeliveryApi()`. They query/index the `Umb_PublishedContent` index.

### Client Architecture

#### Search Index Management UI (`Umbraco.Web.UI.Client/src/packages/search-management`)

Lives inside the backoffice client as an ordinary package — no separate bundle strategy, importmap, or npm project. Extensions are lazy-loaded the same way as everywhere else in the backoffice (`api: () => import(...)`, `element: () => import(...)`).

**Two-Workspace Architecture:**
- **Root Workspace** (`Umbraco.Search.Workspace.Root`) - Collection view of all search indexes
- **Detail Workspace** (`Umbraco.Search.Workspace`) - Detail view for a single index with extensible boxes

**Custom Extension Type:**
- `searchIndexDetailBox` - Allows adding custom UI boxes to the index detail view via extension slot (defined in `search-index/index-detail-box/types.ts`; the Examine provider's own client contributes a box to it)

#### Examine Client (Umbraco.Cms.Search.Provider.Examine)

A simpler **single-bundle**, standalone npm workspace (`examine-bundle.js` ~11kb) that provides:
- `UmbSearchExamineProviderRepository` - Fetches search document fields from the Examine API
- `UmbSearchExamineShowFieldsEntityAction` - Entity action to view document fields
- `UmbSearchExamineShowFieldsModal` - Modal displaying indexed fields with filtering, expand/collapse, and copy

Output goes to `wwwroot/App_Plugins/UmbracoSearchExamine/` (gitignored, built by Vite). It resolves `UMB_SEARCH_WORKSPACE_CONTEXT` from `@umbraco-cms/backoffice/search-management` via its generated tsconfig aliases — see [Examine Client CLAUDE.md](../Umbraco.Cms.Search.Provider.Examine/CLAUDE.md).

## Key Concepts

### Index Field Types

Fields are typed based on how they're queried:

- **Text** - Full-text searchable, analyzed, used with `TextFilter`
- **Keyword** - Exact-match filterable, used for IDs and selections, used with `KeywordFilter`
- **Integer** - Numeric exact or range filtering, used with `IntegerExactFilter` or `IntegerRangeFilter`
- **Decimal** - Decimal exact or range filtering, used with `DecimalExactFilter` or `DecimalRangeFilter`
- **DateTimeOffset** - Date exact or range filtering, used with `DateTimeOffsetExactFilter` or `DateTimeOffsetRangeFilter`

**Mismatched filter types and field types will yield zero results.**

### Search Parameters

`ISearcher.SearchAsync` accepts:
- `query` - Full-text search query (searches Text fields)
- `filters` - AND between filters, OR between values within a filter
- `facets` - Generate facet results for fields
- `sorters` - Multi-field sorting (first sorter is primary)
- `culture` / `segment` - Variant content filtering (invariant always included)
- `accessContext` - Protected content access (requires member ID and optional group IDs)
- `skip` / `take` - Pagination

### Variation Handling

Content variations (culture/segment) are indexed as separate documents with variation-specific field naming:
- Invariant fields: `propertyAlias`
- Culture variant: `propertyAlias_cultureName`
- Segment variant: `propertyAlias__segmentName`
- Both: `propertyAlias_cultureName_segmentName`

When searching with `culture`/`segment`, both invariant and variant fields are queried.

### Protected Content

Content with public access restrictions is indexed with `ContentProtection` metadata:
- `AllowedMemberIds` - Specific members with access
- `AllowedMemberGroupIds` - Member groups with access

Pass `AccessContext` to `SearchAsync` to include protected content in results.

## Development Patterns

### Adding a New Property Value Handler

1. Create handler in `src/Umbraco.Cms.Search.Core/PropertyValueHandlers/`
2. Implement `IPropertyValueHandler` interface (specifically the `CanHandle(string propertyEditorAlias)` method)
3. The handler is auto-discovered via `TypeLoader.GetTypes<IPropertyValueHandler>()` — no manual registration needed

### Adding a New Filter Type

1. Create filter model in `src/Umbraco.Cms.Search.Core/Models/Searching/Filtering/`
2. Inherit from `Filter` base class
3. Create provider-specific implementation in `src/Umbraco.Cms.Search.Provider.Examine/Models/Searching/Filtering/`
4. Update `Searcher` to handle the new filter type

### Adding a New Facet Type

1. Create facet model in `src/Umbraco.Cms.Search.Core/Models/Searching/Faceting/`
2. Inherit from `Facet` base class
3. Create provider-specific implementation in provider project
4. Update `Searcher` to handle the new facet type

### Modifying Index Structure

1. Update `IIndexer.AddOrUpdateAsync` signature if needed
2. Update provider implementations (`Indexer` class)
3. **Important:** Document that existing indexes must be rebuilt
4. Update `FieldOptions` configuration if adding facetable/sortable fields
5. Add migration if persisted index metadata changes

### Adding a New Repository (Client)

Repositories abstract API calls and provide clean interfaces for UI components. Follow this pattern (see `src/Umbraco.Web.UI.Client/docs/data-flow.md` and `docs/repositories.md` for the general convention):

1. **Define Domain Types** in `search-index/types.ts`:
   - Create request/response types that abstract away API-generated types
   - Example: `UmbSearchRequest`, `UmbSearchResult`

2. **Create Server Data Source** (e.g., `search-index/query/search-query.server.data-source.ts`):
   - Implements data fetching and type mapping against `SearchService` (`@umbraco-cms/backoffice/external/backend-api`)
   - Maps domain types → API types (for requests)
   - Maps API types → domain types (for responses)
   - Uses `tryExecute()` for error handling

3. **Create Repository** (e.g., `search-index/query/search-query.repository.ts`):
   - Extends `UmbRepositoryBase`
   - Orchestrates data source calls
   - Provides clean API for consumers
   - Example: `async search(request: UmbSearchRequest) { return this.#dataSource.search(request); }`

4. **Register Repository**:
   - Add constant in `search-index/constants.ts`: `export const UMB_SEARCH_QUERY_REPOSITORY_ALIAS = '...'`
   - Export from the sub-feature's `index.ts`
   - Add manifest in the sub-feature's `manifests.ts` (`api: () => import('./search-query.repository.js')`)

5. **Use in Components**:
   - Import the repository class directly, e.g. `import { UmbSearchQueryRepository } from '../query/search-query.repository.js'`
   - Instantiate: `#repository = new UmbSearchQueryRepository(this)`
   - Call methods: `const { data, error } = await this.#repository.search(request)`

**Benefits**: Separation of concerns, testability, type safety, consistency, reusability.

## Testing Strategy

### Unit Tests (`Umbraco.Tests.UnitTests`, namespace `...Umbraco.Cms.Search.Core`)

- Test extensions, helpers, and models in isolation
- Use Moq for dependencies
- Focus on business logic without infrastructure dependencies

### Integration Tests (`Umbraco.Tests.Integration`, namespaces `...Umbraco.Search.Core` and `...Umbraco.Search.BackOffice`)

- Test core services with real Umbraco infrastructure
- Use `Umbraco.Cms.Tests.Integration` base classes
- Test content indexing workflows end-to-end

### Provider Integration Tests (`Umbraco.Tests.Integration`, namespace `...Umbraco.Search.Provider.Examine`)

- Test Examine-specific implementations
- Verify Lucene index behavior
- Test query translation and result mapping

## Common Gotchas

1. **Faceting/Sorting Fields Must Be Pre-Configured**: Fields used for faceting or sorting must be defined in `FieldOptions` before indexing. Changes require full index rebuild.

2. **Filter Type Must Match Field Type**: Using `KeywordFilter` on a `Text` field (or vice versa) returns zero results. Same for numeric and date filters.

3. **Variation Field Naming**: When querying variant content, ensure field names include culture/segment suffixes where appropriate.

4. **Client Import Paths**: Inside `search-management`, import relatively (`../workspace/search-workspace.context-token.js`), the same as any other backoffice package. Only the Examine provider's separate client needs the `@umbraco-cms/backoffice/search-management` alias.

5. **Segment Variant Search**: Known limitation - segment variant content not created in the targeted segment may be excluded from results. This is a bug being addressed.

6. **Entity Actions vs Workspace Actions**: Entity actions automatically appear in workspace header dropdowns. Don't create duplicate workspace actions for the same functionality. Ensure the workspace's `entityType` matches the entity action's registered entity type.

7. **Enum JSON Serialization**: C# enums used in ViewModels should use `[JsonConverter(typeof(JsonStringEnumConverter))]` to serialize as strings instead of numbers. This prevents confusion in the UI where enum values would appear as numbers.

8. **State Management Race Conditions**: When setting loading states for async operations, set the state BEFORE making the API call, not after. This ensures immediate UI feedback and prevents race conditions where the operation completes before the loading state is set.

9. **Server-Driven State**: UI state should be derived from server health status (e.g., `healthStatus: 'Rebuilding'` → `state: 'loading'`). This keeps the UI synchronized with actual server state after reloads.

10. **Invariant Culture in Examine Index**: The Examine provider uses `"none"` as the `Sys_Culture` field value for invariant documents. Sending `culture: "en-US"` to `SearchAsync` searches `Sys_Culture: "en-US" OR "none"`, so invariant content is always included. Sending `culture: null` returns invariant-only. Always send a real culture code from the client; use `"none"` as the fallback for invariant-only contexts.

11. **Culture State on Workspace Context**: The `UmbSearchWorkspaceContext` owns `selectedCulture` state (observable + getter/setter). The search box writes it, entity actions read it via `getContext()`. Don't read culture from `window.location.href` — use the workspace context as the source of truth. URL params (`?culture=`) are for persistence/bookmarking only.

12. **Examine Client Cross-Package Imports**: The Examine Client imports `UMB_SEARCH_WORKSPACE_CONTEXT` from `@umbraco-cms/backoffice/search-management` via its generated tsconfig path mapping — the same generated-alias mechanism it already uses for every other `@umbraco-cms/backoffice/<sub>` import. Vite externalizes these; the browser importmap resolves them at runtime.

## Coding Conventions

- Follow Umbraco CMS coding standards (StyleCop, .editorconfig)
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Use C# 13 features (latest language version)
- Use `IUmbracoBuilder` extension methods for service registration
- Follow async/await patterns consistently
- Use primary constructors for dependency injection
- Prefix all system fields with `Umb_`
- Use strong types for index aliases (`Umbraco.Cms.Core.Constants.IndexAliases`)

## Version & Dependencies

- **Target Framework**: .NET 10.0
- **Umbraco CMS**: built as part of this repository (project references)
- **Examine**: Search provider implementation
- **Node.js**: 24 (for client build)
- **Versioning**: Uses Nerdbank.GitVersioning (see `version.json` at the repository root)

## Further Reading

Related CLAUDE.md files:
- [Repository CLAUDE.md](../../CLAUDE.md) - Umbraco-CMS repository overview, architecture, and workflow
- [Backoffice Client CLAUDE.md](../Umbraco.Web.UI.Client/CLAUDE.md) - Backoffice package conventions; the search index management UI lives at `src/packages/search-management/`
- [Examine Client CLAUDE.md](../Umbraco.Cms.Search.Provider.Examine/CLAUDE.md) - Examine client architecture and development workflow

External references:
- RFC: ["The Future of Search"](https://github.com/umbraco/rfcs/blob/0027-the-future-of-search/cms/0027-the-future-of-search.md)
- Main CMS Repo: [Umbraco-CMS](https://github.com/umbraco/Umbraco-CMS)
- Examine: [Shazwazza/Examine](https://github.com/Shazwazza/Examine)

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

# Consolidating the `Umbraco.Cms.Search.*` projects into the core CMS

**Status:** Analysis / proposal
**Target:** Umbraco 19 (pre-release)
**Branch analysed:** `origin/v19/dev` @ `d6c5c283c35` ("V19: Search abstractions in core (#23321)")
**Date:** 19-08-2026
**Decisions recorded:** 25-08-2026 — lead developer review of the arguable items under §6.4 and of §7.1; decided items are marked **Decision** inline

---

## 1. Purpose

Umbraco 19 absorbs the previously separate `Umbraco.Cms.Search` package into the CMS repository as **five** new projects. They were ported largely as-is, which means the project boundaries still reflect the shape of a standalone add-on package rather than the layering of the CMS.

This document reviews every class and namespace in those projects and proposes where each should live once the port is complete: what stays, what dissolves into an existing project, and what should be promoted into `Umbraco.Core` so it is reusable from elsewhere.

---

## 2. Executive summary

The five `Umbraco.Cms.Search.*` projects were ported from a standalone package and are still structured like one, even though they are now the CMS's own search implementation rather than an opt-in alternative to it. Most visibly, `Umbraco.Cms.Search.Core` sits *above* the Management API in the dependency graph, so anything that plugs into search inherits a dependency on the entire backoffice.

The proposal consolidates five projects into two:

- **Promote the contracts to `Umbraco.Core`** — `ISearcher`, `IIndexer`, the resolvers, and the query, index and registration models — following the precedent already set by `Constants.IndexAliases`.
- **Dissolve three projects**: `Search.BackOffice` into `Umbraco.Core/Services`, `Search.DeliveryApi` into `Umbraco.Cms.Api.Delivery` (which already owns the index handlers it consumes), and `Search.Core.Client` into `Umbraco.Web.UI.Client`.
- **Move the search Management API** out of `Search.Core` into `Umbraco.Cms.Api.Management`, collapsing one of three OpenAPI documents into the main one.
- **Keep `Search.Core`** as the provider-agnostic engine — indexing pipeline, granular cache refreshers, change-detection persistence — with its project references reduced from five to one. The built-in property value handlers move down to `Umbraco.Infrastructure` (§6.4.3); the granular cache refreshers stay put, pending a separate decision to retire the core ones (§7.1).
- **Keep `Provider.Examine`** as the swappable provider, and give it ownership of its own settings and JSON schema generation, following the pattern Umbraco Forms already uses.

Three defects surfaced by the review are worth fixing regardless of the wider reorganisation: the provider's stray `version.json`, which makes it build and pack as 18.x while the CMS is 19.x; backoffice child search being implicitly bound to Examine because it injects a bare `ISearcher` that only the Examine provider registers; and `ExamineSearchProviderSettings`, a provider-specific configuration type, living in `Umbraco.Core`.

---

## 3. Current state

### 3.1 The projects

| Project | Description and boundary | Files | Lines (C#) | Ships as |
|---|---|---:|---:|---|
| `Umbraco.Cms.Search.Core` | The provider-agnostic engine — search/indexing contracts, the indexing pipeline, property value handlers, granular cache refreshers and change-detection persistence — but it also owns the search Management API and references the backoffice UI, so its boundary currently extends above the API layer rather than stopping at the abstraction. | 195 | 8,280 | NuGet package |
| `Umbraco.Cms.Search.Core.Client` | The backoffice UI for search index management (index collection, per-index detail workspace, rebuild actions), bounded as a self-contained npm/Vite plugin with its own toolchain, OpenAPI client and importmap bundle rather than as part of the backoffice client. | — | — (TS) | NuGet package (`App_Plugins/UmbracoSearch`) |
| `Umbraco.Cms.Search.Provider.Examine` | The Examine/Lucene implementation of `ISearcher`/`IIndexer` plus its own Management API and UI contribution — the one project whose boundary is genuinely a swap point, since replacing it is how a site changes search technology. | 37 | 2,991 | NuGet package (+ `App_Plugins/UmbracoSearchExamine`) |
| `Umbraco.Cms.Search.BackOffice` | A thin adapter implementing `Umbraco.Core`'s backoffice search contracts (`IContentSearchService`, `IMediaSearchService`, `IIndexedEntitySearchService`) against the search indexes; its boundary encloses only `internal` classes with no second implementation. | 8 | 580 | NuGet package |
| `Umbraco.Cms.Search.DeliveryApi` | A thin adapter implementing `IApiContentQueryProvider` so Delivery API querying runs on the published-content index, plus a Delivery-API-specific content indexer; its boundary separates it from the `IContentIndexHandler` implementations it consumes, which live in `Umbraco.Cms.Api.Delivery`. | 4 | 562 | NuGet package |

`Umbraco.Cms.Search.Core.Client` is easily overlooked: it holds the backoffice UI (TypeScript/Vite, built into `wwwroot/App_Plugins/UmbracoSearch`) and is a hard `ProjectReference` of `Umbraco.Cms.Search.Core`, so it sits in the C# dependency graph rather than alongside it as a standalone front-end asset.

`Umbraco.Cms.Targets` references `Search.BackOffice`, `Search.DeliveryApi` and `Search.Provider.Examine`, so all five are in the default install. The legacy `Umbraco.Examine.Lucene` project and the legacy Examine-based search stack have been **removed** from the repository — there is no side-by-side legacy path and no fallback.

### 3.2 The dependency inversion

`Umbraco.Cms.Search.Core.csproj` references:

```
Umbraco.Cms.Api.Common
Umbraco.Cms.Api.Management
Umbraco.Cms.Search.Core.Client
Umbraco.Infrastructure
Umbraco.Web.Common
```

This is the single most important structural finding. A project named "Core" sits at the **top** of the dependency stack, above the Management API. Every downstream project inherits that:

```
Search.Provider.Examine ─┐
Search.BackOffice       ─┼──> Search.Core ──> Api.Management ──> Api.Common ──> Web.Common ──> Infrastructure ──> Core
Search.DeliveryApi      ─┘                └──> Search.Core.Client (static web assets)
```

Consequences today:

- A third-party provider (Algolia, Elasticsearch, Azure AI Search) that only wants to implement `ISearcher` and `IIndexer` must take a dependency on the entire Management API **and** ship a transitive dependency on the backoffice UI static assets.
- The repository's stated layering rule — *"dependency flow is unidirectional and always flows inward"* — is violated by the project that names itself the core of the subsystem.
- `Constants.IndexAliases` had to be pushed into `Umbraco.Core` (`src/Umbraco.Core/Constants-Indexes.cs`) with an explicit comment that it *"lives in Core so `IPublishedContentQuery` can reference it without a circular dependency"*. That workaround is a symptom of the inversion, not a fix for it.

### 3.3 These projects are now the core implementation, not an alternative to it

Search has always been core functionality. What has changed is *which* implementation provides it: `Umbraco.Cms.Search` began life as an opt-in package that a site installed to use **instead of** the built-in Examine stack, and in V19 it becomes the built-in stack — the legacy implementation and the `Umbraco.Examine.Lucene` project are gone.

The project structure has not caught up with that change. It is still shaped like an opt-in add-on: separately packaged, separately versioned, composed through its own `AddSearchCore()` / `AddBackOfficeSearch()` / `AddDeliveryApiSearch()` entry points, and layered as if the CMS were the host rather than the owner. The result is that three core features now depend on projects that are still packaged as though a site could choose to leave them out:

| Core contract | Declared in | Only implementation |
|---|---|---|
| `IContentSearchService`, `IMediaSearchService`, `IIndexedEntitySearchService` | `Umbraco.Core/Services` | `Umbraco.Cms.Search.BackOffice` |
| `IApiContentQueryProvider` | `Umbraco.Core/DeliveryApi` | `Umbraco.Cms.Search.DeliveryApi` |
| `IPublishedContentQuery.Search(...)` | `Umbraco.Core` | `SearchEnabledPublishedContentQuery` in `Search.Core` |

`Umbraco.Cms.Api.Delivery/Services/ApiContentQueryService.cs` injects `IApiContentQueryProvider`, and the **only** registration of it in the whole solution is `AddDeliveryApiSearch()` in `Umbraco.Cms.Search.DeliveryApi`. Likewise, `content.SearchChildren(...)` / `SearchDescendants(...)` are declared in the `Umbraco.Extensions` namespace but compiled into the `Search.Core` assembly, so Razor templates only compile against them if that package is referenced.

**This is the decisive argument for consolidation.** Code that *is* the core implementation should be structured as core, not as an add-on that core happens to depend on. The pluggability that mattered while the package was an alternative to Examine now belongs one level down, at the *provider* — and that is the one project that should stay separate.

---

## 4. Guiding principles applied

Taken from `/CLAUDE.md` and `/src/Umbraco.Core/CLAUDE.md`:

1. **Dependencies flow inward.** Core defines contracts with no dependencies; Infrastructure implements them; Web/APIs consume them. Nothing below the API layer may reference `Umbraco.Cms.Api.Management`.
2. **Interface-first, general-purpose by default.** A contract that any provider must implement, and any consumer may call, belongs in `Umbraco.Core`.
3. **Services live in `Umbraco.Core/Services` unless a concrete Infrastructure dependency forces otherwise.**
4. **A separate project must earn its keep.** The test is: does something outside the CMS need to reference it on its own, and does keeping it separate buy an enforceable boundary?

Under (4), the Examine provider passes clearly (it is replaceable and carries the `Examine`/Lucene dependency). `Search.BackOffice` and `Search.DeliveryApi` fail — they are thin adapters onto contracts the CMS already owns, there is no second implementation, and nothing outside the CMS has reason to reference either on its own. `Search.Core.Client` fails for the same reason plus a build cost (its own npm project, its own OpenAPI client generation, its own importmap bundle).

---

## 5. Recommended target shape

| Layer | Project | Contents |
|---|---|---|
| Contracts | **`Umbraco.Core`** | `ISearcher`, `IIndexer`, resolvers, query/result/index models, `IPropertyValueHandler` + collection, index registration options, search notifications, the `IPublishedContent` search extensions |
| Built-in handlers | **`Umbraco.Infrastructure`** | Built-in property value handlers (`Search/PropertyValueHandlers`) |
| Engine | **`Umbraco.Cms.Search.Core`** *(kept, slimmed)* | Indexing pipeline, change strategies, granular cache refreshers, index-document persistence, DI |
| Provider | **`Umbraco.Cms.Search.Provider.Examine`** *(kept)* | Examine/Lucene implementation, its own Management API + UI |
| Management API | **`Umbraco.Cms.Api.Management`** | Search + index controllers and view models |
| Delivery API | **`Umbraco.Cms.Api.Delivery`** | Query provider + Delivery API content indexer |
| Backoffice search services | **`Umbraco.Core/Services`** | `ContentSearchService`, `MediaSearchService`, `IndexedEntitySearchService` |
| Backoffice UI | **`Umbraco.Web.UI.Client`** | Search index management section UI |
| **Removed** | `Search.BackOffice`, `Search.DeliveryApi`, `Search.Core.Client` | — |

Net result: **5 projects → 2**, plus contributions to four existing projects, and `Search.Core`'s project references reduced from five to one (`Umbraco.Infrastructure`).

> **Two projects, two destinations.** `Search.BackOffice` and `Search.Core.Client` both disappear, but they do not go to the same place. `Search.BackOffice` is a C# project containing server-side services, so its natural home is `Umbraco.Core/Services` (§6.1). `Search.Core.Client` is the backoffice UI, and that is what belongs in `Umbraco.Web.UI.Client` (§6.3).

---

## 6. Per-project migration

### 6.1 `Umbraco.Cms.Search.BackOffice` — dissolve into `Umbraco.Core`

8 files, 580 lines. Every class is `internal` and implements a contract already declared in `Umbraco.Core/Services`. Dependencies are `IContentService`, `IMediaService`, `IEntityService`, `IIdKeyMap`, `IBackOfficeSecurityAccessor`, `AppCaches` and `ISearcher(Resolver)` — all Core interfaces. Per the repository's documented rule this places them in `Umbraco.Core/Services`, not Infrastructure.

| Class(es) | Target project | Target namespace |
|---|---|---|
| `ContentSearchService`, `MediaSearchService`, `ContentSearchServiceBase<T>` | `Umbraco.Core` | `Umbraco.Cms.Core.Services` |
| `IndexedEntitySearchService`, `IndexedSearchServiceBase` | `Umbraco.Core` | `Umbraco.Cms.Core.Services` |
| `Sorting` (static default sorter) | `Umbraco.Core` | `Umbraco.Cms.Core.Services` (or fold into `IndexedSearchServiceBase`) |
| `BackOfficeSearchComposer`, `UmbracoBuilderExtensions.AddBackOfficeSearch()` | *delete* | registrations fold into the core builder |

**Blocker to fix first.** `ContentSearchService` and `MediaSearchService` inject `ISearcher` **directly**, whereas `IndexedEntitySearchService` correctly injects `ISearcherResolver`. A bare `ISearcher` is registered **only** by the Examine provider (`ServiceCollectionExtensions.AddExamineSearchProviderServices()`); `Search.Core` registers no such fallback. So backoffice child search is silently bound to Examine and would fail to construct under any other provider. These two services must be switched to `ISearcherResolver.GetRequiredSearcher(IndexAlias)` — which they already have the index alias for — before or as part of the move.

The `Umbraco.Cms.Search.BackOffice` package disappears and `AddBackOfficeSearch()` goes with it. Since every service in the project is `internal`, the only externally visible surface being removed is that extension method and the composer.

### 6.2 `Umbraco.Cms.Search.DeliveryApi` — dissolve into `Umbraco.Cms.Api.Delivery`

4 files, 562 lines. Both service classes are `internal`. `Umbraco.Cms.Api.Delivery` already owns the `IContentIndexHandler` implementations these two classes consume (`Indexing/Selectors/*`, `Indexing/Filters/*`, `Indexing/Sorts/*`), so this is reuniting code that was only ever split by packaging.

| Class(es) | Target project | Target namespace |
|---|---|---|
| `DeliveryApiContentQueryProvider` | `Umbraco.Cms.Api.Delivery` | `Umbraco.Cms.Api.Delivery.Services` |
| `DeliveryApiContentIndexer` | `Umbraco.Cms.Api.Delivery` | `Umbraco.Cms.Api.Delivery.Indexing` |
| `DeliveryApiSearchComposer`, `AddDeliveryApiSearch()` | *delete* | fold into `AddDeliveryApi()` |

Three clean-ups this move enables:

- The `DeliveryApiSearchComposer` currently probes `builder.Services` for `IApiContentQueryService` to detect whether the Delivery API is composed. Once the code lives inside `Umbraco.Cms.Api.Delivery`, the registration moves into `AddDeliveryApi()` and the probe is unnecessary.
- `DeliveryApiContentIndexer` filters out core handlers with a **string namespace test**: `handler.GetType().Namespace?.StartsWith("Umbraco.Cms.Api.Delivery")`. Inside the owning project this can become a proper marker interface (e.g. `ISystemContentIndexHandler`) or an explicit exclusion list — a string prefix check on a namespace is exactly the fitted-to-one-caller pattern §2 of the repo `CLAUDE.md` warns against.
- `DeliveryApiContentQueryProvider` still carries an `[Obsolete("... Will be removed in V14.")]` overload. Delete it in V19.

Also worth resolving during the move: the hardcoded `MapSystemFieldName` table carries a `TODO` noting that `ancestorIds` maps to `PathIds` (ancestors-**or-self**) while the Delivery API means ancestors only.

### 6.3 `Umbraco.Cms.Search.Core.Client` — dissolve into `Umbraco.Web.UI.Client`

This is the backoffice UI for search index management: a root collection view of all indexes, a per-index detail workspace with an extensible `searchIndexDetailBox` extension type, a search box, stats box, rebuild action, and SignalR-driven rebuild state. It registers a `menuItem` under `UMB_ADVANCED_SETTINGS_MENU_ALIAS` — i.e. exactly where the old built-in Examine Management dashboard lived.

| Area | Target |
|---|---|
| `Client/src/settings/**`, `Client/src/global/**`, `Client/src/bundle/**` | `src/Umbraco.Web.UI.Client/src/packages/search-management/` |
| `Client/src/settings/api/**` (generated hey-api client) | *delete* — regenerate into `packages/core/backend-api` from the Management API |
| `Client/src/bundle/lang/{en,da}.ts` | merge into the backoffice `en`/`da` localization files |
| npm project, `vite.config.ts`, `tsconfig*.json`, `scripts/generate-*.js`, `umbraco-package.json`, importmap | *delete* |
| `Umbraco.Cms.Search.Core.Client.csproj` (~110 lines of MSBuild static-web-asset plumbing) | *delete* |

Rationale beyond tidiness: the current arrangement duplicates the entire backoffice client build for one section — a standalone npm project, its own Node 24 toolchain, its own OpenAPI generation script, a three-bundle code-splitting/importmap scheme, and ~110 lines of MSBuild in the csproj mirroring the Login block in `Umbraco.Cms.StaticAssets`. None of that is needed for first-party UI that ships in every install. It also sidesteps the `App_Plugins` cache-buster and importmap edge cases that only apply to third-party plugins.

**Naming caution:** `src/packages/core/search` already exists in the backoffice client (the global search modal and the `UmbSearchDataSource`/`UmbSearchRepository` contracts). Name the new top-level package something unambiguous — `search-management` or `search-indexes` — not `search`.

The one thing genuinely worth preserving is the `searchIndexDetailBox` extension type: it is how the Examine provider (and any third-party provider) contributes provider-specific UI to the index detail view. Keep it as a first-class, documented backoffice extension type when the code lands in `Umbraco.Web.UI.Client`.

### 6.4 `Umbraco.Cms.Search.Core` — keep, but split contracts out and move the API surface downstream

This is the substantive decision. The recommendation is to keep the project as the **provider-agnostic search engine**, while (a) promoting its *contract* surface into `Umbraco.Core`, (b) moving its *presentation* surface into `Umbraco.Cms.Api.Management`, and (c) moving the built-in *property value handlers* down into `Umbraco.Infrastructure`. After (a) and (b), `Search.Core` references only `Umbraco.Infrastructure`; (c) is a decision taken on top of the original analysis, and does not change that reference set.

#### 6.4.1 Promote to `Umbraco.Core`

These are pure contracts and records with no dependencies beyond `Umbraco.Core` itself. Promoting them removes the layering inversion, lets `IPublishedContentQuery` / `IContentSearchService` / `IApiContentQueryProvider` reference the search types directly, and means a provider package can depend on the contracts without dragging the Management API and backoffice UI in behind them. The precedent is already in the tree: `Constants.IndexAliases` was promoted to `Umbraco.Core` for exactly this reason, and the rest of the vocabulary should follow it.

| Class(es) | Current namespace | Proposed namespace (in `Umbraco.Core`) |
|---|---|---|
| `ISearcher`, `IIndexer`, `ISearcherResolver`, `IIndexerResolver` | `…Search.Core.Services` | `Umbraco.Cms.Core.Search` |
| `SearcherResolverExtensions`, `IndexerResolverExtensions` | `…Search.Core.Extensions` | `Umbraco.Cms.Core.Search` (or `Umbraco.Extensions`) |
| `SearchResult`, `Document`, `AccessContext` | `…Models.Searching` | `Umbraco.Cms.Core.Search.Querying` |
| `Filter` + 17 filter types (`KeywordFilter`, `TextFilter`, `IntegerRangeFilter`, …) | `…Models.Searching.Filtering` | `Umbraco.Cms.Core.Search.Querying.Filtering` |
| `Facet` / `FacetValue` / `FacetResult` + 20 facet types | `…Models.Searching.Faceting` | `Umbraco.Cms.Core.Search.Querying.Faceting` |
| `Sorter` + 6 sorter types | `…Models.Searching.Sorting` | `Umbraco.Cms.Core.Search.Querying.Sorting` |
| `IndexField`, `IndexValue`, `Variation`, `IndexMetadata`, `HealthStatus`, `ContentProtection`, `ContentIndexInfo` | `…Models.Indexing` | `Umbraco.Cms.Core.Search.Indexing` |
| `ContentChange`, `ChangeImpact`, `ContentState` | `…Models.Indexing` | `Umbraco.Cms.Core.Search.Indexing` |
| `IndexOptions`, `IndexRegistration`, `ContentIndexRegistration` | `…Configuration`, `…Models.Configuration` | `Umbraco.Cms.Core.Search.Configuration` |
| `IPropertyValueHandler`, `ICorePropertyValueHandler`, `PropertyValueHandlerCollection(Builder)`, `PropertyValueHandlerCollectionExtensions` | `…PropertyValueHandlers[.Collection]` | `Umbraco.Cms.Core.Search.Indexing` |
| `IContentIndexer`, `ISystemFieldsContentIndexer`, `IContentChangeStrategy` (+ draft/published variants) | `…Services.ContentIndexing` | `Umbraco.Cms.Core.Search.Indexing` |
| `IContentIndexingService`, `IContentIndexingDataCollectionService`, `IContentTypeIndexingService` | `…Services.ContentIndexing` | `Umbraco.Cms.Core.Search.Indexing` |
| `IDistributedContentIndexRebuilder`, `IDistributedContentIndexRefresher` | `…Services.ContentIndexing` | `Umbraco.Cms.Core.Search.Indexing` |
| `IContentProtectionProvider`, `IOriginProvider`, `IIndexDocumentService`, `IIndexDocumentRepository`, `IndexDocument` | `…Services.ContentIndexing`, `…Persistence`, `…Models.Persistence` | `Umbraco.Cms.Core.Search.Indexing` |
| `ContentIndexingNotification`, `IndexRebuildStartingNotification`, `IndexRebuildCompletedNotification` | `…Notifications` | `Umbraco.Cms.Core.Notifications` |
| `Constants.FieldNames` | `…Search.Core.Constants` | merge into `Umbraco.Cms.Core.Constants.IndexFieldNames` (alongside the existing `Constants.IndexAliases`) |
| `GuidExtensions.AsKeyword()` | `…Search.Core.Extensions` | `Umbraco.Extensions` (in `Umbraco.Core`) |
| `PublishedContentSearchExtensions` (`SearchChildren`, `SearchDescendants`) | `…Search.Core.Extensions` | `Umbraco.Extensions` (in `Umbraco.Core`) |
| `IDateTimeOffsetConverter` | `…Search.Core.Helpers` | `Umbraco.Cms.Core` (see note below) |

Four of these deserve extra comment:

- **`Constants.FieldNames` and the obsolete `Constants.IndexAliases`.** `Umbraco.Cms.Search.Core.Constants` already carries an `IndexAliases` class marked `[Obsolete("Please use CoreConstants.IndexAliases instead. Scheduled for removal in Umbraco 21.")]`, forwarding to the Core copy. Since V19 is unreleased and the whole namespace is moving anyway, **delete the obsolete shim rather than carry it**. Move `FieldNames` next to `IndexAliases` in Core so the two halves of the same vocabulary sit together. Keep `Constants.Api.Name` and `Constants.Persistence` with whichever project ends up owning them.

- **`IDateTimeOffsetConverter` / `DateTimeOffsetConverter`.** Genuinely generic (13 + 17 lines) and consumed by three projects today (`Search.Core`, `Search.DeliveryApi`, and indirectly the Examine provider). It carries a stale `// NOTE: in V15 this can be done using dateTime.TryConvert<DateTimeOffset>()`. Either promote it to `Umbraco.Cms.Core` as a general date utility, or **verify the note and delete the abstraction entirely** in favour of the built-in conversion. A single-method interface that exists only to wrap a constructor call is a poor addition to `Umbraco.Core`'s public surface — resolve the TODO before promoting.

- **`ArrayExtensions.NullIfEmpty` / `ListExtensions.NullIfEmpty`.** Both `internal`, both trivial, and duplicated across two files for `T[]` and `List<T>`. Either promote one unified generic version to `Umbraco.Extensions` in `Umbraco.Core`, or leave them internal in `Search.Core`. Not worth a public API addition on their own.

- **`PublishedContentSearchExtensions`** — **Decision (25-08-2026):** promote, as set out here rather than left in `Search.Core` (moved from §6.4.4). They are declared in the `Umbraco.Extensions` namespace but compiled into `Search.Core`, so `content.SearchChildren(...)` in a Razor template only compiles if the search package is referenced — whereas it previously came from the Examine/Web.Common layer. Declaring them in `Umbraco.Core` against the promoted `ISearcherResolver` keeps the surface always available and lets it degrade gracefully when no provider is registered (see open question 1 in §9). They resolve their dependencies through `StaticServiceProvider` today; that still works in `Umbraco.Core`, but the resolver should be the only thing they reach for.

#### 6.4.2 Move to `Umbraco.Cms.Api.Management`

| Class(es) | Target namespace |
|---|---|
| `ApiControllerBase` (search) | `Umbraco.Cms.Api.Management.Controllers.Search` |
| `GetAllIndexesApiController`, `GetIndexApiController`, `RebuildIndexApiController` | `Umbraco.Cms.Api.Management.Controllers.Search.Index` |
| `SearchApiController` | `Umbraco.Cms.Api.Management.Controllers.Search` |
| `IndexViewModel`, `SearchRequestModel`, `SearchResultViewModel`, `DocumentViewModel`, `FacetResultViewModel` | `Umbraco.Cms.Api.Management.ViewModels.Search` |
| `IndexRebuildServerEventNotificationHandler` | `Umbraco.Cms.Api.Management.ServerEvents.NotificationHandlers` (wherever the other server-event handlers live) |
| `AddBackOfficeOpenApiDocument("search", …)` block in `AddSearchCore()` | *delete* — endpoints join the main Management API document |

This move is what actually removes the `Api.Management` / `Api.Common` / `Web.Common` references from `Search.Core`. It also collapses a **separate OpenAPI document**: the search controllers currently declare `[BackOfficeRoute("search/api/v{version:apiVersion}")]`, `[MapToApi("search")]` and their own `AddBackOfficeOpenApiDocument` with a bespoke `ActionNameOperationIdTransformer`. First-party backoffice endpoints belong in the Management API document, generated and versioned with everything else.

The `IndexRebuildServerEventNotificationHandler` move also removes a conditional registration hack in `AddSearchCore()` that probes for `IServerEventRouter` to decide whether the backoffice is composed.

**Downstream work this triggers** (all mandatory, all in the same PR series):

- Routes change from `/umbraco/search/api/v1/*` to `/umbraco/management/api/v1/search/*`.
- Operation IDs change (the main document's operation-ID strategy replaces `ActionNameOperationIdTransformer`).
- `src/Umbraco.Cms.Api.Management/OpenApi.json` must be regenerated, plus the hey-api client — see the `/umb-update-openapi` skill. Note that adding an `[Authorize]` policy also adds a 403 response to the document; do not declare `[ProducesResponseType(403)]` by hand or generation throws.

#### 6.4.3 Move to `Umbraco.Infrastructure`

**Decision (25-08-2026):** the built-in property value handlers move to `Umbraco.Infrastructure`. The analysis originally left them in `Search.Core` as the boundary most likely to need revisiting; the decision is to move them now rather than wait for the friction.

| Class(es) | Current namespace | Proposed namespace |
|---|---|---|
| The 22 handler implementations — `BlockGridPropertyValueHandler`, `RichTextPropertyValueHandler`, `MultiNodeTreePickerPropertyValueHandler`, `TagsPropertyValueHandler` and the rest — plus `IHtmlIndexValueParser` / `HtmlIndexValueParser` | `…Search.Core.PropertyValueHandlers` | `Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers` |

Roughly 1,290 lines. The handlers know intimately about core property editors — they consume `Umbraco.Cms.Core.PropertyEditors.ValueConverters` — so leaving them in a separate project means every new core property editor requires a coordinated change across two projects. Their package dependencies (`HtmlAgilityPack`, `Markdown`) are already referenced by `Umbraco.Infrastructure`, so the move costs nothing on that front.

The contracts they implement (`IPropertyValueHandler`, `ICorePropertyValueHandler`) and the collection (`PropertyValueHandlerCollection(Builder)`, `PropertyValueHandlerCollectionExtensions`) go to `Umbraco.Core` under §6.4.1, leaving the usual split: contract in Core, built-in implementations in Infrastructure, provider-specific handlers wherever the provider lives. Registration can stay in `AddSearchCore()` — `Search.Core` still references `Umbraco.Infrastructure` — but registering the built-ins from Infrastructure is the tidier end state, leaving `AddSearchCore()` to compose the engine rather than the handler set.

#### 6.4.4 Stays in `Umbraco.Cms.Search.Core`

| Area | Files | Why it stays |
|---|---:|---|
| `Services/ContentIndexing/*` (implementations) | ~20 | The engine: orchestration, data collection, change strategies, distributed rebuild/refresh |
| `Services/{Searcher,Indexer}Resolver`, `ResolverBase<T>` | 3 | Implementations of the promoted resolver contracts |
| `Services/SearchEnabledPublishedContentQuery` | 1 | Derives from `Umbraco.Cms.Infrastructure.PublishedContentQuery` |
| `Cache/**` | 34 | Granular cache refreshers — stays for V19 by decision (see §7.1) |
| `Persistence/IndexDocumentRepository`, `IndexDocumentService` | 3 | NPoco + MessagePack change-detection snapshots |
| `NotificationHandlers/*` (minus the server-event one) | 5 | Indexing triggers, `DeferredActions` |
| `Extensions/ContentExtensions` | 1 | Indexing helpers used by the pipeline |
| `DependencyInjection/*` | 2 | `AddSearchCore()`, `SearchCoreComposer` |

Roughly 5,900 lines remain — a coherent, separately-packageable engine with exactly one project reference (`Umbraco.Infrastructure`) and one package reference (`MessagePack`).

**`NotificationHandlers/DeferredActions` — Decision (25-08-2026): stays.** A generic "run these actions when the ambient scope completes" helper enlisting on `IScopeContext` at priority 80. `Umbraco.Core` has no equivalent and the pattern would generalise, so promotion to `Umbraco.Cms.Core.Scoping` remains fair on the merits — but with a single caller it is not obviously an improvement to Core's surface. Leave it as-is; revisit if a second caller materialises.

### 6.5 `Umbraco.Cms.Search.Provider.Examine` — keep as-is, with three promotions and two fixes

Correct as a separate project: it is the swappable half of the provider model, it carries the `Examine`/Lucene package dependency, and it ships its own Management API and backoffice UI contribution.

The point of §6.4.1 is not to get the provider down to a single reference — referencing `Search.Core` is perfectly reasonable for something that plugs into the search engine. The point is *what comes with* that reference. Today `Search.Core` transitively drags in `Api.Management`, `Api.Common`, `Web.Common` and the backoffice UI assets, so a provider cannot depend on `ISearcher`/`IIndexer` without also depending on the entire backoffice. Once the controllers and client have moved out (§6.4.2, §6.3), a `Search.Core` reference costs the provider only `Umbraco.Infrastructure` and below, which is the acceptance criterion worth checking.

**Individual classes reviewed for promotion:**

| Class | Verdict |
|---|---|
| `Lucene/*` — `UmbracoApplicationRoot`, `UmbracoLockFactory`, `NoPrefixSimpleFsLockFactory`, `UmbracoTempEnvFileSystemDirectoryFactory`, `ConfigurationEnabledDirectoryFactory`, `LuceneRAMDirectoryFactory` | **Stay.** All six are Lucene/Examine types with no generic core |
| `Helpers/DocumentIdHelper`, `Helpers/FieldNameHelper` | **Stay.** Encode the Lucene physical document-ID and field-name schemes |
| `Models/Searching/Filtering/{DateTimeExactFilter, DoubleExactFilter, FilterRange<T>}` | **Stay.** All `internal`; they exist purely to match Examine's internal `DateTime`/`double` representations |
| `Services/{IIndexCommitMonitor, IndexCommitMonitor}` | **Stay.** Lucene commit semantics |
| `Configuration/{FieldOptions, SearcherOptions, ConfigureIndexOptions, FieldValues}` | **Stay.** Lucene field typing and boost factors |
| `Services/IActiveIndexManager` | **Consider promoting the contract.** Zero-downtime rebuild via an active/shadow index pair is a provider-agnostic *pattern*, not a Lucene one — any provider implementing rebuild-with-swap needs the same lifecycle (`IsRebuilding` / `StartRebuilding` / `CompleteRebuilding` / `CancelRebuilding`). If a second provider is expected, promote the interface to `Umbraco.Cms.Core.Search.Indexing` and keep `ActiveIndexManager` / `NoopActiveIndexManager` in the provider |
| `Telemetry/ExamineTelemetryProvider` | **Stay, but generalise the key.** `Constants.Telemetry.ExamineIndexCount` already lives in `Umbraco.Core`. Prefer a provider-agnostic `SearchIndexCount` reported by `Search.Core` from `IndexOptions.GetContentIndexRegistrations()`, so telemetry does not assume Examine |
| `Controllers/{ExamineApiController, ExamineApiControllerBase}` + `Models/ViewModels/*` | **Stay**, with their own OpenAPI document — correct for a genuinely separate package (see §7.2) |

**Fix 1 — `ExamineSearchProviderSettings` is in the wrong project.** It currently lives at `src/Umbraco.Core/Configuration/Models/ExamineSearchProviderSettings.cs` and is registered by `Umbraco.Core/DependencyInjection/UmbracoBuilder.Configuration.cs`. A settings class named after one specific provider, in the layer that is supposed to hold nothing but general-purpose contracts, is precisely the pattern §2 of the repo `CLAUDE.md` calls out. The likely reason is `appsettings-schema.Umbraco.Cms.json` generation, which reads config models from `Umbraco.Core`.

Move it to `Umbraco.Cms.Search.Provider.Examine.Configuration` and register it via `AddUmbracoOptions<T>()` in `AddExamineSearchProvider()`. The JSON-schema concern that probably motivated the current placement is real but solvable — §7.6 sets out how the schema is actually produced and what the provider has to ship to keep IntelliSense working.

Note also that `AddExamineSearchProvider()` reads the configuration section **directly** off `builder.Config` rather than through `IOptions<T>`, because `ZeroDowntimeIndexing` is needed synchronously at composition time. That is legitimate, but it means the value cannot be changed by an `IConfigureOptions<ExamineSearchProviderSettings>` — worth a comment at the call site.

**Fix 2 — independent versioning.** `src/Umbraco.Cms.Search.Provider.Examine/version.json` is the **only** `version.json` under `src/` and it sets `"inherit": false`, `"version": "18.0.0-beta.1"` and `"release": { "branchName": "release/examine/{version}" }` — leftovers from the standalone package repository. The root `version.json` is `19.0.0-beta1`. As it stands, the Examine provider will build and pack as **18.0.0-beta.1** while the rest of the CMS is 19.x. Delete this file so the project inherits the repository version, unless independent provider versioning is a deliberate decision — in which case it needs its own release pipeline, and shipping an `18.x` package from the `v19/dev` branch still needs addressing.

---

## 7. Cross-cutting findings

### 7.1 The granular cache refreshers should eventually go to core — deferred beyond V19

`src/Umbraco.Cms.Search.Core/Cache/UmbracoBuilderExtensions.cs` carries an explicit note from the authors:

> *"Eventually these cache refreshers should probably be added to core, or the core cache refreshers should be retrofitted with a higher level of granularity."*

Two real gaps in the core distributed cache are being worked around here, and both affect more than search:

- **Content:** the core refresher cannot distinguish "something was published" from "something was saved", nor "publish a new culture" from "republish an existing culture". `Search.Core` adds `DraftContentCacheRefresher` / `PublishedContentCacheRefresher` with per-culture change types to get that.
- **Public access:** the core refresher only broadcasts "something changed — refresh everything". `PublicAccessDetailedCacheRefresher` adds the detail needed to decide how much to re-index.

34 files / 1,551 lines of parallel cache-refresher infrastructure — including a generic `ContentCacheRefresherNotificationPayload<T>` wrapper that adds server-origin tracking, which is itself a generally useful primitive for any load-balanced feature that wants to skip same-origin work.

**Decision (25-08-2026): not now — postponed to a later version, V20 at the earliest.** These refreshers are genuinely useful, but moving them into core while the existing core refreshers stay as they are would leave two overlapping sets of cache refreshers side by side, which is more confusing than the current split. The prerequisites are a thorough analysis and an announcement to retire the old cache refreshers; the move belongs after that, not as part of a project reorganisation. For V19 the `Cache` folder stays in `Search.Core` exactly as it is (§6.4.4).

The upside stands for whenever that work is picked up: retrofitting the core refreshers with this granularity would let `Search.Core` delete most of its `Cache` folder, and would benefit HybridCache, webhooks and any other notification-driven subsystem.

### 7.2 Three OpenAPI documents where there should be two

Today: the Management API document, plus `"search"` (`/umbraco/search/api/v1`), plus `"search-examine-provider"` (`/umbraco/examine/api/v1`). Each of the latter two declares its own `AddBackOfficeOpenApiDocument` with an identical copy-pasted `ActionNameOperationIdTransformer`.

After §6.4.2 the search endpoints join the Management API document. The Examine provider keeps its own document — correct, since it is a genuinely separate package. But the duplicated `ActionNameOperationIdTransformer` should be promoted to `Umbraco.Cms.Api.Common.OpenApi` as a supported, documented option for package authors rather than copy-pasted per package.

### 7.3 `IndexDocument` persistence is already half in core

`IndexDocumentDto`, its `DatabaseSchemaCreator` registration and the `AddIndexDocumentTable` migration (`Migrations/Upgrade/V_19_0_0/`) all live in `Umbraco.Infrastructure` already, while `IIndexDocumentRepository` / `IndexDocumentRepository` live in `Search.Core`. The schema is core; the repository that reads and writes it is not. Since the engine is always present, the `umbracoIndexDocument` table is never dead weight, so this is purely a consistency question: either move the repository to `Umbraco.Infrastructure/Persistence/Repositories/Implement` so it sits with its DTO and migration, or keep the current pairing and document the split deliberately. The `MessagePack` package reference is the practical argument for leaving the repository where it is: in the whole solution only `Search.Core` and `Umbraco.PublishedCache.HybridCache` reference that package, and `HybridCache` is a separate project precisely so its MessagePack-based serialization stays out of `Umbraco.Infrastructure`. Moving the index-document repository there would undo that boundary for the sake of tidiness, so the stronger recommendation is to leave it in `Search.Core` and note the split as deliberate.

### 7.4 Test namespaces will need to follow

Tests currently sit under namespaces that mirror the current project split:

- `tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Search.Core`
- `tests/Umbraco.Tests.Integration/Umbraco.Search.Core`
- `tests/Umbraco.Tests.Integration/Umbraco.Search.BackOffice`
- `tests/Umbraco.Tests.Integration/Umbraco.Search.Provider.Examine`
- `tests/Umbraco.Tests.Integration/Testing/Search`

The `Umbraco.Search.BackOffice` tests move with their subject. Note also the inconsistent prefix between the unit (`Umbraco.Cms.Search.Core`) and integration (`Umbraco.Search.Core`) trees — worth aligning while the code moves. `Umbraco.Cms.Search.Core.csproj` also grants `InternalsVisibleTo` to both test projects and to `DynamicProxyGenAssembly2`; those grants must be re-homed alongside whatever moves into `Umbraco.Core`.

### 7.5 `Umbraco.Cms.Search.Core/CLAUDE.md` documents an npm workspaces monorepo that does not exist

The "Client Architecture (npm Workspaces Monorepo)" section describes a workspace root at `src/` with hoisted dependencies and a shared `src/scripts/generate-openapi.js`. The csproj files and the actual tree show two **standalone** npm projects, each with its own `package.json`, `package-lock.json` and `scripts/generate-openapi.js`. Whichever way the reorganisation goes, this section needs correcting — and if §6.3 is adopted, most of it is deleted along with the client project.

### 7.6 The Examine provider should generate its own JSON schema, the way Umbraco Forms does

**How the CMS schema is produced today.** `appsettings-schema.Umbraco.Cms.json` is not generated by reflecting over `[UmbracoOptions]`. `tools/Umbraco.JsonSchema/Program.cs` calls `Generate(typeof(UmbracoCmsSchema))`, where `UmbracoCmsSchema` is a hand-written class inside the tool with one explicit property per settings type; NJsonSchema walks that object graph. The `[UmbracoOptions]` attribute plays no part — it only drives runtime binding via `AddUmbracoOptions<T>()`. The tool also has exactly one `ProjectReference`, to `Umbraco.Core`, so it cannot see types declared anywhere else.

So a settings class needs **two** things to reach the CMS schema: compile-time reachability from `Umbraco.Core`, and a hand-added property on `UmbracoCmsSchema`. `ExamineSearchProviderSettings` has both — the tool carries a dedicated `SearchDefinition { ExamineSearchProviderSettings Examine }` nested class, which is what emits `Umbraco:CMS:Search:Examine` to match `Constants.Configuration.ConfigSearchExamine`. That hand-written property is what pins a provider-specific type inside `Umbraco.Core`.

**How an external package does it.** Umbraco Forms already solves this, and the Examine provider should follow the same pattern. Forms ships four small pieces:

1. A `GlobalPackageReference` to `Umbraco.JsonSchema.Extensions` in `Directory.Packages.props` (the CMS repo already has this package at the same version, but references it only from `Umbraco.Cms.Targets`).
2. An `internal sealed class UmbracoFormsSchema` in the package project, describing just its own slice of the configuration tree (`Umbraco` → `Forms` → the settings types), with the same nested-class shape the CMS tool uses.
3. A build target invoking the **`JsonSchemaGenerate` MSBuild task** against the built assembly, and packing the result as content:

```xml
<Target Name="GenerateAppsettingsSchema" BeforeTargets="Build;CopyUmbracoJsonSchemaFiles" Inputs="$(TargetPath)" Outputs="$(_UmbracoFormsJsonSchemaReference)">
  <JsonSchemaGenerate AssemblyPath="$(TargetPath)" TypeName="UmbracoFormsSchema" OutputPath="$(_UmbracoFormsJsonSchemaReference)" IncludeObsoleteProperties="false" />
</Target>
```

4. A one-item `buildTransitive/Umbraco.Forms.props` that registers the fragment for composition into the consuming site:

```xml
<UmbracoJsonSchemaFiles Include="$(MSBuildThisFileDirectory)..\appsettings-schema.Umbraco.Forms.json" Weight="-70" />
```

On the consuming side, `Umbraco.Cms.Targets`' own `buildTransitive` targets do the rest: `CopyUmbracoJsonSchemaFiles` copies every registered fragment into the project directory, and `AddUmbracoJsonSchemaReferences` merges them into the site's `appsettings-schema.json` through the `JsonSchemaAddReferences` task. Weight controls ordering — the CMS fragment sits at `-90`, Forms at `-70`.

Note that Forms uses the `JsonSchemaGenerate` **task**, whereas the CMS still uses a bespoke console app invoked via `dotnet run`. The task-based approach is the better one: no separate tool project, no process launch, and correct MSBuild incrementality via `Inputs`/`Outputs`. Migrating `tools/Umbraco.JsonSchema` to it is out of scope here, but worth a follow-up.

**What this means for §6.5.** There is no obstacle to moving `ExamineSearchProviderSettings` out of `Umbraco.Core`, and good reason to: even though the provider ships with core, it is swappable, so it should own its own configuration and its own schema. The work is:

- Move `ExamineSearchProviderSettings` (and `LuceneDirectoryFactory`) to `Umbraco.Cms.Search.Provider.Examine.Configuration`, registering it via `AddUmbracoOptions<T>()` in `AddExamineSearchProvider()` instead of in `UmbracoBuilder.Configuration.cs`.
- Add an `internal sealed class UmbracoSearchExamineSchema` to the provider describing `Umbraco` → `CMS` → `Search` → `Examine`, so the emitted path still matches `Constants.Configuration.ConfigSearchExamine`.
- Add the `JsonSchemaGenerate` target and a `buildTransitive` props file registering `appsettings-schema.Umbraco.Cms.Search.Examine.json`. A weight of `-80` places it between the CMS (`-90`) and third-party packages such as Forms (`-70`).
- Delete the `SearchDefinition` nested class from `tools/Umbraco.JsonSchema/UmbracoCmsSchema.cs`, and `ExamineSearchProviderSettings` from `Umbraco.Core`.

One in-repo caveat: `buildTransitive` props only import for NuGet package consumers, not across a `ProjectReference`. The development site works around this explicitly — `Umbraco.Web.UI.csproj` imports `..\Umbraco.Cms.Targets\buildTransitive\Umbraco.Cms.Targets.props` and `.targets` directly — so it needs an equivalent import for the provider's props, otherwise the dev site silently loses IntelliSense for `Umbraco:CMS:Search:Examine`. Since `src/Umbraco.Web.UI/appsettings-schema*.json` is gitignored and produced at build, that is behaviour to verify in a build rather than a file to inspect.

---

## 8. Suggested sequencing

Each step is independently shippable and should leave the build green.

| # | Step | Unblocks |
|---:|---|---|
| 1 | Delete `src/Umbraco.Cms.Search.Provider.Examine/version.json` (§6.5) | Correct package versions for V19 — a one-line fix with real release impact |
| 2 | Switch `ContentSearchService` / `MediaSearchService` to `ISearcherResolver` (§6.1) | Makes backoffice search provider-agnostic; prerequisite for step 5 |
| 3 | Promote contracts + models to `Umbraco.Core` (§6.4.1); delete the obsolete `Constants.IndexAliases` shim | Everything else; removes the layering inversion |
| 4 | Move the built-in property value handlers to `Umbraco.Infrastructure/Search/PropertyValueHandlers` (§6.4.3) | Core property editors and their index handlers change together, in one project |
| 5 | Dissolve `Search.BackOffice` into `Umbraco.Core/Services` (§6.1) | One fewer project/package |
| 6 | Dissolve `Search.DeliveryApi` into `Umbraco.Cms.Api.Delivery` (§6.2) | One fewer project/package |
| 7 | Move controllers + view models to `Umbraco.Cms.Api.Management` (§6.4.2); regenerate `OpenApi.json` + hey-api client | Drops the `Api.Management` / `Api.Common` / `Web.Common` refs from `Search.Core` |
| 8 | Move the client into `Umbraco.Web.UI.Client/src/packages/search-management` (§6.3); delete `Search.Core.Client` | Drops the last non-Infrastructure ref from `Search.Core`; removes a whole npm build |
| 9 | Move `ExamineSearchProviderSettings` into the provider and have it generate its own schema fragment, following the Umbraco Forms pattern (§6.5, §7.6) | Correct layering; a swappable provider owns its own configuration and IntelliSense |
| 10 | Verify `Provider.Examine`'s transitive closure no longer contains `Api.Management`, `Api.Common`, `Web.Common` or the backoffice UI assets (§6.5) | Confirms a provider can plug in without depending on the backoffice |
| 11 | Update the three search `CLAUDE.md` files and align test namespaces (§7.4, §7.5) | — |
| — | *Deferred to V20 at the earliest (decision, §7.1):* retrofit granular cache refreshers into core, after an analysis and an announcement to retire the old ones | Would delete ~1,500 lines from `Search.Core`; benefits HybridCache and webhooks |

Steps 7 and 8 must land together with the regenerated `OpenApi.json` and backoffice client, since the client consumes the endpoints that move.

---

## 9. Open questions

1. **What is the intended behaviour when no provider is registered?** The analysis assumes the search stack itself is always present (it is the core implementation), but a site can in principle compose the engine without a provider. Today that fails in inconsistent ways: `ISearcherResolver.GetSearcher()` logs and returns `null`, `GetRequiredSearcher()` throws, and `ContentSearchService` cannot even be constructed because no bare `ISearcher` is registered (§6.1). A single, documented degradation policy is needed — most likely a no-op provider registered as a fallback in `Search.Core`, which would also make the engine testable without Examine.
2. **Is independent versioning of the Examine provider intentional?** §6.5 assumes it is a porting artefact.
3. **Should `Search.Core` remain a separately packaged NuGet at all** once its contracts are in `Umbraco.Core` and it references only `Umbraco.Infrastructure`? Keeping it separate buys a compiler-enforced boundary around ~5,900 lines and isolates the `MessagePack` dependency. Folding it into `Umbraco.Infrastructure` would take the project count from 5 to 1. The recommendation here is to keep it, but the case is closer once steps 3–8 are done — worth revisiting at that point rather than deciding up front.

# Architecture
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---

### Technology Stack

- **Node.js**: >=22.17.1
- **npm**: >=10.9.2
- **Language**: TypeScript 5.9.3
- **Module System**: ESM (ES Modules)
- **Framework**: Lit (Web Components)
- **Build Tool**: Vite 7.1.11
- **Test Framework**: @web/test-runner with Playwright
- **E2E Testing**: Playwright 1.55.1
- **Code Quality**: ESLint 9.37.0, Prettier 3.6.2
- **Mocking**: MSW (Mock Service Worker) 1.3.5
- **Documentation**: Storybook 9.0.14, TypeDoc 0.28.13

### Application Type

Single-page web application (SPA) packaged as an npm library called `@umbraco-cms/backoffice` for typings and built as a bundle, which is copied over to `src/Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice` for the CMS. Provides:
- Extensible Web Components for CMS backoffice UI
- API libraries for extension development
- TypeScript type definitions
- Package manifest system for extensions

### Architecture Pattern

**Modular Package Architecture** with clear separation:

```
src/
├── apps/              # Application entry points
│   ├── app/          # Main backoffice application
│   ├── backoffice/   # Backoffice shell
│   ├── installer/    # CMS installer interface
│   ├── preview/      # Content preview
│   └── upgrader/     # CMS upgrader interface
│
├── libs/              # Core API libraries (infrastructure)
│   ├── class-api/             # Base class utilities
│   ├── context-api/           # Context API for dependency injection
│   ├── context-proxy/         # Context proxying utilities
│   ├── controller-api/        # Controller lifecycle management
│   ├── element-api/           # Element base classes & mixins
│   ├── extension-api/         # Extension registration & loading
│   ├── localization-api/      # Internationalization
│   └── observable-api/        # Reactive state management
│
├── packages/          # Feature packages (50+ packages)
│   ├── core/         # Core utilities (auth, http, router, etc.)
│   ├── content/      # Content management
│   ├── documents/    # Document types & editing
│   ├── media/        # Media management
│   ├── members/      # Member management
│   ├── user/         # User management
│   ├── templating/   # Templates, scripts, stylesheets
│   ├── block/        # Block editor components
│   └── ...           # 30+ more specialized packages
│
├── external/          # External dependency wrappers
│   ├── lit/          # Lit framework wrapper
│   ├── rxjs/         # RxJS wrapper
│   ├── luxon/        # Date/time library wrapper
│   ├── monaco-editor/# Code editor wrapper
│   └── ...           # Other wrapped dependencies
│
├── mocks/             # MSW mock handlers & test data
│   ├── data/         # Mock database
│   └── handlers/     # API request handlers
│
└── assets/            # Static assets (fonts, images, localization)
```

### Key Technologies

**Core Framework**:
- Lit 3.x - Web Components framework with reactive templates
- TypeScript 5.9 - Type-safe development with strict mode
- Vite - Fast build tool and dev server

**UI Components**:
- @umbraco-ui/uui - Umbraco UI component library (80+ Web Components)
- Shadow DOM - Component style encapsulation
- Custom Elements API - Native web components

**State & Data**:
- RxJS - Reactive programming with observables
- Context API - State management & dependency injection
- MSW - API mocking for development & testing

**Testing**:
- @web/test-runner - Fast test runner for web components
- Playwright - E2E browser testing
- @open-wc/testing - Testing utilities for web components

**Code Quality**:
- ESLint with TypeScript plugin - Linting with strict rules
- Prettier - Code formatting
- TypeDoc - API documentation generation
- Web Component Analyzer - Custom element documentation

**Other**:
- Luxon - Date/time manipulation
- Monaco Editor - Code editing
- DOMPurify - HTML sanitization
- Marked - Markdown parsing
- SignalR - Real-time communication

---

## Design Philosophy

The project is built with an **"extension-first"** mindset. The core HQ team builds the extensions the same way a third-party package developer would. Every section, dashboard, property editor, workspace, tree, and action is registered as an **extension** through a manifest system. This means:

- What HQ can do, package developers can do.
- Any default behavior can be replaced, overridden, or removed.
- The system is framework-agnostic for consumers — while HQ uses Lit, extension authors can use React, Vue, Svelte, or plain JS as long as they deliver a Web Component.

*Note: The extension-first mindset is the guiding design principle. While the frontend fully embraces this, some areas of the system are still being aligned to this model.*

---

## The Package System

Each directory under `src/packages/` is a **logical package** — a self-contained domain module. The key design principle:

> **Each package should be considered standalone and, in theory, should be able to be uninstalled. The CMS would ship with only "Core" installed, and each individual package would be opt-in.**

*Note: This hasn't been fully realized across frontend and backend but the frontend is structured this way.*

The `core` package provides the foundational APIs (context system, extension registry, observable state, controllers, routing, etc.), and every other package builds on top of it.

### Package Independence Rules

1. **A package should never directly import from another non-core package's internal files.** If Package A needs something from Package B, Package B must export it through its public `index.ts`.
2. **Cross-package dependencies should be minimized.** Each package registers its own extensions (dashboards, workspaces, property editors, etc.) via manifests.
3. **Core provides the infrastructure; feature packages provide the domain.** The `core/` package contains: context API, extension registry, observable/state management, controller system, routing, modals, notifications, localization, element mixins, and utility types.

### Module Exports

Each package can expose multiple **modules** (subpath exports). For example, the `core` package exposes dozens of subpaths:

- `@umbraco-cms/backoffice/dashboard`
- `@umbraco-cms/backoffice/entity-action`
- `@umbraco-cms/backoffice/tree`
- `@umbraco-cms/backoffice/workspace`
- ... etc.

A feature package like `media` might export:

- `@umbraco-cms/backoffice/media`
- `@umbraco-cms/backoffice/imaging`

Each of these subpath exports maps to an `index.ts` barrel file inside the corresponding package directory.

---

## Design Patterns

1. **Web Components** - Custom elements with Shadow DOM encapsulation
2. **Context API** - Dependency injection via context providers/consumers
3. **Controller Pattern** - Lifecycle-aware controllers for managing component behavior
4. **Extension System** - Manifest-based plugin architecture
5. **Observable Pattern** - Reactive state management with RxJS observables
6. **Repository Pattern** - Data access abstraction via repository classes
7. **Mixin Pattern** - Composable behaviors via TypeScript mixins (`UmbElementMixin`, etc.)
8. **Builder Pattern** - For complex object construction
9. **Registry Pattern** - Extension registry for dynamic feature loading
10. **Observer Pattern** - Event-driven communication between components

### Extension Registry

The Extension Registry is the central nervous system of the backoffice. All UI is registered here as **Extension Manifests**. The registry can be manipulated at runtime — you can add, remove, or replace extensions at any point.

An extension manifest looks like this:

```typescript
const manifest: UmbExtensionManifest = {
  type: 'dashboard',
  alias: 'My.Dashboard',
  name: 'My Dashboard',
  element: () => import('./my-dashboard.element.js'),
  weight: 100,
  meta: {
    label: 'My Dashboard',
    pathname: 'my-dashboard',
  },
  conditions: [
    { alias: 'Umb.Condition.SectionAlias', match: 'Umb.Section.Content' },
  ],
};
```

Key extension types include: `section`, `sectionView`, `dashboard`, `workspace`, `workspaceView`, `workspaceAction`, `workspaceContext`, `propertyEditorUi`, `propertyEditorSchema`, `tree`, `treeItem`, `menuItem`, `entityAction`, `entityBulkAction`, `headerApp`, `globalContext`, `modal`, `bundle`, `backofficeEntryPoint`, `localization`, `condition`, and `kind`.

Extensions are registered in different ways depending on the context:

- **Internal packages (core codebase)**: Each package has an `umbraco-package.ts` that exports a `bundle` manifest. The bundle uses a dynamic `js` import pointing to the package's `manifests.ts`, which aggregates all sub-feature manifests. This is the standard pattern for the core codebase:

```typescript
// umbraco-package.ts — entry point for the extension registry
export const name = 'Umbraco.Core.MyPackage';
export const extensions = [
  {
    name: 'My Package Bundle',
    alias: 'Umb.Bundle.MyPackage',
    type: 'bundle',
    js: () => import('./manifests.js'),
  },
];
```

```typescript
// manifests.ts — aggregates all sub-feature manifests
import { manifests as featureAManifests } from './feature-a/manifests.js';
import { manifests as featureBManifests } from './feature-b/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
  ...featureAManifests,
  ...featureBManifests,
];
```

The `bundle` type ensures all manifests in the package are lazy-loaded together when the bundle is resolved.

- **External packages**: Register extensions statically via `umbraco-package.json`.
- **Runtime**: Extensions can also be registered programmatically via `umbExtensionsRegistry.register(manifest)` or `registerMany(manifests)`.

### Context API

The Context API is a framework-agnostic, DOM-event-based system inspired by the Provider pattern. Contexts flow down through the DOM tree.

**Core concepts:**
- **Provider**: A parent element that provides a context. Created by calling `this.provideContext(TOKEN, instance)`.
- **Consumer**: Any descendant element that consumes a context. Uses `this.consumeContext(TOKEN, callback)`.
- **Context Token (`UmbContextToken`)**: A typed token that identifies a context and provides type safety.
- **Hierarchical scoping**: Global contexts (notifications, current user) are available everywhere. Workspace contexts are scoped to a workspace. Section contexts are scoped to a section.

```typescript
// Defining a context token
export const MY_CONTEXT = new UmbContextToken<MyContext>('MyContext');

// Providing a context (in a parent element)
this.provideContext(MY_CONTEXT, new MyContext(this));

// Consuming a context (in a descendant element)
this.consumeContext(MY_CONTEXT, (ctx) => {
  // ctx is typed as MyContext
});
```

### Observable State Management

State is managed via **UmbState** classes (e.g., `UmbStringState`, `UmbNumberState`, `UmbArrayState`, `UmbObjectState`) which wrap values in RxJS-like observables. Components observe state changes reactively.

```typescript
#counter = new UmbNumberState(0);
readonly counter = this.#counter.asObservable();

increment() {
  this.#counter.setValue(this.#counter.value + 1);
}
```

Lit elements use the `observe()` method (from `UmbLitElement` or `UmbElementMixin`) to subscribe to observables and trigger re-renders.

Even though RxJS is used underneath, avoid using RxJS helper functions (e.g., `map`, `filter`, `combineLatest`, `switchMap`) directly. Instead, use the Umbraco state classes and the `observe()` method to manage subscriptions and reactivity. The RxJS layer is an implementation detail — the `UmbState` classes and `observe()` provide a simpler, more consistent API that handles subscription lifecycle automatically.

### Repository Pattern & Data Flow

All data consumption in the backoffice is abstracted through a **Repository**. No element or context should ever call the Management API (or any other data source) directly. The repository is the single gateway for reading and writing data, and it delegates to one or more **Data Sources** under the hood. This decoupling means the underlying data source can change (e.g., from a REST API to a GraphQL endpoint, a local cache, or a mock) without any consuming code needing to change.

The backend communicates with the frontend exclusively through the **Management API** — a RESTful, Swagger-documented API. The backoffice never calls backend services directly; everything goes through typed HTTP endpoints. The Management API is the primary data source, but the repository pattern ensures this is an implementation detail hidden behind the data source layer.

#### The Data Flow

`Element` ← observes ← `State` ← writes to ← `Context` ← calls ← `Repository` ← delegates to ← `Data Source`

**Layer by layer:**

1. **Data Source** — Handles the raw transport. For the Management API this is typically a server data source class that wraps the generated API client (OpenAPI-based, typed fetch operations). A data source knows *how* to talk to a specific backend but nothing about state or UI.

2. **Repository** — Consumes one or more data sources. It provides a clean, domain-oriented interface (`getItems()`, `create()`, `save()`, `delete()`) and returns results in a normalized form. The repository is the boundary — everything above it is decoupled from where data comes from.

3. **Context / API** — A workspace context, global context, or controller calls the repository methods and writes the results into **State** objects. The context owns the lifecycle of the data: when to fetch, when to invalidate, how to handle errors.

4. **State** — Data is stored in observable state classes (`UmbObjectState`, `UmbArrayState`, `UmbStringState`, `UmbNumberState`, etc.). These expose an `.asObservable()` that any element can subscribe to.

5. **Element** — Lit elements use `this.observe()` to subscribe to state observables. When state changes, the element re-renders automatically. Elements never call repositories directly.

#### Example: A Repository with a Server Data Source

```typescript
// server data source
export class UmbAnalyticsDashboardServerDataSource {
  async getSummary(): Promise<UmbAnalyticsSummaryModel> {
    const response = await fetch('/umbraco/management/api/v1/analytics/summary');
    return await response.json();
  }
}

// repository
export class UmbAnalyticsDashboardRepository {
  #dataSource = new UmbAnalyticsDashboardServerDataSource();

  async requestSummary(): Promise<UmbAnalyticsSummaryModel> {
    return this.#dataSource.getSummary();
  }
}
```

#### Example: A Context Consuming the Repository

```typescript
export class UmbAnalyticsDashboardContext extends UmbContextBase {
  #repository = new UmbAnalyticsDashboardRepository();

  #summary = new UmbObjectState<UmbAnalyticsSummaryModel | undefined>(undefined);
  readonly summary = this.#summary.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, UMB_ANALYTICS_DASHBOARD_CONTEXT);
  }

  async load() {
    const data = await this.#repository.requestSummary();
    this.#summary.setValue(data);
  }
}

export const UMB_ANALYTICS_DASHBOARD_CONTEXT =
  new UmbContextToken<UmbAnalyticsDashboardContext>('UmbAnalyticsDashboardContext');
```

#### Example: An Element Observing the State

```typescript
@customElement('umb-analytics-dashboard')
export class UmbAnalyticsDashboardElement extends UmbLitElement {
  #context = new UmbAnalyticsDashboardContext(this);

  @state()
  private _summary?: UmbAnalyticsSummaryModel;

  constructor() {
    super();
    this.observe(this.#context.summary, (summary) => {
      this._summary = summary;
    });
  }

  connectedCallback() {
    super.connectedCallback();
    this.#context.load();
  }

  render() {
    if (!this._summary) return html`<uui-loader></uui-loader>`;
    return html`
      <uui-box headline=${this.localize.term('analytics_overview')}>
        <umb-analytics-stat-card
          .label=${'Page Views'}
          .value=${this._summary.pageViews}>
        </umb-analytics-stat-card>
      </uui-box>
    `;
  }
}
```

#### Why This Matters

- **Separation of concerns**: Elements know nothing about HTTP. Contexts know nothing about transport details. Repositories know nothing about state management or UI.
- **Consistency**: Every feature in the backoffice follows the same flow, making the codebase predictable and navigable.

### Controller System

Umbraco uses a **Controller** pattern (`UmbControllerBase`) for logic that isn't directly tied to rendering. Controllers attach to a host (typically an element) and participate in its lifecycle. This is used for consuming contexts, managing subscriptions, and encapsulating business logic.

```typescript
export class MyController extends UmbControllerBase {
  constructor(host: UmbControllerHost) {
    super(host);
    // Controller logic here
  }
}
```

### Extension Conditions & Kinds

- **Conditions**: Declarative rules that determine when an extension is active. For example, "only show this dashboard in the Content section" or "only show this action when editing a new document."
- **Kinds**: Preset manifest configurations that other extensions can inherit from. Reduces boilerplate — e.g., a `kind: 'button'` header app inherits all button-related defaults.

---

## Mental Model

```
┌─────────────────────────────────────────────┐
│            UMBRACO BACKOFFICE               │
│                                             │
│  ┌───────────────────────────────────────┐  │
│  │        Extension Registry             │  │
│  │  (All UI registered as manifests)     │  │
│  └───────────────────────────────────────┘  │
│                                             │
│  ┌───────────────────────────────────────┐  │
│  │  Packages (src/packages/)             │  │
│  │                                       │  │
│  │  ┌─────────────────────────────────┐  │  │
│  │  │  core (foundation)              │  │  │
│  │  │  context, registry, state,      │  │  │
│  │  │  routing, controllers           │  │  │
│  │  └─────────────────────────────────┘  │  │
│  │       ▲           ▲           ▲       │  │
│  │       │           │           │       │  │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ │  │
│  │  │documents│ │  media  │ │ members │ │  │
│  │  └─────────┘ └─────────┘ └─────────┘ │  │
│  │  Feature packages depend on core      │  │
│  └───────────────────────────────────────┘  │
│                                             │
│  ┌───────────────────────────────────────┐  │
│  │  Management API (RESTful, Swagger)    │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

The backoffice is a collection of independently-packaged Web Components, glued together by the Extension Registry, communicating via the Context API, with state managed through Observables — all built to be replaceable, removable, and extensible by default.

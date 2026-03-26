# Architecture

---

### Technology Stack

- **Node.js**: >=22.17.1, **npm**: >=10.9.2
- **Language**: TypeScript 5.9.3 (strict mode, ESM)
- **Framework**: Lit 3.x (Web Components with reactive templates)
- **UI Library**: @umbraco-ui/uui (80+ Web Components, Shadow DOM)
- **Build**: Vite 7.1.11
- **State**: RxJS (via UmbState wrapper classes), Context API
- **Testing**: @web/test-runner + Playwright 1.55.1, @open-wc/testing
- **Code Quality**: ESLint 9.37.0, Prettier 3.6.2
- **Mocking**: MSW 1.3.5
- **Other**: Luxon, Monaco Editor, DOMPurify, Marked, SignalR

### Application Type

SPA packaged as npm library (`@umbraco-cms/backoffice`) for typings and built as a bundle copied to `src/Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice`. Provides extensible Web Components, API libraries, TypeScript types, and a manifest-based extension system.

### Architecture Pattern

**Modular Package Architecture**:

```
src/
├── apps/              # Entry points (app, backoffice, installer, preview, upgrader)
├── libs/              # Core API libraries (context-api, controller-api, element-api, extension-api, observable-api, localization-api, etc.)
├── packages/          # Feature packages (50+)
│   ├── core/         # UI framework (auth, http, router, modals, notifications, etc.)
│   ├── documents/    # Document types & editing
│   ├── media/        # Media management
│   └── ...           # 30+ more domain packages
├── external/          # Dependency wrappers (lit, rxjs, luxon, monaco-editor, etc.)
├── mocks/             # MSW mock handlers & test data
└── assets/            # Static assets (fonts, images, localization)
```

---

## Design Philosophy

**Extension-first architecture**: HQ builds extensions the same way third-party developers do. All UI (sections, dashboards, property editors, workspaces, trees, actions) is registered via manifests. Any default behavior can be replaced, overridden, or removed. Consumers can use any framework that outputs a Web Component.

*Note: The extension-first mindset is the guiding principle. The frontend fully embraces this*

---

## Developer Roles & Import Boundaries

The directory a file lives in determines which import rules apply:

| Role | Directory | Can import from | Cannot import from |
|------|-----------|----------------|--------------------|
| Library Developer | `src/libs/` | Other libs | Packages, apps |
| Core Developer | `src/packages/core/` | Libs, other core modules | Non-core packages |
| HQ Package Developer | `src/packages/*` (not core) | Core, libs, other packages (carefully) | — |

**Library Developer** (`src/libs/`): Framework-agnostic infrastructure. No knowledge of the backoffice, CMS, or UI framework. Third-party dependencies included carefully.

**Core Developer** (`src/packages/core/`): UI framework implementation — extension registry, routing, modals, notifications, workspace infrastructure. Does not implement CMS-specific features.

**HQ Package Developer** (`src/packages/*` except `core/`): CMS feature packages (documents, media, members, templating). Cross-package imports increase coupling — minimize them. Use public `index.ts` exports, never import another package's internal files.

---

## The Package System

Each directory under `src/packages/` is a self-contained domain module. Design principle: each package could theoretically be uninstalled independently. `core` provides foundational APIs; every other package builds on top.

### Module Exports

Packages expose subpath exports (e.g., `@umbraco-cms/backoffice/dashboard`, `@umbraco-cms/backoffice/media`). Each maps to an `index.ts` barrel file. See `package.json` exports field for the full list.

---

## Design Patterns

1. **Web Components** - Custom elements with Shadow DOM
2. **Context API** - DOM-event-based dependency injection (provider/consumer)
3. **Controller Pattern** - Lifecycle-aware logic via `UmbControllerBase`
4. **Extension System** - Manifest-based plugin architecture
5. **Observable Pattern** - Reactive state via UmbState classes
6. **Repository Pattern** - Data access abstraction
7. **Mixin Pattern** - Composable behaviors (`UmbElementMixin`, etc.)

### Extension Registry

All UI is registered as **Extension Manifests**. The registry is mutable at runtime — extensions can be added, removed, or replaced.

```typescript
const manifest: UmbExtensionManifest = {
  type: 'dashboard',
  alias: 'My.Dashboard',
  name: 'My Dashboard',
  element: () => import('./my-dashboard.element.js'),
  weight: 100,
  meta: { label: 'My Dashboard', pathname: 'my-dashboard' },
  conditions: [
    { alias: 'Umb.Condition.SectionAlias', match: 'Umb.Section.Content' },
  ],
};
```

Key extension types: `section`, `sectionView`, `dashboard`, `workspace`, `workspaceView`, `workspaceAction`, `workspaceContext`, `propertyEditorUi`, `propertyEditorSchema`, `tree`, `treeItem`, `menuItem`, `entityAction`, `entityBulkAction`, `headerApp`, `globalContext`, `modal`, `bundle`, `backofficeEntryPoint`, `localization`, `condition`, `kind`.

**Registration methods:**

- **Internal packages**: `umbraco-package.ts` exports a `bundle` manifest with `js: () => import('./manifests.js')`. The `manifests.ts` aggregates sub-feature manifests. Bundle type ensures lazy-loading.

```typescript
// umbraco-package.ts
export const name = 'Umbraco.Core.MyPackage';
export const extensions = [
  { name: 'My Package Bundle', alias: 'Umb.Bundle.MyPackage', type: 'bundle', js: () => import('./manifests.js') },
];

// manifests.ts
import { manifests as featureAManifests } from './feature-a/manifests.js';
export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...featureAManifests];
```

- **External packages**: `umbraco-package.json` (static).
- **Runtime**: `umbExtensionsRegistry.register(manifest)` or `registerMany(manifests)`.

**Conditions & Kinds:**
- **Conditions**: Declarative rules for when an extension is active (e.g., section alias match).
- **Kinds**: Preset manifest configurations that extensions inherit from (reduces boilerplate).

### Context API

Framework-agnostic, DOM-event-based dependency injection. Contexts flow down the DOM tree.

- **Provider**: `this.provideContext(TOKEN, instance)` on a parent element
- **Consumer**: `this.consumeContext(TOKEN, callback)` on any descendant
- **Context Token** (`UmbContextToken<T>`): Typed identifier for type safety
- **Scoping**: Global (notifications, current user), workspace-scoped, section-scoped

```typescript
export const MY_CONTEXT = new UmbContextToken<MyContext>('MyContext');
this.provideContext(MY_CONTEXT, new MyContext(this));        // provider
this.consumeContext(MY_CONTEXT, (ctx) => { /* typed */ });   // consumer
```

### Observable State Management

State is managed via `UmbStringState`, `UmbNumberState`, `UmbArrayState`, `UmbObjectState`. These wrap values as observables. Elements use `this.observe()` (from `UmbLitElement`/`UmbElementMixin`) to subscribe and trigger re-renders.

```typescript
#counter = new UmbNumberState(0);
readonly counter = this.#counter.asObservable();
increment() { this.#counter.setValue(this.#counter.value + 1); }
```

**Important**: Avoid using RxJS operators (`map`, `filter`, `combineLatest`, `switchMap`) directly. Use UmbState classes and `observe()` — they handle subscription lifecycle automatically. RxJS is an implementation detail.

### Repository Pattern & Data Flow

No element or context should call the Management API directly. All data flows through repositories.

`Element` <- observes <- `State` <- writes to <- `Context` <- calls <- `Repository` <- delegates to <- `Data Source`

1. **Data Source** - Raw transport (wraps generated OpenAPI client)
2. **Repository** - Domain interface (`getItems()`, `create()`, `save()`, `delete()`); the decoupling boundary
3. **Context** - Calls repository, writes results into State, manages lifecycle
4. **State** - Observable state classes exposing `.asObservable()`
5. **Element** - Uses `this.observe()` to subscribe; never calls repositories directly

```typescript
// Data Source
class MyServerDataSource {
  async getItems() { return await MyService.getItems(); }
}

// Repository
class MyRepository {
  #source = new MyServerDataSource();
  async requestItems() { return this.#source.getItems(); }
}

// Context — calls repository, writes to state
class MyContext extends UmbContextBase {
  #repo = new MyRepository();
  #items = new UmbArrayState<MyItem>([], (x) => x.id);
  readonly items = this.#items.asObservable();

  async load() { this.#items.setValue(await this.#repo.requestItems()); }
}

// Element — observes state
@customElement('umb-my-feature')
class MyElement extends UmbLitElement {
  #ctx = new MyContext(this);
  @state() private _items: MyItem[] = [];

  constructor() {
    super();
    this.observe(this.#ctx.items, (items) => { this._items = items; });
  }
}
```

### Controller System

`UmbControllerBase` for logic not tied to rendering. Controllers attach to a host element and participate in its lifecycle — used for consuming contexts, managing subscriptions, and encapsulating business logic.

---

The backoffice is independently-packaged Web Components glued by the Extension Registry, communicating via the Context API, with state managed through Observables — all built to be replaceable and extensible by default.

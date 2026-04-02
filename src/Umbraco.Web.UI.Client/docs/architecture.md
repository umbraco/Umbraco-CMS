# Architecture

---

### Technology Stack

> Exact versions are in `package.json` (root and workspace packages). Only major versions that affect API usage are listed here.

- **Runtime**: Node.js >=22, npm >=10 (see `engines` in `package.json`)
- **Language**: TypeScript 5.x (strict mode, ESM — all imports must use `.js` extensions)
- **Framework**: **Lit 3** — Web Components with reactive templates, decorators, and `html`/`css` tagged template literals
- **UI Library**: **@umbraco-ui/uui 1.x** — 80+ Web Components (`<uui-button>`, `<uui-input>`, `<uui-box>`, etc.). Always prefer UUI components over native HTML or custom implementations.
- **Rich Text**: **TipTap 3** — ProseMirror-based editor (see `src/packages/tiptap/`)
- **Build**: Vite 7 (config in `vite.config.ts`)
- **State**: RxJS 7 wrapped by UmbState classes — see [Core Primitives](./core-primitives.md)
- **Context/DI**: DOM-event-based Context API — see [Core Primitives](./core-primitives.md)
- **Testing**: @web/test-runner + Playwright, @open-wc/testing — see [Testing](./testing.md)
- **Mocking**: MSW 2 — see [Testing](./testing.md)
- **Code Quality**: ESLint 9 (flat config), Prettier
- **Key Libraries**: Luxon 3, Monaco Editor (code editing), DOMPurify 3 (sanitization — see [Security](./security.md)), Marked (Markdown→HTML), SignalR (real-time server events)

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

*Note: The extension-first mindset is the guiding principle, and the frontend fully embraces this by treating all features as manifest-registered, replaceable extensions.*

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

**Conditions**: Declarative rules for when an extension is active (e.g., section alias match, user permission).

### Kinds

Kinds are **generic, reusable implementations of an extension type**. A kind provides a pre-built `element`, `api`, or both for a specific purpose. Extensions reference a kind to inherit its implementation, then customize behavior through `meta`.

**How it works**: The registry merges kind defaults with the extension manifest. Extension properties override kind properties; `meta` is shallow-merged (extension meta extends/overrides kind meta).

**Defining a kind** (in a `*.kind.ts` file):

```typescript
export const manifest: UmbExtensionManifestKind = {
  type: 'kind',
  alias: 'Umb.Kind.EntityAction.Delete',
  matchKind: 'delete',
  matchType: 'entityAction',
  manifest: {
    type: 'entityAction',
    kind: 'delete',
    api: () => import('./delete.action.js'),
    meta: {
      icon: 'icon-trash',
      label: '#actions_delete',
      itemRepositoryAlias: '',       // to be filled by each integration
      detailRepositoryAlias: '',
    },
  },
};
```

**Using a kind** — the extension only specifies what differs:

```typescript
{
  type: 'entityAction',
  kind: 'delete',                    // inherits element, api, icon, label from the kind
  alias: 'Umb.EntityAction.DocumentType.Delete',
  name: 'Delete Document-Type Entity Action',
  forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
  meta: {                            // customize via meta
    itemRepositoryAlias: UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
    detailRepositoryAlias: UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
  },
}
```

**Best practices:**
- Kinds are for reusability — use them when multiple extensions share the same element/api implementation with only meta differences.
- Any package (core or feature) can register kinds. Core provides common kinds (delete, moveTo, duplicate, default section, default workspace, etc.); feature packages can define domain-specific kinds.
- Individual integrations customize behavior through `meta` — not by overriding `element` or `api` (unless replacing the implementation entirely).
- Kinds can extend other kinds by spreading their manifest: `manifest: { ...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest, kind: 'delete', ... }`.
- **Changing a kind affects all extensions that use it.** Treat kind implementations (element, api, meta shape) as a shared contract — changes have a wide blast radius across packages.

### Context API, Observable State & Controllers

These core primitives are documented in detail in **[Core Primitives](./core-primitives.md)**. In brief:

- **Context API** — DOM-event-based dependency injection. Providers expose typed contexts to descendant elements via `provideContext()`, consumers access them via `consumeContext()` or `getContext()`.
- **Observable State** — Reactive state containers (`UmbStringState`, `UmbArrayState`, `UmbObjectState`, etc.). Elements subscribe via `this.observe()` which handles lifecycle automatically.
- **Controllers** — `UmbControllerBase` for logic not tied to rendering. Attach to a host element and participate in its lifecycle.

**Important**: Avoid using RxJS operators (`map`, `filter`, `combineLatest`, `switchMap`) directly. Use UmbState classes and `observe()` — they handle subscription lifecycle automatically. RxJS is an implementation detail.

### Repository Pattern & Data Flow

No element or context should call the Management API directly. All data flows through repositories:

`Element` → observes → `Context` → calls → `Repository` → delegates to → `Data Source` → calls → `Generated API Client`

See **[Data Flow](./data-flow.md)** for the complete implementation pattern with base classes, `tryExecute()`, and a worked example.

---

The backoffice is independently-packaged Web Components glued by the Extension Registry, communicating via the Context API, with state managed through Observables — all built to be replaceable and extensible by default.

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

### Design Patterns

1. **Web Components** - Custom elements with Shadow DOM encapsulation
2. **Context API** - Dependency injection via context providers/consumers (similar to React Context)
3. **Controller Pattern** - Lifecycle-aware controllers for managing component behavior
4. **Extension System** - Manifest-based plugin architecture
5. **Observable Pattern** - Reactive state management with RxJS observables
6. **Repository Pattern** - Data access abstraction via repository classes
7. **Mixin Pattern** - Composable behaviors via TypeScript mixins (`UmbElementMixin`, etc.)
8. **Builder Pattern** - For complex object construction
9. **Registry Pattern** - Extension registry for dynamic feature loading
10. **Observer Pattern** - Event-driven communication between components

### Key Technologies

**Core Framework**:
- Lit 3.x - Web Components framework with reactive templates
- TypeScript 5.9 - Type-safe development with strict mode
- Vite - Fast build tool and dev server

**UI Components**:
- @umbraco-ui/uui - Umbraco UI component library
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


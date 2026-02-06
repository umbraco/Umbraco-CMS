# Umbraco Backoffice - @umbraco-cms/backoffice

Modern TypeScript/Lit-based web components library for the Umbraco CMS backoffice interface. This project provides extensible UI components, APIs, and utilities for building the Umbraco CMS administration interface.

**Package**: `@umbraco-cms/backoffice`
**Version**: 17.1.0-rc
**License**: MIT
**Repository**: https://github.com/umbraco/Umbraco-CMS
**Live Preview**: https://backofficepreview.umbraco.com/

---

## Documentation Structure

This project's documentation is organized into 9 focused guides:

**Note**: This is a sub-project in the Umbraco CMS monorepo. For Git workflow, PR process, and CI/CD information, see the [repository root CLAUDE.md](../../CLAUDE.md).

### Architecture & Design
- **[Architecture](./docs/architecture.md)** - Technology stack, design patterns, module organization

### Development
- **[Commands](./docs/commands.md)** - Build, test, and development commands

### Code Quality
- **[Style Guide](./docs/style-guide.md)** - Naming and formatting conventions
- **[Clean Code](./docs/clean-code.md)** - Best practices and SOLID principles
- **[Testing](./docs/testing.md)** - Unit, integration, and E2E testing strategies

### Troubleshooting
- **[Error Handling](./docs/error-handling.md)** - Error patterns and debugging
- **[Edge Cases](./docs/edge-cases.md)** - Common pitfalls and gotchas

### Security & AI
- **[Security](./docs/security.md)** - XSS prevention, authentication, input validation
- **[Agentic Workflow](./docs/agentic-workflow.md)** - Three-phase AI development process

---

## Quick Start

### Prerequisites

- **Node.js**: >=22.17.1
- **npm**: >=10.9.2
- Modern browser (Chrome, Firefox, Safari)

### Initial Setup

```bash
# 1. Clone repository
git clone https://github.com/umbraco/Umbraco-CMS.git
cd Umbraco-CMS/src/Umbraco.Web.UI.Client

# 2. Install dependencies
npm install

# 3. Start development
npm run dev
```

### Most Common Commands

See **[Commands](./docs/commands.md)** for all available commands.

| Task | Command |
|------|---------|
| Development | `npm run dev` |
| Testing (all) | `npm test` |
| Testing (specific file) | `npm test -- --files "src/packages/path/to/file.test.ts"` |
| Build | `npm run build` |
| Lint | `npm run lint:fix` |

---

## Quick Reference

| Category | Details |
|----------|---------|
| **Apps** | `src/apps/` - Application entry points (app, installer, upgrader) |
| **Libraries** | `src/libs/` - Core APIs (element-api, context-api, controller-api) |
| **Packages** | `src/packages/` - Feature packages; `src/packages/core/` for utilities |
| **External** | `src/external/` - Dependency wrappers (lit, rxjs, luxon) |
| **Mocks** | `src/mocks/` - MSW handlers and mock data |
| **Config** | `package.json`, `vite.config.ts`, `.env` (create `.env.local`) |
| **Elements** | Custom elements use `umb-{feature}-{component}` pattern |

### Getting Help

**Documentation**: [UI API Docs](npm run generate:ui-api-docs) | [Storybook](npm run storybook) | [Official Docs](https://docs.umbraco.com/)
**Community**: [Issues](https://github.com/umbraco/Umbraco-CMS/issues) | [Discussions](https://github.com/umbraco/Umbraco-CMS/discussions) | [Forum](https://our.umbraco.com/)

---

## npm Package Publishing

### Overview

The backoffice is published to npm as `@umbraco-cms/backoffice` with a plugin-first architecture. All dependencies are **peerDependencies** because:

1. **Importmap provides runtime**: The actual code at runtime comes from importmap, not npm
2. **Types are the primary need**: Plugin developers need types for development, but runtime is managed centrally
3. **Version flexibility**: Allows plugins to use different versions of pre-release packages (e.g., `@hey-api/openapi-ts`)

### Dependency Hoisting & Version Ranges

The `npm pack` process (prepack hook) runs `devops/publish/cleanse-pkg.js` which:

1. **Collects dependencies** from all workspace subpackages
2. **Converts to peerDependencies** at the root level
3. **Intelligently adjusts version ranges** based on stability

#### Version Range Conversion Logic

Uses the `semver` package (npm's own semver library) for robust parsing:

**Pre-release packages (0.x.y)**
```
Input:  ^0.85.0    or    0.85.0
Output: >=0.85.0 <1.0.0

Why: Pre-release caret (^0.85.0) only allows patch updates (0.85.x).
     Explicit range allows plugins to use 0.91.1 without conflicts.
```

**Stable packages with caret (major ≥ 1)**
```
Input:  ^3.3.1
Output: ^3.3.1    (kept as-is)

Why: Caret already implements the correct range: >=3.3.1 <4.0.0
```

**Stable exact versions (major ≥ 1)**
```
Input:  3.16.0    (from @tiptap/*)
Output: ^3.16.0

Why: Normalizes to conventional semver format
```

#### Example Published peerDependencies

```json
{
  "peerDependencies": {
    "lit": "^3.3.1",
    "rxjs": "^7.8.2",
    "@umbraco-ui/uui": "^1.17.0-rc.5",
    "monaco-editor": "^0.55.1",
    "@tiptap/core": "^3.16.0",
    "@hey-api/openapi-ts": ">=0.85.0 <1.0.0"
  }
}
```

### Plugin Developer Guide

When using `@umbraco-cms/backoffice`:

- **Declare dependencies explicitly** in your `package.json` (don't rely on transitive deps from backoffice)
- **Version ranges are flexible**: `>=0.85.0 <1.0.0` means you can use `0.85.0`, `0.91.1`, or `0.99.99`
- **Types come from npm**: TypeScript gets types from your declared versions
- **Runtime comes from importmap**: The actual code at runtime is managed by the backoffice (importmap)
- **Future compatibility**: When `@hey-api` hits `1.0.0`, the published range will automatically become `^1.0.0`

### Key Files

| File | Purpose |
|------|---------|
| `package.json` | Root package with exports and workspace references |
| `devops/publish/cleanse-pkg.js` | Script that runs during `npm pack` to hoist and convert versions |
| `src/external/*` | Dependency wrapper packages |
| `src/packages/core` | Contains `@hey-api/openapi-ts` and other utilities |

---

**This project follows a modular package architecture with strict TypeScript, Lit web components, and an extensible manifest system. Each package is independent but follows consistent patterns. For extension development, use the Context API for dependency injection, controllers for logic, and manifests for registration.**

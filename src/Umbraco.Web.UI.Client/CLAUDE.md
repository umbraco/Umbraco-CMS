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

**This project follows a modular package architecture with strict TypeScript, Lit web components, and an extensible manifest system. Each package is independent but follows consistent patterns. For extension development, use the Context API for dependency injection, controllers for logic, and manifests for registration.**

# Umbraco Backoffice - @umbraco-cms/backoffice

TypeScript/Lit web components library for the Umbraco CMS backoffice. Published as `@umbraco-cms/backoffice` (npm). Sub-project of the Umbraco CMS monorepo — for Git workflow, PR process, and CI/CD, see [repository root CLAUDE.md](../../CLAUDE.md).

## Documentation Structure

### Architecture & Design
- **[Architecture](./docs/architecture.md)** - Technology stack, design philosophy, developer roles, package system, import map pipeline, design patterns
- **[Entities](./docs/entities.md)** - Entity types, entity context, how entityType connects workspaces/trees/actions/routing
- **[Workspaces](./docs/workspaces.md)** - Workspace types, base classes, extension points, workspace contexts, save flow, routing
- **[Core Primitives](./docs/core-primitives.md)** - UmbLitElement, observable state (UmbArrayState, UmbObjectState, etc.), Context API, controller lifecycle
- **[Data Flow](./docs/data-flow.md)** - Data flow chain, data sources, tryExecute, generated API clients, stores, complete worked example
- **[Repositories](./docs/repositories.md)** - Repository categories (detail, item, tree, collection, action-specific), file structure, naming, extension registration, data source delegation
- **[Package Development](./docs/package-development.md)** - Package & module structure, folder structure conventions, localization, organizational rules

### Development
- **[Commands](./docs/commands.md)** - Build, test, and development commands

### Code Quality
- **[Style Guide](./docs/style-guide.md)** - Naming and formatting conventions
- **[Clean Code](./docs/clean-code.md)** - Best practices and SOLID principles
- **[Deprecation](./docs/deprecation.md)** - Breaking changes policy, deprecation patterns (JSDoc + UmbDeprecation)
- **[Testing](./docs/testing.md)** - Testing strategy, priority by code area, MSW mocking, test patterns

### Troubleshooting
- **[Error Handling](./docs/error-handling.md)** - Error patterns and debugging
- **[Edge Cases](./docs/edge-cases.md)** - Common pitfalls and gotchas

### Security & AI
- **[Security](./docs/security.md)** - XSS prevention, authentication, input validation
- **[Agentic Workflow](./docs/agentic-workflow.md)** - Three-phase AI development process

---

## Skills & Documentation — Mandatory Usage

**When a skill or documentation file exists for the task you are performing, you MUST use it. Do NOT improvise, skip steps, or manually create files that a skill is designed to scaffold.**

- Before starting any scaffolding or creation task, check if a matching skill exists in `.claude/skills/`. If one exists, invoke it.
- Skills define prerequisites. If a prerequisite is not met, you MUST resolve it first using the appropriate skill.
- Documentation files (`docs/*.md`) describe conventions and patterns. When they cover the area you are working in, read and follow them.
- If a skill includes verification steps, run them before proceeding to the next step.
- If you are unsure whether a skill applies, check — the cost of checking is low, the cost of skipping is high.

---

## Quick Start

```bash
cd src/Umbraco.Web.UI.Client && npm install && npm run dev
```

### Common Commands

See **[Commands](./docs/commands.md)** for all available commands.

| Task | Command |
|------|---------|
| Development | `npm run dev` |
| Testing (all) | `npm test` |
| Testing (specific file) | `npm test -- --files "src/packages/path/to/file.test.ts"` |
| Build | `npm run build` |
| Lint | `npm run lint:fix` |
| Circular dep check | `npm run check:circular` |

---

## Quick Reference

| Item | Details |
|------|---------|
| **Config** | `package.json`, `vite.config.ts`, `.env` (create `.env.local`) |
| **Element naming** | `umb-{feature}-{component}` for core; package devs use own prefix |
| **Directory structure** | See [Architecture](./docs/architecture.md#architecture-pattern) |

---

## npm Package Publishing

All dependencies are **peerDependencies** because importmap provides runtime code; npm provides types only. This gives plugins version flexibility (e.g., different pre-release versions).

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

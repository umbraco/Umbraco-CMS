---
name: core-add-module
description: Add a new module to the core package (src/packages/core/). Use when adding shared UI framework infrastructure that all packages can use — e.g., a new extension system feature, a new utility, a new shared component pattern, a new picker, or a new workspace primitive. Core modules are imported as @umbraco-cms/backoffice/{module-name}. Also use when the user says things like "add a module to core", "create a new core feature", or "add shared infrastructure".
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Add Core Module

Add a new module to the core package (`src/packages/core/`).

## Naming conventions

- **Module names** are **singular** (e.g., `picker`, `sorter`, `entity-flag`, `modal`, `section`)

## What you need from the user

1. **Module name** — What to call it in singular form (kebab-case, e.g., `picker`, `sorter`, `entity-flag`)
2. **Purpose** — What shared infrastructure this provides

## When to add to core vs. create a new package

- **Core module**: Shared UI framework infrastructure used by multiple packages — extension primitives, pickers, validation, repository base classes, workspace utilities.
- **New package**: Domain-specific CMS features (documents, media, webhook). Use the `create-package` skill instead.

Core modules should not implement CMS-specific features. They provide the building blocks that CMS feature packages use.

## Files to create

```
src/packages/core/{module-name}/
├── index.ts                   # Public API exports
├── manifests.ts               # Extension registrations (if any)
├── constants.ts               # Module constants
└── types.ts                   # Module types
```

## Step 1: Create the module directory and files

### {module-name}/index.ts

The public API. Only export what other packages need:

```typescript
export * from './constants.js';
export type * from './types.js';
```

Use `export type *` for type-only exports.

### {module-name}/manifests.ts

Extension registrations for this module:

```typescript
export const manifests: Array<UmbExtensionManifest> = [
	// Extension registrations
];
```

If the module has no extensions (pure utility), export an empty array.

### {module-name}/constants.ts

```typescript
// Aliases, context tokens, entity types
```

### {module-name}/types.ts

```typescript
// Type definitions, interfaces, meta types
```

## Step 2: Wire into core's manifests.ts

Add the module's manifests to `src/packages/core/manifests.ts`:

```typescript
import { manifests as {moduleName}Manifests } from './{module-name}/manifests.js';

// In the manifests array:
...{moduleName}Manifests,
```

The imports are in alphabetical order — insert in the right position.

## Step 3: Add entry point to core's vite.config.ts

Add the module to the `entry` object in `src/packages/core/vite.config.ts`:

```typescript
'{module-name}/index': './{module-name}/index.ts',
```

The entries are in alphabetical order — insert in the right position. This tells Vite to build the module as a separate entry point.

## Step 4: Register the subpath export

Add to the `exports` field in the root `package.json` (`src/Umbraco.Web.UI.Client/package.json`):

```json
"./{module-name}": "./dist-cms/packages/core/{module-name}/index.js"
```

This enables imports like `@umbraco-cms/backoffice/{module-name}`.

## Step 5: Regenerate TypeScript config

```bash
npm run generate:tsconfig
```

This updates `tsconfig.json` paths so the new module resolves correctly in TypeScript.

## How consumers import

After registration, other packages import from the module like:

```typescript
import { MyThing } from '@umbraco-cms/backoffice/{module-name}';
import type { MyType } from '@umbraco-cms/backoffice/{module-name}';
```

## Reference: existing core modules to study

- **Simple utility**: `src/packages/core/culture/` — repository + components, minimal surface
- **Extension primitive**: `src/packages/core/entity-action/` — kinds, base classes, shared element
- **Infrastructure**: `src/packages/core/repository/` — base classes for the data access pattern

## Checklist

- [ ] Module directory created under `src/packages/core/`
- [ ] `index.ts` exports only the public API
- [ ] `manifests.ts` exports extension registrations (or empty array)
- [ ] `constants.ts` and `types.ts` created
- [ ] Module manifests imported in `src/packages/core/manifests.ts` (alphabetical order)
- [ ] Entry point added to `src/packages/core/vite.config.ts` (alphabetical order)
- [ ] Subpath export added to root `package.json`
- [ ] `npm run generate:tsconfig` executed
- [ ] Compiles: `npm run compile`

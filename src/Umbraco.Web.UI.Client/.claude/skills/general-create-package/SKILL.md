---
name: general-create-package
description: Create a new package in the Umbraco backoffice client with its first module. Use when adding a new domain feature area that needs its own package under src/packages/ — e.g., a new CMS feature like webhooks, tags, or relations. Packages are self-contained domain modules that can theoretically be uninstalled independently. Also use when the user says things like "scaffold a new package", "add a new feature package", or "create a new domain module".
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Create Package

Create a new self-contained package under `src/packages/` with its first module.

## Naming conventions

- **Package names** are **plural** (e.g., `webhooks`, `languages`, `tags`, `documents`, `relations`)
- **Module names** are **singular** (e.g., `webhook`, `language`, `tag`, `document`, `relation`)

## What you need from the user

1. **Package name** — The domain name in plural form (e.g., `webhooks`, `languages`, `tags`) — kebab-case
2. **First module name** — The primary module in singular form (e.g., `webhook`, `language`, `tag`)
3. **Whether it has an entity type** — Most packages represent an entity (e.g., webhook, language)

## When to create a package vs. add to core

- **New package**: Domain-specific CMS features (documents, media, webhook, language). Each package is independent and self-contained.
- **Add to core**: UI framework infrastructure shared by all packages (modals, routing, extension system). Use the `add-core-module` skill instead.

## Files to create

```
src/packages/{package-name}/
├── package.json                    # npm workspace package
├── vite.config.ts                  # Build configuration
├── umbraco-package.ts              # Bundle entry point (lazy-loads manifests)
├── manifests.ts                    # Aggregates all module manifests
├── entity.ts                       # Entity type constants (if entity-based)
├── {module-name}/                  # First module
│   ├── index.ts                   # Public API exports
│   ├── manifests.ts               # Module manifest registrations
│   ├── constants.ts               # Module constants
│   └── types.ts                   # Module types
└── index.ts                        # Package-level public API (optional, only if exporting)
```

## Step 1: Create package.json

Every package is an npm workspace. It needs a minimal `package.json`:

```json
{
	"name": "@umbraco-cms/backoffice-{package-name}",
	"version": "0.0.0",
	"private": true,
	"type": "module",
	"scripts": {
		"build": "vite build"
	}
}
```

## Step 2: Create vite.config.ts

Non-core packages use the simple default config with three entry points (`index.ts`, `manifests.ts`, `umbraco-package.ts`):

```typescript
import { defineConfig } from 'vite';
import { rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/packages/{package-name}';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

export default defineConfig({
	...getDefaultConfig({ dist }),
});
```

## Step 3: Create umbraco-package.ts

The bundle entry point that lazy-loads the package's manifests:

```typescript
export const name = 'Umbraco.Core.{PackageName}';
export const extensions = [
	{
		name: '{PackageName} Bundle',
		alias: 'Umb.Bundle.{PackageName}',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
```

Convention: `name` uses `Umbraco.Core.{PascalCaseName}`, alias uses `Umb.Bundle.{PascalCaseName}`.

## Step 4: Create entity.ts (if entity-based)

Most packages define entity types and workspace aliases:

```typescript
export const UMB_{ENTITY}_ENTITY_TYPE = '{entity-name}';
export const UMB_{ENTITY}_ROOT_ENTITY_TYPE = '{entity-name}-root';
export const UMB_{ENTITY}_WORKSPACE_ALIAS = 'Umb.Workspace.{EntityName}';

export type Umb{EntityName}EntityType = typeof UMB_{ENTITY}_ENTITY_TYPE;
```

## Step 5: Create the package-level manifests.ts

Aggregates all module manifests:

```typescript
import { manifests as {moduleName}Manifests } from './{module-name}/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...{moduleName}Manifests,
];
```

As you add more modules, import and spread their manifests here.

## Step 6: Create the first module

### {module-name}/manifests.ts

```typescript
export const manifests: Array<UmbExtensionManifest> = [
	// Extension registrations go here as you build features
];
```

### {module-name}/index.ts

```typescript
// Public API exports — classes, types, constants needed by other packages
```

### {module-name}/constants.ts

```typescript
// Module constants — aliases, entity types, context tokens
```

### {module-name}/types.ts

```typescript
// Module type definitions — domain models, config interfaces
```

## Step 7: Register the package

Three registrations are needed to wire the package into the build:

### 1. Add subpath export to root package.json

In `/src/Umbraco.Web.UI.Client/package.json`, add to the `exports` field:

```json
"./{package-name}": "./dist-cms/packages/{package-name}/index.js"
```

### 2. Regenerate TypeScript config

```bash
npm run generate:tsconfig
```

This updates `tsconfig.json` paths so `@umbraco-cms/backoffice/{package-name}` resolves correctly.

### 3. Install workspace dependencies

```bash
npm install
```

This registers the new workspace package with npm.

## Reference: existing packages to study

- **Simple package**: `src/packages/webhook/` — straightforward entity with CRUD
- **Complex package**: `src/packages/documents/` — multiple modules, rich entity
- **Minimal package**: `src/packages/tags/` — small feature area

## Checklist

- [ ] `package.json` created with `"type": "module"` and build script
- [ ] `vite.config.ts` created using `getDefaultConfig()`
- [ ] `umbraco-package.ts` created with bundle entry point
- [ ] `manifests.ts` created at package level, aggregating module manifests
- [ ] First module has `manifests.ts`, `index.ts`, `constants.ts`, `types.ts`
- [ ] `entity.ts` created with entity type constants (if entity-based)
- [ ] Subpath export added to root `package.json`
- [ ] `npm run generate:tsconfig` executed
- [ ] `npm install` executed to register the workspace
- [ ] Compiles: `npm run compile`

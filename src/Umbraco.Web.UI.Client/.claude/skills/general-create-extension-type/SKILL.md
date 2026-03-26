---
name: general-create-extension-type
description: Create a new extension type for the Umbraco backoffice extension registry. Use when adding a new type of extension that can be registered via manifests (e.g., a new dashboard type, a new sidebar app type, a new toolbar extension type). Any package can define extension types.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Create Extension Type

Create a new extension type for the Umbraco backoffice extension registry.

## What you need from the user

1. **Type name** — The `type` string used in manifests (e.g., `'dashboard'`, `'searchProvider'`)
2. **Purpose** — What this extension type does (determines which base interface to extend)
3. **Meta shape** — What configuration the extension needs from consumers
4. **Whether it needs conditions** — Should extensions of this type support conditional loading?
5. **Whether it has an element, API, or both** — Does it render UI, provide logic, or both?

## Files to create

All files go in the relevant feature directory (e.g., `src/packages/my-package/my-feature/`).

```
my-feature/
├── my-feature.extension.ts     # Type definition + global declaration
├── my-feature-element.interface.ts  # Element interface (if element-based)
└── types.ts                    # Meta interface (if needed)
```

## Step 1: Choose the base manifest interface

| Your extension... | Extend from | Import |
|---|---|---|
| Renders a custom element | `ManifestElement<T>` | `@umbraco-cms/backoffice/extension-api` |
| Provides an API class (no UI) | `ManifestApi<T>` | `@umbraco-cms/backoffice/extension-api` |
| Has both element and API | `ManifestElementAndApi<E, A>` | `@umbraco-cms/backoffice/extension-api` |
| Needs conditional loading | Also extend `ManifestWithDynamicConditions<UmbExtensionConditionConfig>` | `@umbraco-cms/backoffice/extension-api` |

## Step 2: Define the manifest interface

```typescript
// my-feature.extension.ts
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface MetaMyFeature {
	label?: string;
	// ... properties consumers configure via meta
}

export interface ManifestMyFeature
	extends ManifestElement<UmbMyFeatureElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'myFeature';
	meta: MetaMyFeature;
}

declare global {
	interface UmbExtensionManifestMap {
		umbMyFeature: ManifestMyFeature;
	}
}
```

### Key rules

- The `type` property must be a **string literal type** (not `string`)
- The key in `UmbExtensionManifestMap` must be unique across the entire codebase
- The `declare global` block is what makes the registry type-aware — without it, the type won't be recognized

## Step 3: Define the element/API interface (if needed)

```typescript
// my-feature-element.interface.ts
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

export interface UmbMyFeatureElement extends UmbElement {
	// Properties/methods that implementations must provide
}
```

## Step 4: If the type supports kinds

Add a union type for the base manifest plus kind-specific variants:

```typescript
export interface ManifestMyFeatureButtonKind extends ManifestMyFeature {
	type: 'myFeature';
	kind: 'button';
	meta: MetaMyFeatureButtonKind;
}

export interface MetaMyFeatureButtonKind extends MetaMyFeature {
	icon: string;
}

// Union all variants in the global declaration
declare global {
	interface UmbExtensionManifestMap {
		umbMyFeature: ManifestMyFeature | ManifestMyFeatureButtonKind;
	}
}
```

## Step 5: Export from the package barrel

Add the type export to the package's `index.ts`:

```typescript
export type * from './my-feature/my-feature.extension.js';
```

Use `export type *` since these are type-only exports.

## Step 6: Register manifests that use this type

Verify the type works by creating a test manifest:

```typescript
const manifest: UmbExtensionManifest = {
	type: 'myFeature',
	alias: 'Umb.MyFeature.Test',
	name: 'Test My Feature',
	element: () => import('./my-feature-test.element.js'),
	meta: {
		label: 'Test',
	},
};
```

TypeScript should provide autocomplete for `type` and validate `meta` against `MetaMyFeature`.

## Checklist

- [ ] Manifest interface extends the correct base (`ManifestElement`, `ManifestApi`, or `ManifestElementAndApi`)
- [ ] `type` property is a string literal, not `string`
- [ ] `declare global` block adds to `UmbExtensionManifestMap` with a unique key
- [ ] Meta interface is defined separately and exported
- [ ] Element/API interface is defined if the type renders UI or provides logic
- [ ] If kinds are supported: kind-specific interfaces extend the base with literal `kind` property
- [ ] Type is exported from the package's `index.ts`
- [ ] Compiles: `npm run compile`

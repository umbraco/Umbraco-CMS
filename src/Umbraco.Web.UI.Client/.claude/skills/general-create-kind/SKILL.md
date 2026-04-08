---
name: general-create-kind
description: Create a new extension kind — a reusable default implementation for an extension type. Use when multiple extensions of the same type share the same element/API implementation and only differ in configuration. Kinds reduce boilerplate by providing pre-built defaults that extensions customize via meta.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Create Kind

Create a new extension kind — a reusable default implementation for an extension type.

## What you need from the user

1. **Which extension type** — The `type` this kind applies to (e.g., `'entityAction'`, `'section'`)
2. **Kind name** — The `kind` string (e.g., `'delete'`, `'default'`, `'button'`)
3. **What it provides** — Default element, API, or both
4. **Meta defaults** — What meta values the kind pre-fills for consumers

## When to create a kind

Create a kind when **multiple extensions of the same type share the same element/API implementation** and only differ in meta configuration. If the implementation is unique, a kind adds unnecessary indirection.

## Files to create

```
my-feature/
├── kinds/
│   └── my-kind/
│       ├── my-kind.kind.ts              # Kind manifest definition
│       ├── my-kind.element.ts           # Default element (if providing UI)
│       ├── my-kind.api.ts               # Default API class (if providing logic)
│       └── manifests.ts                 # Exports the kind manifest
```

## Step 1: Define the kind manifest

```typescript
// my-kind.kind.ts
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MY_FEATURE_MY_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.MyFeature.MyKind',
	matchType: 'myFeature',
	matchKind: 'myKind',
	manifest: {
		type: 'myFeature',
		kind: 'myKind',
		element: () => import('./my-kind.element.js'),
		api: () => import('./my-kind.api.js'),
		weight: 1000,
		meta: {
			icon: '',
			label: '',
			// ... defaults that consumers override
		},
	},
};

export const manifest = UMB_MY_FEATURE_MY_KIND_MANIFEST;
```

### Key fields

- **`type`**: Always `'kind'`
- **`alias`**: Unique identifier. Convention: `Umb.Kind.{ExtensionType}.{KindName}`
- **`matchType`**: The extension type this kind applies to
- **`matchKind`**: The kind name that extensions reference
- **`manifest`**: Partial manifest with defaults — any property here becomes the base for extensions using this kind

## Step 2: Create the default element (if providing UI)

```typescript
// my-kind.element.ts
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-my-kind-element')
export class UmbMyKindElement extends UmbLitElement {
	// This element is shared by all extensions using this kind.
	// Use manifest meta for customization — not hardcoded values.

	override render() {
		return html`<!-- default rendering -->`;
	}
}

export { UmbMyKindElement as element };
```

**Important:** Export as `element` (named export) — the lazy `import()` in the kind manifest resolves this name.

## Step 3: Create the default API (if providing logic)

```typescript
// my-kind.api.ts
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbMyKindApi extends UmbControllerBase {
	// Shared logic for all extensions using this kind.
	// Access extension-specific config via the manifest meta.
}

export { UmbMyKindApi as api };
```

## Step 4: Register the kind in manifests

```typescript
// manifests.ts
import { manifest as myKindManifest } from './my-kind.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [myKindManifest];
```

Add this to the parent feature's `manifests.ts` so it bubbles up to the bundle.

## Step 5: Add kind-specific type to the extension type (if not already done)

In the extension type definition file, add a variant interface for this kind:

```typescript
export interface ManifestMyFeatureMyKind extends ManifestMyFeature {
	type: 'myFeature';
	kind: 'myKind';
	meta: MetaMyFeatureMyKind;
}

export interface MetaMyFeatureMyKind extends MetaMyFeature {
	icon: string;
	label: string;
}

// Update the global declaration to include the kind variant
declare global {
	interface UmbExtensionManifestMap {
		umbMyFeature: ManifestMyFeature | ManifestMyFeatureMyKind;
	}
}
```

## How consumers use the kind

Extensions reference the kind and only specify what differs:

```typescript
{
	type: 'myFeature',
	kind: 'myKind',
	alias: 'My.Package.MyFeature.Specific',
	name: 'My Specific Feature',
	meta: {
		icon: 'icon-heart',
		label: 'My Label',
	},
}
```

The registry merges the kind defaults with the extension manifest:
- Extension properties **override** kind properties
- `meta` is **shallow-merged** (extension meta extends kind meta, doesn't replace it)

## Extending an existing kind

A kind can build on another kind by spreading its manifest:

```typescript
import { UMB_MY_FEATURE_DEFAULT_KIND_MANIFEST } from '../default/default.kind.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.MyFeature.Special',
	matchType: 'myFeature',
	matchKind: 'special',
	manifest: {
		...UMB_MY_FEATURE_DEFAULT_KIND_MANIFEST.manifest,
		type: 'myFeature',
		kind: 'special',
		api: () => import('./special.api.js'),
		meta: {
			...UMB_MY_FEATURE_DEFAULT_KIND_MANIFEST.manifest.meta,
			// additional meta defaults
		},
	},
};
```

## Checklist

- [ ] Kind manifest has `type: 'kind'` with unique `alias`
- [ ] `matchType` matches the target extension type exactly
- [ ] `matchKind` matches the `kind` string extensions will use
- [ ] Default element exported as `element`, default API exported as `api` (named exports for lazy import)
- [ ] Kind-specific manifest interface added to the extension type's `declare global` block
- [ ] Kind registered in `manifests.ts` with type `Array<UmbExtensionManifest | UmbExtensionManifestKind>`
- [ ] Meta defaults use empty strings or sensible fallbacks (not undefined)
- [ ] Compiles: `npm run compile`
- [ ] Blast radius considered: changing this kind affects all extensions that use it

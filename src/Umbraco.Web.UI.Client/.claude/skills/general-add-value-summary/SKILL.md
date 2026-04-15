---
name: general-add-value-summary
description: Add a value summary extension so a feature's typed value can be displayed compactly in collection views (e.g., table columns). Use when a feature or property editor already has a value type and needs a renderer for collection table cells. Two variants — simple (element only, no API call) and resolver (batch API resolution needed). Prerequisite: a value type constant must already exist — use the general-add-value-type skill first if it doesn't.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Add Value Summary

Add a `valueSummary` extension so a feature's values are rendered in collection views.

## Foundational documentation

Read before proceeding:

- **[Value Summary](../../../docs/value-summary.md)** — extension type, base classes, resolver contract, rules

## Prerequisite

A value type constant (`UMB_{FEATURE}_VALUE_TYPE`) must already exist and be exported from the package `index.ts`. If it doesn't exist yet, use the **`general-add-value-type`** skill first.

## What you need from the user

1. **Feature name** — kebab-case (e.g., `color-picker`, `user-group`)
2. **Package path** — where the feature lives (e.g., `src/packages/property-editors/color-picker/`)
3. **Value type constant** — the existing constant to bind to (e.g., `UMB_COLOR_PICKER_VALUE_TYPE`)
4. **Resolver needed?** — Yes if rendering requires an API call (resolving IDs to names). No if the raw value can be formatted directly.

**If resolver is needed, also ask:**
5. **Resolved type** — the TypeScript type after resolution (e.g., `UmbUserGroupItemModel`)
6. **Repository to use** — which item/detail repository resolves the values

---

## Option A: Simple summary (no resolver)

Use when the raw value can be formatted or rendered without an API call — dates, booleans, strings, color hex codes, numeric ranges, etc.

### Files to create

```
{package-path}/value-summary/
├── manifests.ts
└── {feature}-value-summary.element.ts
```

### Step 1: Create the summary element

File: `{package-path}/value-summary/{feature}-value-summary.element.ts`

```typescript
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-{feature}-value-summary')
export class Umb{Feature}ValueSummaryElement extends UmbValueSummaryElementBase<{TValue}> {
	override render() {
		if (!this._value) return html``;
		return html`<span>${this._value}</span>`;
	}
}

export { Umb{Feature}ValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-{feature}-value-summary']: Umb{Feature}ValueSummaryElement;
	}
}
```

Adjust `render()` for the value type:
- Dates → `this.localize.dateTime(value)`
- Booleans → `<umb-icon>`
- References → tags or comma-separated names

### Step 2: Create the manifest

File: `{package-path}/value-summary/manifests.ts`

```typescript
import type { UmbExtensionManifest } from '@umbraco-cms/backoffice/extension-api';
import { UMB_{FEATURE}_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.{Feature}',
		name: '{Feature} Value Summary',
		forValueType: UMB_{FEATURE}_VALUE_TYPE,
		element: () => import('./{feature}-value-summary.element.js'),
	},
];
```

### Step 3: Wire manifests into parent

```typescript
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...valueSummaryManifests,
	// ... other manifests
];
```

### Simple summary checklist

- [ ] Element extends `UmbValueSummaryElementBase` and exports as `element`
- [ ] Manifest uses `kind: 'default'`, correct `forValueType`, lazy `element` import
- [ ] Manifests wired into parent `manifests.ts`
- [ ] Compiles: `npm run compile`

---

## Option B: Summary with resolver (batch API resolution)

Use when displaying the value requires fetching data — e.g., resolving reference uniques to display names. The resolver receives all values from the entire column in one batch call.

### Files to create

```
{package-path}/value-summary/
├── manifests.ts
├── {feature}-value-summary.element.ts
└── {feature}-value-summary.resolver.ts
```

### Step 1: Create the summary element

Same pattern as Option A, Step 1. Pass `{TResolved}` as the generic instead of `{TValue}` — the element receives the resolved value in `this._value`, already correctly typed.

### Step 2: Create the resolver

File: `{package-path}/value-summary/{feature}-value-summary.resolver.ts`

```typescript
import type { UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { Umb{Entity}ItemRepository } from '../repository/item/{entity}-item.repository.js';

export class Umb{Feature}ValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<{TValue}, {TResolved}>
{
	#repo = new Umb{Entity}ItemRepository(this);

	async resolveValues(values: ReadonlyArray<{TValue}>) {
		// Deduplicate all IDs needed across the entire batch
		const allUniques = [...new Set(values.flatMap((v) => /* extract IDs from v */))];

		const { data: items, asObservable } = await this.#repo.requestItems(allUniques);
		if (!items) return { data: values.map(() => undefined as unknown as {TResolved}) };

		// Positional mapping — output must align with input
		return {
			data: values.map((v) => /* find resolved item(s) for v */),
			asObservable: asObservable
				? () => asObservable().pipe(map((latest) => values.map((v) => /* remap */)))
				: undefined,
		};
	}
}

export { Umb{Feature}ValueSummaryResolver as valueResolver };
```

Critical rules:
- `data` must have the **same length** as `values` and be **positionally aligned**.
- Export the class as `valueResolver` (named export) — the loader checks for this name.
- Provide `asObservable` only when the resolved data can change after initial load.

### Step 3: Create the manifest

File: `{package-path}/value-summary/manifests.ts`

```typescript
import type { UmbExtensionManifest } from '@umbraco-cms/backoffice/extension-api';
import { UMB_{FEATURE}_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.{Feature}',
		name: '{Feature} Value Summary',
		forValueType: UMB_{FEATURE}_VALUE_TYPE,
		element: () => import('./{feature}-value-summary.element.js'),
		valueResolver: () => import('./{feature}-value-summary.resolver.js'),
	},
];
```

### Step 4: Wire manifests into parent

Same as Option A, Step 3.

### Resolver checklist

- [ ] Element extends `UmbValueSummaryElementBase`, renders resolved type, exports as `element`
- [ ] Resolver extends `UmbControllerBase`, implements `UmbValueSummaryResolver<{TValue}, {TResolved}>`
- [ ] `resolveValues()` returns `data` positionally aligned with `values` (same length, same order)
- [ ] Resolver exported as `valueResolver` (named export)
- [ ] Manifest includes `valueResolver: () => import(...)` lazy loader
- [ ] `asObservable` included only if resolved data can change reactively
- [ ] Manifests wired into parent `manifests.ts`
- [ ] Compiles: `npm run compile`

---

## Using the value type in a collection column

How you connect the value summary to a collection depends on the context.

### Property editors — no manifest step needed

Document collection views receive `editorAlias` from the server for each property value and pass it directly as `valueType` to `<umb-value-summary-extension>`. Registering the `valueSummary` manifest with `forValueType` equal to the schema alias is sufficient — the document collection picks it up automatically.

### Generic table collection kind — set `valueType` in the manifest

For non-property-editor columns (e.g., user state, dates, reference lists), set `valueType` statically in the table collection view manifest:

```typescript
import { UMB_{FEATURE}_VALUE_TYPE } from '@umbraco-cms/backoffice/{package}';

{
  type: 'collectionView',
  kind: 'table',
  alias: 'Umb.CollectionView.MyEntity.Table',
  meta: {
    columns: [
      { field: 'myField', label: 'My Field', valueType: UMB_{FEATURE}_VALUE_TYPE },
    ],
  },
}
```

`valueType` is `keyof UmbValueTypeMap` — TypeScript validates the key at compile time. This field is specific to the table collection view kind.

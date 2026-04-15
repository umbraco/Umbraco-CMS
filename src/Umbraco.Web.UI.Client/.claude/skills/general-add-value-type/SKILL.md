---
name: general-add-value-type
description: Add a new value type — a named, typed string constant that extends UmbValueTypeMap. Use when a feature needs to declare what type of value it holds so it can be referenced in collection columns or value summary manifests. Also use when a property editor needs to expose its value type for use in collection views.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Add Value Type

Add a new value type constant to `UmbValueTypeMap`.

## Foundational documentation

Read before proceeding:

- **[Value Type](../../../docs/value-type.md)** — full reference, naming convention, rules

## What you need from the user

1. **Feature name** — kebab-case (e.g., `color-picker`, `user-group`)
2. **Package path** — where the feature lives (e.g., `src/packages/property-editors/color-picker/`)
3. **Value type key** — the string alias:
   - For a property editor: use the schema alias (e.g., `'Umbraco.ColorPicker'`)
   - For a domain type: use `Umb.ValueType.{Entity}.{Shape}` (e.g., `'Umb.ValueType.UserGroup.References'`)
4. **TypeScript type** — the type of the raw value (e.g., `string`, `boolean`, `UmbReferenceByUnique[]`)

---

## Step 1: Create the constant file

File: `{package-path}/value-type/constants.ts`

```typescript
export const UMB_{FEATURE}_VALUE_TYPE = '{value-type-key}' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_{FEATURE}_VALUE_TYPE]: {TValue};
	}
}
```

Rules:
- The string value is always `as const`.
- The `declare global` block is in the **same file** as the constant.
- One constant per file — do not group multiple value types.

### Property editor example

```typescript
import type { UmbColorPickerPropertyEditorValue } from '../types.js';

export const UMB_COLOR_PICKER_VALUE_TYPE = 'Umbraco.ColorPicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_COLOR_PICKER_VALUE_TYPE]: UmbColorPickerPropertyEditorValue;
	}
}
```

### Domain type example

```typescript
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export const UMB_USER_GROUP_REFERENCES_VALUE_TYPE = 'Umb.ValueType.UserGroup.References' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_USER_GROUP_REFERENCES_VALUE_TYPE]: UmbReferenceByUnique[];
	}
}
```

---

## Step 2: Export from the package index

Add to the package `index.ts`:

```typescript
export * from './value-type/constants.js';
```

For **property editors**: the document collection connects to the value summary automatically via `editorAlias` from the server — no collection manifest step needed. The export is still required so the `valueSummary` manifest within the same package can import the constant.

For **generic table collection kind** (non-property-editor columns): other packages import the constant to set `valueType` in their collection view manifests.

---

## Checklist

- [ ] Constant declared with `as const`
- [ ] `declare global` block extends `UmbValueTypeMap` in the same file
- [ ] Key follows naming convention (schema alias for property editors, `Umb.ValueType.{Entity}.{Shape}` for domain types)
- [ ] TypeScript type is the correct raw value type
- [ ] One constant per file
- [ ] Constant exported from package `index.ts`
- [ ] Compiles: `npm run compile`

# Value Type

**Module**: `@umbraco-cms/backoffice/value-type`
**Location**: `src/packages/core/value-type/`

---

A compile-time type map that assigns a TypeScript type to each named value type. No runtime behaviour — purely a TypeScript contract.

## How it works

A global interface `UmbValueTypeMap` maps string keys to TypeScript types using **declaration merging** — the same pattern as `UmbExtensionConditionConfigMap`. Any package can extend it. There is no runtime registry.

```typescript
// base declaration in types.ts
declare global {
  interface UmbValueTypeMap {}
}
export type UmbValueType = keyof UmbValueTypeMap;
```

Each value type is a typed string constant that extends the map:

```typescript
export const UMB_BOOLEAN_VALUE_TYPE = 'Umb.ValueType.Boolean' as const;
declare global {
  interface UmbValueTypeMap {
    [UMB_BOOLEAN_VALUE_TYPE]: boolean;
  }
}
```

Any field typed `keyof UmbValueTypeMap` gets compile-time autocomplete and a type error if an undeclared key is used.

## Built-in types

| Constant | Key | TypeScript type |
|---|---|---|
| `UMB_STRING_VALUE_TYPE` | `Umb.ValueType.String` | `string` |
| `UMB_BOOLEAN_VALUE_TYPE` | `Umb.ValueType.Boolean` | `boolean` |
| `UMB_DATE_TIME_VALUE_TYPE` | `Umb.ValueType.DateTime` | `string` (ISO 8601) |

Import from `@umbraco-cms/backoffice/value-type`.

## Naming convention

| Category | Pattern | Example |
|---|---|---|
| System / primitive | `Umb.ValueType.{Type}` | `Umb.ValueType.DateTime` |
| Domain — reference shapes | `Umb.ValueType.{Entity}.{Shape}` | `Umb.ValueType.UserGroup.References` |
| Property editor | Property editor schema alias | `Umbraco.ColorPicker` |

Property editor value types use the schema alias as the key because document collection views receive an `editorAlias` field from the server for each property value and pass it directly as the `valueType` lookup key. The key must therefore match the schema alias exactly.

## Rules

- The key is always a string constant — always `as const`.
- The `declare global` block lives in the **same file** as the constant.
- **One value type per file.** File is named `constants.ts` inside a `value-type/` sub-folder.
- Export the constant from the package `index.ts` so other packages can import it.
- Property editor value types always use the schema alias — do not invent a different key.

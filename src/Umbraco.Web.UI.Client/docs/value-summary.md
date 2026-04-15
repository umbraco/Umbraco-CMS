# Value Summary

**Module**: `@umbraco-cms/backoffice/value-summary`
**Location**: `src/packages/core/value-summary/`

Depends on: [Value Type](./value-type.md)

---

Defines the `valueSummary` extension type and provides base classes, a batching coordinator, and built-in summaries. Used to render compact, formatted value representations in collection views.

## Extension type: `valueSummary`

```typescript
interface ManifestValueSummary
  extends ManifestElementAndApi<UmbValueSummaryElement, UmbValueSummaryApi> {
  type: 'valueSummary';
  forValueType: keyof UmbValueTypeMap;
  valueResolver?: UmbValueSummaryResolverLoaderProperty;
}
```

`forValueType` is the link between a value type key and its renderer. The extension system finds the manifest whose `forValueType` matches the requested type. **Consumers never reference a `valueSummary` manifest by alias.**

## Global component: `<umb-value-summary-extension>`

The only element consumers use. Takes `valueType` and `value`, renders the registered summary.

```html
<umb-value-summary-extension
  .valueType=${UMB_BOOLEAN_VALUE_TYPE}
  .value=${true}>
</umb-value-summary-extension>
```

Falls back to rendering the raw value as text when no summary is registered.

## Base classes

All implementations **must** extend these. Never implement the interfaces from scratch.

**`UmbValueSummaryApiBase`** (`src/packages/core/value-summary/base/value-summary-api.base.ts`)
- Connects to the coordinator context automatically.
- Exposes `value` as an observable (resolved or raw).
- Falls back gracefully when no coordinator is present (e.g., outside a collection).

**`UmbValueSummaryElementBase<T>`** (`src/packages/core/value-summary/base/value-summary-element.base.ts`)
- Generic — pass the value type as `T` so `_value` is correctly typed in `render()`.
- Subscribes to `api.value` and stores the result in `_value` (protected Lit state).
- Subclasses only implement `render()`.

## Simple summary (no API resolution)

Use when the raw value can be rendered directly — dates, booleans, strings, color codes, etc.

```typescript
@customElement('umb-boolean-value-summary')
export class UmbBooleanValueSummaryElement extends UmbValueSummaryElementBase<boolean> {
  override render() {
    return html`<umb-icon .name=${this._value ? 'icon-check' : 'icon-wrong'}></umb-icon>`;
  }
}
```

Manifest — always `kind: 'default'`, omit `valueResolver`:

```typescript
{
  type: 'valueSummary',
  kind: 'default',
  alias: 'Umb.ValueSummary.Boolean',
  name: 'Boolean Value Summary',
  forValueType: UMB_BOOLEAN_VALUE_TYPE,
  element: () => import('./boolean-value-summary.element.js'),
}
```

## Summary with resolver (batch API resolution)

Use when displaying the value requires a server call — e.g., resolving reference uniques to display names. The resolver receives all raw values from the entire column in **one batch call**.

### Resolver interface

```typescript
interface UmbValueSummaryResolver<TValue, TResolved> extends UmbApi {
  resolveValues(
    values: ReadonlyArray<TValue>,
  ): Promise<{
    data: ReadonlyArray<TResolved>; // positional — same order and length as input
    asObservable?: () => Observable<ReadonlyArray<TResolved>>;
  }>;
}
```

`data` must be the **same length** as `values` and **positionally aligned**. The resolver module must export the class as `valueResolver` (named export):

```typescript
export { UmbMyFeatureValueSummaryResolver as valueResolver } from './my-feature-value-summary.resolver.js';
```

Manifest — add `valueResolver`:

```typescript
{
  type: 'valueSummary',
  kind: 'default',
  alias: 'Umb.ValueSummary.UserGroup.References',
  name: 'User Group References Value Summary',
  forValueType: UMB_USER_GROUP_REFERENCES_VALUE_TYPE,
  element: () => import('./user-group-value-summary.element.js'),
  valueResolver: () => import('./user-group-value-summary.resolver.js'),
}
```

Provide `asObservable` only when the resolved data can change after initial load (e.g., linked entities that can be renamed).

## Coordinator (automatic — do not configure)

`UmbValueSummaryCoordinatorContext` is instantiated on the collection default context automatically. It batches all `preRegister()` calls within the same microtask queue into a single `resolveValues()` call per value type, deduplicating values before the resolver is called.

**Agents do not create or configure the coordinator.** It is transparent to value summary implementations — the base API class handles discovery and fallback.

## Collection integration

There are two distinct ways `<umb-value-summary-extension>` is driven from collection views.

### Document collection (automatic — driven by server data)

The document collection table and card views receive an `editorAlias` from the server for each property value. They pass it directly as `valueType` to `<umb-value-summary-extension>`:

```typescript
// document-table-column-property-value.element.ts (simplified)
const { value, editorAlias } = this.#getPropertyValueByAlias();
return editorAlias
  ? html`<umb-value-summary-extension .valueType=${editorAlias} .value=${value}></umb-value-summary-extension>`
  : value;
```

Property editors do not configure collection manifests. Registering a `valueSummary` manifest with `forValueType` equal to the schema alias is sufficient — the document collection picks it up automatically.

### Generic table collection kind (manifest-driven)

`MetaCollectionViewTableKindColumn` has an optional `valueType` field set statically in the collection view manifest:

```typescript
interface MetaCollectionViewTableKindColumn {
  field: string;
  label: string;
  valueType?: keyof UmbValueTypeMap;
}
```

This is used for non-property-editor columns where the value type is known at manifest time — e.g. user state, last login date, user group references. When set, the table renders `<umb-value-summary-extension>` per cell. When omitted, the raw value is rendered as-is.

## Rules

1. Always use `kind: 'default'` on `valueSummary` manifests unless creating a new kind.
2. Always extend `UmbValueSummaryApiBase` and `UmbValueSummaryElementBase` — do not implement the interfaces from scratch.
3. Never reference a `valueSummary` manifest by alias — consumers look up by `forValueType`.
4. Resolver `data` array must be positionally aligned with the `values` input — same length, same order.
5. Use `asObservable` in resolvers only when resolved data can change (e.g., linked entities that can be renamed).
6. Export the resolver class as `valueResolver` (named export) — the loader checks for this name.

## File structure

```
my-feature/
└── value-summary/
    ├── manifests.ts
    ├── my-feature-value-summary.element.ts
    └── my-feature-value-summary.resolver.ts   # only if resolver is needed
```

The `value-type/` folder lives alongside `value-summary/` — see [Value Type](./value-type.md) for its structure.

## Reference implementations

| Pattern | Location |
|---|---|
| Simple (no resolver) | `src/packages/core/value-summary/value-types/boolean/` |
| Simple (date formatting) | `src/packages/core/value-summary/value-types/date-time/` |
| Resolver with `asObservable` | `src/packages/user/user-group/value-summary/` |

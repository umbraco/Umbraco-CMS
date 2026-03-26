# Deprecation & Breaking Changes

---

The `@umbraco-cms/backoffice` npm package, Extension Registry, Context tokens, exported types, and global components form a public contract. Breaking changes require the deprecation process below.

## SemVer & The 2-Major-Version Rule

> If something is deprecated in **vN**, the earliest it can be removed is **vN+2**.
>
> Example: deprecated in v16 -> removed in v18 at the earliest.

---

## How to Deprecate: Two Required Mechanisms

Every deprecation must use **both**:

### 1. JSDoc `@deprecated` Tag

Always include: what, when deprecated, replacement, when removed.

```typescript
/**
 * @deprecated Deprecated since v16. Use `UmbAnalyticsItemRepository` instead. Will be removed in v18.
 */
export class UmbAnalyticsLegacyRepository { /* ... */ }
```

Same pattern applies to methods, properties, constants, and types.

### 2. Runtime Warning via `UmbDeprecation`

Emit a console warning so consumers who miss the JSDoc annotation are still notified at runtime.

```typescript
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated since v16. Use `requestSummary()` instead. Will be removed in v18.
 */
getSummary() {
  new UmbDeprecation({
    deprecated: 'UmbAnalyticsDashboardContext.getSummary()',
    deprecatedSinceVersion: '16.0.0',
    removeInVersion: '18.0.0',
    solution: 'Use requestSummary() instead.',
  }).warn();

  return this.requestSummary();
}
```

For deprecated classes, place `UmbDeprecation` in the constructor so it fires on instantiation.

---

## Checklist for Any Breaking Change

1. **Introduce the replacement** alongside the old API.
2. **Add `@deprecated` JSDoc** with migration instructions and removal version.
3. **Add `UmbDeprecation` runtime warning**.
4. **Keep the old code working** — ideally delegate to the new implementation internally.
5. **Do not remove until 2 major versions later**.
6. **Document in release changelog** when the removal happens.

---

## What Counts as a Breaking Change

Any of the following to a **publicly exported** symbol:

- Removing/renaming an exported class, function, type, constant, or context token
- Removing/renaming a public method or property
- Changing a public method signature (required params, return type)
- Removing/renaming a subpath export from `package.json`
- Changing an extension type alias or manifest schema
- Removing/renaming a global component's tag name
- Changing public API behavior in unexpected ways

**Not breaking**: Internal code (`local-components/`, unexported classes, private methods) can be changed freely.

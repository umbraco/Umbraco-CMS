# Deprecation & Breaking Changes
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../CLAUDE.md)

---

Umbraco is a **platform** with a public-facing API consumed by package developers and implementors. The `@umbraco-cms/backoffice` npm package, the Extension Registry, Context tokens, exported types, and global components all form a public contract. Breaking that contract without warning breaks the ecosystem.

---

## SemVer & The 2-Major-Version Rule

The project follows **Semantic Versioning (SemVer)**. Any change to the public API that would break existing consumer code is a breaking change. Breaking changes are handled with a strict rule:

> **All breaking changes require a 2 major version deprecation period.**
>
> If something is deprecated in **v16**, the earliest it can be removed is **v18**.

This gives package developers and implementors a full major release cycle to discover the deprecation and a second to migrate their code.

---

## How to Deprecate: Two Required Mechanisms

Every deprecation must be communicated through **both** of the following:

### 1. JSDoc `@deprecated` Tag

Add a `@deprecated` annotation to the exported symbol (class, function, type, constant, property, method). This surfaces the deprecation in IDEs — TypeScript will show a strikethrough and a warning when consumers hover or autocomplete.

Always include: what is deprecated, when it was deprecated, what to use instead, and when it will be removed.

```typescript
/**
 * @deprecated Deprecated since v16. Use `UmbAnalyticsItemRepository` instead. Will be removed in v18.
 */
export class UmbAnalyticsLegacyRepository {
  // ...
}
```

```typescript
export class UmbAnalyticsDashboardContext extends UmbContextBase {
  /**
   * @deprecated Deprecated since v16. Use `requestSummary()` instead. Will be removed in v18.
   */
  getSummary() {
    return this.requestSummary();
  }

  requestSummary() {
    // new implementation
  }
}
```

```typescript
/**
 * @deprecated Deprecated since v16. Use `UMB_ANALYTICS_SECTION_ALIAS` from
 * `@umbraco-cms/backoffice/analytics` instead. Will be removed in v18.
 */
export const UMB_ANALYTICS_LEGACY_ALIAS = 'Umb.Analytics.Section';
```

### 2. Runtime Deprecation Warning via `UmbDeprecation`

In addition to the static JSDoc annotation, emit a **runtime deprecation warning** using the `UmbDeprecation` class. This ensures that consumers who miss the JSDoc warning still get notified in the browser console when the deprecated code path is executed.

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

When this code executes, the console will display a clear deprecation warning with the name of the deprecated API, the version it will be removed in, and what to use instead.

### For Deprecated Classes or Entry Points

When an entire class or module is deprecated, place the `UmbDeprecation` warning in the constructor or at the top of the module so it fires as soon as the deprecated code is touched:

```typescript
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated since v16. Use `UmbAnalyticsItemRepository` instead. Will be removed in v18.
 */
export class UmbAnalyticsLegacyRepository {
  constructor() {
    new UmbDeprecation({
      deprecated: 'UmbAnalyticsLegacyRepository',
      deprecatedSinceVersion: '16.0.0',
      removeInVersion: '18.0.0',
      solution: 'Use UmbAnalyticsItemRepository from @umbraco-cms/backoffice/analytics instead.',
    }).warn();
  }

  // ... keep the old implementation working during the deprecation period
}
```

---

## Checklist for Any Breaking Change

1. **Introduce the replacement** — ship the new API, class, constant, or component alongside the old one.
2. **Deprecate the old one** — add `@deprecated` JSDoc with migration instructions and the removal version.
3. **Add a runtime warning** — use `UmbDeprecation` so consumers see a console warning when the deprecated path is hit.
4. **Keep the old code working** — the deprecated API must continue to function correctly throughout the deprecation period. Ideally, the old implementation internally delegates to the new one.
5. **Do not remove until 2 major versions later** — deprecated in v16 → removed in v18 at the earliest.
6. **Document the breaking change** — when the removal finally happens, note it in the release changelog and Umbraco Announcements.

---

## What Counts as a Breaking Change

Any of the following to a publicly exported symbol requires the deprecation process:

- Removing or renaming an exported class, function, type, constant, or context token
- Removing or renaming a public method or property on an exported class
- Changing the signature of a public method (required parameters, return type)
- Removing or renaming a subpath export from `package.json`
- Changing an extension type alias or manifest schema that consumers rely on
- Removing or renaming a global component's tag name
- Changing the behavior of a public API in a way that existing consumers would not expect

### What Is NOT a Breaking Change

Internal code (`local-components/`, unexported classes, private methods) is **not** part of the public API and can be changed freely.

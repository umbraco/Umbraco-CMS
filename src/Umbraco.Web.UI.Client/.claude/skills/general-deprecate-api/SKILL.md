---
name: general-deprecate-api
description: Deprecate a public API (class, method, property, type, constant, or context token) following the 2-major-version rule. Use when removing or replacing any publicly exported symbol that external consumers may depend on — includes exports from package index.ts files, global components, extension type aliases, and manifest schemas.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Deprecate API

Deprecate a public API following Umbraco's breaking changes policy.

## What you need from the user

1. **What to deprecate** — The class, method, property, type, or constant
2. **The replacement** — What consumers should use instead
3. **Current major version** — Read from `version.json` at the repository root

## The 2-Major-Version Rule

> Deprecated in **vN** -> earliest removal in **vN+2**.
>
> Example: deprecated in v17 -> scheduled for removal in v19.

Read `version.json` to determine the current major version and calculate the removal version.

## Both mechanisms are required

Every deprecation **must** use both:

1. **JSDoc `@deprecated` tag** — for IDE warnings and documentation
2. **`UmbDeprecation` runtime warning** — for console output at runtime

## Step 1: Add the replacement API

Introduce the new API alongside the old one. The old API should delegate to the new implementation internally.

```typescript
// New method (the replacement)
requestSummary() {
	// Real implementation here
}
```

## Step 2: Add JSDoc `@deprecated` tag

Include: what was deprecated, when, the replacement, and when it will be removed.

```typescript
/**
 * @deprecated Deprecated since v17. Use `requestSummary()` instead. Will be removed in v19.
 */
getSummary() {
	return this.requestSummary();
}
```

For types and interfaces:

```typescript
/**
 * @deprecated Deprecated since v17. Use `UmbNewInterface` instead. Scheduled for removal in Umbraco 19.
 */
export interface UmbOldInterface {
	// ...
}
```

## Step 3: Add `UmbDeprecation` runtime warning

```typescript
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated since v17. Use `requestSummary()` instead. Will be removed in v19.
 */
getSummary() {
	new UmbDeprecation({
		deprecated: 'UmbMyContext.getSummary()',
		removeInVersion: '19.0.0',
		solution: 'Use requestSummary() instead.',
	}).warn();

	return this.requestSummary();
}
```

### Placement by symbol type

| Symbol | Where to place `UmbDeprecation` |
|--------|-------------------------------|
| Method | Inside the method body |
| Class | In the `constructor` |
| Property getter | Inside the getter |
| Constant | At module level (executes on import) |

### For deprecated classes

```typescript
/**
 * @deprecated Deprecated since v17. Use `UmbNewRepository` instead. Will be removed in v19.
 */
export class UmbOldRepository extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);

		new UmbDeprecation({
			deprecated: 'UmbOldRepository is deprecated.',
			removeInVersion: '19.0.0',
			solution: 'Use UmbNewRepository instead.',
		}).warn();
	}
}
```

## Step 4: Delegate old to new

The deprecated API must keep working. Delegate to the replacement internally:

```typescript
/**
 * @deprecated Deprecated since v17. Use `getContentTypeUnique()` instead. Will be removed in v19.
 */
getContentTypeId(): string | undefined {
	new UmbDeprecation({
		deprecated: 'UmbMemberWorkspaceContext.getContentTypeId()',
		removeInVersion: '19.0.0',
		solution: 'Use getContentTypeUnique() instead.',
	}).warn();

	return this.getContentTypeUnique();
}
```

## Step 5: Update internal callers

All internal code must use the new API. No code within the repository should call the deprecated member.

## What counts as a breaking change

Any of the following to a **publicly exported** symbol:

- Removing/renaming an exported class, function, type, constant, or context token
- Removing/renaming a public method or property
- Changing a public method signature (required params, return type)
- Removing/renaming a subpath export from `package.json`
- Changing an extension type alias or manifest schema
- Removing/renaming a global component's tag name

**Not breaking**: Internal code (unexported classes, private methods) can be changed freely without deprecation.

## Checklist

- [ ] Replacement API introduced and working
- [ ] `@deprecated` JSDoc tag added with: since version, replacement, removal version
- [ ] `UmbDeprecation` runtime warning added with `deprecated`, `removeInVersion`, `solution`
- [ ] Deprecated code delegates to the replacement (still works)
- [ ] All internal callers updated to use the new API
- [ ] Removal version follows the 2-major-version rule (current + 2)
- [ ] Compiles: `npm run compile`

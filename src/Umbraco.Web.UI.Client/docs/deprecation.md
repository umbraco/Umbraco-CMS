# Deprecation & Breaking Changes

---

The `@umbraco-cms/backoffice` npm package, Extension Registry, Context tokens, exported types, and global components form a public contract. Breaking changes require the deprecation process below.

## SemVer & The 2-Major-Version Rule

> If something is deprecated in **vN**, the earliest it can be removed is **vN+2**.
>
> Example: deprecated in v16 -> removed in v18 at the earliest.

---

## How to Deprecate

Every deprecation must use **both** mechanisms:

1. **JSDoc `@deprecated` tag** — for IDE warnings and documentation
2. **Runtime warning via `UmbDeprecation`** — console output so consumers are notified at runtime

For step-by-step implementation instructions, use the `general-deprecate-api` skill.

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

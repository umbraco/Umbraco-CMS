# Breaking Changes Reference

This document describes how to detect and validate breaking changes during PR review. It covers both backend (.NET) and frontend (TypeScript/Lit) patterns.

---

## Version Detection

**Always read `version.json`** at the repository root to determine the current major version. This drives the obsolete removal target calculation:

- Current major version: read from `version.json` → `version` field (e.g., `"17.4.0-rc"` → major version `17`)
- Obsolete removal target: `current + 2` (e.g., if current is 17, removal is scheduled for Umbraco 19)
- Format: `[Obsolete("... Scheduled for removal in Umbraco {current+2}.")]`

---

## Backend (.NET) Breaking Changes

### What Constitutes a Breaking Change

Any of these on a `public` or `protected` member:

- Removing or renaming a class, interface, struct, record, or enum
- Removing or renaming a method, property, or field
- Changing a method signature (parameters, return type)
- Adding required parameters to an existing method
- Adding methods to a public interface (without default implementation)
- Changing a constructor signature on a public class
- Removing or changing enum values
- Changing type hierarchy (base class, implemented interfaces)

### Pattern 1: Obsolete Constructor + StaticServiceProvider

When a public class needs new dependencies, the existing constructor must be preserved.

**Correct pattern:**

```csharp
[Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
public MyService(IDependencyA depA)
    : this(
        depA,
        StaticServiceProvider.Instance.GetRequiredService<IDependencyB>())
{
}

public MyService(IDependencyA depA, IDependencyB depB)
{
    _depA = depA;
    _depB = depB;
}
```

**Validation checklist:**

- [ ] Old constructor has `[Obsolete]` attribute with correct removal version
- [ ] Old constructor calls new constructor via `: this(...)`
- [ ] `StaticServiceProvider.Instance.GetRequiredService<T>()` used for new params only
- [ ] DI registration uses the NEW constructor (old is for external consumers only)
- [ ] Removal version is `{current_major + 2}`

**Common mistakes to flag:**

- Removing the old constructor entirely (breaking change!)
- Old constructor NOT calling new constructor (code duplication)
- Wrong removal version in `[Obsolete]`
- Missing `StaticServiceProvider` resolution for new dependencies
- DI registration still using the old constructor

### Pattern 2: Obsolete Method + New Overload

When a method signature needs to change, add the new overload and obsolete the old.

**Correct pattern:**

```csharp
[Obsolete("Use the overload taking all parameters. Scheduled for removal in Umbraco 19.")]
public void DoThing(string name)
    => DoThing(name, extraParam: null);

public void DoThing(string name, string? extraParam)
{
    // Real implementation here
}
```

**Validation checklist:**

- [ ] Old method has `[Obsolete]` attribute with correct removal version
- [ ] Old method calls new method, providing defaults for new parameters
- [ ] All internal callers updated to use the new method
- [ ] No internal code references the obsolete method (except the delegation)

### Pattern 3: Default Interface Implementation

When adding methods to a public interface, provide a default implementation.

**Correct pattern:**

```csharp
public interface IMyService
{
    void ExistingMethod();

    // New method with default implementation
    void NewMethod(string param)
        => ExistingMethod(); // delegate to existing if possible
}
```

**Strategies for defaults (in order of preference):**

1. Use existing interface methods to satisfy the contract
2. Return a sensible default (empty collection, null, etc.)
3. Throw `NotImplementedException` if no reasonable default exists

**Validation checklist:**

- [ ] New interface method has a default implementation
- [ ] TODO comment present: `// TODO (V{next-major}): Remove the default implementation when {obsolete method} is removed.`
- [ ] Default implementation is functionally correct (even if not optimal)
- [ ] If `StaticServiceProvider` is used in default impl, noted as temporary

### Obsolete Attribute Validation

For any `[Obsolete]` attribute found in changed code:

1. **Format**: Must contain `"Scheduled for removal in Umbraco {version}."`
2. **Version**: Must be `current_major + 2` (read from `version.json`)
3. **Pragma**: Where obsolete members must call each other, `#pragma warning disable CS0618` / `#pragma warning restore CS0618` must be present

### Internal Caller Check

After finding obsolete patterns, verify:

- Search the codebase for usages of the obsolete member
- **No internal code** (inside `src/`) should reference obsolete members
- Only the obsolete member's own delegation (calling the new version) is acceptable
- External consumers (outside the repo) get the deprecation period to migrate

---

## Frontend (TypeScript/Lit) Breaking Changes

The backoffice is published as `@umbraco-cms/backoffice` with 140+ named exports. Plugin developers depend on this public API surface.

**Critical frontend rule (does not apply to backend .NET where `public`/`protected` visibility determines the API surface): only symbols reachable through the `package.json` `exports` field are public API.** Anything not exported — whether classes, functions, constants, types, or entire files — is an internal implementation detail, even if other internal code imports it. Removing or changing unexported frontend symbols is not a breaking change. Before flagging a frontend deletion or rename as breaking, verify the symbol is reachable via `package.json` exports. If it is not, do not flag it.

### Custom Elements (Web Components)

**Breaking changes:**

- Renaming or removing a registered custom element tag (`umb-*`)
- Removing elements from `HTMLElementTagNameMap`
- Removing or changing `@property()` decorated fields on exported components
- Removing event emissions (checked via `this.dispatchEvent`)
- Removing CSS custom properties (`@cssprop` in JSDoc)
- Removing CSS parts (`@csspart` in JSDoc)

**How to detect:**

- Check diff for removed `@customElement('umb-...')` decorators
- Check diff for removed `@property()` fields on exported components
- Check diff for removed entries in `HTMLElementTagNameMap` declarations

### Exported Types/Interfaces

**Breaking changes:**

- Removing exports from `package.json` `exports` field
- Changing the shape of exported interfaces (removing properties, changing types)
- Renaming exported types (consumers import by name)
- Removing union type members
- Changing generic type parameter constraints

**How to detect:**

- Check if `package.json` `exports` field is modified
- Check diff for removed `export` statements
- Check diff for changed interface/type shapes

### Manifest/Extension System

**Breaking changes:**

- Renaming a manifest `alias` value — plugin developers reference aliases by string in conditions, overwrites, and extension registry lookups. Alias renames are not caught by the compiler since they are string-based. A renamed alias silently breaks any plugin that references the old string.
- Removing support for a manifest `type` that plugins use
- Changing manifest `alias` resolution or validation
- Removing or renaming manifest `kind` types
- Changing extension bundle structure

**How to detect:**

- **Alias renames**: Compare `alias:` values in manifest files before and after. Changed alias strings are Critical — the old alias should be preserved as a deprecated entry.
- Search for changes to manifest type definitions
- Check for removed or renamed manifest kinds

### Context API

**Breaking changes:**

- Removing context tokens from exports
- Changing the shape of data provided by a context
- Removing context provider/consumer mechanisms

**How to detect:**

- Check for removed context token exports
- Check for changes to context provider classes

### Controllers/Lifecycle

**Breaking changes:**

- Changing controller base class inheritance requirements
- Removing controller lifecycle hooks
- Breaking cleanup mechanisms in `disconnectedCallback()`

### Observable/State

**Breaking changes:**

- Removing observable properties from the public API
- Changing observable emission patterns

### npm Publishing

**Breaking changes:**

- Changing version constraints that exclude previously-supported versions
- Adding incompatible peer dependency constraints

**How to detect:**

- Check if `package.json` `peerDependencies` or `dependencies` changed
- Verify version ranges are not narrowed

---

## Reporting Breaking Changes

When a breaking change is detected, report:

1. **What**: The specific change and which public symbol is affected
2. **Pattern**: Which mitigation pattern should be applied (Pattern 1, 2, or 3 for backend)
3. **Severity**: Critical (no mitigation present) or Important (mitigation present but incorrect)
4. **Fix**: Concrete code suggestion showing the correct pattern

If no breaking changes are detected, state: "No breaking changes detected."

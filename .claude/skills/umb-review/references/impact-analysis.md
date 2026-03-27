# Impact Analysis Reference

This document describes how to perform impact analysis during PR review. The goal is to look beyond the diff to understand how changes affect consumers in other parts of the codebase.

---

## 1. Extract Changed Public Symbols

Scan the diff output for changes to public API surface:

### Backend (.NET)

Look for added, modified, or removed lines containing:

- `public class`, `public abstract class`, `public sealed class`
- `public interface`
- `public record`, `public struct`, `public enum`
- `public` or `protected` methods, properties, fields
- `public static` members
- Constructor signatures on public types

### Frontend (TypeScript/Lit)

Look for changes to:

- `export class`, `export interface`, `export type`, `export enum`
- `export function`, `export const`
- `@property()` decorated fields on exported components
- `@customElement()` registrations
- Entries in `package.json` `exports` field

Collect a list of all changed public symbol names (type names, method names, property names).

---

## 2. Search for Consumers

For each changed public symbol, search the `src/` directory for usages **outside the changed file itself**.

### Grep Strategy

Use the Grep tool with these settings:

```
pattern: {symbol name}
path: src/
output_mode: files_with_matches
head_limit: 20
```

Use `head_limit: 20` to avoid overwhelming results — if there are more than 20 consumers, note "20+ consumers found" and list the first 20.

### What to Search For

For each changed type/method, search for:

- **Type references**: class name, interface name (e.g., `IContentService`)
- **Method calls**: method name in context (e.g., `\.GetById\(` for a method rename)
- **Constructor usage**: `new TypeName(`
- **DI registrations**: `.AddSingleton<IType, Type>`, `.AddScoped<`, `.AddTransient<`
- **Notification handlers**: if a notification type changed, search for `INotificationHandler<NotificationTypeName>` and `INotificationAsyncHandler<NotificationTypeName>`
- **Interface implementations**: if an interface changed, search for `: IInterfaceName` or `IInterfaceName,`

### Excluding the Changed File

When reporting consumers, exclude files that are part of the PR's changes (they're already being reviewed). The interesting consumers are those **outside** the PR that may be affected.

---

## 3. Check Dependency Flow Direction

The Umbraco architecture enforces strict unidirectional dependencies:

```
Api.Management / Api.Delivery (depend on Api.Common)
  ↓
Api.Common (depends on Web.Common)
  ↓
Web.Common (depends on Infrastructure)
  ↓
Infrastructure (depends on Core)
  ↓
Core (no dependencies)
```

### Layer Mapping

Map each changed file to its architectural layer:

| Path prefix | Layer |
|---|---|
| `src/Umbraco.Core/` | Core |
| `src/Umbraco.Infrastructure/` | Infrastructure |
| `src/Umbraco.PublishedCache.*` | Infrastructure |
| `src/Umbraco.Examine.Lucene/` | Infrastructure |
| `src/Umbraco.Cms.Persistence.*` | Infrastructure |
| `src/Umbraco.Web.Common/` | Web |
| `src/Umbraco.Web.UI/` | Web (Application) |
| `src/Umbraco.Web.Website/` | Web |
| `src/Umbraco.Cms.Api.Common/` | API |
| `src/Umbraco.Cms.Api.Management/` | API |
| `src/Umbraco.Cms.Api.Delivery/` | API |
| `src/Umbraco.Web.UI.Client/` | Frontend |
| `tests/` | Test |

### Violation Detection

Flag if a change introduces:

- **Core depending on Infrastructure**: Core file importing/referencing Infrastructure types
- **Core depending on Web/API**: Core file importing/referencing Web or API types
- **Infrastructure depending on Web/API**: Infrastructure file importing Web or API types
- **Cross-API dependencies**: Management API depending on Delivery API or vice versa

### How to Check

1. For each changed file, identify its layer
2. Read the file's `using` statements (C#) or `import` statements (TS)
3. Check if any imports reference a higher layer
4. Also check if new parameters or return types come from higher layers

---

## 4. Flag Cross-Project Risks

### High-Risk Patterns

These changes have high ripple potential:

- **Interface changes in Core** — all implementations in Infrastructure must be updated
- **Notification type changes** — all handlers across the codebase are affected
- **Base class changes** — all derived classes are affected
- **Composer changes** — can affect DI container and runtime behavior globally
- **Shared model/DTO changes** — can affect serialization, API contracts, and consumers

### What to Report

For each cross-project risk found, report:

1. **What changed**: The specific symbol and how it changed
2. **Who is affected**: List of consuming files/projects found via Grep
3. **Risk level**: Whether the consumers will break (compile error), behave differently (runtime), or are unaffected
4. **Recommendation**: Whether the PR should include updates to affected consumers

---

## 5. Performance Notes

- Use `head_limit: 20` on all Grep searches to cap results
- Only search for symbols that actually changed (not every symbol in the file)
- For very common type names (e.g., `IScope`, `ILogger`), consider adding more context to the search pattern to reduce false positives
- Skip impact analysis for test files — they don't have external consumers
- Skip impact analysis for private/internal members — they can't have external consumers

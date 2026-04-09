# Coding Preferences & Review Criteria

These are the coding preferences and code review standards used by the review skill. They define what the review evaluates against.

---

## Testing

- **Always create blackbox tests** for new/changed code
- Choose the appropriate test level:
  - **Unit tests** for isolated logic
  - **Integration tests** for application services/use cases
  - **E2E tests** for API endpoints

### Test Class Naming

- Test classes must be postfixed with `Tests` (e.g., `OrderServiceTests`)
- One test class per class under test

### Test Method Naming

**C# tests**: Use the `Can_`/`Cannot_` pattern with PascalCase underscore-separated words:

- `Can_Schedule_Publish_Invariant`
- `Cannot_Delete_Non_Existing`
- `Can_Schedule_Publish_Single_Culture`

Large test classes are split into partial files by method: `ContentServiceTests.Delete.cs`, `ContentServiceTests.Publish.cs`.

**TypeScript tests**: Use BDD-style `it()` with natural language descriptions:

- `it('should not allow the returned value to be lower than min')`
- `it('converts string to camelCase')`

### Unit Tests

- Optional, but must be blackbox tests so refactoring does not break tests

### Integration Tests

- Every use case / application service must have integration tests
- Tests run against real database (containerized or similar)
- Test the full flow from application layer through infrastructure

### E2E Tests

- Every API endpoint must have E2E tests
- Test realistic scenarios including error cases

---

## Trade-offs

When making decisions, prioritize:

- **Readability** over cleverness
- **Flexibility** over rigidity
- Explain trade-offs when deviating from these defaults

---

## Breaking Changes

- Communicate breaking changes at the **OpenAPI/openapi.json level**
- Clearly document what changed and the migration path

---

## Documentation

- **Document all public or exported types** (classes, interfaces, types, methods, properties)
- Keep documentation in sync with code changes
- Add **JS Docs** on all public frontend APIs (classes, methods, properties)
- Focus on "why" and usage, not restating the obvious

---

## Dependencies

- Use what's available in the codebase, unless there is no good choice
- **Flag new dependencies** for review — new packages should be justified
- Prefer well-maintained, widely-used packages

---

## Error Messages & Logging

- **User-facing errors**: Clear, friendly, actionable
- **Log messages**: Technical, detailed, with context
- Include correlation IDs and relevant data in logs

---

## Security

- **Always check for security issues** using OWASP Top 10 as baseline
- Flag potential vulnerabilities immediately
- Suggest secure alternatives when spotting risky patterns
- Apply principle of least privilege

---

## Immutability

- Prefer **immutability** by default
- Allow internal properties to be mutated, as long as they are not direct references coming from the outside

---

## Nullability

- **TypeScript / JavaScript**
  - Prefer `undefined` for optional/omitted values (e.g., optional parameters, props, and fields)
  - Use `null` only when the domain model explicitly encodes "no value" or "not set" (e.g., `string | null` from APIs/DB), and be consistent with existing types
  - Avoid mixing `null` and `undefined` for the same concept within the same model or API surface
  
- **C#**  
  - use nullable types (e.g., `string?`, `int?`) where absence is valid
  - Prefer domain modeling (value objects, options/results, empty collections) over `null` where appropriate, but respect existing conventions in the codebase

---

## C# Specific

- use Notification pattern (not C# events), Composer pattern (DI registration), Scoping with `Complete()`, Attempt pattern for operation results.

---

## Architecture

- Follow **Clean Architecture** principles
- **Fail-fast** principle: detect and report errors as early as possible
- Within the established layered architecture (Core/Infrastructure/Web/API), organize code by feature inside each layer where practical, while preserving dependency direction
- One class per file
- Avoid N+1 queries
- Profile before optimizing non-critical paths

### Type Hierarchy Consistency

When parallel model types have inconsistent relationships to a shared base type:

**TypeScript**: manipulations via `Omit`, `Pick`, intersection overrides, or workarounds like `as unknown as` / double-casts to bridge type mismatches.

**C#**: hiding base members with `new` to change types, explicit interface implementations to mask mismatches, or downcasting base return types in derived classes.

- **Do NOT suggest** the PR code should deviate from its base type to match a sibling that already deviates. Copying the deviation spreads the problem.
- **Do flag** the architectural inconsistency: parallel models should share a compatible base contract. The model that manipulates or deviates from the base type is the one that needs attention — not the one that extends it correctly.
- **Frame the suggestion** as: "These related models have inconsistent type hierarchies. `{deviating type}` manipulates the base contract of `{base type}`, which forces shared consumers like `{shared utility}` to require a shape that conforming subtypes can't satisfy."

---

## Code Style

- Follow standard naming conventions for the language (C# or JS/TS)
- Keep components small and focused on a single responsibility
- Prefer early returns
- Small functions
- No nested ternaries

---

## Severity Levels

| Severity | Meaning |
|----------|---------|
| **Critical** | Must fix before merge — security vulnerabilities, data loss risks, broken functionality |
| **Important** | Should fix — performance issues, missing tests, architectural violations |
| **Suggestion** | Nice to have — readability, minor refactoring, alternative approaches |

# Coding Preferences & Review Criteria

These are the coding preferences and code review standards used by the review-pr skill. They define what the review evaluates against.

---

## Communication Style

- **Think thoroughly** before responding — consider implications, edge cases, and alternatives
- **Ask clarifying questions** before providing detailed answers (in interactive contexts; not applicable during automated review)
- Understand the context and requirements before diving into implementation details

### Uncertainty

- Communicate uncertainty with a **confidence score (1-100)**
- If confidence is **below 85**, flag the uncertainty explicitly
- Be explicit about assumptions and unknowns

---

## Code Changes

- Show only the **changed/relevant sections**, not entire files
- Prefer **diff format** when showing modifications
- Always explain **why** the change is made
- Mention **alternatives** when relevant, with brief pros/cons

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

Use the pattern: `MethodName_Scenario_ExpectedResult`

Examples:
- `CreateOrder_WithValidInput_ReturnsCreatedOrder`
- `CreateOrder_WithDuplicateId_ReturnsConflict`
- `GetOrders_WithPagination_ReturnsPagedResult`
- `DeleteOrder_WhenNotFound_ReturnsNotFound`

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
- **Always ask for approval** before adding new dependencies
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

## Architecture

- Follow **Clean Architecture** principles
- **Fail-fast** principle: detect and report errors as early as possible
- Feature-based folder structure (not layer-based)
- One class per file
- Avoid N+1 queries
- Profile before optimizing non-critical paths

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

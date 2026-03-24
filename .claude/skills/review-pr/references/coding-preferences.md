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

- Prefer `undefined` over `null`
- Only use `null` for the state of a value without a value

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

## Code Review Scoring

### Scale

Rate every category on a **1-100 scale**:

- Provide an **overall score** (weighted by relevance, not a simple average)
- Scores are advisory — no automatic approve/reject thresholds

### Fixed Categories (always include)

| Category | Focus Areas |
|----------|------------|
| **Security** | OWASP top 10, input validation, auth/authz, secrets exposure |
| **Performance** | N+1 queries, caching, hot paths, async patterns, allocations |
| **Architecture** | Clean Architecture, SOLID, separation of concerns, dependency direction |
| **Consistency** | Adherence to existing codebase conventions, naming, patterns, style |
| **Testing** | Test coverage, test quality, edge cases, appropriate test level |
| **Readability** | Code clarity, naming, complexity, documentation |
| **Error Handling** | Result pattern, fail-fast, edge cases, logging, resilience |

### Context-Specific Categories (select 3-6 per PR)

Choose categories relevant to the particular PR:

- **Concurrency** — for code with shared state or database writes
- **API Design** — for new/changed endpoints (REST conventions, status codes, OpenAPI)
- **Database** — for migrations, queries, indexes, EF Core usage
- **Frontend** — for Lit components, TypeScript, accessibility
- **Breaking Changes** — when public API surface is affected
- **Observability** — for logging, tracing, health checks
- **Configuration** — for DI registration, options, environment handling
- **Domain Modeling** — for value objects, aggregates, invariants

### Severity Levels

| Severity | Meaning |
|----------|---------|
| **Critical** | Must fix before merge — security vulnerabilities, data loss risks, broken functionality |
| **Important** | Should fix — performance issues, missing tests, architectural violations |
| **Suggestion** | Nice to have — readability, minor refactoring, alternative approaches |
| **Praise** | Good patterns and decisions worth noting |

---
name: efcore-order-by-memberinit-not-positional-record
description: "EF Core cannot translate .OrderBy() after a query has been projected via a positional-record constructor call (new Foo(a, b)) — throws InvalidOperationException at runtime, not compile time. Object-initializer syntax (new Foo { A = a, B = b }) translates and orders correctly. Empirically verified against real SQLite, not just inferred."
metadata:
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-21T10:03:30.480Z
---

When composing an EF Core `IQueryable` pipeline where a *shared* intermediate type needs to survive
further `.OrderBy()`/`.Where()` calls after being projected via `.Select(x => new SharedType(...))`, the
declaration form of `SharedType` matters and the compiler will not warn you:

- **Positional record / any constructor-call projection** (`new SharedType(x.A, x.B)`, including a C#
  positional `record`): translates fine as long as nothing orders it afterward. The moment you
  `.OrderBy(y => y.SomeField)` on the *result* of that projection, EF Core throws
  `System.InvalidOperationException: The LINQ expression '...OrderBy(y => new SharedType(...).SomeField)'
  could not be translated.` at query-execution time — not a compile error, not a silent client-eval
  fallback, a hard runtime throw.
- **Object-initializer / member-init projection** (`new SharedType { A = x.A, B = x.B }`, works for
  `record` or plain `class`): translates correctly for subsequent ordering through *arbitrary* joined
  DTOs on the shape, not just the row's own columns, producing clean SQL (confirmed via `.ToQueryString()`
  showing a real `ORDER BY`/`CASE WHEN`, not a subquery-then-filter). This is the same mechanism that
  already lets you order through an anonymous type before a final projection — anonymous types are
  member-init under the hood too.

This also composes: a member-init type can be wrapped as a single nested field inside a *further*
`GroupJoin`/`SelectMany` anonymous type (`new { joined, extraComputedValue }`), ordered by the computed
value, then unwrapped again (`.Select(x => x.joined)`) and fed into a final projection — verified this
exact multi-level pattern translates to a clean SQL `CASE WHEN ... END` for the computed value, no
client-eval.

One related gotcha in the same area: the null-conditional operator (`x?.Y`) cannot appear inside an
`Expression<Func<...>>` tree at all (`CS8072`, compile error, not a translation failure) — use a ternary
(`x == null ? null : x.Y`) instead when a projected member on the shared shape is nullable.

**Why:** Discovered while consolidating ~10 independently-duplicated join-building methods in
`AsyncDocumentRepository.cs` into one shared `BuildBaseQuery`/`DocumentJoinRow` pipeline (see
[[project_ef_core_document_repository_status]]) — the existing `DocumentRow` (a positional record) was
already why the old code had to duplicate its entire join+projection per sort strategy instead of
factoring it out: ordering had to happen *before* that final projection, every time, inline. The fix
generalized correctly only once the *new* shared intermediate type (`DocumentJoinRow`) was deliberately
declared as member-init rather than repeating the same mistake one level up.

**How to apply:** Before designing a shared intermediate `IQueryable<TShared>` type meant to be composed
with further `.OrderBy()`/`.Where()` calls by multiple callers, declare it as a plain class (or a record
with `init` properties and no positional constructor) and always construct it with `{ }` syntax. Reserve
positional records / constructor-call projections for a query's *final* shape, where nothing composes
further after it. When in doubt, verify empirically with a throwaway spike against the real test database
checking `.ToQueryString()` and actual execution — do not assume based on general EF Core familiarity,
since this specific failure mode is a runtime throw with no compile-time signal.

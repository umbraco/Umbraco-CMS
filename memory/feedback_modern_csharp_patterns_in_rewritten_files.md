---
name: modern-c-patterns-in-rewritten-files
description: "The user wants rewritten files on the EF Core branch to use property patterns, is null/is not null, pattern combinators and collection expressions, but only in files the branch substantially rewrote, never spread across the code base"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 4f8da05c-fe36-4b55-8eed-9028f7cca30c
  modified: 2026-09-29T09:39:30.084Z
---

Write `item is { PublishedState: PublishedState.Publishing }` rather than `item.PublishedState == PublishedState.Publishing`, `x is null` / `x is not null` rather than `== null`, `publishedState is not (A or B)` for excluded enum values, `[.. source]` / `[]` rather than `ToArray()` / `Enumerable.Empty<T>()`, `??=`, `?? throw`, and `is false` for negated booleans. Scope it to files the branch essentially rewrote (churn ≥ 50% against the merge base), not to lightly touched files.

**Why:** a colleague reviewed the EF Core document repository and content service rewrite (2026-09-29) and pointed out that freshly written code still used the old comparison idioms; the user wants the rewrite to read as modern C# without a repo-wide churn.

**How to apply:** run `dotnet format style` with the IDE pattern diagnostics (IDE0019/20/38/41/66/78/83/170/260/270/300-305/090/016/031/074) on the chosen files with a temporary severity bump appended to `.editorconfig` (restore it afterwards), then do a manual pass for enum equality → property patterns and `== null` → `is null`. Never convert `== null` or `== false` inside EF Core query lambdas: `is` patterns are not allowed in expression trees (CS8122). Supersedes the literal reading of [[feedback-enumerable-empty]]: `[]` is fine for an `IEnumerable<T>` result; that memory was about cast hacks.

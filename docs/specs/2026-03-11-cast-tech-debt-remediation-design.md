# CAST Tech Debt Remediation Demo — Design Spec

**Date**: 2026-03-11
**Context**: Client demo showing Claude Code's ability to assess and remediate tech debt from CAST-style static analysis results in the Umbraco CMS codebase.

---

## 1. Goals

- Demonstrate Claude Code performing a realistic CAST-aligned code quality assessment of a real enterprise codebase
- Show the full workflow: assessment → implementation plan → remediation → re-assessment
- Scope the *remediation* to one chosen module so the demo is legible, while the *assessment* covers the full codebase
- Produce artifacts (report, branch, PR, delta report) that a client can inspect

---

## 2. Scope

| Phase | Coverage |
|---|---|
| Quality scan & report | Entire codebase (C# + TypeScript) |
| Module selection | User picks after seeing the report |
| Remediation & PRs | Chosen module only |
| Re-assessment | Chosen module, scoped delta |

**Definition of "module"**: For this demo, a module is one top-level C# project under `src/` (e.g., `Umbraco.Infrastructure`) or one package directory under `src/Umbraco.Web.UI.Client/src/packages/`.

---

## 3. Violation Categories

Fifteen CAST-aligned categories are measured across four groups. All are detectable via grep or file-level analysis — no external linters required.

### 3.1 Style & Readability

| Category | Rule | Rationale | Applies To |
|---|---|---|---|
| **Long lines** | > 120 characters | Reduces readability, increases horizontal scrolling | C#, TypeScript |
| **Magic numbers** | Numeric literals inline, excluding 0, 1, -1 | Obscures intent, makes changes error-prone | C#, TypeScript |
| **Magic strings** | Non-empty string literals appearing 2+ times in the same file, excluding attribute arguments, log messages, and exception messages | Duplication and fragility | C#, TypeScript |
| **Short identifiers** | 1–2 character names, excluding: loop counters in `for`/`foreach` initializers, catch-clause variables, lambda/arrow-function parameters, and generic type parameters | Harms comprehension and searchability | C#, TypeScript |
| **Commented-out code** | Blocks of 3+ consecutive `//`-prefixed lines containing code-like tokens (`;`, `{`, `}`) | Dead code adds noise and confusion | C#, TypeScript |

### 3.2 Structural Complexity

| Category | Rule | Rationale | Applies To |
|---|---|---|---|
| **Method length** | Methods/functions > 30 lines | Long methods violate single-responsibility and are hard to test | C#, TypeScript |
| **Too many parameters** | Methods/functions with > 4 parameters | High parameter counts signal missing abstractions | C#, TypeScript |
| **Deep nesting** | Code indented > 4 levels | Deeply nested logic is hard to follow and test | C#, TypeScript |
| **High cyclomatic complexity** | Methods with > 10 control-flow keywords (`if`, `else`, `switch`, `case`, `for`, `foreach`, `while`, `catch`) | High complexity correlates with defect density | C#, TypeScript |
| **Copy-paste blocks** | 5+ consecutive identical lines appearing in 2+ files within the same module (detected via sliding-window hash comparison) | Duplicated logic means bug fixes must be applied in multiple places | C#, TypeScript |

### 3.3 Error Handling

| Category | Rule | Rationale | Applies To |
|---|---|---|---|
| **Empty catch blocks** | `catch` block body contains no statements | Silently swallows exceptions, making failures invisible | C#, TypeScript |
| **Catching generic Exception** | `catch (Exception` without a more specific type | Catches unintended exceptions, masks bugs | C# |
| **String concat in loops** | `+=` on a string variable inside a `for`/`foreach`/`while` block | O(n²) allocations; use `StringBuilder` or `string.Join` | C# |

### 3.4 TypeScript-Specific

| Category | Rule | Rationale | Applies To |
|---|---|---|---|
| **`any` type usage** | Explicit `any` type annotation | Defeats TypeScript's type safety | TypeScript |
| **Missing return types** | Exported functions without a return type annotation | Reduces API clarity and type inference reliability | TypeScript |
| **Debug statements** | `console.log` or `console.error` in source files | Debug output left in production code | TypeScript |

---

## 4. Scoring Algorithm

**Score = 100 − (weighted violations per 1000 LOC)**

Weights per violation type:

| Category | Weight |
|---|---|
| High cyclomatic complexity | 3 |
| Copy-paste blocks | 3 |
| Empty catch blocks | 3 |
| Magic numbers | 3 |
| Magic strings | 3 |
| Catching generic Exception | 2 |
| String concat in loops | 2 |
| Method length | 2 |
| Too many parameters | 2 |
| Deep nesting | 2 |
| Short identifiers | 2 |
| `any` type usage | 2 |
| Long lines | 1 |
| Commented-out code | 1 |
| Missing return types | 1 |
| Debug statements | 1 |

Score is clamped to [0, 100]. Higher is better.

Applied at file, project, and language level. Project score is the weighted average of its file scores by LOC. Files under 20 LOC are included in the project's LOC total but are not reported as individual file scores — their violations still count toward the project-level rate.

---

## 5. Phase 1 — Full Codebase Quality Report

### 5.1 Execution

Claude scans all non-generated, non-vendor source files using Grep/Glob/Read tools. Excluded paths:

- `**/node_modules/**`
- `**/obj/**`, `**/bin/**`
- `**/Migrations/**`
- Auto-generated C# files (`**/GeneratedCode/**`, `*.Generated.cs`)
- Auto-generated TypeScript API client files (`**/generated/**` within `src/Umbraco.Web.UI.Client/`)
- Test projects (`tests/**`)
- Third-party embedded assets

### 5.2 Output: `docs/quality/cast-assessment-baseline.md`

Structure:

1. **Executive summary** — overall score, total violations by category, LOC scanned
2. **By-language breakdown** — C# score, TypeScript score, violation tables
3. **By-project table** — columns: Project, LOC, Score, and a violation count per category (15 columns total)
4. **Top 10 worst files per language** — file path (relative), score, top violation category
5. **Methodology note** — explains scoring formula and exclusions

### 5.3 Interactive Narrative

During the scan, Claude narrates its findings in the terminal — what patterns it's encountering, which modules look problematic, observations about the code. This is visible to the client during a live demo.

---

## 6. Phase 2 — Module Selection

After the report is produced:

1. Claude presents the ranked project list (worst score first)
2. User reviews and selects the target module
3. Claude confirms scope: estimated number of violations and files affected

---

## 7. Phase 3 — Implementation Plan

Claude produces a grouped remediation plan for the chosen module. Each group is scoped to one violation category — small enough for a single coherent PR, large enough to show meaningful score improvement.

### Groups (in order)

Groups are ordered highest-weight violations first to maximize early score improvement.

| Group | Violation Categories | Branch Name |
|---|---|---|
| 1 | High cyclomatic complexity, method length, deep nesting, too many parameters, copy-paste blocks | `v17/improvement/cast-structural-complexity` |
| 2 | Empty catch blocks, catching generic Exception, string concat in loops | `v17/improvement/cast-error-handling` |
| 3 | Magic numbers, magic strings | `v17/improvement/cast-magic-values` |
| 4 | Short identifiers, long lines, commented-out code | `v17/improvement/cast-style` |
| 5 | TypeScript: `any` types, missing return types, debug statements | `v17/improvement/cast-typescript` |

The plan lists specific files and the changes needed in each, so execution is mechanical and reviewable.

---

## 8. Phase 4 — Remediation Cycle (per group)

For each group, in order:

1. **Create branch** from `main`
2. **Apply fixes** across all affected files in the chosen module
3. **Verify build** — run `dotnet build` if the chosen module is a C# project, `npm run compile` if it is a TypeScript package, or both if it contains both. Build must pass with no new errors.
4. **Create PR** targeting `main` with:
   - Title: `Improvement: CAST remediation — <category> (<module>)`
   - Body: before/after violation counts, files changed, score delta estimate
5. **Run scoped re-assessment** — same scan logic but scoped to the changed files, producing a delta report

### 8.1 Delta Report: `docs/quality/cast-assessment-group-<N>-<category>.md`

Where `<N>` is the group number (1–5) and `<category>` is the kebab-case category name matching the branch suffix (e.g., `cast-assessment-group-1-structural-complexity.md`).

Structure:
- Violations before vs. after (table)
- Score before vs. after
- Files changed and per-file improvement
- Running total: cumulative score improvement across all completed groups

---

## 9. Non-Goals

- No external linters, CAST tooling, or CI integration required
- No remediation of test files
- No changes to generated code
- No architectural refactoring — only the fifteen targeted violation categories
- No changes outside the chosen module

---

## 10. Success Criteria

- Baseline report produced covering all 21 production C# projects and the TypeScript client, with per-project scores and violation counts consistent with the overall weighted score
- At least 2 remediation groups completed with branches and PRs
- Each PR passes `dotnet build` / `npm run compile`
- Delta report shows measurable score improvement per group
- The full workflow is demonstrable in a single client session

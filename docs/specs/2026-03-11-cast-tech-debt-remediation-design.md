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

Four CAST-aligned categories are measured, matching common CAST findings:

| Category | Rule | Rationale |
|---|---|---|
| **Long lines** | > 120 characters | Reduces readability, increases horizontal scrolling |
| **Magic numbers** | Numeric literals inline, excluding 0, 1, -1 | Obscures intent, makes changes error-prone |
| **Magic strings** | Non-empty string literals appearing 2+ times in the same file, excluding attribute arguments, log messages, and exception messages | Duplication and fragility |
| **Short identifiers** | 1–2 character names, excluding: loop counters in `for`/`foreach` initializers, catch-clause variables, lambda/arrow-function parameters, and generic type parameters (applies to both C# and TypeScript) | Harms comprehension and searchability |

---

## 4. Scoring Algorithm

**Score = 100 − (weighted violations per 1000 LOC)**

Weights per violation type:

| Category | Weight |
|---|---|
| Magic numbers | 3 |
| Magic strings | 3 |
| Short identifiers | 2 |
| Long lines | 1 |

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
3. **By-project table** — columns: Project, LOC, Long Lines, Magic Numbers, Magic Strings, Short IDs, Score
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

| Group | Violation Category | Branch Name |
|---|---|---|
| 1 | Magic numbers → named constants | `v17/improvement/cast-magic-numbers` |
| 2 | Magic strings → constants | `v17/improvement/cast-magic-strings` |
| 3 | Short identifiers → descriptive names | `v17/improvement/cast-short-identifiers` |
| 4 | Long lines → reformatted for readability | `v17/improvement/cast-long-lines` |

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

Where `<N>` is the group number (1–4) and `<category>` is the kebab-case category name (e.g., `cast-assessment-group-1-magic-numbers.md`).

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
- No architectural refactoring — only the four targeted violation categories
- No changes outside the chosen module

---

## 10. Success Criteria

- Baseline report produced covering all 21 production C# projects and the TypeScript client, with per-project scores and violation counts consistent with the overall weighted score
- At least 2 remediation groups completed with branches and PRs
- Each PR passes `dotnet build` / `npm run compile`
- Delta report shows measurable score improvement per group
- The full workflow is demonstrable in a single client session

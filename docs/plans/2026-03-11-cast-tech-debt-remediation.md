# CAST Tech Debt Remediation — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Demonstrate Claude Code performing a full CAST-aligned code quality assessment and remediation of the Umbraco CMS codebase.

**Architecture:** Four-phase workflow — full codebase scan → module selection → grouped remediation (5 groups, each with a branch + PR) → delta re-assessment. The scan uses Grep/Glob/Read tools with a Python script for copy-paste detection. Remediation is scoped to a single user-selected module.

**Tech Stack:** C# (.NET 10), TypeScript, Python (copy-paste detection), `dotnet build`, `npm run compile`, `gh` CLI for PRs, `git` for branching.

---

## Prerequisites & Orientation

- Repository root: `/Users/ronstarling/repos/Umbraco-CMS`
- Current version: `17.3.0-rc` (major version = 17)
- 21 production C# projects under `src/`, plus a TypeScript client at `src/Umbraco.Web.UI.Client/`
- TypeScript packages live under `src/Umbraco.Web.UI.Client/src/packages/<package-name>/`
- Spec reference: `docs/specs/2026-03-11-cast-tech-debt-remediation-design.md`
- Output baseline: `docs/quality/cast-assessment-baseline.md`
- Output delta reports: `docs/quality/cast-assessment-group-<N>-<category>.md`

---

## Chunk 1: Phase 1 — Full Codebase Scan

### Task 1: Write the Python Copy-Paste Detection Script

This task creates the sliding-window hash script used to detect duplicated code blocks across files within a module. Python is used because doing this purely with Grep/Glob would miss multi-file cross-comparison logic.

- [ ] **1.1** Create the directory `tools/cast/`:
  ```bash
  mkdir -p /Users/ronstarling/repos/Umbraco-CMS/tools/cast
  ```

- [ ] **1.2** Write `tools/cast/detect_copy_paste.py` with this exact algorithm:
  - Accept two arguments: `--dir <directory>` and `--ext <.cs|.ts>`
  - Enumerate all files matching the extension, excluding: `node_modules`, `obj`, `bin`, `Migrations`, `GeneratedCode`, files ending in `.Generated.cs`, and directories named `generated`
  - For each file, strip blank lines and store the remaining lines as a sequence
  - Use a sliding window of 5 consecutive non-blank lines; hash each window using SHA-256 of the joined content
  - Build a dict: `{hash -> [(file_path, line_number), ...]}`
  - Report any hash that appears in 2+ distinct files as a copy-paste violation
  - Output: JSON array of `{"hash": str, "line_count": 5, "occurrences": [{"file": str, "line": int}], "sample": str}` — one entry per duplicated block
  - Exit code 0 always (violations reported in JSON, not as errors)

  Key implementation:
  ```python
  import hashlib, json, sys, os, argparse
  from collections import defaultdict

  def hash_window(lines):
      content = "\n".join(lines)
      return hashlib.sha256(content.encode()).hexdigest()

  def should_exclude(path):
      parts = path.replace("\\", "/").split("/")
      excluded_dirs = {"node_modules", "obj", "bin", "Migrations", "GeneratedCode", "generated"}
      return any(p in excluded_dirs for p in parts) or path.endswith(".Generated.cs")

  def scan(directory, ext, window_size=5):
      hashes = defaultdict(list)
      for root, dirs, files in os.walk(directory):
          dirs[:] = [d for d in dirs if d not in {"node_modules","obj","bin","Migrations","GeneratedCode","generated"}]
          for fname in files:
              if not fname.endswith(ext):
                  continue
              fpath = os.path.join(root, fname)
              if should_exclude(fpath):
                  continue
              with open(fpath, encoding="utf-8", errors="replace") as f:
                  raw = f.readlines()
              non_blank = [(i+1, ln.rstrip()) for i, ln in enumerate(raw) if ln.strip()]
              for i in range(len(non_blank) - window_size + 1):
                  window = non_blank[i:i+window_size]
                  h = hash_window([ln for _, ln in window])
                  hashes[h].append({"file": fpath, "line": window[0][0], "sample": window[0][1]})
      results = []
      for h, occurrences in hashes.items():
          files_seen = {o["file"] for o in occurrences}
          if len(files_seen) >= 2:
              results.append({"hash": h, "line_count": window_size,
                              "occurrences": occurrences[:10],
                              "sample": occurrences[0]["sample"]})
      return results

  if __name__ == "__main__":
      parser = argparse.ArgumentParser()
      parser.add_argument("--dir", required=True)
      parser.add_argument("--ext", required=True)
      args = parser.parse_args()
      results = scan(args.dir, args.ext)
      print(json.dumps(results, indent=2))
  ```

- [ ] **1.3** Test the script against a small known directory:
  ```bash
  python3 /Users/ronstarling/repos/Umbraco-CMS/tools/cast/detect_copy_paste.py \
    --dir /Users/ronstarling/repos/Umbraco-CMS/src/Umbraco.Infrastructure \
    --ext .cs | python3 -m json.tool | head -50
  ```
  Expected: valid JSON array (may be empty or contain entries — either is correct).

---

### Task 2: C# Scanner — All 13 C# Violation Categories

For each of the 21 C# projects under `src/`, use Grep/Glob/Read tools to count violations. Scan project by project to enable per-project scoring.

**Excluded paths for all C# scans:**
- `**/obj/**`, `**/bin/**`, `**/Migrations/**`, `**/GeneratedCode/**`
- Files matching `*.Generated.cs`
- `tests/**` (all test projects)

**The 21 C# projects:**
```
Umbraco.Cms, Umbraco.Cms.Api.Common, Umbraco.Cms.Api.Delivery,
Umbraco.Cms.Api.Management, Umbraco.Cms.DevelopmentMode.Backoffice,
Umbraco.Cms.Imaging.ImageSharp, Umbraco.Cms.Imaging.ImageSharp2,
Umbraco.Cms.Persistence.EFCore, Umbraco.Cms.Persistence.EFCore.Sqlite,
Umbraco.Cms.Persistence.EFCore.SqlServer, Umbraco.Cms.Persistence.Sqlite,
Umbraco.Cms.Persistence.SqlServer, Umbraco.Cms.StaticAssets,
Umbraco.Cms.Targets, Umbraco.Core, Umbraco.Examine.Lucene,
Umbraco.Infrastructure, Umbraco.PublishedCache.HybridCache,
Umbraco.Web.Common, Umbraco.Web.UI, Umbraco.Web.Website
```

- [ ] **2.1** For each project `src/<ProjectName>/`, collect all `.cs` files using Glob, excluding the paths above. Store the file list for use across all category checks.

- [ ] **2.2** **Long lines (weight 1):** Read each `.cs` file and count lines exceeding 120 characters. Narrate: "In `<project>`, found N long-line violations across M files."

- [ ] **2.3** **Magic numbers (weight 3):** Grep for numeric literals excluding `0`, `1`, `-1`; also exclude literals inside XML doc comments (`///`) and inside `[Attribute(...)]` lines. Count distinct occurrences per file.

- [ ] **2.4** **Magic strings (weight 3):** Read each file and find string literals appearing 2+ times in the same file. Exclude: attribute arguments, `throw new ...Exception(...)` messages, logger call arguments. Count per file.

- [ ] **2.5** **Short identifiers (weight 2):** Grep for `\b[a-zA-Z]{1,2}\b`. Exclude: loop counter variables in `for`/`foreach` initializers, catch clause variables, generic type parameters (`<T>`, `<TKey>`, etc.). Count remaining occurrences per file.

- [ ] **2.6** **Commented-out code (weight 1):** Find 3+ consecutive lines starting with `//` where at least one contains `;`, `{`, or `}`. Count blocks per file.

- [ ] **2.7** **Method length (weight 2):** Read each `.cs` file and parse method boundaries by tracking brace depth. Count methods exceeding 30 source lines. Narrate the top offenders.

- [ ] **2.8** **Too many parameters (weight 2):** Grep for `\(([^)]*,){4,}` to find signatures with 5+ parameters. Filter to method definitions (not call sites). Count per file.

- [ ] **2.9** **Deep nesting (weight 2):** Read each file and count the maximum indentation depth. Flag files where nesting exceeds 4 levels (> 16 spaces or > 4 tabs). Count violations per file.

- [ ] **2.10** **High cyclomatic complexity (weight 3):** For each method found in 2.7, count control-flow keywords: `if`, `else`, `switch`, `case`, `for`, `foreach`, `while`, `catch`. Flag methods where count exceeds 10. Count per file.

- [ ] **2.11** **Empty catch blocks (weight 3):** Grep for `catch\s*\([^)]*\)\s*\{\s*\}` and multi-line empty variants. Count per file.

- [ ] **2.12** **Catching generic Exception (weight 2):** Grep for `catch\s*\(\s*Exception\s` on `.cs` files. Count per file.

- [ ] **2.13** **String concat in loops (weight 2):** Find `+=` on a string variable inside `for`/`foreach`/`while` blocks. Count per file.

- [ ] **2.14** **God classes (weight 2):** Count total lines per `.cs` file. Flag files exceeding 500 LOC. Count per project.

- [ ] **2.15** **Copy-paste blocks (weight 3):** Run the Python script per project:
  ```bash
  python3 /Users/ronstarling/repos/Umbraco-CMS/tools/cast/detect_copy_paste.py \
    --dir /Users/ronstarling/repos/Umbraco-CMS/src/<ProjectName> \
    --ext .cs
  ```
  Count the number of duplicated block groups as violations.

- [ ] **2.16** After each project, narrate findings: "Scanning `Umbraco.Infrastructure`: ~87,000 LOC. Found 12 God classes, 34 high-complexity methods, 8 empty catch blocks..." This keeps the demo engaging.

---

### Task 3: TypeScript Scanner — All TypeScript Violation Categories

TypeScript files live in `src/Umbraco.Web.UI.Client/src/packages/<package>/`. Packages are treated as separate modules for module-selection but grouped as a single "TypeScript" section in the baseline report.

**Excluded paths:** `**/node_modules/**`, `**/generated/**`, `**/obj/**`, `**/bin/**`, `tests/**`

- [ ] **3.1** Enumerate all TypeScript packages using Glob on `src/Umbraco.Web.UI.Client/src/packages/**/*.ts`. Group files by their immediate parent package directory.

- [ ] **3.2** **Long lines (weight 1):** Count lines > 120 chars per file.

- [ ] **3.3** **Magic numbers (weight 3):** Numeric literals excluding 0, 1, -1.

- [ ] **3.4** **Magic strings (weight 3):** String literals appearing 2+ times per file, excluding decorator arguments and log/error messages.

- [ ] **3.5** **Short identifiers (weight 2):** 1-2 character names excluding loop counters, catch clause variables, arrow function parameters (e.g., `arr.map(x => ...)`).

- [ ] **3.6** **Commented-out code (weight 1):** 3+ consecutive `//`-prefixed lines containing `;`, `{`, or `}`.

- [ ] **3.7** **Method length (weight 2):** Functions/methods > 30 lines. Track brace depth from function declaration to find boundaries.

- [ ] **3.8** **Too many parameters (weight 2):** Functions with > 4 parameters. Grep for `\(([^)]*,){4,}`.

- [ ] **3.9** **Deep nesting (weight 2):** Code indented > 8 spaces (5+ nesting levels with 2-space TS indentation).

- [ ] **3.10** **High cyclomatic complexity (weight 3):** Count `if`, `else`, `switch`, `case`, `for`, `forEach`, `while`, `catch` per function. Flag > 10.

- [ ] **3.11** **Copy-paste blocks (weight 3):** Run Python script per package:
  ```bash
  python3 /Users/ronstarling/repos/Umbraco-CMS/tools/cast/detect_copy_paste.py \
    --dir /Users/ronstarling/repos/Umbraco-CMS/src/Umbraco.Web.UI.Client/src/packages/<package> \
    --ext .ts
  ```

- [ ] **3.12** **God classes (weight 2):** `.ts` files exceeding 500 LOC.

- [ ] **3.13** **Empty catch blocks (weight 3):** `catch\s*\([^)]*\)\s*\{\s*\}` in TypeScript.

- [ ] **3.14** **`any` type usage (weight 2):** Grep for `:\s*any\b`, `<any>`, or `as any`. Count per file.

- [ ] **3.15** **Missing return types (weight 1):** Exported functions without a return type annotation. Grep for `^export\s+(async\s+)?function\s+\w+\s*\([^)]*\)\s*\{` (no `:` type before `{`).

- [ ] **3.16** **Debug statements (weight 1):** Grep for `console\.log|console\.error` in source `.ts` files.

- [ ] **3.17** **Missing semicolons (weight 1):** Lines ending with `)`, `]`, or a word character that are not followed by `{`, `,`, `;` — heuristic for statement-ending lines lacking `;`.

- [ ] **3.18** Narrate findings per package: "Scanning `documents` package: 847 TypeScript files. Found 23 `any` usages, 156 missing return types, 4 debug statements..."

---

### Task 4: Produce the Baseline Report

- [ ] **4.1** Compile all scan results. For each project (C# and TS), calculate:
  ```
  weighted_violations = sum(violation_count[category] × weight[category])
  score = max(0, 100 − (weighted_violations / total_LOC × 1000))
  ```
  Per-file scores: only for files with ≥ 20 LOC. Files under 20 LOC count toward project LOC and violations but are not reported individually.

- [ ] **4.2** Identify top 10 worst files per language by per-file score.

- [ ] **4.3** Write `docs/quality/cast-assessment-baseline.md` with this structure:

  **Section 1: Executive Summary**
  - Overall codebase score (LOC-weighted across all projects)
  - Total LOC scanned (C# + TypeScript separately)
  - Total violations by category (table: Category | Weight | Count | Weighted Count)
  - Top 3 most common violation types

  **Section 2: By-Language Breakdown**
  - C# overall score + violation table (13 categories)
  - TypeScript overall score + violation table (8 TypeScript-specific categories)

  **Section 3: By-Project Table**
  - Columns: Project | LOC | Score | [17 violation category columns]
  - Rows sorted by Score ascending (worst first)
  - This is the primary input for Phase 2 module selection

  **Section 4: Top 10 Worst Files Per Language**
  - C# table: File (relative path) | LOC | Score | Top Violation Category
  - TypeScript table: same structure

  **Section 5: Methodology Note**
  - Formula: `Score = max(0, 100 − (weighted_violations / LOC × 1000))`
  - Files < 20 LOC excluded from per-file reporting (violations still count at project level)
  - Excluded paths listed
  - Copy-paste detection described (Python sliding-window SHA-256, window = 5 lines, ≥ 2 files)

- [ ] **4.4** Narrate to the user: "Baseline assessment complete. Overall score: XX/100. Worst C# project: `<Name>` (YY/100). Worst TypeScript package: `<name>` (ZZ/100). Ready for module selection."

---

## Chunk 2: Phase 2 — Module Selection

### Task 5: Present Ranked Project List and Confirm Module Selection

- [ ] **5.1** Print the by-project table from the baseline report sorted worst-to-best:
  ```
  Rank | Project                    | LOC   | Score | Key Violations
  ---- | -------------------------- | ----- | ----- | -------------------------------------------
  1    | Umbraco.Infrastructure     | 87420 | 43.2  | God classes: 12, High CC: 34
  2    | Umbraco.Cms.Api.Management | 54210 | 51.7  | Method length: 28, Magic strings: 89
  ...
  ```

- [ ] **5.2** Ask the user: "Which module would you like to remediate? Enter the project name or TypeScript package name (e.g., `Umbraco.Infrastructure` or `documents`)."

- [ ] **5.3** Once the user responds, confirm the selection:
  - Determine whether it is a C# project (`src/<Name>/`) or TypeScript package (`src/Umbraco.Web.UI.Client/src/packages/<name>/`)
  - Report: "Selected module: `<name>`. Estimated violations: N total. Files affected: M. Proceeding to Phase 3."
  - Store the module path for all subsequent tasks.

- [ ] **5.4** If the module contains both C# and TypeScript, note that both `dotnet build` and `npm run compile` will be run for build verification.

---

## Chunk 3: Phase 3 — Grouped Remediation Plan for Chosen Module

### Task 6: Produce a Grouped Remediation Plan

- [ ] **6.1** Re-read all violation data for the chosen module only. Organize into 5 groups:
  - **Group 1 — Structural Complexity:** High cyclomatic complexity, method length, deep nesting, too many parameters, copy-paste blocks, God classes
  - **Group 2 — Error Handling:** Empty catch blocks, catching generic Exception, string concat in loops
  - **Group 3 — Magic Values:** Magic numbers, magic strings
  - **Group 4 — Style:** Short identifiers, long lines, commented-out code
  - **Group 5 — TypeScript** (if applicable): `any` types, missing return types, debug statements, missing semicolons

- [ ] **6.2** For each group, list every affected file with:
  - File path (relative to repo root)
  - Violation type(s) present
  - Specific methods/classes to refactor (with line numbers from the scan)
  - Refactoring approach (e.g., extract method, add StringBuilder, introduce constant)

- [ ] **6.3** Output the plan as an in-session summary:
  ```
  === Group 1: Structural Complexity — 8 files, 23 violations ===
  src/Umbraco.Infrastructure/Services/ContentService.cs
    - God class (847 LOC): Extract IContentPublishingService, IContentVersionService
    - High CC method 'SaveAndPublish' (CC=18): Extract validation, notification, and persistence steps
    - Deep nesting in 'ProcessBulkOperation' line 423: Use guard clauses
  ...
  ```

---

## Chunk 4: Phase 4 — Remediation Cycle (One per Group)

Each group follows the same pattern: branch → fix → code review → build verify → PR → delta report.

---

### Task 7: Group 1 — Structural Complexity

**Branch:** `v17/improvement/cast-structural-complexity`
**Categories:** High cyclomatic complexity, method length, deep nesting, too many parameters, copy-paste blocks, God classes
**Delta report:** `docs/quality/cast-assessment-group-1-structural-complexity.md`

- [ ] **7.1** Create the branch from `main`:
  ```bash
  cd /Users/ronstarling/repos/Umbraco-CMS
  git checkout main && git pull origin main
  git checkout -b v17/improvement/cast-structural-complexity
  ```

- [ ] **7.2** **Fix God classes** (files > 500 LOC): Identify natural responsibility boundaries and extract cohesive method groups into new focused classes. Update DI registrations in `DependencyInjection/` directories. Follow the Obsolete Constructor pattern (CLAUDE.md §5.1) if changing public constructor signatures.

- [ ] **7.3** **Fix high cyclomatic complexity** (methods with CC > 10): Apply extract-method refactoring — move cohesive sub-logic into private helper methods. For `switch` statements with complex cases, consider a strategy dictionary. Keep public method signatures unchanged.

- [ ] **7.4** **Fix deep nesting** (nesting > 4 levels): Apply guard clauses (invert conditions to return early at the top of the method). Extract inner loops into separate private methods.
  ```csharp
  // Before (nested)
  if (condition1) {
      if (condition2) {
          foreach (var item in items) {
              if (item.IsValid) { DoWork(item); }
          }
      }
  }
  // After (guard clauses + extraction)
  if (!condition1 || !condition2) return;
  ProcessValidItems(items);
  ```

- [ ] **7.5** **Fix too many parameters** (> 4 params): Introduce a parameter object (new `record` or `class` in `Models/`). Follow the Obsolete Method pattern (CLAUDE.md §5.2) for public API methods.

- [ ] **7.6** **Fix copy-paste blocks**: Extract duplicated blocks into a shared static helper class or extension method. Replace all occurrences with a call to the extracted method.

- [ ] **7.7** **Fix method length** (> 30 lines, not already addressed above): Apply extract-method — identify 5–15 line logical sub-steps and name them clearly.

- [ ] **7.8** Invoke `superpowers:code-reviewer` to review all Group 1 changes. Focus on: correctness of refactoring, no behavioral changes, adherence to Umbraco patterns (Attempt pattern, notification pattern, DI registration), no breaking changes.

- [ ] **7.9** Verify the build:
  - C# module: `dotnet build` — expected: `Build succeeded.` with 0 errors
  - TypeScript module: `cd src/Umbraco.Web.UI.Client && npm run compile` — expected: 0 errors
  - Both if applicable. Fix any errors before proceeding.

- [ ] **7.10** Commit:
  ```bash
  git add src/<chosen-module>/
  git commit -m "$(cat <<'EOF'
  refactor(<module>): reduce structural complexity per CAST analysis

  - Extract God classes into focused service classes
  - Reduce cyclomatic complexity via method extraction
  - Apply guard clauses to reduce nesting depth
  - Introduce parameter objects for methods with 5+ params
  - Extract duplicated code blocks into shared helpers

  Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
  EOF
  )"
  ```

- [ ] **7.11** Create the PR:
  ```bash
  gh pr create \
    --title "Improvement: CAST remediation — structural complexity (<module>)" \
    --base main \
    --body "$(cat <<'EOF'
  ## Summary

  CAST-aligned structural complexity remediation for `<module>`. Addresses Group 1 violations from the baseline assessment (`docs/quality/cast-assessment-baseline.md`).

  ## Violation Counts

  | Category | Before | After | Delta |
  |---|---|---|---|
  | God classes | N | M | -X |
  | High cyclomatic complexity | N | M | -X |
  | Deep nesting | N | M | -X |
  | Too many parameters | N | M | -X |
  | Copy-paste blocks | N | M | -X |
  | Method length | N | M | -X |

  **Estimated score delta:** +Y points

  ## Files Changed

  [List all changed files with a one-line description of each change]

  ## Build Verification

  - [x] `dotnet build` passes with 0 errors
  - [ ] `npm run compile` (if applicable)

  ## References

  - Spec: `docs/specs/2026-03-11-cast-tech-debt-remediation-design.md`
  - Baseline: `docs/quality/cast-assessment-baseline.md`
  - Delta report: `docs/quality/cast-assessment-group-1-structural-complexity.md`

  🤖 Generated with [Claude Code](https://claude.com/claude-code)
  EOF
  )"
  ```

- [ ] **7.12** Run scoped re-assessment on all changed files. Re-run Group 1 violation checks scoped to those files and compare to baseline counts.

- [ ] **7.13** Write `docs/quality/cast-assessment-group-1-structural-complexity.md`:
  ```markdown
  # CAST Delta Report — Group 1: Structural Complexity
  **Module:** <chosen module>
  **Branch:** v17/improvement/cast-structural-complexity
  **Date:** 2026-03-11

  ## Violations Before vs. After
  | Category | Before | After | Reduction |
  |---|---|---|---|
  ...

  ## Score Before vs. After
  | Metric | Before | After |
  |---|---|---|
  | Module Score | XX | YY |

  ## Files Changed and Per-File Improvement
  | File | Before Score | After Score | Violations Removed |
  |---|---|---|---|
  ...

  ## Running Total
  - Group 1 complete: +Y points improvement
  - Cumulative score change: XX → YY
  ```

---

### Task 8: Group 2 — Error Handling

**Branch:** `v17/improvement/cast-error-handling`
**Categories:** Empty catch blocks, catching generic Exception, string concat in loops
**Delta report:** `docs/quality/cast-assessment-group-2-error-handling.md`

- [ ] **8.1** Create branch: `git checkout main && git checkout -b v17/improvement/cast-error-handling`

- [ ] **8.2** **Fix empty catch blocks:** For each empty `catch` block, read the surrounding context and choose an appropriate fix: log the exception, rethrow as a more specific type, or add a comment explaining WHY silence is intentional (`// Intentionally suppressed: <reason>`). Do NOT add `// ignored`.

- [ ] **8.3** **Fix catching generic Exception:** Replace `catch (Exception` with the most specific exception type(s) that could realistically be thrown by the `try` block. If a top-level catch-all is genuinely needed, add a comment.

- [ ] **8.4** **Fix string concat in loops:** Replace `+=` on strings inside loops with `StringBuilder`:
  ```csharp
  // Before
  foreach (var item in items) result += item.Name + ", ";
  // After
  var sb = new StringBuilder();
  foreach (var item in items) sb.Append(item.Name).Append(", ");
  var result = sb.ToString().TrimEnd(',', ' ');
  ```
  Or use `string.Join(separator, collection.Select(...))` where appropriate.

- [ ] **8.5** Invoke `superpowers:code-reviewer`. Focus on: no silent exception swallowing introduced, StringBuilder usage correct, no behavioral changes.

- [ ] **8.6** Verify build (`dotnet build` / `npm run compile` / both). Fix any errors.

- [ ] **8.7** Commit, create PR (`Improvement: CAST remediation — error handling (<module>)`), run scoped re-assessment, write `docs/quality/cast-assessment-group-2-error-handling.md`. Follow the Task 7 pattern (steps 7.10–7.13).

---

### Task 9: Group 3 — Magic Values

**Branch:** `v17/improvement/cast-magic-values`
**Categories:** Magic numbers, magic strings
**Delta report:** `docs/quality/cast-assessment-group-3-magic-values.md`

- [ ] **9.1** Create branch: `git checkout main && git checkout -b v17/improvement/cast-magic-values`

- [ ] **9.2** **Fix magic numbers:** For each numeric literal (excluding 0, 1, -1), derive a meaningful constant name from context. Check `src/Umbraco.Core/Constants-*.cs` first — if a matching constant exists, use it. Otherwise, declare `private const int <ConstantName> = <value>;` at class level and replace all occurrences.

- [ ] **9.3** **Fix magic strings:** For string literals appearing 2+ times per file, check `src/Umbraco.Core/Constants-*.cs` for an existing constant. If not found, declare `private const string <ConstantName> = "<value>";`. For strings shared across multiple files in the same project, consider a project-level constants file.

- [ ] **9.4** Invoke `superpowers:code-reviewer`. Focus on: constant names are meaningful and PascalCase, no duplication of existing Core constants.

- [ ] **9.5** Verify build. Fix any errors.

- [ ] **9.6** Commit, create PR (`Improvement: CAST remediation — magic values (<module>)`), run scoped re-assessment, write `docs/quality/cast-assessment-group-3-magic-values.md`. Follow the Task 7 pattern.

---

### Task 10: Group 4 — Style

**Branch:** `v17/improvement/cast-style`
**Categories:** Short identifiers, long lines, commented-out code
**Delta report:** `docs/quality/cast-assessment-group-4-style.md`

- [ ] **10.1** Create branch: `git checkout main && git checkout -b v17/improvement/cast-style`

- [ ] **10.2** **Fix short identifiers:** For each 1-2 char variable NOT exempt (not a loop counter, catch var, lambda param, or generic type param), rename to a descriptive name based on context (e.g., `s` → `serializedContent`, `dt` → `documentType`). For public method parameters, follow the Obsolete Method pattern (CLAUDE.md §5.2) to avoid breaking named-argument call sites.

- [ ] **10.3** **Fix long lines:** Wrap method call chains at `.` boundaries, break parameter lists one-per-line, or extract intermediate variables:
  ```csharp
  // Before (150 chars)
  var result = _contentService.GetById(id).Children().Where(x => x.IsPublished).OrderBy(x => x.Name).ToList();
  // After
  var result = _contentService
      .GetById(id)
      .Children()
      .Where(x => x.IsPublished)
      .OrderBy(x => x.Name)
      .ToList();
  ```

- [ ] **10.4** **Fix commented-out code:** Delete the lines entirely. Dead code is in version control history. Exception: rewrite as a single clean prose comment if it contains a meaningful TODO.

- [ ] **10.5** Invoke `superpowers:code-reviewer`. Focus on: renames don't break named argument call sites, no logic accidentally removed with commented-out code, long-line wrapping preserves exact semantics.

- [ ] **10.6** Verify build. Fix any errors.

- [ ] **10.7** Commit, create PR (`Improvement: CAST remediation — style (<module>)`), run scoped re-assessment, write `docs/quality/cast-assessment-group-4-style.md`. Follow the Task 7 pattern.

---

### Task 11: Group 5 — TypeScript

**Branch:** `v17/improvement/cast-typescript`
**Categories:** `any` type usage, missing return types, debug statements, missing semicolons
**Delta report:** `docs/quality/cast-assessment-group-5-typescript.md`
**Applies to:** Only if the chosen module is a TypeScript package. Skip if C# only.

- [ ] **11.1** Create branch: `git checkout main && git checkout -b v17/improvement/cast-typescript`

- [ ] **11.2** **Fix `any` type usage:** Replace each `any` with the correct type inferred from surrounding code. Use generated types from `src/Umbraco.Web.UI.Client/src/external/` or the management-api package where available. Use `unknown` with a type guard if the type truly cannot be determined.

- [ ] **11.3** **Fix missing return types:** Add explicit return type annotations to exported functions:
  - Synchronous: `function foo(): ReturnType { ... }`
  - Async: `async function foo(): Promise<ReturnType> { ... }`
  - Void: `function foo(): void { ... }`

- [ ] **11.4** **Remove debug statements:** Delete `console.log` and `console.error` lines in source files. If the output appears intentional (error reporting), replace with the appropriate Umbraco logging mechanism (check `src/Umbraco.Web.UI.Client/CLAUDE.md` for patterns).

- [ ] **11.5** **Add missing semicolons:** Add `;` to statement-ending lines that lack one. Be careful with multi-line object literals, array literals, and template literals — only add `;` to the closing line.

- [ ] **11.6** Invoke `superpowers:code-reviewer`. Focus on: `any` replacements are type-safe, no runtime behavior changed by removing `console` calls, semicolon additions don't break template literals.

- [ ] **11.7** Verify the TypeScript build:
  ```bash
  cd /Users/ronstarling/repos/Umbraco-CMS/src/Umbraco.Web.UI.Client
  npm run compile
  ```
  Expected: 0 TypeScript errors.

- [ ] **11.8** Commit, create PR (`Improvement: CAST remediation — typescript (<module>)`), run scoped re-assessment, write `docs/quality/cast-assessment-group-5-typescript.md`. Follow the Task 7 pattern.

---

## Final Verification Checklist

- [ ] **F.1** Confirm all PRs created (minimum: Groups 1 and 2 for success criteria).
- [ ] **F.2** Confirm all delta reports exist:
  ```bash
  ls /Users/ronstarling/repos/Umbraco-CMS/docs/quality/
  ```
  Expected: `cast-assessment-baseline.md` plus one `cast-assessment-group-N-*.md` per completed group.

- [ ] **F.3** Present cumulative score improvement:
  ```
  Module: <name>
  Baseline score:    XX/100
  After Group 1:     YY/100  (+ΔY)
  After Group 2:     ZZ/100  (+ΔZ)
  ...
  Final score:       WW/100  (total improvement: +ΔTotal)
  ```

- [ ] **F.4** Confirm no test files modified:
  ```bash
  git diff main --name-only | grep "^tests/"
  ```
  Expected: no output.

- [ ] **F.5** Confirm no generated files modified:
  ```bash
  git diff main --name-only | grep -E "(Generated|\.Generated\.cs|/generated/)"
  ```
  Expected: no output.

---

## Appendix: Violation Weight Reference

| Category | Weight | Applies To |
|---|---|---|
| High cyclomatic complexity | 3 | C#, TypeScript |
| Copy-paste blocks | 3 | C#, TypeScript |
| Empty catch blocks | 3 | C#, TypeScript |
| Magic numbers | 3 | C#, TypeScript |
| Magic strings | 3 | C#, TypeScript |
| Catching generic Exception | 2 | C# only |
| String concat in loops | 2 | C# only |
| God classes | 2 | C#, TypeScript |
| Method length | 2 | C#, TypeScript |
| Too many parameters | 2 | C#, TypeScript |
| Deep nesting | 2 | C#, TypeScript |
| Short identifiers | 2 | C#, TypeScript |
| `any` type usage | 2 | TypeScript only |
| Long lines | 1 | C#, TypeScript |
| Commented-out code | 1 | C#, TypeScript |
| Missing return types | 1 | TypeScript only |
| Debug statements | 1 | TypeScript only |
| Missing semicolons | 1 | TypeScript only |

---

## Appendix: Scoring Formula

```
Score = max(0, 100 − (weighted_violations / LOC × 1000))
```

- `weighted_violations` = sum of (violation count × weight) for all categories
- `LOC` = total non-blank lines in scope
- Files with < 20 LOC: violations counted at project level, no per-file score reported
- Clamped to [0, 100], higher is better

---

## Appendix: Output Files Created by This Plan

```
tools/
└── cast/
    └── detect_copy_paste.py                               ← Task 1

docs/
├── specs/
│   └── 2026-03-11-cast-tech-debt-remediation-design.md   (existing spec)
├── plans/
│   └── 2026-03-11-cast-tech-debt-remediation.md          (this file)
└── quality/
    ├── cast-assessment-baseline.md                        ← Task 4
    ├── cast-assessment-group-1-structural-complexity.md   ← Task 7
    ├── cast-assessment-group-2-error-handling.md          ← Task 8
    ├── cast-assessment-group-3-magic-values.md            ← Task 9
    ├── cast-assessment-group-4-style.md                   ← Task 10
    └── cast-assessment-group-5-typescript.md              ← Task 11
```

---

*Spec reference: `docs/specs/2026-03-11-cast-tech-debt-remediation-design.md`*
*Umbraco CMS v17.3.0-rc*

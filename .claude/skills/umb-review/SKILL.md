---
name: umb-review
description: Automated PR code review for Umbraco CMS. Analyzes changed files for intent, impact on consumers, breaking changes, architecture compliance, and code quality. Non-interactive — outputs a full structured review. Use this skill whenever the user asks to review a branch, review a PR, check their changes for issues, analyze a diff, or validate breaking change patterns — even if they don't say "review" explicitly. Does NOT apply to writing new code, fixing bugs, refactoring, explaining architecture, writing tests, or reviewing documentation content.
argument-hint: <target-branch>
---

# PR Review - Umbraco CMS

Automated, non-interactive PR code review. Analyzes changed files for intent, impact on consumers, breaking changes, architecture compliance, and code quality.

**Do NOT use AskUserQuestion at any point. This skill runs fully autonomously.**

## Arguments

- `$ARGUMENTS` - Optional: target branch to diff against (auto-detected from PR, falls back to `origin/main`)

## Instructions

### 0. Verify GH CLI is Available

Run `gh auth status`. If it fails, read `references/gh-cli-setup.md` and present the setup instructions to the user. Do not proceed with the review.

### 1. Resolve Target Branch

Determine the target branch for comparison using this priority order:

1. **Explicit argument**: If `$ARGUMENTS` is provided and non-empty, use it as the target branch
2. **PR target branch**: If no argument, run `gh pr view --json baseRefName --jq '.baseRefName'` to detect the target branch of the current branch's open PR. If a PR exists, use `origin/{baseRefName}` as the target branch.
3. **Fallback**: If no argument and no PR found (command fails or returns empty), default to `origin/main`

Store the resolved target branch for use in subsequent steps. Log which resolution method was used (e.g., "Target branch: `origin/v18/dev` (from PR #1234)").

### 2. Load Review Standards

#### 2a. Load coding preferences

Read the coding preferences and code review scoring criteria from:

- `references/coding-preferences.md` (relative to this skill file)

Parse and internalize all rules, conventions, scoring categories, and severity definitions. These are your review criteria.

#### 2b. Load area-specific documentation

Once the changed file list is known (after step 3a), determine which areas of the codebase are touched and load the relevant documentation. Execute this sub-step between 3a and 3b. This documentation takes precedence over sibling comparison for architectural and pattern validation.

**Resolution order for each changed file:**

1. **Find the nearest `CLAUDE.md`** — walk up from the changed file's directory toward the repository root. The first `CLAUDE.md` found is the area guide for that file. Read it.
2. **Read referenced docs** — if the `CLAUDE.md` references documentation files (e.g., a `docs/` directory), use the descriptions in the `CLAUDE.md` to determine which docs are relevant to the type of code being changed, and read those. If unsure, read all referenced docs — the cost of reading is low, the cost of missing a convention is high.
3. **Follow cross-references in loaded docs** — if a loaded doc references another doc as covering a complementary or related concern, and the changed files touch that concern, read the referenced doc too. Repeat until no new relevant cross-references remain.
4. **Check for applicable skills** — review the available skills list. If a skill exists for the type of code being changed, read the skill file to understand the expected patterns, structure, and conventions it enforces. Do NOT invoke the skill — just use it as a reference for what the correct implementation should look like.

**Store all loaded documentation** for use in step 4. These docs define the authoritative patterns and conventions that the review evaluates against.

### 3. Gather Changed Files

#### 3a. Collect file list, stats, and diff

Run these git commands (where `{target}` is the resolved target branch):

```bash
git diff {target}...HEAD --name-only --diff-filter=d   # changed files (excluding deleted)
git diff {target}...HEAD --stat                         # line counts per file
git log {target}...HEAD --oneline                       # commit history
git diff {target}...HEAD                                # full diff (primary review source)
```

**If no changes found**: Output "No changes found between current branch and `{target}`. Nothing to review." and stop.

#### 3b. Filter out noise files

From the changed file list, classify each file as **noise** or **reviewable**.

**Noise files** (skip entirely — do not read, do not review):

| Pattern                                              | Reason                          |
| ---------------------------------------------------- | ------------------------------- |
| `*.gen.ts`, `*.gen.cs`                               | Auto-generated API client code  |
| `*.generated.cs`, `*.Designer.cs` (in `Migrations/`) | Auto-generated models/snapshots |
| `*/assets/lang/*.ts` (except `en.ts`)                | Non-English translation files   |
| `*/mocks/data/*.ts`                                  | Test fixture data               |
| `*/dist-cms/*`, `*/storybook-static/*`               | Build output                    |
| `*/TEMP/InMemoryAuto/*`                              | Runtime-generated models        |
| `package-lock.json`                                  | Dependency lock file            |
| `appsettings-schema.*.json`                          | Generated JSON schema           |

Log the skip list: "Skipped {N} noise files: {comma-separated list of filenames}"

#### 3c. Read reviewable changed files

Read the full file for every reviewable changed file.

#### 3d. Track file counts

Keep track of these numbers for the review output in step 7: total changed files, noise files skipped, and reviewable files read. Also record: distinct production layers touched, distinct project directories, and total lines changed — these feed step 3e.

#### 3e. Assess PR complexity

Follow the procedure in `references/complexity-assessment.md`. Store the triggered dimensions and suggestions for step 7.

#### 3f. Classify PR scope

Classify the PR to determine which review steps are relevant:

| Classification  | Condition                                                                       | Effect                                                                          |
| --------------- | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| **Gen-only**    | All reviewable files are `gen.ts`                                               | Skip steps 5 and 6; step 4 reviews impact on other code only                    |
| **Docs-only**   | All reviewable files are `.md`                                                  | Skip steps 5 and 6; step 4 reviews intent and readability only                  |
| **Test-only**   | All reviewable files are in `tests/`                                            | Skip steps 5 and 6; step 4 reviews intent, code quality, and test coverage only |
| **Config-only** | All reviewable files are `.csproj`, `.props`, `.json` config, or CI/build files | Skip step 5; step 6 checks dependency version changes only                      |
| **Standard**    | Anything else                                                                   | No skips — run all steps                                                        |

### 4. Raw Code Review

Review each changed file holistically. Think like a senior developer reading a colleague's PR. Note all findings without worrying about format or severity yet.

#### 4a. Read and reason about each file

For each changed file, reason about: What does this code do? Is it correct? What's missing — validation, error handling, notifications, cleanup, edge cases? Could this break anything for consumers?

#### 4b. Validate against documentation and patterns

Use a **docs-first** approach: classify the code by what it does, check it against documented conventions, and only fall back to sibling comparison when docs don't cover the pattern.

**Step 1 — Determine the correct approach from documentation, then check whether the PR matches**

A PR is a proposed solution, not the source of truth. This step has two parts that must happen in order — do not start part B until part A is complete.

**Part A — Before validating/judging the implementation**, determine what the correct approach is for each new class or file based on what it does. Use the documentation loaded in step 2b to identify the expected base classes, patterns, and conventions. Write down the expected approach. Classify based on what the code does, not based on what neighboring files look like.

**Part B — Now compare the PR's implementation** against the expected approach from Part A. If it deviates from the documented approach, flag it. If the documentation specifies reference examples, read those examples to verify the implementation matches.

**Pattern match is the leading finding.** If the documentation defines a pattern that fits what the code does, the first and most important finding is whether the code follows that pattern.

**Step 2 — Fall back to sibling comparison**

If the documentation does not cover the specific pattern, or for cross-cutting concerns not addressed in docs, fall back to sibling comparison:

1. **New method on existing class/interface**: Grep for the most similar existing method on the same class using `-A 80` to capture the full method body (e.g., `UpdateCurrentUserAsync` → grep for `UpdateAsync` in the same file with `-A 80`). Compare line by line for missing cross-cutting concerns: notifications/events, validation, scoping, authorization, error handling, audit logging.
2. **New TS class**: Grep for siblings by base class (`extends {BaseClass}`) or by interface (`implements {Interface}`) or by name suffix (e.g., `CurrentUserController` → grep for `UserController`). Compare for missing concerns.
3. **New CS class**: Grep for siblings by base class (`class {ClassName} : {BaseClass}`) or by interface (`class {ClassName} : {Interface}`) or by name suffix (e.g., `ManagementApiComposer` → grep for `ApiComposer`). Compare for missing concerns.

**Important:** Sibling comparison validates cross-cutting concerns, but it must not override documented conventions. If a sibling deviates from documented patterns, that sibling is wrong — do not copy its deviation.

Store your raw findings — they feed into step 7.

### 5. Impact Analysis

**Skip this step if PR scope is docs-only, test-only, or config-only.**

Follow the procedure in `references/impact-analysis.md`.

### 6. Breaking Changes Check

**Skip this step if PR scope is docs-only or test-only. If config-only, only check for dependency version changes that could break consumers.**

Follow the procedure in `references/breaking-changes.md`.

### 7. Consolidate and Output Review

Merge findings from step 4 (raw review), step 5 (impact analysis), and step 6 (breaking changes). For each finding, assign severity (Critical/Important/Suggestion) and verify it relates to changed code — not pre-existing issues. Before outputting, drop any finding about whitespace, blank lines, formatting, or comment wording. Then present the review in this exact format:

```markdown
## PR Review

**Target:** `{target_branch}` · **Based on commit:** `{head_sha}`
[If any skipped files, append: · **Skipped:** {skipped} files out of {total} total]
[If step 3f classification is not "Standard", append: · **Classified as:** {classification}]

[1–2 sentences: what this PR accomplishes , keep it as short as possible, only highlight the primary essence.]

- **Modified public API:** {changed existing interfaces/types/classes/methods}
  [Omit bullet if none]
- **Affected implementations (outside this PR):** {interfaces/types/classes/methods using modified public API}
  [Omit bullet if none]
- **Breaking changes:** {violations with specifics}
  [Omit bullet if none]
- **Other changes:** {changes not listed above that an Umbraco user, plugin developer, or API consumer would notice — e.g., behavior changes, default value changes, error message changes, new configuration options, removed functionality. Exclude internal renames, formatting, and private implementation details.}
  [Omit bullet if none]

[If step 3e triggered any dimensions, insert this block. Omit entirely if nothing triggered:]

> [!NOTE]
> **Complexity advisory** — This PR may benefit from splitting.
>
> - **{Dimension}:** {Explanation and concrete split suggestion from step 3e}
>   [one bullet per triggered dimension]
>
> _This is an observation, not a blocker. The full review follows below._

---

### Critical

[Must fix before merge — security vulnerabilities, data loss, broken functionality, breaking changes without proper patterns]

- **`{file}:{line}`**: {problem} → {fix}

[Omit section if none]

### Important

[Should fix — performance issues, missing tests, architectural violations, pattern misuse]

- **`{file}:{line}`**: {observation} → {suggestion}

[Omit section if none]

### Suggestions

[Nice to have — readability, minor refactoring, alternative approaches]

- **`{file}:{line}`**: {detail}

[Omit section if none]

---

[One of:]

## Approved

This looks good to be merged as-is, but please do a manual sanity check and testing before merging.

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions.

## Request Changes

Critical and important issues must be addressed first.

## Needs re-work

This is in such a bad state that the feedback of this review is not sufficient to guide improvements, the PR cannot be approved.
```

**Guidelines for the review output:**

— When reporting information, be extremely concise and sacrifice grammar for sake of concision.

- Only review code that was changed in the diff — pre-existing issues are out of scope. Focus on what compilers and linters cannot catch: behavioral side-effects (e.g., a changed default alters runtime behavior for consumers), architectural violations (e.g., a new dependency breaks layering), breaking changes for external consumers of the public API, and security implications. Leave type errors, missing imports, and broken references to CI.
- Be specific — always reference file and line number
- Explain WHY something is an issue, not just WHAT, but avoid stating the obvious.
- For complex matters, provide concrete fix suggestions, including code snippets when helpful
- Keep it constructive — the goal is to help, not gatekeep
- Don't repeat the same finding for every occurrence — mention it once and note "same pattern in {other files}"
- Focus on substantive issues only. Do NOT flag purely cosmetic or stylistic concerns. Specifically, never flag: code formatting or whitespace, comment grammar or wording, redundant-but-harmless syntax (e.g., optional chaining after a truthiness check), code duplication that doesn't cause bugs, or HTML template cosmetics. The only exception is when a stylistic issue has a concrete impact on performance or rendering. Note: missing JSDoc/documentation on public or exported APIs is a substantive finding (per coding preferences), not a cosmetic one — flag it as a Suggestion.
- For breaking changes, reference the specific pattern from the CLAUDE.md that should be applied
- Do not suggest changes that would themselves introduce breaking changes. If a suggestion would alter public API surface (e.g., changing return types, renaming public members), it is not appropriate for a PR targeting `main` within a major version. Only suggest non-breaking alternatives.

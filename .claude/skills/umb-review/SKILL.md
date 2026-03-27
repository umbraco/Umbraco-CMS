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

Read the coding preferences and code review scoring criteria from:

- `references/coding-preferences.md` (relative to this skill file)

Parse and internalize all rules, conventions, scoring categories, and severity definitions. These are your review criteria.

### 3. Gather Changed Files

#### 3a. Collect file list, stats, and diff

Run the following git commands to understand what changed:

```bash
git diff {target}...HEAD --name-only --diff-filter=d
```

```bash
git diff {target}...HEAD --stat
```

```bash
git log {target}...HEAD --oneline
```

Where `{target}` is the resolved target branch from step 1. Use `--diff-filter=d` to exclude deleted files (we only review files that exist).

**If no changes found**: Output "No changes found between current branch and `{target}`. Nothing to review." and stop.

Then get the full diff — this is your **primary review source**:

```bash
git diff {target}...HEAD
```

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

##### For files under 1501 lines

Read the full file.

##### For files over 1500 lines

Use targeted `offset`/`limit` reads around the diff hunks instead, and read the first 100 lines of the file to capture `using`/`import` statements and class declarations (needed for dependency-flow checks in step 5):

If an extended class, interface or type has been changed as part of this PR, then read the full file.

#### 3d. Track file counts

Keep track of these numbers for the review output in step 7: total changed files, noise files skipped, and reviewable files read. Also record: distinct production layers touched, distinct project directories, and total lines changed — these feed step 3e.

#### 3e. Assess PR complexity

Evaluate whether the PR's scope suggests it should be split. This assessment is **informational only** — it never blocks or shortens the review.

##### Exemptions — skip this step entirely if ANY of these hold

- More than 80% of reviewable files are `.md` documentation
- All reviewable production files reside in a single project directory
- The PR is purely test additions/modifications (no production file changes)
- The PR is primarily a dependency bump (`Directory.Packages.props` or `package.json` are the main changes)
- The majority of changes are renames (`git diff {target}...HEAD --diff-filter=R --name-only`)

##### Trigger rules

If no exemption applies, flag each dimension whose condition is met:

| Dimension                       | Condition                                                                                                                                                                                 |
| ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Size**                        | More than 30 reviewable files OR more than 1500 lines changed, AND changes span more than one project                                                                                     |
| **Layer spread**                | 3+ distinct production layers touched (using the layer mapping from `references/impact-analysis.md`, excluding Test), at least one is Core or Frontend, AND more than 10 reviewable files |
| **Mixed intent**                | 2+ distinct intent categories detected AND more than 15 reviewable files or more than 2 distinct projects                                                                                 |
| **Formatting mixed with logic** | A file's whitespace-ignored diff is significantly smaller than its full diff (see detection method below)                                                                                 |

**Intent categories** — detect from diff characteristics only (do not rely on commit message wording):

- **New functionality**: new files added to the PR, or new `public`/`export` type declarations in the diff
- **Bug fix**: no new files AND small targeted edits to existing logic (few lines changed per file). If new functionality is detected, do not also flag bug fix — edits to existing files alongside new files are normal feature implementation, not a separate concern.
- **Refactoring**: `git diff {target}...HEAD --diff-filter=R --name-only` shows file renames; symbols appear in both removed and added lines with changed locations but same logic
- **Dependency update**: changes to `.csproj`, `Directory.Packages.props`, `package.json` dependency sections

**Formatting mixed with logic** — detection method:

Run `git diff {target}...HEAD --stat --ignore-all-space` and compare the per-file line counts against the regular `git diff {target}...HEAD --stat`. For any file where the whitespace-ignored diff is less than **half** the size of the full diff, that file has significant formatting changes mixed with logic. Flag this dimension and list the affected files. Skip files where the full diff is under 50 lines (small formatting tweaks are not worth flagging).

##### Split suggestions

For each triggered dimension, prepare a concrete suggestion:

- **Layer spread (Core + higher layers):** "Consider a first PR adding Core contracts and Infrastructure implementations, followed by a second PR for API/Web/Frontend consumers."
- **Layer spread (Backend + Frontend):** "Consider splitting backend (.NET) and frontend (TypeScript) changes into separate PRs."
- **Mixed intent (feature + refactor):** "Consider extracting the refactoring into a preparatory PR, then building the new feature on top."
- **Mixed intent (bugfix + feature):** "The bug fix could be merged independently for faster delivery, with the new feature as a follow-up."
- **Size (many projects):** "If changes in {projectA} and {projectB} are independently functional, they could be separate PRs."
- **Formatting mixed with logic:** "File(s) {list} contain significant formatting changes mixed with logic changes. Consider a separate formatting-only commit or PR to keep the functional diff reviewable."

Store the triggered dimensions and their suggestions for use in step 7.

#### 3f. Classify PR scope

Classify the PR to determine which review steps are relevant:

| Classification  | Condition                                                                       | Effect                                                                          |
| --------------- | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| **Docs-only**   | All reviewable files are `.md`                                                  | Skip steps 4 and 5; step 6 reviews intent and readability only                  |
| **Test-only**   | All reviewable files are in `tests/`                                            | Skip steps 4 and 5; step 6 reviews intent, code quality, and test coverage only |
| **Config-only** | All reviewable files are `.csproj`, `.props`, `.json` config, or CI/build files | Skip step 4; step 5 checks dependency version changes only                      |
| **Standard**    | Anything else                                                                   | No skips — run all steps                                                        |

### 4. Impact Analysis

**Skip this step if PR scope is docs-only, test-only, or config-only.**

Follow the procedure in `references/impact-analysis.md`.

### 5. Breaking Changes Check

**Skip this step if PR scope is docs-only or test-only. If config-only, only check for dependency version changes that could break consumers.**

Follow the procedure in `references/breaking-changes.md`.

### 6. Review Against All Criteria

**If PR scope is docs-only:** review only for intent and readability. **If test-only:** review only for intent, code quality, and test coverage. **Otherwise:** review all criteria below.

Analyze each changed file against:

- **Intent**: Does the change accomplish what the commits describe? Are there unintended side effects?
- **Impact**: What consumers are affected? Does this change ripple through the architecture? (from step 4)
- **Breaking changes**: Public API surface changes without proper obsolete patterns? (from step 5)
- **Architecture compliance**: Correct dependency direction, layer separation, pattern usage (from CLAUDE.md files)
- **Umbraco patterns**: Notification pattern (not C# events), Composer pattern (DI registration), Scoping with `Complete()`, Attempt pattern for operation results
- **Code quality**: Per coding preferences in `references/coding-preferences.md`
- **Test coverage**: New public methods have tests? Test files included in the PR?
- **Security**: Authorization checks, no hardcoded secrets, OWASP Top 10 baseline

### 7. Output Structured Review

Present the review in this exact format:

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
- Focus on substantive issues only. Do NOT flag purely cosmetic or stylistic concerns. Specifically, never flag: code formatting or whitespace, comment grammar or wording, redundant-but-harmless syntax (e.g., optional chaining after a truthiness check), code duplication that doesn't cause bugs, or HTML template cosmetics. The only exception is when a stylistic issue has a concrete impact on performance or rendering.
- For breaking changes, reference the specific pattern from the CLAUDE.md that should be applied
- Do not suggest changes that would themselves introduce breaking changes. If a suggestion would alter public API surface (e.g., changing return types, renaming public members), it is not appropriate for a PR targeting `main` within a major version. Only suggest non-breaking alternatives.

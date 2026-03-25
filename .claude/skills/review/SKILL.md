---
name: review
description: Automated PR code review for Umbraco CMS. Analyzes changed files for intent, impact on consumers, breaking changes, architecture compliance, and code quality. Non-interactive — outputs a full structured review.
argument-hint: <target-branch>
---

# PR Review - Umbraco CMS

Automated, non-interactive PR code review. Analyzes changed files for intent, impact on consumers, breaking changes, architecture compliance, and code quality.

**Do NOT use AskUserQuestion at any point. This skill runs fully autonomously.**

## Arguments

- `$ARGUMENTS` - Optional: target branch to diff against (auto-detected from PR, falls back to `origin/main`)

## Instructions

### 0. Verify GH CLI is Available

Before doing anything else, run `gh auth status` to check that the GitHub CLI is installed and authenticated.

**If the command fails** (not installed or not authenticated), **stop immediately** and output this message:

> **GH CLI is required for this review skill.**
>
> Install it (e.g. `brew install gh`) and then authorize by running this in your terminal (use the `!` prefix):
>
> ```
> ! gh auth login
> ```

Do not proceed with the review if this check fails.

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

### 3. Read Relevant CLAUDE.md Files

**Always load** the root `/CLAUDE.md` first (it contains the architecture overview, breaking changes policy, and project structure).

Then, for each changed file in the PR, determine which project it belongs to and load that project's CLAUDE.md if it exists. Use the file path to map to projects:

| Path prefix                               | CLAUDE.md to load                                  |
| ----------------------------------------- | -------------------------------------------------- |
| `src/Umbraco.Core/`                       | `src/Umbraco.Core/CLAUDE.md`                       |
| `src/Umbraco.Infrastructure/`             | `src/Umbraco.Infrastructure/CLAUDE.md`             |
| `src/Umbraco.Web.Common/`                 | `src/Umbraco.Web.Common/CLAUDE.md`                 |
| `src/Umbraco.Web.UI/`                     | `src/Umbraco.Web.UI/CLAUDE.md`                     |
| `src/Umbraco.Web.UI.Client/`              | `src/Umbraco.Web.UI.Client/CLAUDE.md`              |
| `src/Umbraco.Cms.Api.Management/`         | `src/Umbraco.Cms.Api.Management/CLAUDE.md`         |
| `src/Umbraco.Cms.Api.Delivery/`           | `src/Umbraco.Cms.Api.Delivery/CLAUDE.md`           |
| `src/Umbraco.Cms.Api.Common/`             | `src/Umbraco.Cms.Api.Common/CLAUDE.md`             |
| `src/Umbraco.Cms.Persistence.EFCore/`     | `src/Umbraco.Cms.Persistence.EFCore/CLAUDE.md`     |
| `src/Umbraco.PublishedCache.HybridCache/` | `src/Umbraco.PublishedCache.HybridCache/CLAUDE.md` |
| `src/Umbraco.Examine.Lucene/`             | `src/Umbraco.Examine.Lucene/CLAUDE.md`             |
| (other `src/*` projects)                  | Check if a CLAUDE.md exists in that project root   |

**Only load each CLAUDE.md once**, even if multiple files changed in the same project. There are 23 CLAUDE.md files across the repo — only load those relevant to the PR's changed files.

### 4. Gather Changed Files

#### 4a. Collect file list, stats, and diff

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

#### 4b. Filter out noise files

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

#### 4c. Read file context selectively

**Do NOT read every changed file in full.** The diff is the primary review source. Read file content only when you need structural context that the diff does not provide.

**Partial header reads** — for every reviewable source file, read just the top of the file to capture `using`/`import` statements and class declarations (needed for dependency-flow checks in step 5):

- `.cs` files: Read first 50 lines
- `.ts` files: Read first 30 lines

Use the `Read` tool with the `limit` parameter. Run these in parallel for efficiency.

**Full file reads** — only when the diff reveals one of these situations:

- **Public constructor changed or added** — need to verify the obsolete constructor pattern (old constructor must delegate to new)
- **Interface method added** — need to verify default implementation is present
- **Class inherits from a base class that also changed in this PR** — need to understand the inheritance chain
- **Complex control flow change** (e.g., scope usage, transaction handling) — need surrounding method context. Use `Read` with `offset`/`limit` targeting the relevant method rather than the whole file.
- **File is a new addition** (not in the target branch) — read the full file for proper line-number context

**For files over 1500 lines**, never read in full. Use targeted `offset`/`limit` reads around the regions identified from the diff hunks.

#### 4d. Track file counts

Keep track of these numbers for the review output in step 8: total changed files, noise files skipped, reviewable files, files read in full, and files read header-only. Also record: distinct production layers touched, distinct project directories, and total lines changed — these feed step 4e.

#### 4e. Assess PR complexity

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

Store the triggered dimensions and their suggestions for use in step 8.

#### 4f. Classify PR scope

Classify the PR to determine which review steps are relevant:

| Classification  | Condition                                                                       | Effect                                                                          |
| --------------- | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| **Docs-only**   | All reviewable files are `.md`                                                  | Skip steps 5 and 6; step 7 reviews intent and readability only                  |
| **Test-only**   | All reviewable files are in `tests/`                                            | Skip steps 5 and 6; step 7 reviews intent, code quality, and test coverage only |
| **Config-only** | All reviewable files are `.csproj`, `.props`, `.json` config, or CI/build files | Skip step 5; step 6 checks dependency version changes only                      |
| **Standard**    | Anything else                                                                   | No skips — run all steps                                                        |

### 5. Impact Analysis

**Skip this step if PR scope is docs-only, test-only, or config-only.**

Follow the detailed procedure in `references/impact-analysis.md` (relative to this skill file).

In summary:

1. **Extract changed public symbols** — scan the diff for changes to `public` or `protected` interfaces, classes, methods, properties, and type exports
2. **Search for consumers** — use Grep to find usages of changed types/methods in `src/`, excluding the changed file itself. Use `head_limit: 20` to avoid overwhelming results
3. **Check dependency flow** — verify changes respect the dependency direction: `Core <- Infrastructure <- Web/APIs`, never backwards
4. **Flag cross-project risks** — if a change in one project could affect consumers in another project

### 6. Breaking Changes Check

**Skip this step if PR scope is docs-only or test-only. If config-only, only check for dependency version changes that could break consumers.**

Follow the detailed procedure in `references/breaking-changes.md` (relative to this skill file).

In summary:

1. **Read `version.json`** to determine the current major version
2. **Backend (.NET)**: Check for public API surface changes without proper obsolete patterns (constructor, method, interface patterns)
3. **Frontend (TypeScript)**: Check for removed exports, changed custom element APIs, manifest/extension system changes
4. **Validate obsolete attributes** have correct format: `[Obsolete("... Scheduled for removal in Umbraco {current+2}.")]`
5. **Verify internal callers** are updated to use new APIs (no internal code should reference obsolete members)

### 7. Review Against All Criteria

**If PR scope is docs-only:** review only for intent and readability. **If test-only:** review only for intent, code quality, and test coverage. **Otherwise:** review all criteria below.

Analyze each changed file against:

- **Intent**: Does the change accomplish what the commits describe? Are there unintended side effects?
- **Impact**: What consumers are affected? Does this change ripple through the architecture? (from step 5)
- **Breaking changes**: Public API surface changes without proper obsolete patterns? (from step 6)
- **Architecture compliance**: Correct dependency direction, layer separation, pattern usage (from CLAUDE.md files)
- **Umbraco patterns**: Notification pattern (not C# events), Composer pattern (DI registration), Scoping with `Complete()`, Attempt pattern for operation results
- **Code quality**: Per coding preferences in `references/coding-preferences.md`
- **Test coverage**: New public methods have tests? Test files included in the PR?
- **Security**: Authorization checks, no hardcoded secrets, OWASP Top 10 baseline

### 8. Output Structured Review

Present the review in this exact format:

```markdown
## PR Review

**Target:** `{target_branch}` · **Based on commit:** `{head_sha}` · **Files:** {total} changed, {skipped} skipped, {reviewed} reviewed ({full_read} full, {header_only} diff + header-only)
[If step 4f classification is not "Standard", append: · **Classified as:** {classification}]

[1–2 sentences: what this PR accomplishes , keep it as short as possible, only highlight the primary essence.]

- **Modified public API:** {changed existing interfaces/types/classes/methods}
  [Omit bullet if none]
- **Affected implementations (outside this PR):** {interfaces/types/classes/methods using modified public API}
  [Omit bullet if none]
- **Breaking changes:** {violations with specifics}
  [Omit bullet if none]
- **Other changes:** {changes not listed above that an Umbraco user, plugin developer, or API consumer would notice — e.g., behavior changes, default value changes, error message changes, new configuration options, removed functionality. Exclude internal renames, formatting, and private implementation details.}
  [Omit bullet if none]

[If step 4e triggered any dimensions, insert this block. Omit entirely if nothing triggered:]

> [!NOTE]
> **Complexity advisory** — This PR may benefit from splitting.
>
> - **{Dimension}:** {Explanation and concrete split suggestion from step 4e}
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

- Be specific — always reference file and line number
- Explain WHY something is an issue, not just WHAT
- Provide concrete fix suggestions, including code snippets when helpful
- Keep it constructive — the goal is to help, not gatekeep
- Don't repeat the same finding for every occurrence — mention it once and note "same pattern in {other files}"
- Focus on substantive issues, not trivial formatting. Do not flag cosmetic differences in HTML templates that have no rendering impact (e.g., whitespace in HTML is collapsed by the browser).
- For breaking changes, reference the specific pattern from the CLAUDE.md that should be applied

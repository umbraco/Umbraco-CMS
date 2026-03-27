# PR Complexity Assessment

Evaluate whether the PR's scope suggests it should be split. This assessment is **informational only** — it never blocks or shortens the review.

## Exemptions — skip Size, Layer spread, and Mixed intent if ANY hold

- More than 80% of reviewable files are `.md` documentation
- All reviewable production files reside in a single project directory
- The PR is purely test additions/modifications (no production file changes)
- The PR is primarily a dependency bump (`Directory.Packages.props` or `package.json` are the main changes)
- The majority of changes are renames (`git diff {target}...HEAD --diff-filter=R --name-only`)

**"Formatting mixed with logic" is always checked regardless of exemptions** — it's about per-file reviewability, not PR scope.

## Trigger rules

Flag each dimension whose condition is met (respecting exemptions above):

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

## Split suggestions

For each triggered dimension, prepare a concrete suggestion:

- **Layer spread (Core + higher layers):** "Consider a first PR adding Core contracts and Infrastructure implementations, followed by a second PR for API/Web/Frontend consumers."
- **Layer spread (Backend + Frontend):** "Consider splitting backend (.NET) and frontend (TypeScript) changes into separate PRs."
- **Mixed intent (feature + refactor):** "Consider extracting the refactoring into a preparatory PR, then building the new feature on top."
- **Mixed intent (bugfix + feature):** "The bug fix could be merged independently for faster delivery, with the new feature as a follow-up."
- **Size (many projects):** "If changes in {projectA} and {projectB} are independently functional, they could be separate PRs."
- **Formatting mixed with logic:** "File(s) {list} contain significant formatting changes mixed with logic changes. Consider a separate formatting-only commit or PR to keep the functional diff reviewable."

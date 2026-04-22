# PR Complexity Assessment

Evaluate whether the PR's scope suggests it should be split. This assessment is **informational only** — it never blocks or shortens the review.

## Always check: Formatting mixed with logic

This check applies to every PR regardless of size or scope.

Run both commands and compare per-file line counts:
```bash
git diff {target}...HEAD --stat
git diff {target}...HEAD --stat --ignore-all-space
```

For any file where the whitespace-ignored diff is less than **half** the full diff size (and the full diff is over 50 lines), that file has significant formatting changes mixed with logic. Flag it with a split suggestion: "File(s) {list} contain significant formatting changes mixed with logic. Consider a separate formatting-only commit or PR to keep the functional diff reviewable."

## Multi-project scope check

Skip this section entirely if ALL production files reside in a single project directory or if the PR is docs-only, test-only, dependency-bump-only, or rename-only.

Otherwise, flag any dimension that applies:

| Dimension | Condition | Suggestion |
|---|---|---|
| **Size** | 30+ files OR 1500+ lines, spanning 2+ projects | "If changes in {projectA} and {projectB} are independently functional, they could be separate PRs." |
| **Layer spread** | 3+ layers touched (Core/Infrastructure/Web/API/Frontend), 10+ files | "Consider splitting by layer — e.g., Core+Infrastructure first, then API/Frontend consumers." |
| **Mixed intent** | 2+ intent categories (new feature, bugfix, refactor, dependency update) with 15+ files or 3+ projects | "Consider extracting the {secondary intent} into a separate PR." |

Intent categories — detect from diff characteristics, not commit messages:
- **New feature**: new files or new `public`/`export` declarations
- **Bug fix**: small targeted edits, no new files (don't co-flag with new feature)
- **Refactor**: file renames, symbols moved but logic unchanged
- **Dependency update**: changes to `.csproj`, `Directory.Packages.props`, `package.json`

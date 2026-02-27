# PR Classification Heuristics

## BROWSER_TESTABLE

A PR is browser-testable when it changes something a user can see or interact with in the Umbraco backoffice.

### Strong Signals (any one of these is sufficient)
- PR body contains a test plan section with checkbox items (`- [ ]`) that include UI action words: "Go to", "Click", "Verify", "Navigate", "Open", "Select", "Check", "Ensure"
- Changed files include `.ts` or `.js` files in `src/Umbraco.Web.UI.Client/`
- Labels include `area/frontend`, `area/backoffice`, `area/content`, `area/media`, `area/settings`

### Supporting Signals (strengthen the case)
- PR title mentions UI elements: "modal", "picker", "button", "dialog", "form", "field", "editor", "tree", "menu"
- Changed files include Razor views (`.cshtml`), controller files
- PR body describes visual or interaction changes

### Common Types
- **Bugfixes with UI impact**: Form validation fixes, picker behavior, modal sizing, navigation issues
- **Feature additions**: New buttons, modals, pickers, workspace views, tree items
- **UI improvements**: Layout changes, responsive fixes, accessibility improvements

## API_TESTABLE

A PR is API-testable when it changes backend behavior that's verifiable through HTTP requests to the Management API or Delivery API, but doesn't have a UI test plan.

### Signals
- Changes to files in `Umbraco.Cms.Api.Management/` or `Umbraco.Cms.Api.Delivery/`
- New or modified API endpoints (controller actions)
- Changes to service behavior that affects API responses
- Webhook payload changes
- Validation rule changes that return different API error responses

### Common Types
- **New API endpoints**: New routes, new response models
- **Behavioral changes**: Different validation rules, different response payloads
- **Performance optimizations**: Same behavior, different implementation (partially testable - can verify correctness)

## NOT_TESTABLE

A PR has no user-facing behavioral change that automated testing can verify.

### Automatic NOT_TESTABLE (these are always not testable)
- **Dependency bumps**: Title starts with "Bump", labels include `dependencies`
- **Draft PRs**: `isDraft: true`
- **WIP PRs**: Title or body contains "WIP", "work in progress", "do not merge"

### Likely NOT_TESTABLE (check the PR body to confirm)
- **Test infrastructure**: All changed files in `tests/` directory
- **Build/CI changes**: Changed files are `.yml`, `.props`, `.targets`, `Directory.Build.*`
- **Documentation only**: Changed files are `.md` files
- **Static assets with no behavioral impact**: Icon changes, font updates
- **Pure refactoring**: PR body says "no behavioral change", "refactoring", "internal restructuring"
- **Template changes**: Files in `templates/` directory

### Edge Cases
- **Performance PRs**: Usually NOT_TESTABLE (the optimization is invisible), but check if the PR body describes a functional regression that was fixed alongside the perf work
- **Auth flow changes**: Technically browser-testable but risky — testing the login system requires being able to log in. Mark as BROWSER_TESTABLE but note the circular dependency risk.
- **Multi-server PRs**: Require distributed infrastructure, mark as NOT_TESTABLE

## Tiebreaking

When a PR has mixed signals:
1. If there's an explicit test plan with UI steps → BROWSER_TESTABLE (the PR author intended it to be tested this way)
2. If changed files are mostly frontend → BROWSER_TESTABLE
3. If changed files are mostly backend with API changes → API_TESTABLE
4. If unsure → check the PR body for any behavioral expectations. No behavioral expectations → NOT_TESTABLE

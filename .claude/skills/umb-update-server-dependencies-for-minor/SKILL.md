---
name: umb-update-server-dependencies-for-minor
description: Update all server-side (NuGet) dependencies to their latest minor/patch versions for the current branch's version line, then open a PR. Runs dotnet-outdated, bumps minors and patches only (never majors; never moves a stable package onto a pre-release), syncs the template projects, builds and runs unit tests, commits, and creates a PR. Use when asked to update/bump backend/server/NuGet/.NET dependencies for a release (e.g. "update server dependencies for 17.6", "bump NuGet packages to latest minors").
argument-hint: [base-branch] (optional; defaults to the current branch)
---

# Update Server-Side Dependencies (minor & patch) - Umbraco CMS

Bumps every NuGet dependency that is behind to its latest available **minor or patch** version for a single version line (the current branch), then builds, tests, commits and opens a PR.

**Scope: one version line at a time.** This skill operates on the branch that is currently checked out (or the branch given as `$ARGUMENTS`). To update multiple release lines (e.g. `v17/dev` and `main`), run it once per line.

**Guardrails (non-negotiable):**
- **Never** update a major version. Only minors and patches where a newer one exists.
- **Never** move a stable package onto a pre-release. Packages already on a pre-release *may* be updated to a newer pre-release — this is what `--pre-release Auto` does.
- **Never** update packages explicitly held back for compatibility (e.g. marked "HOLD"/"do not bump" in `Directory.Packages.props`). `TODO` comments there may also mark transitive security pins, which are in-scope to bump if needed (see step 3).

## Prerequisites

- `dotnet-outdated-tool` installed globally: `dotnet tool install --global dotnet-outdated-tool` (verify with `dotnet outdated --version`).
- `gh` CLI authenticated (`gh auth status`).

## Where versions live

| File | Scope |
|------|-------|
| `Directory.Packages.props` (root) | Production packages + `GlobalPackageReference` build tooling + transitive security pins |
| `tests/Directory.Packages.props` | Test-only packages (inherits the root file) |
| `src/Umbraco.Web.UI/Umbraco.Web.UI.csproj` | Inline versions (opts out of central management) |
| `templates/UmbracoExtension/Umbraco.Extension.csproj` and its `Directory.Packages.props` | **Not in `umbraco.sln`** — dotnet-outdated will NOT touch these; sync them by hand (step 4) |

## Instructions

### 1. Establish the branch

Read `version.json` and take the major version (e.g. `18` from `18.1.0-rc`) — call it `{major}`.

If the current branch is a long-lived line (`main`, `v{major}/dev`, a release branch) rather than a task branch, create and switch to a new branch:

```bash
git switch -c v{major}/task/update-backend-dependencies
```

(If `$ARGUMENTS` names a base branch, branch from that instead.) If you are already on a suitable task branch, stay on it. Confirm the working tree is clean before starting.

### 2. Discover what is outdated

Run dotnet-outdated restricted to within the current major. `--version-lock Major` keeps updates inside the current major version; `--pre-release Auto` (the default) offers a pre-release upgrade **only when the package is already on a pre-release** — so a stable package never jumps to a pre-release, while an existing pre-release can move to a newer pre-release:

```bash
dotnet outdated --version-lock Major --pre-release Auto umbraco.sln
```

Review the report. Everything listed is a legitimate minor/patch (or same-track pre-release) candidate. Note anything you intend to skip, and verify afterwards (via `git diff`) that no intentionally pinned/held versions were changed; revert any that were.

### 3. Apply the updates

Apply the same filter with `-u`:

```bash
dotnet outdated --version-lock Major --pre-release Auto -u umbraco.sln
```

`dotnet-outdated` restores per-package and may print `Failed to restore project after upgrading!` for the test projects — this is a known quirk with the multi-level `tests/Directory.Packages.props` merge; the version writes still land. Verify with `git diff` afterwards.

This updates the two `Directory.Packages.props` files, the inline `Umbraco.Web.UI.csproj` versions, and any in-solution project. It also handles `GlobalPackageReference` entries (e.g. `Nerdbank.GitVersioning`).

You can also update transitive **security pins** if a new minor or patch is available.

### 4. Sync the template projects by hand

`templates/UmbracoExtension/` is not part of `umbraco.sln`, so its versions are not updated automatically. For any package that (a) was bumped in the root `Directory.Packages.props` **and** (b) appears in the template, update it to match:

```bash
grep -rnE "Version=\"[0-9]" templates/UmbracoExtension/
```

Typically this is `Microsoft.AspNetCore.OpenApi` and/or `Swashbuckle.AspNetCore*` in both `templates/UmbracoExtension/Umbraco.Extension.csproj` and `templates/UmbracoExtension/Directory.Packages.props`. Keep them aligned with the root file.

### 5. Build and run unit tests

Build the full solution in Release, skipping the frontend/npm steps (these are gated for CI/non-VS builds):

```bash
dotnet build umbraco.sln -c Release -p:UmbracoBuild=true
```

Then run the unit test suite:

```bash
dotnet test tests/Umbraco.Tests.UnitTests/Umbraco.Tests.UnitTests.csproj -c Release -p:UmbracoBuild=true --no-build
```

Both build and tests must be green before continuing.

Confirm the bumps did not leave (or introduce) a known-vulnerable transitive version:

```bash
dotnet list umbraco.sln package --vulnerable --include-transitive
```

Pre-existing, unrelated advisories are out of scope for a routine bump — but if a bump you made is the cause, resolve it (raise the pin, or add an inline reference for `Umbraco.Web.UI`).

### 6. Review and commit

Confirm `git status` shows only the intended package files (the two `Directory.Packages.props`, `Umbraco.Web.UI.csproj`, and any template files) — no stray build output, logs, or the schema placeholder. Stage those files explicitly (do not `git add -A`) and commit:

```
Update NuGet packages to latest minor and patch versions

Bump all NuGet dependencies that were behind to their latest available
minor or patch release, as reported by dotnet-outdated. No major-version
updates are included.
```

### 7. Push and open the PR

```bash
git push -u origin <branch>
```

Generate the PR body from the actual `git diff` of the package files (do not invent versions). Use tables grouping the changes, and keep the **Testing** section to a single line — CI is the source of truth, do not paste build/test counts.

Fill in the template below and **write it to a temp file** (e.g. `/tmp/pr-body.md`), then create the PR against the line's dev branch (`v{major}/dev` for a v-line, `main` for the current major):

```bash
gh pr create --base <dev-branch> --head <branch> \
  --title "Dependencies: Update NuGet packages to latest minor and patch versions" \
  --body-file /tmp/pr-body.md
```

**PR body template:**

```markdown
## Description

Routine dependency maintenance for the **{version}** release. Updates all NuGet packages that were behind to their latest available **minor or patch** version, as reported by `dotnet-outdated`. No major-version updates are included.

### Production packages (`Directory.Packages.props`)

| Package | From | To |
|---|---|---|
| … | … | … |

### Test packages (`tests/Directory.Packages.props`)

| Package | From | To |
|---|---|---|
| … | … | … |

### Inline / template versions

- `src/Umbraco.Web.UI/Umbraco.Web.UI.csproj`: …
- `templates/UmbracoExtension`: …

### Deliberately left unchanged

- Any package where only a **major** update is available (out of scope for this PR).
- Intentionally-held packages carrying a `HOLD`/`do not bump` comment (e.g. `Umbraco.Code`).

## Testing

Solution should build and CI checks pass.
```

### 8. Report

Output the PR URL, the branch name, and a short summary of what was bumped (and anything deliberately skipped or any pin that had to be raised).

## Notes

- If `dotnet-outdated` reports nothing outdated, say so and stop — do not create an empty PR.
- No `OpenApi.json` regeneration is needed for pure dependency bumps (no Management API controllers/models change).

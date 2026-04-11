---
name: umb-bump-version
description: Bump the Umbraco CMS version across all required files. Use when the user asks to bump, update, or set the version number — e.g., "bump version to 17.3.4", "set version to 18.0.0-rc", "update version". Accepts the target version as an argument.
argument-hint: <version> (e.g., 17.3.4, 18.0.0-rc)
---

# Bump Version - Umbraco CMS

Updates the Umbraco CMS version string across all files that track it.

**Do NOT use AskUserQuestion if a version argument is provided. Only ask if `$ARGUMENTS` is empty or cannot be parsed as a version.**

## Arguments

- `$ARGUMENTS` - Required: the target version string (e.g., `17.3.4`, `18.0.0-rc`)

## Files to Update

The following 5 files must be updated with the new version:

| # | File | Field |
|---|------|-------|
| 1 | `version.json` | `"version"` |
| 2 | `src/Umbraco.Web.UI.Client/package.json` | `"version"` |
| 3 | `src/Umbraco.Web.UI.Client/package-lock.json` | top-level `"version"` AND `packages[""].version` |
| 4 | `tests/Umbraco.Tests.AcceptanceTest/package.json` | `"version"` |
| 5 | `tests/Umbraco.Tests.AcceptanceTest/package-lock.json` | top-level `"version"` AND `packages[""].version` |

## Instructions

### 1. Parse and Validate the Version

Extract the version from `$ARGUMENTS`. It must be a valid semver-like string (e.g., `17.3.4`, `18.0.0-rc`, `17.4.0-preview.1`). If no version is provided or it cannot be parsed, ask the user for the target version.

### 2. Read the Current Version

Read `version.json` and extract the current `"version"` value. If the current version already equals the target version, report that the version is already set and stop — do not edit, stage, or commit anything.

Otherwise, display both versions:

```
Bumping version: {current} -> {target}
```

### 3. Update All Files

Update each of the 5 files listed above, replacing the old version with the new version. For each file:

- **`version.json`**: Replace the `"version"` value.
- **`package.json` files**: Replace the `"version"` value (near the top of the file).
- **`package-lock.json` files**: Replace BOTH the top-level `"version"` value AND the `"version"` inside the `"packages": { "": { ... } }` block. These are always in the first ~10 lines of the file.

Use targeted edits — do NOT rewrite entire files. Be precise to avoid changing version strings in dependency entries.

### 4. Verify

After all edits, grep for the target version value across the 5 files to confirm all updates landed correctly:

```bash
grep -n "\"version\": \"{version}\"" version.json src/Umbraco.Web.UI.Client/package.json src/Umbraco.Web.UI.Client/package-lock.json tests/Umbraco.Tests.AcceptanceTest/package.json tests/Umbraco.Tests.AcceptanceTest/package-lock.json
```

Expect exactly 7 matches (one per `package.json` and `version.json`, two per `package-lock.json`).

### 5. Stage and Commit

Stage only the 5 changed files:

```bash
git add version.json src/Umbraco.Web.UI.Client/package.json src/Umbraco.Web.UI.Client/package-lock.json tests/Umbraco.Tests.AcceptanceTest/package.json tests/Umbraco.Tests.AcceptanceTest/package-lock.json
```

Then commit with the message `Bump version to {version}.` — replacing `{version}` with the target version:

```bash
git commit -m "Bump version to {version}."
```

### 6. Report

Output a summary:

```
Version bumped to {version} in:
- version.json
- src/Umbraco.Web.UI.Client/package.json
- src/Umbraco.Web.UI.Client/package-lock.json
- tests/Umbraco.Tests.AcceptanceTest/package.json
- tests/Umbraco.Tests.AcceptanceTest/package-lock.json

Changes staged and committed.
```

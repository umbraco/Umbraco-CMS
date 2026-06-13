---
name: umb-release-notes
description: Improve a set of auto-generated GitHub release notes for an Umbraco CMS release. Cross-checks the notes against every PR carrying the release label, adds any that are missing, re-files every PR under the most appropriate category, and strips purely-internal entries. Use whenever the user asks to tidy up, improve, complete, or recategorize release notes for a given version, or mentions a release-notes text file plus a version number.
argument-hint: <version> <path-to-generated-notes-file>
---

# Umbraco CMS - Improve Release Notes

Takes a file of auto-generated GitHub release notes and produces an improved version that:

1. **Is complete** — every merged PR carrying the `release/<version>` label appears.
2. **Is well-categorized** — every PR sits under the most appropriate heading.
3. **Is free of noise** — purely-internal entries of no value to a reader are removed.

The result is written to a **new** file alongside the input, so the user can diff the two.

**Run autonomously.** Do not use `AskUserQuestion`. Make the categorization calls yourself using the rules below; if a handful are genuinely borderline, place them anyway and note the borderline ones in your closing summary so the user can override.

## Arguments

`$ARGUMENTS` contains two values:

1. **Version** — e.g. `17.5.0`, `18.1.0`. The GitHub label to search is `release/<version>` (so version `17.5.0` → label `release/17.5.0`).
2. **Input file path** — full path to the text file holding the auto-generated notes (e.g. `C:\Temp\release-17.5.0-rc.md`).

If either is missing, ask the user once for the missing value, then proceed.

## Prerequisites

Run `gh auth status`. If it fails, tell the user to authenticate `gh` (e.g. `gh auth login`) and stop — the skill needs the GitHub CLI to query PRs. The repo is always `umbraco/Umbraco-CMS`.

## Procedure

### 1. Read the input notes

Read the input file. Note its structure — it is GitHub's generated format:

- A leading HTML comment (`<!-- Release notes generated ... -->`).
- A `## What's Changed` heading followed by `### <emoji> <Category>` sub-headings, each with `* <title> by @<author> in <url>` bullets.
- A trailing `## New Contributors` section and a `**Full Changelog**: ...` line.

Extract the set of PR numbers already present (parse the `/pull/<number>` from each bullet). Preserve each existing bullet's **exact text** (title, author, URL) when you re-emit it — only its category placement may change.

### 2. Fetch every labelled PR

```bash
gh pr list --repo umbraco/Umbraco-CMS --label "release/<version>" --state closed --limit 1000 \
  --json number,title,author,labels,mergedAt \
  --jq '.[] | select(.mergedAt != null) | "\(.number)\t\(.author.login)\t\([.labels[].name] | join(", "))\t\(.title)"' | sort -n
```

This is the authoritative list of what the release *should* contain. Each row gives number, author, labels, title.

**Guard against silent truncation.** `gh pr list` caps at `--limit` without warning, so a large release could drop the overflow and the skill would still look "complete". Count the returned rows and compare against the limit:

```bash
gh pr list --repo umbraco/Umbraco-CMS --label "release/<version>" --state closed --limit 1000 --json number --jq 'length'
```

If this equals 1000, the limit was hit — raise `--limit` and re-fetch before continuing. Do **not** proceed on a truncated list.

### 3. Reconcile

- **Missing labelled PRs** (labelled but not in the input file): these must be **added**. Build a bullet as `* <title> by @<author> in https://github.com/umbraco/Umbraco-CMS/pull/<number>`.
  - **Normalize the author handle.** `gh`'s `.author.login` returns bot accounts as `app/dependabot`, whereas GitHub's own generated notes render them as `@dependabot[bot]`. When the author login starts with `app/` (or is otherwise a bot), emit the `[bot]` form — e.g. `app/dependabot` → `@dependabot[bot]` — so added bullets match the existing ones.
- **PRs in the file but not labelled**: keep them. The generated notes span a commit range (see the `Full Changelog` compare link), so they legitimately include backports / earlier-version PRs that lack the current label. For any of these you need to categorize, fetch its labels with `gh pr view <number> --repo umbraco/Umbraco-CMS --json number,title,labels --jq '...'`.

Do **not** invent or alter the `New Contributors` section — carry it over verbatim. You cannot reliably recompute first-time contributors, so leave it as the generator produced it (mention this in the summary).

### 4. Categorize every PR

Use exactly these headings, in this order. Omit any heading that ends up with no entries.

| Heading | What goes here | Primary signal |
|---|---|---|
| `### 🙌 Notable Changes` | **Do not add or remove.** Label-driven. | label `category/notable` |
| `### 💥 Breaking Changes` | **Do not add or remove.** Label-driven. | label `category/breaking` |
| `### 📦 Dependencies` | Dependency bumps | label `dependencies`; or dependabot author |
| `### 🚀 New Features` | New user- or developer-facing capability | label `type/feature` / `category/feature`; or title introduces/adds a genuinely new capability |
| `### 🚤 Performance` | Performance improvements | label `category/performance`; or `Performance:` title prefix |
| `### 🌈 Accessibility Improvements` | A11y improvements (labels, contrast, keyboard) | label `category/accessibility` / `accessibility`; or clear a11y intent (e.g. "improve contrast", "missing labels") |
| `### 🐛 Bug Fixes` | Fixes to broken/incorrect behaviour | default for anything describing a fix |
| `### 🧪 Testing` | Test additions/changes only | label `category/test-automation` / `area/test`; or `E2E`/`QA`/"acceptance tests"/"unit test coverage"/"add tests" titles |
| `### 🛡️ Code Quality, Documentation and Refactoring` | Refactors, deprecations, API tidy-ups, XML/MD documentation, knowledge-base (`MD`) updates | label `category/refactor`; or titles about refactoring, deprecating, renaming, documenting, constants extraction, MD/CLAUDE.md content |
| `### 🧑‍💻 Developer Experience` | Things that improve the experience of developers building on or contributing to Umbraco — dev tooling, build/watch ergonomics, test mocks/harnesses, backoffice dev utilities | `Developer Experience` title prefix; dev tooling; mock/harness changes |

**Rules:**

- **Notable and Breaking are off-limits** — never move PRs in or out of them; they are driven purely by their labels and the generator already placed them correctly.
- Label signals beat title wording, except a `Performance:`/`Developer Experience:` title prefix is decisive for its section.
- A PR with both `type/feature` and `category/refactor` whose title clearly describes a refactor (e.g. "swap relative imports", "re-export type") belongs under Code Quality, not New Features.
- "Add ... tests"/"unit test coverage" → Testing, even if it also touches docs. If a PR adds XML documentation *and* tests, lead with where the title's emphasis lies (documentation → Code Quality; test coverage → Testing).
- When a PR is genuinely 50/50, pick the more reader-useful heading and list it in your closing summary as borderline.

### 5. Remove purely-internal noise

Drop entries that have **no value to anyone reading release notes** — pure repository plumbing with no shipped impact. Examples:

- Branch/merge maintenance ("Fix main branch after merge issue").
- CI/pipeline fixes that don't change the product.
- Reverts of changes that never shipped in a release.

**Keep** anything that ships in the product or genuinely helps developers building on Umbraco — that includes documentation/MD updates, dev tooling, and test mocks (those go to Code Quality or Developer Experience, they are *not* noise). When unsure whether something is noise, keep it and flag it in the summary rather than silently dropping it. List every removal in your closing summary.

### 6. Write the output

Write to a new file in the **same folder** as the input, named by appending ` - with updates` before the extension:

- Input `C:\Temp\release-17.5.0-rc.md` → Output `C:\Temp\release-17.5.0-rc - with updates.md`

Preserve the leading HTML comment, the `## What's Changed` heading, the `## New Contributors` section, and the `**Full Changelog**` line exactly. Only the `### <category>` groupings and their bullets change.

### 7. Report

Give a concise summary:

- Count of PRs added (with their numbers), and which categories they landed in.
- Notable recategorizations (PRs moved out of the catch-all Bug Fixes into Features/Performance/Testing/etc.).
- Every entry removed, with the one-line reason.
- Any borderline calls the user may want to override.
- The output file path.

## Verification

Before reporting done, confirm:

- Every PR number from step 2 is present in the output (except any you deliberately removed in step 5 — and those must be in the removal list).
- No PR appears under more than one heading.
- Notable and Breaking sections are byte-for-byte unchanged from the input.
- The header comment, New Contributors, and Full Changelog lines are intact.

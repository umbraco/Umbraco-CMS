---
name: umb-merge-up
description: Merge one Umbraco CMS branch up into another (e.g. v17/dev → v18/dev, release/18.4.0 → release/19.0, release/19.0 → main) through a merge branch and PR. Creates `v{N}/task/merge-from-{source}` from the destination, merges the source, resolves every conflict against the destination's structure (asking for guidance where intent is unclear), sweeps for the silent breakages clean merges cause, verifies builds and tests, pushes and opens a PR describing the PRs merged up and the reconciliations made — then stops for CI and review. When the user says to continue, merges the branch into the destination locally, pushes, and confirms the PR closed as merged. Use when the user asks to merge up, merge a branch into another, or do a merge-up/merge-from between Umbraco release or dev branches. Does NOT apply to retargeting a contributor's PR (use umb-retarget-pr) or cherry-picking a single fix.
argument-hint: <source-branch> <destination-branch> (e.g., v17/dev v18/dev)
---

# Merge Up - Umbraco CMS

Changes flow up through the maintained lines: release branch → its dev branch, and older line → newer line (17 → 18 → 19/main). This skill does one such merge through a reviewable PR so CI runs on the exact result before it lands.

Two phases, separated by a hard stop:

1. **Prepare** (steps 1–8): build the merge branch, resolve, verify, push, open the PR, report. **Then stop.** The user monitors CI, reviews the write-up, may build and run the site locally, and tells you to continue.
2. **Land** (step 9): only after the user says so.

A clean auto-merge is **not** evidence of a correct merge. Most of the real work is in step 5, finding what git merged "cleanly" into something broken.

## Arguments

- `$ARGUMENTS`: the source branch, then the destination branch, e.g. `v17/dev v18/dev`, `release/18.4.0 release/19.0`, `release/19.0 main`. Strip any `origin/` prefix. If either is missing, ask.

**Merge branch name:** `v{D}/task/merge-from-{S}`, where:
- `{D}` is the major from the destination's `version.json` (`git show origin/{dest}:version.json`), so `main` resolves to its current major;
- `{S}` is the source's major for a dev branch (`v17/dev` → `17`), or the version for a release branch (`release/18.4.0` → `18.4.0`, `release/19.0` → `19.0`).

Examples: `v17/dev` → `v18/dev` gives `v18/task/merge-from-17`; `release/18.4.0` → `release/19.0` gives `v19/task/merge-from-18.4.0`; `release/19.0` → `main` gives `v19/task/merge-from-19.0`.

## Required reading

- [`../_shared/major-version-landmarks.md`](../_shared/major-version-landmarks.md): structural changes between majors, known renames, and files each line keeps its own copy of. Read it before resolving anything in a cross-major merge. It tells you where the source's changes now live on the destination.
- Root `CLAUDE.md` §6 (obsoletion rules) and §11 (verification discipline).
- The `umb-update-openapi` skill, if the merge touches the Management API.

## Phase 1: Prepare

### 1. Preflight

```bash
gh auth status
git status --short          # must be clean: this skill works in the current checkout
git fetch origin {source} {dest}
git rev-list --count origin/{dest}..origin/{source}   # 0 → nothing to merge; say so and stop
```

If the working tree isn't clean, stop and ask. Don't stash the user's work.

If a local or remote branch with the merge branch's name exists, delete it. But first check for an open PR from it (`gh pr list --head {merge-branch} --state open`). If there is one, that's probably someone's in-progress merge, so ask before deleting: removing the remote branch closes their PR.

```bash
git branch -D {merge-branch} 2>/dev/null
git push origin --delete {merge-branch} 2>/dev/null
```

### 2. Inventory what's coming up

```bash
git log --no-merges --format='%h %s' origin/{dest}..origin/{source}
```

Squash-merged PRs appear as `Title (#NNNNN)`. Collect them for the PR description, and leave out commits that are themselves merges or merge-ups (`Merge ...`, `Merge-up: ...`). If a commit has no PR number, list it by subject. For each PR, skim what it changes (`gh pr view <n> --json title,files`): the files tell you which landmarks it may cross.

### 3. Merge

```bash
git switch -c {merge-branch} origin/{dest}
git merge --no-ff --no-commit origin/{source}
git diff --name-only --diff-filter=U     # conflicted files
```

### 4. Resolve conflicts

For each conflicted file, understand both sides before editing:

```bash
git diff $(git merge-base HEAD MERGE_HEAD) MERGE_HEAD -- <file>   # what the source changed
git diff $(git merge-base HEAD MERGE_HEAD) HEAD -- <file>         # what the destination changed
```

Resolve by **applying the source's intent to the destination's code**, never by picking a side wholesale. Check the landmarks file for where the code now lives (for example, a v17 `ContentService` change may belong in v18's `PublishableContentServiceBase`).

Recurring patterns:

- **Obsolete constructor or method pairs** (§6.1/§6.2): keep the destination's signatures and parameter types, and add the source's new parameter to both. When merging into the line that removed the members obsoleted for its version, drop them instead, along with any stray `: this(...)` arguments left over from a deleted constructor. Re-check the usings the remaining obsolete constructor needs.
- **"Keep both"** (both sides added something adjacent): keep both, in an order that makes sense. In client manifests and test files, re-read the result: a "keep both" can land inside the wrong object or `describe` suite. A test placed in a suite with a different `afterEach` can break the whole test run even though every test passes.
- **Migrations**: see the landmarks file's merge-up section (plan placement, re-runs for sites already past that point, no back-porting of newer states).
- **Files each line keeps its own copy of** (landmarks file): take the destination's version, or regenerate.

**Ask for guidance** (`AskUserQuestion`, one focused question with the options you see) when the right resolution depends on product intent rather than code: both sides changed behaviour differently, a source fix may not apply to the destination's redesigned feature, or porting it would need new design. Say what each side does and what you'd recommend. Don't guess at behaviour.

**Mirroring:** when the source changes document-specific code that has an element (v18+) or search-abstraction (v19+) counterpart on the destination, decide whether the counterpart needs the same change. If it does, make it, and record it as a reconciliation. If the change doesn't apply at all on the destination (e.g. it fixed the removed Examine integration), drop it, record why, and check whether the bug exists in the replacement. If it does, flag it to the user as needing its own fix.

### 5. Sweep for silent breakage

Do all of these, even when there were no conflicts:

1. **Completeness.** List files the source changed (`git diff --name-only $(git merge-base HEAD MERGE_HEAD) MERGE_HEAD`). For each one the destination also changed, compare the staged result with both sides. Pay attention to renames and file splits: git records the new file as an add, so the destination's edits to the original file can vanish without a conflict.
2. **Renamed and deleted APIs in clean-merged additions.** Grep the merged diff (`git diff --cached HEAD`) for the "older line" names in the landmarks file's renames table, and for any type the destination deleted. Pay particular attention to tests: an obsolete method still compiles, but a Moq setup on it never matches.
3. **Package and project files.** Check `git diff --cached HEAD --stat | grep -iE 'csproj|props|package.json'`. Look for duplicate `PackageReference`s, resurrected pins, and versions moving backwards. Keep the higher version of any package both sides changed.
4. **Files each line keeps its own copy of.** Confirm every one in the landmarks list is the destination's copy, or has been regenerated.
5. **Generated artefacts.** If the merged changes touch Management API controllers or models, regenerate `OpenApi.json` and the client on this branch (`umb-update-openapi`). If localization keys were added, regenerate the known-keys files.

### 6. Verify

- **Build** the solution, or at least every project touched, plus the test projects. Do a fresh build, and don't pipe the output in a way that hides the exit code. A running local site's file locks (MSB3021) can mask real compile errors, so look for non-lock errors.
- **Run tests** for the areas the merged PRs touched: the unit and integration tests they added or changed, plus any tests around your reconciliations. A test you added for a reconciliation must fail without it.
- **Client** (if client files were merged): `npx tsc --noEmit` from `src/Umbraco.Web.UI.Client`. New mock data files from an older line often lack properties the newer line's models require. Compare the errors against the destination's baseline: some errors may already exist there, and only new ones are yours. Run eslint on the changed files. Run client unit tests for the touched areas when feasible. The browser runner can time out on cold start locally; if it does, say so and leave it to CI.
- Remove unused usings from the C# files you modified (root `CLAUDE.md` §11).

### 7. Commit, push, open the PR

Commit the merge with conflict resolutions and reconciliations included. Keep git's default message (`Merge branch '{source}' into {merge-branch}`), or for a remote-tracking source write it the same way without the `origin/` prefix. Fixes found later (for example from CI) go in follow-up commits with conventional messages.

```bash
git commit --no-edit        # or: git commit -m "Merge branch '{source}' into {merge-branch}"
git push -u origin {merge-branch}
gh pr create --base {dest} --head {merge-branch} --title "<title>" --body-file <scratch file>
```

**Title:** `Merge {source} into {dest}`, optionally followed by the headline areas in parentheses when there are notable reconciliations, e.g. `Merge-up: Merge v17/dev into v18/dev (tree batch endpoints, domain routing from publish status)`.

**Body** (see #24733 for a full example):

```markdown
Merges `{source}` into `{dest}`.

## PRs merged up

- #NNNNN Title
- ...

## V{D} reconciliations

**<Area or PR>**
- What differed on `{dest}`, and what was done (ported to X, mirrored to elements, kept destination's signature, dropped because…).

**Other**
- Smaller items, regenerated files, follow-ups for the user (e.g. a bug that needs its own fix on this line).

## Testing

- What was built and run, with pass counts; client checks and any pre-existing failures; what couldn't be run locally.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

Use the destination's major in the reconciliations heading. If nothing needed reconciling, say "Merged cleanly; no reconciliations needed." Keep the Testing section short.

### 8. Report and stop

Tell the user:
- the PR link and the branch;
- the PRs merged up (count, plus any notable ones);
- the reconciliations made, and any decisions they guided;
- verification results, including what wasn't run;
- anything they should check: areas worth testing on a running site, follow-up fixes needed on this line.

Then stop. Don't land the merge. The user will monitor CI, review, possibly build and run the branch locally, and ask you to continue.

## Phase 2: Land (only when the user says to continue)

### 9. Merge into the destination and push

```bash
gh pr checks {pr}                 # confirm the required checks passed; if not, report and stop
git fetch origin {dest} {merge-branch}
git switch {dest}
git merge --ff-only origin/{dest}  # bring the local destination up to date
git merge {merge-branch}
git push origin {dest}
```

If `origin/{dest}` moved since the merge branch was created, the final `git merge` produces a new merge commit that CI hasn't tested. Say so, and ask whether to merge `{dest}` into the PR branch and re-run CI first, or to go ahead.

If the push is rejected (someone pushed in the meantime), fetch and re-assess. Never force-push a shared branch.

Then verify and clean up:

```bash
gh pr view {pr} --json state,mergedAt     # expect MERGED: GitHub closes the PR when its commits reach the base
git branch -d {merge-branch}
```

If the PR shows as closed but not merged, or is still open, report it rather than closing it by hand. Report the final state: destination pushed at `<sha>`, PR merged, local branch deleted.

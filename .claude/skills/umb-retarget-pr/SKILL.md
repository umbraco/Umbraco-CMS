---
name: umb-retarget-pr
description: Retarget an open Umbraco CMS pull request to a different base branch (typically an older maintained line such as v17/dev, so a fix ships in every supported major), keeping the contributor's commits and authorship. Verifies the contributor's branch can be pushed to, replays the PR's commits onto the destination branch in a throwaway worktree, assesses conflicts and branch differences, proposes any adaptations, and — only after the user confirms — force-pushes the rewritten branch, changes the PR's base and comments to the contributor. Use when the user asks to retarget, rebase or move a PR to another branch, or to get a contributor's fix into an older version by changing its target. Does NOT apply to cherry-picking a merged PR into another branch, or to merge-ups (use umb-merge-up).
argument-hint: <pr-number-or-url> <destination-branch> (e.g., 24736 v17/dev)
---

# Retarget PR - Umbraco CMS

Moves an open PR onto a different base branch so it ends up **exactly as if the contributor had opened it against that branch in the first place**:

- the PR targets the destination branch;
- its diff contains only the PR's own changes;
- every commit keeps its original author, author date, committer and message — only parents (and so SHAs) change;
- the PR itself (author, title, description, discussion) is untouched apart from its base.

The fix then reaches the newer lines through the normal merge-ups (17 → 18 → 19), so no separate cherry-pick PRs are needed.

**Nothing is pushed and the PR is not edited until the user has confirmed the plan (step 6).** Steps 1–5 are read-only as far as GitHub is concerned; all local work happens in a temporary worktree so the user's checkout is untouched.

## Arguments

- `$ARGUMENTS` — the PR number or URL, then the destination branch. Examples: `24736 v17/dev`, `https://github.com/umbraco/Umbraco-CMS/pull/24736 v18/dev`.

If either is missing or ambiguous, ask. Strip any `origin/` prefix from the destination.

Throughout, `{n}` is the PR number, `{dest}` the destination branch, `{base}` the PR's current base branch.

## Stop conditions

Stop and report clearly (no partial changes) when any of these hold:

| Condition | Report |
|---|---|
| PR is merged or closed | Nothing to retarget; for a merged PR suggest a cherry-pick PR instead. |
| `{dest}` doesn't exist on `origin`, or equals `{base}` | Say which. |
| Fork PR with `maintainerCanModify: false` | The contributor hasn't allowed edits from maintainers — or can't: GitHub doesn't offer the option for forks owned by an organisation. Suggest they enable it (personal forks), or rebase and retarget themselves (offer the commands from step 7). |
| PR touches `.github/workflows/**` and the `gh` token lacks the `workflow` scope | The push will be rejected; say so. |
| The changes don't apply or need substantial rework (see step 4) | Explain why, with specifics. |
| Every commit becomes empty on `{dest}` | The change already exists on `{dest}`; retargeting would produce an empty PR. |

## Procedure

### 1. Gather PR facts

```bash
gh auth status
gh pr view {n} --repo umbraco/Umbraco-CMS --json number,title,state,isDraft,baseRefName,headRefName,headRefOid,headRepository,headRepositoryOwner,isCrossRepository,maintainerCanModify,author,commits,files
git ls-remote --exit-code --heads origin {dest}
```

Check the stop conditions that these answer.

Record `headRefOid` as **`{oldHead}`** — it's the lease for the force-push and the rollback point.

### 2. Fetch and identify the PR's commits

```bash
git fetch origin {base} {dest}
git fetch origin pull/{n}/head:retarget-backup/{n}
git rev-parse retarget-backup/{n}     # must equal {oldHead}
mb=$(git merge-base origin/{base} retarget-backup/{n})
git log --format='%H %P | %an <%ae> | %s' $mb..retarget-backup/{n}
```

Cross-check that list against the commits in `gh pr view`. Then classify:

- **Plain commits** — replayed in order.
- **Merge commits that merge `{base}` into the PR branch** (one parent is on `{base}`) — dropped; they exist only to keep the branch current with the old base. If such a merge contains conflict resolution (`git show --remerge-diff <sha>` is non-empty), those resolutions are lost by dropping it; account for them in step 4 when the replay conflicts in the same places.
- **Any other merge commit** (merging another feature branch, etc.) — treat as "complicated" and stop unless the user explicitly wants to proceed.

Also note whether commits are signed (`git log --format='%H %G?' $mb..retarget-backup/{n}`). Rewriting a commit always drops its signature, so a "Verified" badge will disappear — mention it in the plan.

### 3. Replay onto `{dest}` in a throwaway worktree

```bash
git worktree add --detach ../retarget-{n} origin/{dest}
```

If `../retarget-{n}` already exists, it's left over from an earlier run. If `git worktree list` shows it, remove it with `git worktree remove --force`; otherwise delete the folder. On Windows, a removed worktree can leave its folder behind (build processes may still hold locks on its output), and that blocks `worktree add`. Don't delete a folder that isn't a `retarget-*` folder this skill created.

Replay each plain commit, oldest first, with the **original committer identity and date** so only the parent changes (`cherry-pick` already preserves author, author date and message; it does not preserve the committer):

```bash
cd ../retarget-{n}
for sha in <plain commits, oldest first>; do
  IFS='|' read -r cn ce cd <<< "$(git log -1 --format='%cn|%ce|%cI' "$sha")"
  GIT_COMMITTER_NAME="$cn" GIT_COMMITTER_EMAIL="$ce" GIT_COMMITTER_DATE="$cd" \
    git cherry-pick --allow-empty-message "$sha" || break
done
```

Don't use `-x`: it appends "cherry picked from" to the message, which the contributor never wrote.

If a pick stops, note the conflicted files (`git status --short`, `git diff`), and keep going only far enough to understand the whole picture (step 4). When resolving a conflict, continue with the same committer variables set for that commit:

```bash
GIT_COMMITTER_NAME="$cn" GIT_COMMITTER_EMAIL="$ce" GIT_COMMITTER_DATE="$cd" git cherry-pick --continue --no-edit
```

If a pick reports the commit is now empty, the change already exists on `{dest}` — note it and `git cherry-pick --skip`.

### 4. Assess

A clean replay is necessary but not sufficient — the PR was written against `{base}`, and `{dest}` can differ in ways that compile-time conflicts don't reveal. Work out what the contributor would have written had they targeted `{dest}`.

**Check the landmarks first.** Read [`../_shared/major-version-landmarks.md`](../_shared/major-version-landmarks.md) and match the PR's changed files against the "Signals" for every major between `{dest}` and `{base}`. A hit usually means the code lives somewhere else on `{dest}`, works differently there, or doesn't exist there at all. When it doesn't exist, don't stop with a bare "can't retarget": say where the equivalent code lives on `{dest}`, so the user can decide whether a separate fix is needed there.

**Conflicts.** For each one, determine why the code differs between the branches (`git log origin/{dest}..origin/{base} -- <file>` usually explains it):

- *Mechanical* — surrounding lines changed, a member was renamed, a file moved, a constructor gained a parameter, an obsolete overload exists on one branch but not the other. Resolve so the PR's intent is applied to `{dest}`'s code.
- *Substantive* — the PR builds on a feature, API or refactoring that doesn't exist on `{dest}`, or the fix's root cause is different there (the bug may not even exist on `{dest}` — check). Stop: this needs the contributor or a separate PR, not a retarget.

**Branch differences that apply cleanly but are wrong** — check each against the diff:

- `[Obsolete("... Scheduled for removal in Umbraco {N}.")]` and `// TODO (V{N})` comments: `N` must follow `{dest}`'s `version.json` (current major + 2), per root `CLAUDE.md` §6.
- Migrations: the plan, state names and folder/version (e.g. `Upgrade/V_17_x_0`) belong to `{dest}`'s version. Moving a migration between majors changes the upgrade path — usually "substantive" unless it's trivially re-homed.
- `OpenApi.json` / generated `*.gen.ts`: never hand-merge. They must be regenerated from `{dest}` (see the `umb-update-openapi` skill); include that in the plan.
- Target framework and language: APIs or C# features only available in a newer .NET than `{dest}` targets.
- Package versions in `Directory.Packages.props` / `package.json` that differ on `{dest}`.
- Backoffice client: imports, element names or contexts that exist only on the newer line.

**Verify the result builds and its tests pass on `{dest}`:**

- Build every C# project the PR touches (fresh build, no `--no-build`) and run the tests the PR adds or changes, e.g. `dotnet test <test project> --filter "FullyQualifiedName~<TestClass>"`.
- For backoffice changes, run the relevant client checks from `src/Umbraco.Web.UI.Client` (typecheck/lint, and the unit tests for the touched area) when `node_modules` is available; otherwise say they weren't run.

**Compare against the original** to see exactly what changed per commit:

```bash
git range-diff $mb..retarget-backup/{n} origin/{dest}..HEAD
```

For a clean replay with no adaptations, each pair shows `=` (identical patch). Pairs marked `!` are your adaptations — every one of them must be intentional and explained in the plan.

If the adaptations are more than modest, mechanical edits — a reviewer couldn't confirm them at a glance, or they change behaviour rather than just fitting `{dest}`'s code — stop and say so instead of proposing them.

### 5. Present the plan

Report concisely:

- PR, current base → `{dest}`; contributor and whether maintainer edits are allowed.
- Commits that will be replayed (and any dropped merge commits, with why); note lost signatures if any.
- Result: **clean** (range-diff all `=`) or **with adaptations** — list each adaptation: file, what changed and why `{dest}` needs it. Adaptations are folded into the contributor's commits, because the goal is the PR as they'd have written it against `{dest}`; that's why they must be transparent here and in the comment to the contributor.
- Build/test results on `{dest}` (what ran, pass/fail counts; what didn't run).
- Anything else the reviewer should know (follow-up regeneration, bug not present on `{dest}` for part of the PR, etc.).

### 6. Confirm

Ask the user to confirm before changing anything on GitHub (use `AskUserQuestion`: "Push and retarget" / "Don't retarget"). Show the comment you'll post in step 8 as part of the plan, so one confirmation covers the push, the retarget and the comment. A confirmation applies to this PR and this plan only. If the user declines, go to step 9.

### 7. Execute

Push first, then change the base. In this order, the PR's diff is only transiently noisy; changing the base first would briefly show every commit between the two branches and auto-request reviews from code owners of all of them.

```bash
# Fork PR — push to the contributor's fork, guarded by the lease from step 1
git push --force-with-lease={headRefName}:{oldHead} \
  https://github.com/{headRepositoryOwner}/{headRepository}.git HEAD:refs/heads/{headRefName}
# Same-repo PR
git push --force-with-lease={headRefName}:{oldHead} origin HEAD:refs/heads/{headRefName}

gh pr edit {n} --repo umbraco/Umbraco-CMS --base {dest}
```

If the lease is rejected, the contributor pushed in the meantime: stop, don't retry with `--force`, report, and offer to restart from step 1.

### 8. Verify

```bash
gh pr view {n} --repo umbraco/Umbraco-CMS --json baseRefName,headRefOid,commits,files
```

Confirm: base is `{dest}`; `headRefOid` is the pushed HEAD; same number of commits (minus dropped merges) with the same authors and messages; the changed-file list matches the original (plus or minus only the adaptations reported). Report the result.

Then post a short comment to the contributor (`gh pr comment {n} --repo umbraco/Umbraco-CMS --body "..."`), using the text the user confirmed in step 6:

> Thanks @{author}. I have retargeted this PR to `{dest}` so the fix ships in {dest major}, and it will reach {newer majors} through our merge-ups. Your branch was rebased onto `{dest}` with your commit unchanged, so please `git fetch` and `git reset --hard origin/{headRefName}` before pushing anything else to it.

Adjust the wording to fit:
- **Several commits:** say "your commits".
- **Adaptations:** replace "unchanged" with a sentence listing each adaptation and why `{dest}` needed it.
- **Not the oldest maintained line** (e.g. retargeted to `v18/dev`): add one sentence saying why the PR can't go further back. For example: "It can't go to 17, because `X` arrived in 18 with Y."

Report the comment link.

### 9. Clean up

```bash
git worktree remove --force ../retarget-{n}
git worktree prune
rm -rf ../retarget-{n}    # remove the folder if worktree removal left it behind
```

Keep `retarget-backup/{n}` until the user is satisfied — it's the rollback (push `{oldHead}` back with `--force-with-lease` and restore the base with `gh pr edit --base {base}`). Tell the user it exists and that `git branch -D retarget-backup/{n}` removes it.

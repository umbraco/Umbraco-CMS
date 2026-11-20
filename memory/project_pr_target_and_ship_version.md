---
name: pr-target-and-ship-version
description: "The ef-core-document-repository branch PRs into v18/feature/ef-core-repositories (not v19/dev) and ships in Umbraco 20; the v18/ branch prefix is wrong; review scope is the diff against ef-core-repositories only"
metadata:
  type: project
---

On 22-09-2026 the user said the PR for `v18/feature/ef-core-document-repository` targets `origin/v18/feature/ef-core-repositories`, not `v19/dev`, and that the work ships in Umbraco 20 ("the branch naming is wrong, this is gonna go into V20"). Breaking changes are already advertised for that release, so they are out of scope for review (see [[this-branch-allows-breaking-changes]]).

**Why:** Diffing against `v19/dev` or `main` pulls in 55+ unrelated commits from the ef-core-repositories parent branch and misreports scope; "Scheduled for removal in Umbraco N" text should be computed from a v20 ship version (N = 22), not from version.json's 19.

**How to apply:** Use `git diff origin/v18/feature/ef-core-repositories...HEAD` for anything "what does this branch change"; never flag public API removals as breaking here; treat migration-plan placement in `V_18_0_0`/`V_19_0_0` folders as inherited convention from the parent branch, not a defect to block on. A full review of the branch was delivered on 22-09-2026 (findings: version-delete published-version bug, SQLite key-column migration failure, NPoco key rewrite, mid-chain plan insertion, sync-over-async sweep).

# Campaign learnings

Carried from one NPoco to EF Core campaign to the next. Read this before planning a new campaign on this branch, and add to it when a campaign closes.

Campaigns so far:

| Campaign | Folder | Ran | PRs |
|---|---|---|---|
| Element repository and service | `ef-core-element-repository/` | 08-10-2026 to 19-11-2026 | Nine, all merged; last one `cfaf78790e7` (#24678) |
| Media repository and service | `ef-core-media-repository/` | planned 20-11-2026 | Nine planned (1, 1b, 2, 3, 3b, 4, 5, 6, 7); 1, 1b, 2, 3 and 3b merged by 26-11-2026, PR 4 and 5 next |

## How this branch is organised

- One folder per campaign, each with its own `CAMPAIGN.md` (plan), `status.json` (source of truth for progress) and `progress.html` (page source for the board artifact).
- `grand-plan/` is the map above the campaigns: `GRAND-PLAN.md`, `grand-plan.json` (every repository still on NPoco, grouped into proposed campaigns in order, plus loose ends) and `overview.html` (page source for the "EF Core Migration Map" artifact). Update it when a campaign starts or closes.
- The branch is an orphan branch checked out as a worktree at `.claude/campaign/` in the main checkout. Nothing here ever goes on a PR branch.
- `sync-memory.sh` / `sync-memory.ps1` and the `memory/` snapshot live at the branch root since 20-11-2026 and are shared by every campaign.
- The branch is still named `campaign/ef-core-element-repository`. Renaming it means a new remote branch and a new `git worktree add` on every machine.

## What worked

- **Small PRs in a fixed order.** Schema first (DTOs and a no-op migration), then hoists that change no behaviour, then the new repository as a purely additive PR with its tests, then the service cutover that deletes the NPoco repository, then a last PR for deletions and renames. Each was reviewable on its own.
- **Hoist before you add.** Moving the generic logic into the shared base first kept the new repository thin and avoided a fork that goes stale on every merge-up.
- **Spike the risky design point on both providers before committing to it.** The generic row and query hook were proven on SQLite and SQL Server before the rest of the plan depended on them.
- **Write the contract rules into the plan.** Guid keys, the `GetAll`/`GetMany` split, no `IQuery` or `out` totals, `Attempt` with a status enum, Guid user key, trailing `CancellationToken`. They were only implied at first, and were then enforced by greps in the acceptance of the PRs that add contracts.
- **Record the known-failing baseline on day one.** It stopped unrelated red tests from being treated as regressions in every later PR.
- **`status.json` as the single source of truth**, with the page only rendering it. Any session on any machine can pick up from it.
- **A dated decisions table in the plan.** Most "why is it like this" questions in later PRs were answered from it.
- **Independent reviewer after each PR.** It caught the one real regression of the campaign (an empty notification and an audit row on every content type delete) and one wrongly removed public member.

## What cost time, and what to do instead

- **The baseline fix was left optional until the end.** The last PR then had to be stacked on it to get green tests, retargeted, and rebased once the lower PR merged with an extra review commit. Next time: if a known-failing baseline can be fixed by a small independent PR, do it first.
- **Stacked PRs.** Point the upper PR at the lower PR's branch so it shows only its own changes. After the lower PR is squash merged, rebase the upper PR's own commits onto the integration branch (`git rebase --onto <integration> <old base>`) and force push with a lease. A merge instead leaves the lower PR's commits in the list and conflicts on anything the lower PR changed late.
- **Review findings became extra PRs (2b and 4b).** Both were cleanups of something an earlier PR left half-true (setters added only to rebuild a graph; a nullable parameter nothing could handle). Expect one or two of these and leave room in the numbering.
- **A shared base hardcodes the first entity's flavour.** Moving the second entity onto the async engine exposed hardcoded Content notifications and the document recycle bin id. Before a cutover, grep the base for constants and notification types that belong to the entity that got there first, and turn them into hooks.
- **"Unreferenced" is not "removable".** A protected member of a public class is public API with zero callers. Check the declaring type's accessibility before an orphan sweep deletes anything.
- **Parity changes that touch the already-migrated entity get reverted.** A change made for the new entity that also alters document behaviour needs its own decision; do not fold it into a cutover.
- **Renaming scaffolding onto freed names hides merge-up conflicts.** Once an `Async*` type takes the name of the sync file it replaced, a merge-up from upstream merges sync hunks into the async file instead of raising a modify/delete conflict. Every hunk landing in those files has to be walked by hand. List the affected files in the plan when the rename happens.
- **Per-step commits.** Recording a tree per step and committing later only works if nothing is edited after the snapshot. If review rounds follow, the intermediate trees have to be adjusted and are no longer the states that were built.
- **Bulk edits.** Git Bash `sed -i` rewrites CRLF files as LF; use a script that preserves line endings and the BOM. After any scripted rename, diff the whole file and grep a `--no-incremental` build for `CS1574`.
- **EF Core code that needs logic from a repository not yet migrated.** Keep it in EF Core where it is used and mark both ends with a `TODO` naming the move, plus a decision row. Do not route it through the NPoco repository or add new `IQuery` use.

## Working agreements that held throughout

- Never commit, push or open a PR without being asked in that moment. Never commit this folder on a PR branch.
- Breaking changes are allowed on the integration branch: no obsolete constructors or forwarding overloads.
- PR descriptions are short: what changed, why now, what breaks, how it was tested.
- Review bots flag breaking changes on every PR; answer once that they are allowed here.
- Locally, run the tests the change touches plus a red-first proof for behaviour changes. Full suites run on the pipeline.

## Pointers for the next campaign (media)

Not a plan, and not re-verified since the element campaign closed. Check each against the code first.

- Media is not publishable, so it sits on the wide tier: `AsyncContentRepositoryBase`, `IAsyncContentRepository` and `IAsyncContentServiceBase`. These three kept their `Async` prefix because media and members still use the sync twins.
- `IMediaService`'s async members are a merge artifact that delegates to the sync members; they do not mean media is migrated.
- Version, container and database-cache repositories stayed NPoco in both earlier campaigns.
- The relation lookups in the EF Core `RelationRepository` carry a `TODO` to move the entity work to the entity repository when that migrates.

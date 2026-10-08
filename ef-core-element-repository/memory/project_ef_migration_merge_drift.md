---
name: ef-core-migration-snapshot-drift-after-merge
description: "How to detect and fix a botched UmbracoDbContextModelSnapshot.cs after resolving merge conflicts across two EF Core migration branches, WITHOUT touching a migration that's already merged/shared"
metadata: 
  node_type: memory
  type: project
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-10T06:56:00.005Z
---

When two branches each add their own EF Core migrations (both touching `UmbracoDbContextModelSnapshot.cs`, `EFCoreMigration.cs`, `UmbracoPlan.cs`, and both `*MigrationProvider.cs` switches), a manual/git-resolved merge of the snapshot file is NOT reliably correct even if it compiles — EF Core's snapshot format isn't safely line-mergeable. In one real case, a merge silently dropped most of a newly-added entity's columns and gave it the wrong table name on SQLite, and dropped the entity's table entirely on SQL Server, yet the build still succeeded with 0 errors.

**Why:** `dotnet build` only proves the C# compiles — it says nothing about whether the snapshot matches the actual `DbContext` model. Verify with EF Core's own tool, not by eyeballing the diff:
```bash
dotnet ef migrations has-pending-model-changes -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext
dotnet ef migrations has-pending-model-changes -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext
```
(Match the global `dotnet-ef` tool version to the project's `Microsoft.EntityFrameworkCore` package version in `Directory.Packages.props` first — `dotnet tool update -g dotnet-ef --version <match>` — a mismatch only prints a warning but is worth eliminating before trusting the verdict.)

**Critical constraint — figure out which side's migration is safe to touch before fixing anything:** only regenerate a migration that is still local/unmerged/unreleased (e.g. your own feature branch's migration that hasn't been merged into the shared integration branch or shipped). A migration that came in from the OTHER branch (already merged/shared, possibly already applied to someone's real database) must NEVER be removed or regenerated — ask/check this explicitly; don't assume. First attempt at this got corrected by the user for exactly this reason: nearly ran `dotnet ef migrations remove` on a migration (`MemberPropertyTypeToEFCore`) that had already been merged elsewhere, purely because it happened to be chronologically last in the id-sorted chain.

**How to apply, when it's your own migration that needs fixing (call it `X`), and it happens to sit BEFORE an already-merged migration `Y` in timestamp order** (`dotnet ef migrations remove` only ever pops the last migration, so you cannot get back to "before X" without removing Y — which you must not do):
1. Do NOT use `dotnet ef migrations remove` for this. Instead, manually reset the snapshot to the state it should be in without X:
   - `Y`'s own `.Designer.cs` (untouched by the merge, since only the cumulative snapshot file conflicted, not individual per-migration files) contains a `BuildTargetModel(ModelBuilder modelBuilder)` method whose body is — by construction, this is how EF Core's tooling always generates it — byte-for-byte identical to what `UmbracoDbContextModelSnapshot.cs`'s `BuildModel(ModelBuilder modelBuilder)` body would be if Y were the last migration and X didn't exist yet.
   - Splice: keep the snapshot file's own header (namespace/class/`BuildModel` signature + opening brace) and footer (closing braces), but replace the body in between with Y's Designer.cs body. Do this per provider. Verify with `grep` that the new-entity's table name no longer appears (since Y's own migration, authored before X existed, never knew about it).
2. Delete X's old migration files (`.cs`/`.Designer.cs`, both providers) directly (`rm`) — safe since X is unmerged.
3. Temporarily comment out X's case in both `*MigrationProvider.GetMigrationType()` switches (needed only because the class file is briefly gone — `dotnet ef migrations add` needs a successful build). See [[project_ef_migration_regeneration]] for the provider-switch appsettings dance.
4. Run `dotnet ef migrations add X` for both providers against the now-correctly-reset snapshot — this generates a fresh, correct migration positioned after Y (new timestamp, same class name X, so the enum/plan/switch wiring elsewhere needs zero changes since they reference the type by name only).
5. Uncomment the switch cases, empty the new migration's `Up()`/`Down()` (same no-op convention as every EF Core migration in this repo — NPoco owns real schema creation).
6. Re-run `has-pending-model-changes` for both providers — must report clean before considering it done.
7. Restore `appsettings.json` to its original committed value (note: in this repo `src/Umbraco.Web.UI/appsettings.json` is actually gitignored/untracked — `git status` won't show drift either way, so diff its content against what it was before you started, not against git).

End state: no new enum value, no new `UmbracoPlan` entry, no new Umbraco migration class — X keeps its original name and position in the enum/plan, only its underlying generated migration files got a new timestamp and correct content. Only the already-unmerged side's artifacts changed.

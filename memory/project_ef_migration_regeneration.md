---
name: EF Core migration regeneration procedure
description: Step-by-step procedure for removing and regenerating EF Core migrations when both SQL Server and SQLite providers exist
type: project
---

When model configurations change after a migration has already been generated, both provider migrations must be removed and regenerated. The procedure is error-prone due to the provider switch dance.

**Why:** The `appsettings.json` startup project controls which provider EF Core tools use. SQLite operations require `Microsoft.Data.Sqlite`; SQL Server operations require `Microsoft.Data.SqlClient`. Both provider `GetMigrationType()` switches reference the migration class by type — when `migrations remove` deletes the class file, the switch breaks the build, which prevents the next `migrations add` from running.

**How to apply:** Follow these steps in order every time a migration needs regenerating.

### Step-by-step

1. **Remove SQL Server migration** (provider name doesn't matter — `--force` skips DB check):
   ```bash
   dotnet ef migrations remove --force -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext
   ```

2. **Comment out the migration case** from `SqlServerMigrationProvider.GetMigrationType()` (class file was just deleted, build would fail otherwise).

3. **Switch provider** in `src/Umbraco.Web.UI/appsettings.json`:
   ```json
   "umbracoDbDSN_ProviderName": "Microsoft.Data.Sqlite"
   ```

4. **Remove SQLite migration**:
   ```bash
   dotnet ef migrations remove --force -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext
   ```
   > Note: `--force` is needed because the SQLite connection string (`Cache=Shared`) is rejected by `Microsoft.Data.SqlClient` when it was the active provider.

5. **Comment out the migration case** from `SqliteMigrationProvider.GetMigrationType()` for the same reason.

6. **Switch provider back** to SQL Server in `appsettings.json`.

7. **Regenerate SQL Server migration**:
   ```bash
   dotnet ef migrations add <Name> -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext
   ```

8. **Restore the SQL Server switch case** (class file now exists again).

9. **Switch provider** to SQLite in `appsettings.json`.

10. **Regenerate SQLite migration**:
    ```bash
    dotnet ef migrations add <Name> -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext
    ```

11. **Restore the SQLite switch case**.

12. **Restore provider** to SQL Server in `appsettings.json`.

13. **Empty `Up()` and `Down()`** in both generated migration `.cs` files (keep Designer files and snapshot unchanged).

### Local settings override the env vars (added 2026-09-29)

`src/Umbraco.Web.UI/Program.cs` adds `appsettings.Local.json` (git-ignored) *after* environment variables, so `ConnectionStrings__umbracoDbDSN_ProviderName=...` env vars are ignored whenever that file sets a provider - `dotnet ef` then reports "target project doesn't match your migrations assembly". Move the file aside (back it up first, restore with a trap) for the duration of the remove/add/has-pending-model-changes commands; the env-var approach only worked in agent worktrees because they had no local settings file. The whole remove/add dance with env vars is scripted in this pattern: remove SQL Server → comment its switch case → remove SQLite → comment its case → add SQL Server → restore case → add SQLite → restore case → copy the previous no-op `.cs` bodies over the generated ones.

### Scaffolder may prefix the class name (seen 2026-10-08)

`dotnet ef migrations add RemoveUserGroupDefaultPermissionsFromModel` generated `partial class _20261008084151_RemoveUserGroupDefaultPermissionsFromModel` in both the `.cs` and `.Designer.cs`. If the `.cs` is then rewritten from the no-op template with the plain class name, the build fails with CS0115 "BuildTargetModel: no suitable method found to override" because the two partials no longer join. Rename the class in the Designer to the plain name (the `[Migration("<timestamp>_<Name>")]` attribute keeps the id) so the provider switch can keep referencing `Migrations.<Name>`. Always diff the generated `.cs` before overwriting it.

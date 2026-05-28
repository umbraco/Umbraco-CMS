---
name: ef-core-empty-migration
description: Generate empty (no-op) EF Core migration stubs for both SQL Server and SQLite providers in Umbraco CMS, create the Umbraco migration class that invokes it, add it to UmbracoPlan, and wire up the migration runner plumbing (enum + switch cases). Use this whenever you need to generate EF Core migrations after completing the DTO, configuration, and DbSet work — i.e., after completing Steps 1–3 of the EF Core DTO Migration Guide (Section 12 in Infrastructure CLAUDE.md). Also trigger this skill when the user mentions "dotnet ef migrations add", "adding an EF Core migration", "generating migrations for both providers", or "updating the migration runner plumbing".
argument-hint: <MigrationName>
---

# EF Core Empty Migration Generation

Automates the full workflow of adding an empty EF Core migration to Umbraco:

1. Generate empty migration stubs in both SQL Server and SQLite provider projects
2. Create the Umbraco migration class (`AsyncMigrationBase`) that runs it
3. Add it to `UmbracoPlan`
4. Wire up the migration runner plumbing (enum + switch cases)

These migrations are intentionally no-ops — NPoco creates the actual database tables. The EF Core migrations exist only to update the model snapshot so EF Core tracks the schema state correctly.

## Prerequisites Check

Before running any commands, verify Steps 1–3 of the EF Core DTO Migration Guide are complete (Step 4, the SQL Server model customizer, is only needed if the NPoco DTO has `[IncludeColumns]` indexes). Read the relevant files to confirm:

- EF Core DTO exists in `src/Umbraco.Infrastructure/Persistence/Dtos/EFCore/` with `[EntityTypeConfiguration(...)]` attribute
- DTO configuration exists in `src/Umbraco.Infrastructure/Persistence/Dtos/EFCore/Configurations/`
- `DbSet<FooDto>` is registered in `src/Umbraco.Infrastructure/Persistence/EFCore/UmbracoDbContext.cs`
- SQL Server model customizer exists in `src/Umbraco.Cms.Persistence.EFCore.SqlServer/DtoCustomization/` (only if the NPoco DTO has `[IncludeColumns]` indexes)

If any are missing, tell the user which step is incomplete — do not run migration commands yet. The snapshot will silently omit the entity if the DbSet is missing.

## Step 1: Determine Migration Name

Use `$ARGUMENTS` as the migration name if provided. If not, ask:
> What should the migration be named? (PascalCase, e.g. `AddRelationDtos`)

Convention: names start with `Add` followed by the DTO or feature name.

## Step 2: Prepare appsettings.json

Read `src/Umbraco.Web.UI/appsettings.json`. This file is gitignored — it may not exist yet.

Record the current value of `ConnectionStrings.umbracoDbDSN_ProviderName` as `originalProviderName` (default to `Microsoft.Data.Sqlite` if the file doesn't exist or the key is absent).

The EF Core tooling reads these keys to select the correct SQL dialect — it does not need a real/reachable database for generating migrations, but `UmbracoDbContext` will reject a null or empty connection string before reading the provider.

## Step 3: Generate SQL Server Migration

Set both `umbracoDbDSN` (placeholder) and `umbracoDbDSN_ProviderName` in `src/Umbraco.Web.UI/appsettings.json`. If the file doesn't exist, create a minimal one:

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Server=localhost;Database=UmbracoDev;Integrated Security=true",
    "umbracoDbDSN_ProviderName": "Microsoft.Data.SqlClient"
  }
}
```

If the file already exists, update `umbracoDbDSN_ProviderName` to `Microsoft.Data.SqlClient` and ensure `umbracoDbDSN` is non-empty (add a placeholder if missing).

Run from the repository root:

```bash
dotnet ef migrations add {MigrationName} -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext
```

If the command fails, restore `originalProviderName` and stop. Show the full error. Common causes:
- DTO not yet registered in `UmbracoDbContext`
- Missing `[EntityTypeConfiguration]` attribute on DTO

## Step 4: Generate SQLite Migration

Set `umbracoDbDSN` to the SQLite placeholder and `umbracoDbDSN_ProviderName` to `Microsoft.Data.Sqlite` in `src/Umbraco.Web.UI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True",
    "umbracoDbDSN_ProviderName": "Microsoft.Data.Sqlite"
  }
}
```

Run:

```bash
dotnet ef migrations add {MigrationName} -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext
```

If the command fails, restore `originalProviderName` and stop. Show the full error.

## Step 5: Restore appsettings.json

- If the file did not exist before Step 2, delete it: `rm src/Umbraco.Web.UI/appsettings.json`
- If it existed, restore `umbracoDbDSN_ProviderName` to `originalProviderName` (and remove any placeholder `umbracoDbDSN` entry that wasn't there before)

## Step 6: Empty Up() and Down() Methods

Find the two generated migration files (they will have a timestamp prefix):

```bash
ls src/Umbraco.Cms.Persistence.EFCore.SqlServer/Migrations/*_{MigrationName}.cs
ls src/Umbraco.Cms.Persistence.EFCore.Sqlite/Migrations/*_{MigrationName}.cs
```

In each file, replace the `Up()` and `Down()` method bodies with empty bodies:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
}

protected override void Down(MigrationBuilder migrationBuilder)
{
}
```

Do NOT touch the `.Designer.cs` files or `UmbracoDbContextModelSnapshot.cs` — those are auto-generated and must stay as produced by the tooling.

## Step 7: Create the Umbraco Migration Class

The Umbraco migration class lives in the version-specific upgrade directory. These EF Core repository migrations target v19.

Check whether `src/Umbraco.Infrastructure/Migrations/Upgrade/V_19_0_0/` exists. If not, create the directory.

Create `src/Umbraco.Infrastructure/Migrations/Upgrade/V_19_0_0/{MigrationName}.cs`:

```csharp
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

public class {MigrationName} : AsyncMigrationBase
{
    private readonly IEFCoreMigrationExecutor _migrationExecutor;

    public {MigrationName}(
        IMigrationContext context,
        IEFCoreMigrationExecutor migrationExecutor)
        : base(context)
    {
        _migrationExecutor = migrationExecutor;
    }

    protected override async Task MigrateAsync()
    {
        // NO-OP migration to ensure that EF Core doesn't complain about missing migrations.
        await _migrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.{MigrationName});
    }
}
```

## Step 8: Add to UmbracoPlan

File: `src/Umbraco.Infrastructure/Migrations/Upgrade/UmbracoPlan.cs`

Read the file to find the last existing `To<>` call. Append a new call after it with a freshly generated GUID:

```csharp
To<V_19_0_0.{MigrationName}>("{new-unique-guid}");
```

Generate the GUID and format it in uppercase with braces (works cross-platform):

```bash
python3 -c "import uuid; print(str(uuid.uuid4()).upper())"
```

Verify the generated GUID does not already appear in `UmbracoPlan.cs` before using it.

## Step 9: Update Migration Runner Plumbing

Three files need updating so the Umbraco migration executor can invoke the new migration by name.

### 9a. EFCoreMigration Enum

File: `src/Umbraco.Infrastructure/Migrations/EFCoreMigration.cs`

Read the current enum to find the highest integer value. Add the new entry at the end with the next sequential value:

```csharp
{MigrationName} = {nextValue},
```

### 9b. SqlServerMigrationProvider Switch

File: `src/Umbraco.Cms.Persistence.EFCore.SqlServer/SqlServerMigrationProvider.cs`

Add a case to `GetMigrationType()` before the `_ => throw` line:

```csharp
EFCoreMigration.{MigrationName} => typeof(Migrations.{MigrationName}),
```

Note: `GetMigrationType()` on the SQL Server provider returns `Type?` — it can return `null` for SQLite-only migrations (like `SqliteCollation`). For standard migrations, always return the migration type.

### 9c. SqliteMigrationProvider Switch

File: `src/Umbraco.Cms.Persistence.EFCore.Sqlite/SqliteMigrationProvider.cs`

Add the same case to `GetMigrationType()` before the `_ => throw` line:

```csharp
EFCoreMigration.{MigrationName} => typeof(Migrations.{MigrationName}),
```

Note: `GetMigrationType()` on the SQLite provider returns `Type` (non-nullable) — every enum value must have a case. Always add the case even for no-ops.

## Step 10: Verify Snapshots

Confirm the new entity was captured by the EF Core model snapshot. Look up the table name constant from the DTO (e.g., `Constants.DatabaseSchema.Tables.Relation` resolves to `"umbracoRelation"`), then search both snapshots:

```bash
grep -n "{tableName}" src/Umbraco.Cms.Persistence.EFCore.SqlServer/Migrations/UmbracoDbContextModelSnapshot.cs
grep -n "{tableName}" src/Umbraco.Cms.Persistence.EFCore.Sqlite/Migrations/UmbracoDbContextModelSnapshot.cs
```

If the table name is absent from either snapshot, the DTO was not discovered. Fix by checking:

1. `[EntityTypeConfiguration(typeof({Name}DtoConfiguration))]` attribute is present on the DTO
2. `public required DbSet<{Name}Dto> {Foos} { get; set; }` is in `UmbracoDbContext`

Then remove the migrations and regenerate (repeating from Step 2):

```bash
dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer
dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite
```

## Completion Summary

When all steps are done, confirm each item:

- [ ] SQL Server migration generated: `src/Umbraco.Cms.Persistence.EFCore.SqlServer/Migrations/*_{MigrationName}.cs`
- [ ] SQLite migration generated: `src/Umbraco.Cms.Persistence.EFCore.Sqlite/Migrations/*_{MigrationName}.cs`
- [ ] Both EF Core migration files have empty `Up()` and `Down()` methods
- [ ] Umbraco migration class created: `src/Umbraco.Infrastructure/Migrations/Upgrade/V_19_0_0/{MigrationName}.cs`
- [ ] `UmbracoPlan.cs` — new `To<>` call added with a unique GUID
- [ ] `EFCoreMigration.cs` — new enum value added
- [ ] `SqlServerMigrationProvider.cs` — new switch case added
- [ ] `SqliteMigrationProvider.cs` — new switch case added
- [ ] Both `UmbracoDbContextModelSnapshot.cs` files contain the expected table name
- [ ] `appsettings.json` provider name restored to original value

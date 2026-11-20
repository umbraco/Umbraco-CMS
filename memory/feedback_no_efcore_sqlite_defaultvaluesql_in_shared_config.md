---
name: Provider-specific defaultValueSql must use SQLite customizer, not shared config
description: HasDefaultValueSql in shared IEntityTypeConfiguration must not use SQL Server syntax; override in SQLiteContentVersionDtoModelCustomizer instead
type: feedback
---

Do not put SQL Server-specific SQL expressions (like `GETUTCDATE()`) in `HasDefaultValueSql` inside a shared `IEntityTypeConfiguration` file without a provider-specific override for SQLite.

**Why:** EF Core will use that exact SQL string when generating the SQLite migration/snapshot. `GETUTCDATE()` is not valid SQLite SQL — it would silently produce a wrong snapshot and fail if EF Core ever creates the table on SQLite. The correct SQLite equivalent is `datetime('now')`.

**How to apply:** When a DateTime column needs a DB-level default:
1. Put `HasDefaultValueSql("GETUTCDATE()")` in the shared configuration (documents intent, correct for SQL Server)
2. Create a `Sqlite{Entity}DtoModelCustomizer` in `Umbraco.Cms.Persistence.EFCore.Sqlite` that overrides it with `HasDefaultValueSql("datetime('now')")`
3. Register the customizer in `UmbracoBuilderExtensions.AddUmbracoEFCoreSqliteSupport()`
4. Regenerate both provider migrations so snapshots diverge correctly

Existing example: `SqliteContentVersionDtoModelCustomizer` for `ContentVersionDto.VersionDate`.

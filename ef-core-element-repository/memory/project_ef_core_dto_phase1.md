---
name: EF Core DTO phase 1 completion status
description: Phase 1 (DTOs only) of the document repository EF Core migration is complete as of 2026-04-10
type: project
---

The `v18/feature/ef-core-document-repository` branch has completed Phase 1: all 10 EF Core DTOs for the document repository are wired up and both provider migrations are clean.

**Why:** The plan (dreamy-riding-penguin.md) covers only Phase 1. Repository implementation comes later.

**How to apply:** When continuing work on this branch, Phase 1 is done. Next work is the repository implementation layer.

### What was done
- 10 EF Core DTOs created in `src/Umbraco.Infrastructure/Persistence/Dtos/EFCore/`
- 10 configurations in `.../Configurations/`
- 4 SQL Server customizers in `src/Umbraco.Cms.Persistence.EFCore.SqlServer/DtoCustomization/`
- 1 SQLite customizer: `SqliteContentVersionDtoModelCustomizer` — overrides `VersionDate` default to `datetime('now')` (SQL Server config uses `GETUTCDATE()`)
- 10 `DbSet<T>` properties added to `UmbracoDbContext`
- NPoco migration wiring: `EFCoreMigration.AddDocumentRepositoryDtos = 10`, `AddDocumentRepositoryDtos.cs` trigger class, `UmbracoPlan` entry, both provider `GetMigrationType()` switches
- Bug fix: `SqliteMigrationProvider` was missing the `AddDomainDto` case

### Key decisions
- No navigation properties on DTOs (flat by default; add when repository `Include()` actually needs them)
- `Culture` property omitted from `DocumentCultureVariationDto` and `ContentVersionCultureVariationDto` (NPoco `[Ignore]` — not a DB column)
- `Value` computed property omitted from `PropertyDataDto` (not a DB column)
- `LanguageId → LanguageDto` FKs configured on `DocumentUrlDto` and `DocumentUrlAliasDto` with `OnDelete(DeleteBehavior.NoAction)`
- `PropertyDataDto.DecimalValue` has `HasPrecision(20, 9)` matching NPoco's `DefaultDecimalPrecision=20, DefaultDecimalScale=9`

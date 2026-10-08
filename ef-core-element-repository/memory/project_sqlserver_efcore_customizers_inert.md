---
name: sqlserver-efcore-model-customizers-inert
description: RESOLVED by #24001 — SQL Server customizers now return Constants.ProviderNames.EFCore.SQLServer and their IncludeProperties land in the snapshot; this note is history
metadata:
  type: project
---

**Resolved (verified 08-10-2026):** the customizers now return `Constants.ProviderNames.EFCore.SQLServer`; the SQL Server snapshot carries 14 `IncludeProperties` calls at `f1583e70294` (fixed in #24001). New SQL Server customizers just follow the existing ones. Everything below is the original finding.

Found 2026-09-24 during the ef-core-document-repository final review; pre-exists on v18/feature/ef-core-repositories (SqlServerNodeDto/RedirectUrl customizers) and the branch added 8 more with the same bug.

`IEFCoreModelCustomizer.ProviderName` is compared against `Database.ProviderName` ("Microsoft.EntityFrameworkCore.SqlServer"). The SQL Server customizers return `Constants.ProviderNames.SQLServer` ("Microsoft.Data.SqlClient"); the SQLite ones correctly return `Constants.ProviderNames.EFCore.SQLite`. `Constants.ProviderNames.EFCore.SQLServer` exists and is referenced nowhere.

**Why:** the EF SQL Server model silently lacks included columns, non-clustered PKs and clustered indexes; comments in the shared configurations claiming "SQL Server included columns are added by the customizer" are false.

**How to apply:** verify with `grep -c 'IncludeProperties\|IsClustered' ...SqlServer/Migrations/UmbracoDbContextModelSnapshot.cs` (0 = still broken). Fix = switch constant to `EFCore.SQLServer` and regenerate the SQL Server migration/snapshot. Related: [[ef-core-migration-regeneration-procedure]].

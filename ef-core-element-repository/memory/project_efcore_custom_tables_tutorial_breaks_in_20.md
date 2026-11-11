---
name: project_efcore_custom_tables_tutorial_breaks_in_20
description: The docs tutorial "Creating Custom Database Tables with Entity Framework" is written against the Umbraco.Cms.Persistence.EFCore package, the Umbraco.Cms.Persistence.EFCore.Scoping namespace and UseDatabaseProvider, all of which the ef-core-repositories branch (ships in 20) removed in #21746
metadata:
  type: project
---

Verified 19-10-2026 against `origin/main` (18.4), `origin/v19/dev` and `origin/v18/feature/ef-core-repositories`.

What #21746 (merged 16-02-2026 into the feature branch only) changed, none of it on main or v19/dev:
- The `Umbraco.Cms.Persistence.EFCore` package is gone; its types live in `Umbraco.Cms.Infrastructure`.
- Namespaces: `Umbraco.Cms.Persistence.EFCore.Scoping` -> `Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping`;
  `AddUmbracoDbContext` moved from `Umbraco.Extensions` to `Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions`.
- `DbContextOptionsBuilder.UseDatabaseProvider(providerName, connectionString)` no longer exists. The branch idiom is
  `options.UseUmbracoDatabaseProvider(serviceProvider)`, which resolves the registered `IDatabaseConfigurator`
  (`SqlServerDatabaseConfigurator` / `SqliteDatabaseConfigurator` in the `Umbraco.Cms.Persistence.EFCore.*` provider
  packages) by the configured provider name. A custom provider implements `IDatabaseConfigurator`.
- `AddUmbracoDbContext<T>(optionsAction, shareUmbracoConnection)` keeps its shape; `shareUmbracoConnection` still
  works by wrapping the NPoco `IScope` and reusing its `Database.Transaction` (`EFCoreScope._innerScope`), so that
  mechanism is what 21 has to reimplement when NPoco goes; the parameter itself can stay.

The tutorial at docs.umbraco.com/umbraco-cms/extend-your-project/tutorials/getting-started-with-entity-framework-core
therefore breaks on 20 at: the package prerequisite, the `using` lines in steps 3 and "Going Further", and the
`UseDatabaseProvider` call in step 3. Steps 1, 2, 4, 5 and the `IEFCoreScopeProvider<T>` usage pattern are unchanged.

**Why:** these are public-API removals with no obsolete shims (the branch allows breaking changes), so every
existing EF Core custom-table implementation breaks on upgrade to 20; that belongs in the NPoco/EF Core
announcement or its own, and the tutorial needs a 20 revision.

**How to apply:** when drafting the announcement or the docs update, name the package, the two namespaces and
`UseUmbracoDatabaseProvider` explicitly; link [[project_ef_core_element_repository_campaign]] for the branch.

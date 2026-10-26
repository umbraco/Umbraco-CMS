// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_19_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddIncludeInvariantForVariantColumnToContentScheduleTests : UmbracoIntegrationTest
{
    private const string ColumnName = "includeInvariantForVariant";

    private const string PremigrationStateBeforeColumn = "{2B8F0C41-9D57-4E63-B1A9-7F4E52D6C08B}";

    [Test]
    public async Task Adds_The_Column_When_Upgrading_From_Before_It_Existed()
    {
        DropColumn();

        await ExecutePlanAsync(new UmbracoPremigrationPlan(), PremigrationStateBeforeColumn);

        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        var table = scope.SqlContext.SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.ContentSchedule);
        var column = scope.SqlContext.SqlSyntax.GetQuotedColumnName(ColumnName);
        Assert.DoesNotThrow(() => scope.Database.ExecuteScalar<int>($"SELECT COUNT({column}) FROM {table}"));
    }

    private async Task ExecutePlanAsync(MigrationPlan plan, string fromState)
    {
        var executor = new MigrationPlanExecutor(
            GetRequiredService<ICoreScopeProvider>(),
            ScopeAccessor,
            LoggerFactory,
            GetRequiredService<IMigrationBuilder>(),
            GetRequiredService<IUmbracoDatabaseFactory>(),
            Mock.Of<IDatabaseCacheRebuilder>(),
            GetRequiredService<DistributedCache>(),
            Mock.Of<IKeyValueService>(),
            GetRequiredService<IServiceScopeFactory>(),
            GetRequiredService<AppCaches>(),
            GetRequiredService<IPublishedContentTypeFactory>());

        ExecutedMigrationPlan result = await executor.ExecutePlanAsync(plan, fromState);

        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
    }

    private void DropColumn()
    {
        using IScope scope = ScopeProvider.CreateScope();
        var table = scope.SqlContext.SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.ContentSchedule);
        var column = scope.SqlContext.SqlSyntax.GetQuotedColumnName(ColumnName);
        scope.Database.Execute($"ALTER TABLE {table} DROP COLUMN {column}");
        scope.Complete();
    }
}

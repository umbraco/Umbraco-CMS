// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_19_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddIncludeInvariantForVariantColumnToContentScheduleTests : UmbracoIntegrationTest
{
    private const string ColumnName = "includeInvariantForVariant";

    private const string PremigrationStateBeforeColumn = "{2B8F0C41-9D57-4E63-B1A9-7F4E52D6C08B}";

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

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

    [Test]
    public async Task Existing_Schedules_Include_Invariant_Properties_After_Upgrading_From_Before_The_Column_Existed()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        var content = ContentBuilder.CreateBasicContent(contentType);
        ContentService.Save(content);
        ContentService.PersistContentSchedule(content, ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)));

        DropColumn();

        await ExecutePlanAsync(new UmbracoPremigrationPlan(), PremigrationStateBeforeColumn);

        ContentSchedule[] schedules = ContentService.GetContentScheduleByContentId(content.Id).FullSchedule.ToArray();
        Assert.AreEqual(2, schedules.Length);
        Assert.IsTrue(schedules.All(schedule => schedule.IncludeInvariantForVariant));
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

        if (BaseTestDatabase.IsSqlite() is false)
        {
            scope.Database.Execute($"ALTER TABLE {table} DROP CONSTRAINT {scope.SqlContext.SqlSyntax.GetQuotedName($"DF_{Constants.DatabaseSchema.Tables.ContentSchedule}_{ColumnName}")}");
        }

        scope.Database.Execute($"ALTER TABLE {table} DROP COLUMN {column}");
        scope.Complete();
    }
}

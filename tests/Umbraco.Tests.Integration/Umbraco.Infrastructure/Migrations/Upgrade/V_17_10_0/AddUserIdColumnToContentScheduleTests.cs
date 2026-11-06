using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_10_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_17_10_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddUserIdColumnToContentScheduleTests : UmbracoIntegrationTest
{
    private const string TableName = Constants.DatabaseSchema.Tables.ContentSchedule;
    private const string ColumnName = "userId";
    private const string ForeignKeyName = "FK_umbracoContentSchedule_umbracoUser_id";

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IUmbracoDatabaseFactory UmbracoDatabaseFactory => GetRequiredService<IUmbracoDatabaseFactory>();

    private IMigrationPlanExecutor MigrationPlanExecutor => new MigrationPlanExecutor(
        ScopeProvider,
        ScopeAccessor,
        LoggerFactory,
        GetRequiredService<IMigrationBuilder>(),
        UmbracoDatabaseFactory,
        GetRequiredService<IDatabaseCacheRebuilder>(),
        GetRequiredService<DistributedCache>(),
        Mock.Of<IKeyValueService>(),
        GetRequiredService<IServiceScopeFactory>(),
        AppCaches.NoCache,
        GetRequiredService<IPublishedContentTypeFactory>());

    [Test]
    public async Task Adds_The_Column_With_A_Foreign_Key_To_Users_And_Keeps_Existing_Schedules_Unattributed()
    {
        IContentType contentType = ContentTypeBuilder.CreateBasicContentType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        IContent content = ContentBuilder.CreateBasicContent(contentType);
        ContentService.Save(content);
        var release = DateTime.UtcNow.AddDays(1).TruncateTo(DateTimeExtensions.DateTruncate.Second);
        var expire = DateTime.UtcNow.AddDays(2).TruncateTo(DateTimeExtensions.DateTruncate.Second);
        ContentService.PersistContentSchedule(content, ContentScheduleCollection.CreateWithEntry(release, expire));

        RecreateTableWithoutColumn();

        ExecutedMigrationPlan result = await new Upgrader(
                new MigrationPlan("AddUserIdColumnToContentScheduleTest")
                    .From(string.Empty)
                    .To<AddUserIdColumnToContentSchedule>("done"))
            .ExecuteAsync(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());

        Assert.That(result.Successful, Is.True, result.Exception?.ToString());

        ContentSchedule[] schedules = ContentService.GetContentScheduleByContentId(content.Id).FullSchedule.ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(HasForeignKeyToUsers(), Is.True);
            Assert.That(schedules.Select(x => (x.Action, x.Date)), Is.EquivalentTo(new[] { (ContentScheduleAction.Release, release), (ContentScheduleAction.Expire, expire) }));
            Assert.That(schedules.All(x => x.UserId is null), Is.True);
        });
    }

    private void RecreateTableWithoutColumn()
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();

        if (BaseTestDatabase.IsSqlite())
        {
            db.Execute($"CREATE TABLE {TableName}_old AS SELECT id, nodeId, languageId, date, action FROM {TableName}");
            db.Execute($"DROP TABLE {TableName}");
            db.Execute($"ALTER TABLE {TableName}_old RENAME TO {TableName}");
        }
        else
        {
            db.Execute($"ALTER TABLE [{TableName}] DROP CONSTRAINT [{ForeignKeyName}]");
            db.Execute($"ALTER TABLE [{TableName}] DROP COLUMN [{ColumnName}]");
        }
    }

    // Queried directly rather than through the syntax provider, so that the assertion doesn't share a code path
    // with the migration it is verifying.
    private bool HasForeignKeyToUsers()
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();

        if (BaseTestDatabase.IsSqlite())
        {
            return db.ExecuteScalar<int>(
                $"SELECT COUNT(*) FROM pragma_foreign_key_list('{TableName}') WHERE \"table\" = @0 AND \"from\" = @1",
                Constants.DatabaseSchema.Tables.User,
                ColumnName) == 1;
        }

        return db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sys.foreign_keys WHERE [name] = @0 AND OBJECT_NAME(parent_object_id) = @1",
            ForeignKeyName,
            TableName) == 1;
    }
}

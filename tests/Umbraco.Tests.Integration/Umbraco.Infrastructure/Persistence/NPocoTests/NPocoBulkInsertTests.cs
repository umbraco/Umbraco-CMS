// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.NPocoTests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class NPocoBulkInsertTests : UmbracoIntegrationTest
{
    private readonly TestHelper _testHelper = new();

    private IProfilingLogger ProfilingLogger => _testHelper.ProfilingLogger;

    [Ignore("Ignored because you need to configure your own SQL Server to test this with")]
    [Test]
    public void Can_Bulk_Insert_Native_Sql_Server_Bulk_Inserts()
    {
        // create the db
        // prob not what we want, this is not a real database, but hey, the test is ignored anyways
        // we'll fix this when we have proper testing infrastructure
        // var dbSqlServer = TestObjects.GetUmbracoSqlServerDatabase(new NullLogger<UmbracoDatabase>());
        using (var scope = ScopeProvider.CreateScope())
        {
            // Still no what we want, but look above.
            var dbSqlServer = ScopeAccessor.AmbientScope.Database;

            // drop the table
            dbSqlServer.Execute("DROP TABLE [umbracoServer]");

            // re-create it
            dbSqlServer.Execute(@"CREATE TABLE [umbracoServer](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [address] [nvarchar](500) NOT NULL,
    [computerName] [nvarchar](255) NOT NULL,
    [registeredDate] [datetime] NOT NULL CONSTRAINT [DF_umbracoServer_registeredDate]  DEFAULT (getdate()),
    [lastNotifiedDate] [datetime] NOT NULL,
    [isActive] [bit] NOT NULL,
    [isMaster] [bit] NOT NULL,
 CONSTRAINT [PK_umbracoServer] PRIMARY KEY CLUSTERED
(
    [id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)");
            var data = new List<ServerRegistrationDto>();
            for (var i = 0; i < 1000; i++)
            {
                data.Add(new ServerRegistrationDto
                {
                    ServerAddress = "address" + i,
                    ServerIdentity = "computer" + i,
                    DateRegistered = DateTime.UtcNow,
                    IsActive = true,
                    DateAccessed = DateTime.UtcNow
                });
            }

            using (var tr = dbSqlServer.GetTransaction())
            {
                dbSqlServer.BulkInsertRecords(data);
                tr.Complete();
            }

            // Assert
            Assert.That(dbSqlServer.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoServer"), Is.EqualTo(1000));
        }
    }

    [Test]
    public void Can_Bulk_Insert_Native_Sql_Bulk_Inserts()
    {
        var servers = new List<ServerRegistrationDto>();
        for (var i = 0; i < 1000; i++)
        {
            servers.Add(new ServerRegistrationDto
            {
                ServerAddress = "address" + i,
                ServerIdentity = "computer" + i,
                DateRegistered = DateTime.UtcNow,
                IsActive = true,
                DateAccessed = DateTime.UtcNow
            });
        }

        // Act
        using (ProfilingLogger.TraceDuration<NPocoBulkInsertTests>("starting insert", "finished insert"))
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.BulkInsertRecords(servers);
                scope.Complete();
            }
        }

        // Assert
        using (var scope = ScopeProvider.CreateScope())
        {
            Assert.That(ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoServer"), Is.EqualTo(1000));
        }
    }

    [Test]
    public void Can_Bulk_Insert_Native_Sql_Bulk_Inserts_Transaction_Rollback()
    {
        var servers = new List<ServerRegistrationDto>();
        for (var i = 0; i < 1000; i++)
        {
            servers.Add(new ServerRegistrationDto
            {
                ServerAddress = "address" + i,
                ServerIdentity = "computer" + i,
                DateRegistered = DateTime.UtcNow,
                IsActive = true,
                DateAccessed = DateTime.UtcNow
            });
        }

        // Act
        using (ProfilingLogger.TraceDuration<NPocoBulkInsertTests>("starting insert", "finished insert"))
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.BulkInsertRecords(servers);

                // Don't call complete here - the transaction will be rolled back.
            }
        }

        // Assert
        using (var scope = ScopeProvider.CreateScope())
        {
            Assert.That(ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoServer"), Is.EqualTo(0));
        }
    }

    [Test]
    public void Generate_Bulk_Import_Sql()
    {
        var servers = new List<ServerRegistrationDto>();
        for (var i = 0; i < 2; i++)
        {
            servers.Add(new ServerRegistrationDto
            {
                ServerAddress = "address" + i,
                ServerIdentity = "computer" + i,
                DateRegistered = DateTime.UtcNow,
                IsActive = true,
                DateAccessed = DateTime.UtcNow
            });
        }

        IDbCommand[] commands;
        using (var scope = ScopeProvider.CreateScope())
        {
            commands = ScopeAccessor.AmbientScope.Database.GenerateBulkInsertCommands(servers.ToArray());
            scope.Complete();
        }

        // Assert
        Assert.That(
            commands[0].CommandText,
            Is.EqualTo(
                "INSERT INTO [umbracoServer] ([umbracoServer].[address], [umbracoServer].[computerName], [umbracoServer].[registeredDate], [umbracoServer].[lastNotifiedDate], [umbracoServer].[isActive], [umbracoServer].[isSchedulingPublisher]) VALUES (@0,@1,@2,@3,@4,@5), (@6,@7,@8,@9,@10,@11)"));
    }

    [Test]
    public void Generate_Bulk_Import_Sql_Exceeding_Max_Params()
    {
        var servers = new List<ServerRegistrationDto>();
        for (var i = 0; i < 1500; i++)
        {
            servers.Add(new ServerRegistrationDto
            {
                ServerAddress = "address" + i,
                ServerIdentity = "computer" + i,
                DateRegistered = DateTime.UtcNow,
                IsActive = true,
                DateAccessed = DateTime.UtcNow,
                IsSchedulingPublisher = true
            });
        }

        IDbCommand[] commands;
        using (var scope = ScopeProvider.CreateScope())
        {
            commands = ScopeAccessor.AmbientScope.Database.GenerateBulkInsertCommands(servers.ToArray());
            scope.Complete();
        }

        // Assert
        Assert.That(commands.Length, Is.EqualTo(5));
        foreach (var s in commands.Select(x => x.CommandText))
        {
            Assert.LessOrEqual(Regex.Matches(s, "@\\d+").Count, 2000);
        }
    }

    [Test]
    public async Task InsertBulk_Does_Not_Create_Untrusted_Foreign_Key_Constraints()
    {
        if (BaseTestDatabase.IsSqlServer() is false)
        {
            Assert.Ignore("This test only applies to SQL Server (SQLite has no constraint trust concept).");
        }

        // Updating property data invokes SQL bulk insert, so we'll replicate that scenario for testing.

        // Create a content item so we have valid FK references for PropertyDataDto.
        var contentTypeService = Services.GetRequiredService<IContentTypeService>();
        var contentService = Services.GetRequiredService<IContentService>();
        var templateService = Services.GetRequiredService<ITemplateService>();

        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await templateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("testPage", "Test Page", defaultTemplateId: template.Id);
        await contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        contentService.Save(content);

        // Replicate what ContentRepositoryBase.ReplacePropertyValues does:
        // delete existing property data for the version, then re-insert via InsertBulk.
        //
        // NPoco's Database.InsertBulk() dispatches to DatabaseType.InsertBulk() →
        // SqlBulkCopyHelper.BulkInsert(). Without our UmbracoSqlServerDatabaseType override,
        // NPoco uses SqlBulkCopyOptions.Default which bypasses FK validation during SqlBulkCopy,
        // causing SQL Server to mark constraints as untrusted (is_not_trusted = 1).
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            var db = ScopeAccessor.AmbientScope!.Database;

            // Fetch existing property data for this content's version.
            var versionId = content.VersionId;
            var existing = db.Fetch<PropertyDataDto>(
                db.SqlContext.Sql()
                    .Select("*")
                    .From<PropertyDataDto>()
                    .Where<PropertyDataDto>(x => x.VersionId == versionId));
            Assert.That(existing, Has.Count.GreaterThan(0), "Content save should have created property data.");

            // Delete existing rows, then re-insert them via InsertBulk (mirroring ReplacePropertyValues).
            db.Execute(
                db.SqlContext.Sql()
                    .Delete<PropertyDataDto>()
                    .Where<PropertyDataDto>(x => x.VersionId == versionId));

            foreach (var dto in existing)
            {
                dto.Id = 0; // reset PK so InsertBulk generates new identity values
            }

            db.InsertBulk(existing);

            var untrustedCount = db.ExecuteScalar<int>(
                @"SELECT COUNT(*)
                  FROM sys.foreign_keys
                  WHERE is_not_trusted = 1
                    AND OBJECT_NAME(parent_object_id) = @0",
                "umbracoPropertyData");

            Assert.That(
                untrustedCount,
                Is.EqualTo(0),
                "FK constraints on umbracoPropertyData should be trusted after InsertBulk. " +
                "NPoco's InsertBulk with SqlBulkCopyOptions.Default causes constraints to become untrusted.");
        }
    }

    [Test]
    public async Task InsertBulkAsync_Does_Not_Create_Untrusted_Foreign_Key_Constraints()
    {
        if (BaseTestDatabase.IsSqlServer() is false)
        {
            Assert.Ignore("This test only applies to SQL Server (SQLite has no constraint trust concept).");
        }

        // Replicate what WebhookRepository does when saving a webhook with associated events:
        // insert the webhook row, then InsertBulkAsync the event associations.
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            var db = ScopeAccessor.AmbientScope!.Database;

            // Insert a parent webhook row so the FK reference is valid.
            var webhookDto = new WebhookDto
            {
                Key = Guid.NewGuid(),
                Url = "https://example.com/hook",
                Enabled = true,
            };
            await db.InsertAsync(webhookDto);

            // InsertBulkAsync the event associations (mirrors WebhookRepository.CreateAsync).
            var eventDtos = new List<Webhook2EventsDto>
            {
                new() { WebhookId = webhookDto.Id, Event = "Umbraco.ContentPublish" },
                new() { WebhookId = webhookDto.Id, Event = "Umbraco.ContentUnpublish" },
            };
            await db.InsertBulkAsync(eventDtos);

            var untrustedCount = db.ExecuteScalar<int>(
                @"SELECT COUNT(*)
                  FROM sys.foreign_keys
                  WHERE is_not_trusted = 1
                    AND OBJECT_NAME(parent_object_id) = @0",
                Constants.DatabaseSchema.Tables.Webhook2Events);

            Assert.That(
                untrustedCount,
                Is.EqualTo(0),
                "FK constraints on umbracoWebhook2Events should be trusted after InsertBulkAsync. " +
                "NPoco's InsertBulkAsync with SqlBulkCopyOptions.Default causes constraints to become untrusted.");
        }
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.NPocoTests;

// FIXME: npoco - is this still appropriate?
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class NPocoBulkInsertTests : UmbracoIntegrationTest
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
                    DateRegistered = DateTime.Now,
                    IsActive = true,
                    DateAccessed = DateTime.Now
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
                DateRegistered = DateTime.Now,
                IsActive = true,
                DateAccessed = DateTime.Now
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
                DateRegistered = DateTime.Now,
                IsActive = true,
                DateAccessed = DateTime.Now
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
                DateRegistered = DateTime.Now,
                IsActive = true,
                DateAccessed = DateTime.Now
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
                DateRegistered = DateTime.Now,
                IsActive = true,
                DateAccessed = DateTime.Now,
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    // fixme.npoco - is this still appropriate?
    //
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class NPocoExtensionsTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Bulk_Insert_One_By_One()
        {
            // Arrange
            var db = DatabaseContext.Database;

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
            using (ProfilingLogger.TraceDuration<NPocoExtensionsTest>("starting insert", "finished insert"))
            {
                using (var tr = db.GetTransaction())
                {
                    db.BulkInsertRecords(servers, false);
                    tr.Complete();
                }
            }

            // Assert
            Assert.That(db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoServer"), Is.EqualTo(1000));
        }

        [Test]
        public void Can_Bulk_Insert_One_By_One_Transaction_Rollback()
        {
            // Arrange
            var db = DatabaseContext.Database;

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
            using (ProfilingLogger.TraceDuration<NPocoExtensionsTest>("starting insert", "finished insert"))
            {
                using (var tr = db.GetTransaction())
                {
                    db.BulkInsertRecords(servers, false);
                    //don't call complete here - the trans will be rolled back
                }
            }

            // Assert
            Assert.That(db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoServer"), Is.EqualTo(0));
        }


        [NUnit.Framework.Ignore("Ignored because you need to configure your own SQL Server to test thsi with")]
        [Test]
        public void Can_Bulk_Insert_Native_Sql_Server_Bulk_Inserts()
        {
            // create the db
            // prob not what we want, this is not a real database, but hey, the test is ignored anyways
            // we'll fix this when we have proper testing infrastructure
            var dbSqlServer = TestObjects.GetUmbracoSqlServerDatabase(new DebugDiagnosticsLogger());

            //drop the table
            dbSqlServer.Execute("DROP TABLE [umbracoServer]");

            //re-create it
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

        [Test]
        public void Can_Bulk_Insert_Native_Sql_Bulk_Inserts()
        {
            // Arrange
            var db = DatabaseContext.Database;

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
            using (ProfilingLogger.TraceDuration<NPocoExtensionsTest>("starting insert", "finished insert"))
            {
                using (var tr = db.GetTransaction())
                {
                    db.BulkInsertRecords(servers);
                    tr.Complete();
                }
            }

            // Assert
            Assert.That(db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoServer"), Is.EqualTo(1000));
        }

        [Test]
        public void Can_Bulk_Insert_Native_Sql_Bulk_Inserts_Transaction_Rollback()
        {
            // Arrange
            var db = DatabaseContext.Database;

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
            using (ProfilingLogger.TraceDuration<NPocoExtensionsTest>("starting insert", "finished insert"))
            {
                using (var tr = db.GetTransaction())
                {
                    db.BulkInsertRecords(servers);
                    //don't call complete here - the trans will be rolled back
                }
            }

            // Assert
            Assert.That(db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoServer"), Is.EqualTo(0));
        }

        [Test]
        public void Generate_Bulk_Import_Sql()
        {
            // Arrange
            var db = DatabaseContext.Database;

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
            db.OpenSharedConnection();

            // Act
            var commands = db.GenerateBulkInsertCommands(servers.ToArray());
            db.CloseSharedConnection();

            // Assert
            Assert.That(commands[0].CommandText,
                        Is.EqualTo("INSERT INTO [umbracoServer] ([umbracoServer].[address], [umbracoServer].[computerName], [umbracoServer].[registeredDate], [umbracoServer].[lastNotifiedDate], [umbracoServer].[isActive], [umbracoServer].[isMaster]) VALUES (@0,@1,@2,@3,@4,@5), (@6,@7,@8,@9,@10,@11)"));
        }


        [Test]
        public void Generate_Bulk_Import_Sql_Exceeding_Max_Params()
        {
            // Arrange
            var db = DatabaseContext.Database;

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
                    IsMaster = true
                });
            }
            db.OpenSharedConnection();

            // Act
            var commands = db.GenerateBulkInsertCommands(servers.ToArray());
            db.CloseSharedConnection();

            // Assert
            Assert.That(commands.Length, Is.EqualTo(5));
            foreach (var s in commands.Select(x => x.CommandText))
            {
                Assert.LessOrEqual(Regex.Matches(s, "@\\d+").Count, 2000);
            }
        }
    }
}
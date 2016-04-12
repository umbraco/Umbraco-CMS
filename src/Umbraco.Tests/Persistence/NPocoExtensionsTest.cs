using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    // fixme.npoco - is this still appropriate?
    //
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class NPocoExtensionsTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Can_Bulk_Insert()
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
                db.BulkInsertRecords(SqlSyntax, servers);
            }

            // Assert
            Assert.That(db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoServer"), Is.EqualTo(1000));
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
            string[] sql;
            db.GenerateBulkInsertCommand(servers, db.Connection, out sql);
            db.CloseSharedConnection();

            // Assert
            Assert.That(sql[0],
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
            string[] sql;
            db.GenerateBulkInsertCommand(servers, db.Connection, out sql);
            db.CloseSharedConnection();

            // Assert
            Assert.That(sql.Length, Is.EqualTo(5));
            foreach (var s in sql)
            {
                Assert.LessOrEqual(Regex.Matches(s, "@\\d+").Count, 2000);
            }
        }
    }
}
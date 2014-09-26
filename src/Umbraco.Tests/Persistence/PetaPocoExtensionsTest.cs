using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class PetaPocoCachesTest : BaseServiceTest
    {
        [Test]
        public void Check_Peta_Poco_Caches()
        {
            var result = new List<Tuple<double, long>>();

            for (int i = 0; i < 10; i++)
            {
                int id1, id2, id3;
                string alias;
                CreateStuff(out id1, out id2, out id3, out alias);
                QueryStuff(id1, id2, id3, alias);

                double totalBytes1;
                long totalDelegates1;
                Console.Write(Database.PocoData.PrintDebugCacheReport(out totalBytes1, out totalDelegates1));

                result.Add(new Tuple<double, long>(totalBytes1, totalDelegates1));
            }

            foreach (var tuple in result)
            {
                Console.WriteLine("Bytes: {0}, Delegates: {1}", tuple.Item1, tuple.Item2);
            }

            var allByteResults = result.Select(x => x.Item1).Distinct();
            var allDelegateResults = result.Select(x => x.Item2).Distinct();

            Assert.AreEqual(1, allByteResults.Count());
            Assert.AreEqual(1, allDelegateResults.Count());
        }

        private void QueryStuff(int id1, int id2, int id3, string alias1)
        {
            var contentService = ServiceContext.ContentService;

            ServiceContext.TagService.GetTagsForEntity(id1);

            ServiceContext.TagService.GetAllContentTags();

            ServiceContext.TagService.GetTagsForEntity(id2);

            ServiceContext.TagService.GetTagsForEntity(id3);

            contentService.CountDescendants(id3);

            contentService.CountChildren(id3);

            contentService.Count(contentTypeAlias: alias1);

            contentService.Count();

            contentService.GetById(Guid.NewGuid());

            contentService.GetByLevel(2);

            contentService.GetChildren(id1);

            contentService.GetDescendants(id2);

            contentService.GetVersions(id3);

            contentService.GetRootContent();

            contentService.GetContentForExpiration();

            contentService.GetContentForRelease();

            contentService.GetContentInRecycleBin();

            ((ContentService)contentService).GetPublishedDescendants(new Content("Test", -1, new ContentType(-1))
            {
                Id = id1,
                Path = "-1," + id1
            });

            contentService.GetByVersion(Guid.NewGuid());
        }

        private void CreateStuff(out int id1, out int id2, out int id3, out string alias)
        {
            var contentService = ServiceContext.ContentService;

            var ctAlias = "umbTextpage" + Guid.NewGuid().ToString("N");
            alias = ctAlias;

            for (int i = 0; i < 20; i++)
            {
                contentService.CreateContentWithIdentity("Test", -1, "umbTextpage", 0);
            }
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType(ctAlias, "test Doc Type");
            contentTypeService.Save(contentType);
            for (int i = 0; i < 20; i++)
            {
                contentService.CreateContentWithIdentity("Test", -1, ctAlias, 0);
            }
            var parent = contentService.CreateContentWithIdentity("Test", -1, ctAlias, 0);
            id1 = parent.Id;

            for (int i = 0; i < 20; i++)
            {
                contentService.CreateContentWithIdentity("Test", parent, ctAlias);
            }
            IContent current = parent;
            for (int i = 0; i < 20; i++)
            {
                current = contentService.CreateContentWithIdentity("Test", current, ctAlias);
            }
            contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory" + Guid.NewGuid().ToString("N"), "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", DataTypeDatabaseType.Ntext)
                {
                    Alias = "tags",
                    DataTypeDefinitionId = 1041
                });
            contentTypeService.Save(contentType);
            var content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content1);
            id2 = content1.Id;

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetTags("tags", new[] { "hello", "world", "some", "tags" }, true);
            contentService.Publish(content2);
            id3 = content2.Id;

            contentService.MoveToRecycleBin(content1);
        }
    }

    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class PetaPocoExtensionsTest : BaseDatabaseFactoryTest
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
                        Address = "address" + i,
                        ComputerName = "computer" + i,
                        DateRegistered = DateTime.Now,
                        IsActive = true,
                        LastNotified = DateTime.Now
                    });
            }

            // Act
            using (DisposableTimer.TraceDuration<PetaPocoExtensionsTest>("starting insert", "finished insert"))
            {
                db.BulkInsertRecords(servers);    
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
                        Address = "address" + i,
                        ComputerName = "computer" + i,
                        DateRegistered = DateTime.Now,
                        IsActive = true,
                        LastNotified = DateTime.Now
                    });
            }
            db.OpenSharedConnection();

            // Act
            string[] sql;
            db.GenerateBulkInsertCommand(servers, db.Connection, out sql);
            db.CloseSharedConnection();

            // Assert
            Assert.That(sql[0],
                        Is.EqualTo("INSERT INTO [umbracoServer] ([umbracoServer].[address], [umbracoServer].[computerName], [umbracoServer].[registeredDate], [umbracoServer].[lastNotifiedDate], [umbracoServer].[isActive]) VALUES (@0,@1,@2,@3,@4), (@5,@6,@7,@8,@9)"));
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
                    Address = "address" + i,
                    ComputerName = "computer" + i,
                    DateRegistered = DateTime.Now,
                    IsActive = true,
                    LastNotified = DateTime.Now
                });
            }
            db.OpenSharedConnection();

            // Act
            string[] sql;
            db.GenerateBulkInsertCommand(servers, db.Connection, out sql);
            db.CloseSharedConnection();

            // Assert
            Assert.That(sql.Length, Is.EqualTo(4));
            foreach (var s in sql)
            {
                Assert.LessOrEqual(Regex.Matches(s, "@\\d+").Count, 2000);
            }
        }
    }
}
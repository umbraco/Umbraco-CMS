using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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
    [TestFixture, NUnit.Framework.Ignore]
    public class PetaPocoCachesTest : BaseServiceTest
    {
        /// <summary>
        /// This tests the peta poco caches
        /// </summary>
        /// <remarks>
        /// This test WILL fail. This is because we cannot stop PetaPoco from creating more cached items for queries such as
        ///  ContentTypeRepository.GetAll(1,2,3,4);
        /// when combined with other GetAll queries that pass in an array of Ids, each query generated for different length
        /// arrays will produce a unique query which then gets added to the cache.
        /// 
        /// This test confirms this, if you analyze the DIFFERENCE output below you can see why the cached queries grow.
        /// </remarks>
        [Test]
        public void Check_Peta_Poco_Caches()
        {
            var result = new List<Tuple<double, int, IEnumerable<string>>>();

            Database.PocoData.UseLongKeys = true;

            for (int i = 0; i < 2; i++)
            {
                int id1, id2, id3;
                string alias;
                CreateStuff(out id1, out id2, out id3, out alias);
                QueryStuff(id1, id2, id3, alias);

                double totalBytes1;
                IEnumerable<string> keys;
                Console.Write(Database.PocoData.PrintDebugCacheReport(out totalBytes1, out keys));

                result.Add(new Tuple<double, int, IEnumerable<string>>(totalBytes1, keys.Count(), keys));
            }

            for (int index = 0; index < result.Count; index++)
            {
                var tuple = result[index];
                Console.WriteLine("Bytes: {0}, Delegates: {1}", tuple.Item1, tuple.Item2);
                if (index != 0)
                {
                    Console.WriteLine("----------------DIFFERENCE---------------------");
                    var diff = tuple.Item3.Except(result[index - 1].Item3);
                    foreach (var d in diff)
                    {
                        Console.WriteLine(d);
                    }
                }
                
            }

            var allByteResults = result.Select(x => x.Item1).Distinct();
            var totalKeys = result.Select(x => x.Item2).Distinct();

            Assert.AreEqual(1, allByteResults.Count());
            Assert.AreEqual(1, totalKeys.Count());
        }

        [Test]
        public void Verify_Memory_Expires()
        {
            Database.PocoData.SlidingExpirationSeconds = 2;

            var managedCache = new Database.ManagedCache();

            int id1, id2, id3;
            string alias;
            CreateStuff(out id1, out id2, out id3, out alias);
            QueryStuff(id1, id2, id3, alias);

            var count1 = managedCache.GetCache().GetCount();
            Console.WriteLine("Keys = " + count1);
            Assert.Greater(count1, 0);
            
            Thread.Sleep(10000);

            var count2 = managedCache.GetCache().GetCount();
            Console.WriteLine("Keys = " + count2);
            Assert.Less(count2, count1);
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
                new PropertyType("test", DataTypeDatabaseType.Ntext, "tags")
                {
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
                        ServerAddress = "address" + i,
                        ServerIdentity = "computer" + i,
                        DateRegistered = DateTime.Now,
                        IsActive = true,
                        DateAccessed = DateTime.Now
                    });
            }

            // Act
            using (ProfilingLogger.TraceDuration<PetaPocoExtensionsTest>("starting insert", "finished insert"))
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
            Assert.That(sql.Length, Is.EqualTo(4));
            foreach (var s in sql)
            {
                Assert.LessOrEqual(Regex.Matches(s, "@\\d+").Count, 2000);
            }
        }
    }
}
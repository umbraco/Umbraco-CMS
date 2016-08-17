using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Content = Umbraco.Core.Models.Content;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class PublicAccessRepositoryTest : BaseDatabaseFactoryTest
    {
        [Test]
        public void Can_Delete()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repo = new PublicAccessRepository(unitOfWork, CacheHelper, Logger, MappingResolver);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.AddOrUpdate(entry);
                unitOfWork.Flush();

                repo.Delete(entry);
                unitOfWork.Flush();

                entry = repo.Get(entry.Key);
                Assert.IsNull(entry);
            }
        }

        [Test]
        public void Can_Add()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                unitOfWork.Database.EnableSqlTrace = true;
                var repo = new PublicAccessRepository(unitOfWork, CacheHelper, Logger, MappingResolver);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.AddOrUpdate(entry);
                unitOfWork.Flush();

                var found = repo.GetAll().ToArray();

                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(content[0].Id, found[0].ProtectedNodeId);
                Assert.AreEqual(content[1].Id, found[0].LoginNodeId);
                Assert.AreEqual(content[2].Id, found[0].NoAccessNodeId);
                Assert.IsTrue(found[0].HasIdentity);
                Assert.AreNotEqual(default(DateTime), found[0].CreateDate);
                Assert.AreNotEqual(default(DateTime), found[0].UpdateDate);
                Assert.AreEqual(1, found[0].Rules.Count());
                Assert.AreEqual("test", found[0].Rules.First().RuleValue);
                Assert.AreEqual("RoleName", found[0].Rules.First().RuleType);
                Assert.AreNotEqual(default(DateTime), found[0].Rules.First().CreateDate);
                Assert.AreNotEqual(default(DateTime), found[0].Rules.First().UpdateDate);
                Assert.IsTrue(found[0].Rules.First().HasIdentity);
            }
        }

        [Test]
        public void Can_Add2()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                unitOfWork.Database.EnableSqlTrace = true;
                var repo = new PublicAccessRepository(unitOfWork, CacheHelper, Logger, MappingResolver);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                    new PublicAccessRule
                    {
                        RuleValue = "test2",
                        RuleType = "RoleName2"
                    },
                });
                repo.AddOrUpdate(entry);
                unitOfWork.Flush();

                var found = repo.GetAll().ToArray();

                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(content[0].Id, found[0].ProtectedNodeId);
                Assert.AreEqual(content[1].Id, found[0].LoginNodeId);
                Assert.AreEqual(content[2].Id, found[0].NoAccessNodeId);
                Assert.IsTrue(found[0].HasIdentity);
                Assert.AreNotEqual(default(DateTime), found[0].CreateDate);
                Assert.AreNotEqual(default(DateTime), found[0].UpdateDate);
                Assert.AreEqual(2, found[0].Rules.Count());
                Assert.AreEqual("test", found[0].Rules.First().RuleValue);
                Assert.AreEqual("RoleName", found[0].Rules.First().RuleType);
                Assert.AreNotEqual(default(DateTime), found[0].Rules.First().CreateDate);
                Assert.AreNotEqual(default(DateTime), found[0].Rules.First().UpdateDate);
                Assert.IsTrue(found[0].Rules.First().HasIdentity);
                Assert.AreEqual("test2", found[0].Rules.Skip(1).First().RuleValue);
            }
        }

        [Test]
        public void Can_Update()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repo = new PublicAccessRepository(unitOfWork, CacheHelper, Logger, MappingResolver);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.AddOrUpdate(entry);
                unitOfWork.Flush();

                //re-get
                entry = repo.Get(entry.Key);

                entry.Rules.First().RuleValue = "blah";
                entry.Rules.First().RuleType = "asdf";
                repo.AddOrUpdate(entry);

                unitOfWork.Flush();

                //re-get
                entry = repo.Get(entry.Key);

                Assert.AreEqual("blah", entry.Rules.First().RuleValue);
                Assert.AreEqual("asdf", entry.Rules.First().RuleType);
            }
        }

        [Test]
        public void Get_By_Id()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repo = new PublicAccessRepository(unitOfWork, CacheHelper, Logger, MappingResolver);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.AddOrUpdate(entry);
                unitOfWork.Flush();

                //re-get
                entry = repo.Get(entry.Key);

                Assert.IsNotNull(entry);
            }
        }

        [Test]
        public void Get_All()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repo = new PublicAccessRepository(unitOfWork, CacheHelper, Logger, MappingResolver);

                var entry1 = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.AddOrUpdate(entry1);

                var entry2 = new PublicAccessEntry(content[1], content[0], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.AddOrUpdate(entry2);

                unitOfWork.Flush();

                var found = repo.GetAll().ToArray();
                Assert.AreEqual(2, found.Count());
            }
        }


        [Test]
        public void Get_All_With_Id()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repo = new PublicAccessRepository(unitOfWork, CacheHelper, Logger, MappingResolver);

                var entry1 = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.AddOrUpdate(entry1);

                var entry2 = new PublicAccessEntry(content[1], content[0], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.AddOrUpdate(entry2);

                unitOfWork.Flush();

                var found = repo.GetAll(entry1.Key).ToArray();
                Assert.AreEqual(1, found.Count());
            }
        }


        private ContentRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository)
        {
            var templateRepository = new TemplateRepository(unitOfWork, CacheHelper, Logger, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>(), MappingResolver);
            var tagRepository = new TagRepository(unitOfWork, CacheHelper, Logger, MappingResolver);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper, Logger, templateRepository, MappingResolver);
            var repository = new ContentRepository(unitOfWork, CacheHelper, Logger, contentTypeRepository, templateRepository, tagRepository, Mock.Of<IContentSection>(), MappingResolver);
            return repository;
        }

        private IEnumerable<IContent> CreateTestData(int count)
        {
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository ctRepo;
                var repo = CreateRepository(unitOfWork, out ctRepo);

                var ct = MockedContentTypes.CreateBasicContentType("testing");
                ctRepo.AddOrUpdate(ct);
                unitOfWork.Flush();
                var result = new List<IContent>();
                for (int i = 0; i < count; i++)
                {
                    var c = new Content("test" + i, -1, ct);
                    repo.AddOrUpdate(c);
                    result.Add(c);
                }
                unitOfWork.Complete();

                return result;
            }
        }
    }
}
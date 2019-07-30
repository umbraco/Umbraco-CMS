using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Content = Umbraco.Core.Models.Content;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class PublicAccessRepositoryTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Delete()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor) provider, AppCaches, Logger);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.Save(entry);


                repo.Delete(entry);


                entry = repo.Get(entry.Key);
                Assert.IsNull(entry);
            }
        }

        [Test]
        public void Can_Add()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                scope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                var repo = new PublicAccessRepository((IScopeAccessor) provider, AppCaches, Logger);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.Save(entry);


                var found = repo.GetMany().ToArray();

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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                scope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                var repo = new PublicAccessRepository((IScopeAccessor) provider, AppCaches, Logger);

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
                repo.Save(entry);


                var found = repo.GetMany().ToArray();

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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor) provider, AppCaches, Logger);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.Save(entry);


                //re-get
                entry = repo.Get(entry.Key);

                entry.Rules.First().RuleValue = "blah";
                entry.Rules.First().RuleType = "asdf";
                repo.Save(entry);



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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor) provider, AppCaches, Logger);

                var entry = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.Save(entry);


                //re-get
                entry = repo.Get(entry.Key);

                Assert.IsNotNull(entry);
            }
        }

        [Test]
        public void Get_All()
        {
            var content = CreateTestData(30).ToArray();

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor) provider, AppCaches, Logger);

                var allEntries = new List<PublicAccessEntry>();
                for (int i = 0; i < 10; i++)
                {
                    var rules = new List<PublicAccessRule>();
                    for (int j = 0; j < 50; j++)
                    {
                        rules.Add(new PublicAccessRule
                        {
                            RuleValue = "test" + j,
                            RuleType = "RoleName" + j
                        });
                    }
                    var entry1 = new PublicAccessEntry(content[i], content[i + 1], content[i + 2], rules);
                    repo.Save(entry1);

                    allEntries.Add(entry1);
                }

                //now remove a few rules from a few of the items and then add some more, this will put things 'out of order' which
                //we need to verify our sort order is working for the relator
                // FIXME: no "relator" in v8?!
                for (int i = 0; i < allEntries.Count; i++)
                {
                    //all the even ones
                    if (i % 2 == 0)
                    {
                        var rules = allEntries[i].Rules.ToArray();
                        for (int j = 0; j < rules.Length; j++)
                        {
                            //all the even ones
                            if (j % 2 == 0)
                            {
                                allEntries[i].RemoveRule(rules[j]);
                            }
                        }
                        allEntries[i].AddRule("newrule" + i, "newrule" + i);
                        repo.Save(allEntries[i]);

                    }
                }

                var found = repo.GetMany().ToArray();
                Assert.AreEqual(10, found.Length);

                foreach (var publicAccessEntry in found)
                {
                    var matched = allEntries.First(x => x.Key == publicAccessEntry.Key);

                    Assert.AreEqual(matched.Rules.Count(), publicAccessEntry.Rules.Count());
                }
            }
        }


        [Test]
        public void Get_All_With_Id()
        {
            var content = CreateTestData(3).ToArray();

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor) provider, AppCaches, Logger);

                var entry1 = new PublicAccessEntry(content[0], content[1], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.Save(entry1);

                var entry2 = new PublicAccessEntry(content[1], content[0], content[2], new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                });
                repo.Save(entry2);



                var found = repo.GetMany(entry1.Key).ToArray();
                Assert.AreEqual(1, found.Count());
            }
        }


        private DocumentRepository CreateRepository(IScopeProvider provider, out ContentTypeRepository contentTypeRepository)
        {
            var accessor = (IScopeAccessor) provider;
            var templateRepository = new TemplateRepository(accessor, AppCaches, Logger, TestObjects.GetFileSystemsMock());
            var tagRepository = new TagRepository(accessor, AppCaches, Logger);
            var commonRepository = new ContentTypeCommonRepository(accessor, templateRepository, AppCaches);
            contentTypeRepository = new ContentTypeRepository(accessor, AppCaches, Logger, commonRepository);
            var languageRepository = new LanguageRepository(accessor, AppCaches, Logger);
            var repository = new DocumentRepository(accessor, AppCaches, Logger, contentTypeRepository, templateRepository, tagRepository, languageRepository);
            return repository;
        }

        private IEnumerable<IContent> CreateTestData(int count)
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                ContentTypeRepository ctRepo;
                var repo = CreateRepository(provider, out ctRepo);

                var ct = MockedContentTypes.CreateBasicContentType("testing");
                ctRepo.Save(ct);

                var result = new List<IContent>();
                for (int i = 0; i < count; i++)
                {
                    var c = new Content("test" + i, -1, ct);
                    repo.Save(c);
                    result.Add(c);
                }
                scope.Complete();

                return result;
            }
        }
    }
}

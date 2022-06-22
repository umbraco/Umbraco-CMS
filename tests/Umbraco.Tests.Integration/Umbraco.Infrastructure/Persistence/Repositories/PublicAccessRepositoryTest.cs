// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Content = Umbraco.Cms.Core.Models.Content;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class PublicAccessRepositoryTest : UmbracoIntegrationTest
    {
        private IContentTypeRepository ContentTypeRepository => GetRequiredService<IContentTypeRepository>();

        private DocumentRepository DocumentRepository => (DocumentRepository)GetRequiredService<IDocumentRepository>();

        [Test]
        public void Can_Delete()
        {
            IContent[] content = CreateTestData(3).ToArray();

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor)provider, AppCaches, LoggerFactory.CreateLogger<PublicAccessRepository>());

                PublicAccessRule[] rules = new[]
                    {
                        new PublicAccessRule
                        {
                            RuleValue = "test",
                            RuleType = "RoleName"
                        },
                    };
                var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
                repo.Save(entry);

                repo.Delete(entry);

                entry = repo.Get(entry.Key);
                Assert.IsNull(entry);
            }
        }

        [Test]
        public void Can_Add()
        {
            IContent[] content = CreateTestData(3).ToArray();

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                var repo = new PublicAccessRepository((IScopeAccessor)provider, AppCaches, LoggerFactory.CreateLogger<PublicAccessRepository>());

                PublicAccessRule[] rules = new[]
                    {
                        new PublicAccessRule
                        {
                            RuleValue = "test",
                            RuleType = "RoleName"
                        },
                    };
                var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
                repo.Save(entry);

                PublicAccessEntry[] found = repo.GetMany().ToArray();

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
            IContent[] content = CreateTestData(3).ToArray();

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                var repo = new PublicAccessRepository((IScopeAccessor)provider, AppCaches, LoggerFactory.CreateLogger<PublicAccessRepository>());

                PublicAccessRule[] rules = new[]
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
                    };
                var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
                repo.Save(entry);

                PublicAccessEntry[] found = repo.GetMany().ToArray();

                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(content[0].Id, found[0].ProtectedNodeId);
                Assert.AreEqual(content[1].Id, found[0].LoginNodeId);
                Assert.AreEqual(content[2].Id, found[0].NoAccessNodeId);
                Assert.IsTrue(found[0].HasIdentity);
                Assert.AreNotEqual(default(DateTime), found[0].CreateDate);
                Assert.AreNotEqual(default(DateTime), found[0].UpdateDate);
                CollectionAssert.AreEquivalent(found[0].Rules, entry.Rules);
                Assert.AreNotEqual(default(DateTime), found[0].Rules.First().CreateDate);
                Assert.AreNotEqual(default(DateTime), found[0].Rules.First().UpdateDate);
                Assert.IsTrue(found[0].Rules.First().HasIdentity);
            }
        }

        [Test]
        public void Can_Update()
        {
            IContent[] content = CreateTestData(3).ToArray();

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor)provider, AppCaches, LoggerFactory.CreateLogger<PublicAccessRepository>());

                PublicAccessRule[] rules = new[]
                    {
                        new PublicAccessRule
                        {
                            RuleValue = "test",
                            RuleType = "RoleName"
                        }
                    };
                var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
                repo.Save(entry);

                // re-get
                entry = repo.Get(entry.Key);

                entry.Rules.First().RuleValue = "blah";
                entry.Rules.First().RuleType = "asdf";
                repo.Save(entry);

                // re-get
                entry = repo.Get(entry.Key);

                Assert.AreEqual("blah", entry.Rules.First().RuleValue);
                Assert.AreEqual("asdf", entry.Rules.First().RuleType);
            }
        }

        [Test]
        public void Get_By_Id()
        {
            IContent[] content = CreateTestData(3).ToArray();

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor)provider, AppCaches, LoggerFactory.CreateLogger<PublicAccessRepository>());

                PublicAccessRule[] rules = new[]
                    {
                        new PublicAccessRule
                        {
                            RuleValue = "test",
                            RuleType = "RoleName"
                        }
                    };
                var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
                repo.Save(entry);

                // re-get
                entry = repo.Get(entry.Key);

                Assert.IsNotNull(entry);
            }
        }

        [Test]
        public void Get_All()
        {
            IContent[] content = CreateTestData(30).ToArray();

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor)provider, AppCaches, LoggerFactory.CreateLogger<PublicAccessRepository>());

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

                // now remove a few rules from a few of the items and then add some more, this will put things 'out of order' which
                // we need to verify our sort order is working for the relator
                // FIXME: no "relator" in v8?!
                for (int i = 0; i < allEntries.Count; i++)
                {
                    // all the even ones
                    if (i % 2 == 0)
                    {
                        PublicAccessRule[] rules = allEntries[i].Rules.ToArray();
                        for (int j = 0; j < rules.Length; j++)
                        {
                            // all the even ones
                            if (j % 2 == 0)
                            {
                                allEntries[i].RemoveRule(rules[j]);
                            }
                        }

                        allEntries[i].AddRule("newrule" + i, "newrule" + i);
                        repo.Save(allEntries[i]);
                    }
                }

                PublicAccessEntry[] found = repo.GetMany().ToArray();
                Assert.AreEqual(10, found.Length);

                foreach (PublicAccessEntry publicAccessEntry in found)
                {
                    PublicAccessEntry matched = allEntries.First(x => x.Key == publicAccessEntry.Key);

                    Assert.AreEqual(matched.Rules.Count(), publicAccessEntry.Rules.Count());
                }
            }
        }

        [Test]
        public void Get_All_With_Id()
        {
            IContent[] content = CreateTestData(3).ToArray();

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new PublicAccessRepository((IScopeAccessor)provider, AppCaches, LoggerFactory.CreateLogger<PublicAccessRepository>());

                PublicAccessRule[] rules1 = new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                };
                var entry1 = new PublicAccessEntry(content[0], content[1], content[2], rules1);
                repo.Save(entry1);

                PublicAccessRule[] rules2 = new[]
                {
                    new PublicAccessRule
                    {
                        RuleValue = "test",
                        RuleType = "RoleName"
                    },
                };
                var entry2 = new PublicAccessEntry(content[1], content[0], content[2], rules2);
                repo.Save(entry2);

                PublicAccessEntry[] found = repo.GetMany(entry1.Key).ToArray();
                Assert.AreEqual(1, found.Count());
            }
        }

        private IEnumerable<IContent> CreateTestData(int count)
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                ContentType ct = ContentTypeBuilder.CreateBasicContentType("testing");
                ContentTypeRepository.Save(ct);

                var result = new List<IContent>();
                for (int i = 0; i < count; i++)
                {
                    var c = new Content("test" + i, -1, ct);
                    DocumentRepository.Save(c);
                    result.Add(c);
                }

                scope.Complete();

                return result;
            }
        }
    }
}

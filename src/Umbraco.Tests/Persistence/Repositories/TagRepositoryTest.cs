using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class TagRepositoryTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var tag = new Tag
                {
                    Group = "Test",
                    Text = "Test"
                };

                repository.Save(tag);

                Assert.That(tag.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_Repository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var tag = new Tag
                {
                    Group = "Test",
                    Text = "Test"
                };

                repository.Save(tag);

                var tag2 = new Tag
                {
                    Group = "Test",
                    Text = "Test2"
                };

                repository.Save(tag2);

                Assert.That(tag.HasIdentity, Is.True);
                Assert.That(tag2.HasIdentity, Is.True);
                Assert.AreNotEqual(tag.Id, tag2.Id);
            }
        }

        [Test]
        public void Can_Create_Tag_Relations()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                // create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                    }, false);

                Assert.AreEqual(2, repository.GetTagsForEntity(content.Id).Count());
            }
        }

        [Test]
        public void Can_Append_Tag_Relations()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                    }, false);

                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"},
                    }, false);

                Assert.AreEqual(4, repository.GetTagsForEntity(content.Id).Count());
            }
        }

        [Test]
        public void Can_Replace_Tag_Relations()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                    }, false);

                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"},
                    }, true);

                var result = repository.GetTagsForEntity(content.Id).ToArray();
                Assert.AreEqual(2, result.Length);
                Assert.AreEqual("tag3", result[0].Text);
                Assert.AreEqual("tag4", result[1].Text);
            }
        }

        [Test]
        public void Can_Merge_Tag_Relations()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                    }, false);

                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"},
                    }, false);

                var result = repository.GetTagsForEntity(content.Id);
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void Can_Clear_Tag_Relations()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                    }, false);

                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    Enumerable.Empty<ITag>(), true);

                var result = repository.GetTagsForEntity(content.Id);
                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void Can_Remove_Specific_Tags_From_Property()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"}
                    }, false);

                repository.Remove(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"}
                    });

                var result = repository.GetTagsForEntity(content.Id).ToArray();
                Assert.AreEqual(2, result.Length);
                Assert.AreEqual("tag1", result[0].Text);
                Assert.AreEqual("tag4", result[1].Text);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Content_By_Id()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);
                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content2);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"}
                    }, false);

                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"}
                    }, false);

                var result = repository.GetTagsForEntity(content2.Id);
                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void Can_Get_Tags_For_Content_By_Key()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);
                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content2);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"}
                    }, false);

                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"}
                    }, false);

                //get by key
                var result = repository.GetTagsForEntity(content2.Key);
                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void Can_Get_All()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);
                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content2);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"}
                    }, false);

                var result = repository.GetMany();
                Assert.AreEqual(4, result.Count());
            }
        }

        [Test]
        public void Can_Get_All_With_Ids()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);
                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content2);

                var repository = CreateRepository(provider);
                var tags = new[]
                {
                    new Tag {Text = "tag1", Group = "test"},
                    new Tag {Text = "tag2", Group = "test"},
                    new Tag {Text = "tag3", Group = "test"},
                    new Tag {Text = "tag4", Group = "test"}
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags, false);

                //TODO: This would be nice to be able to map the ids back but unfortunately we are not doing this
                //var result = repository.GetAll(new[] {tags[0].Id, tags[1].Id, tags[2].Id});
                var all = repository.GetMany().ToArray();

                var result = repository.GetMany(all[0].Id, all[1].Id, all[2].Id);
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void Can_Get_Tags_For_Content_For_Group()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);
                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content2);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test1"}
                    }, false);

                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"}
                    }, false);

                var result = repository.GetTagsForEntity(content1.Id, "test1");
                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void Can_Get_Tags_For_Property_By_Id()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"}
                    }, false);

                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"}
                    }, false);

                var result1 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.First().Alias).ToArray();
                var result2 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.Last().Alias).ToArray();
                Assert.AreEqual(4, result1.Length);
                Assert.AreEqual(2, result2.Length);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Property_By_Key()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"}
                    }, false);

                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"}
                    }, false);

                var result1 = repository.GetTagsForProperty(content1.Key, contentType.PropertyTypes.First().Alias).ToArray();
                var result2 = repository.GetTagsForProperty(content1.Key, contentType.PropertyTypes.Last().Alias).ToArray();
                Assert.AreEqual(4, result1.Length);
                Assert.AreEqual(2, result2.Length);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Property_For_Group()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test1"}
                    }, false);

                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"}
                    }, false);

                var result1 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.First().Alias, "test1").ToArray();
                var result2 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.Last().Alias, "test1").ToArray();

                Assert.AreEqual(2, result1.Length);
                Assert.AreEqual(1, result2.Length);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Entity_Type()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);
                var mediaRepository = CreateMediaRepository(provider, out var mediaTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);

                var mediaType = MockedContentTypes.CreateImageMediaType("image2");
                mediaTypeRepository.Save(mediaType);

                var media1 = MockedMedia.CreateMediaImage(mediaType, -1);
                mediaRepository.Save(media1);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"},
                        new Tag {Text = "tag3", Group = "test"}
                    }, false);

                repository.Assign(
                    media1.Id,
                    mediaType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag4", Group = "test1"}
                    }, false);

                var result1 = repository.GetTagsForEntityType(TaggableObjectTypes.Content).ToArray();
                var result2 = repository.GetTagsForEntityType(TaggableObjectTypes.Media).ToArray();
                var result3 = repository.GetTagsForEntityType(TaggableObjectTypes.All).ToArray();

                Assert.AreEqual(3, result1.Length);
                Assert.AreEqual(2, result2.Length);
                Assert.AreEqual(4, result3.Length);

                Assert.AreEqual(1, result1.Single(x => x.Text == "tag1").NodeCount);
                Assert.AreEqual(2, result3.Single(x => x.Text == "tag1").NodeCount);
                Assert.AreEqual(1, result3.Single(x => x.Text == "tag4").NodeCount);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Entity_Type_For_Group()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);
                var mediaRepository = CreateMediaRepository(provider, out var mediaTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);

                var mediaType = MockedContentTypes.CreateImageMediaType("image2");
                mediaTypeRepository.Save(mediaType);

                var media1 = MockedMedia.CreateMediaImage(mediaType, -1);
                mediaRepository.Save(media1);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test1"}
                    }, false);

                repository.Assign(
                    media1.Id,
                    mediaType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"}
                    }, false);

                var result1 = repository.GetTagsForEntityType(TaggableObjectTypes.Content,  "test1").ToArray();
                var result2 = repository.GetTagsForEntityType(TaggableObjectTypes.Media, "test1").ToArray();

                Assert.AreEqual(2, result1.Length);
                Assert.AreEqual(1, result2.Length);
            }
        }

        [Test]
        public void Cascade_Deletes_Tag_Relations()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                        {
                            new Tag {Text = "tag1", Group = "test"},
                            new Tag {Text = "tag2", Group = "test"},
                            new Tag {Text = "tag3", Group = "test"},
                            new Tag {Text = "tag4", Group = "test"}
                        }, false);

                contentRepository.Delete(content1);

                Assert.AreEqual(0, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = content1.Id, propTypeId = contentType.PropertyTypes.First().Id }));
            }
        }

        [Test]
        public void Can_Get_Tagged_Entities_For_Tag_Group()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);
                var mediaRepository = CreateMediaRepository(provider, out var mediaTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);

                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);

                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content2);

                var mediaType = MockedContentTypes.CreateImageMediaType("image2");
                mediaTypeRepository.Save(mediaType);

                var media1 = MockedMedia.CreateMediaImage(mediaType, -1);
                mediaRepository.Save(media1);

                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"},
                        new Tag {Text = "tag3", Group = "test"}
                    }, false);

                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"},
                        new Tag {Text = "tag3", Group = "test"}
                    }, false);

                repository.Assign(
                    media1.Id,
                    mediaType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"}
                    }, false);

                var contentTestIds = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Content, "test").ToArray();
                //there are two content items tagged against the 'test' group
                Assert.AreEqual(2, contentTestIds.Length);
                //there are a total of two property types tagged against the 'test' group
                Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).Count());
                //there are a total of 2 tags tagged against the 'test' group
                Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                var contentTest1Ids = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Content, "test1").ToArray();
                //there are two content items tagged against the 'test1' group
                Assert.AreEqual(2, contentTest1Ids.Length);
                //there are a total of two property types tagged against the 'test1' group
                Assert.AreEqual(2, contentTest1Ids.SelectMany(x => x.TaggedProperties).Count());
                //there are a total of 1 tags tagged against the 'test1' group
                Assert.AreEqual(1, contentTest1Ids.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                var mediaTestIds = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Media, "test");
                Assert.AreEqual(1, mediaTestIds.Count());

                var mediaTest1Ids = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Media, "test1");
                Assert.AreEqual(1, mediaTest1Ids.Count());
            }
        }

        [Test]
        public void Can_Get_Tagged_Entities_For_Tag()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (ScopeProvider.CreateScope())
            {
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepository);
                var mediaRepository = CreateMediaRepository(provider, out var mediaTypeRepository);

                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);


                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content1);


                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.Save(content2);


                var mediaType = MockedContentTypes.CreateImageMediaType("image2");
                mediaTypeRepository.Save(mediaType);

                var media1 = MockedMedia.CreateMediaImage(mediaType, -1);
                mediaRepository.Save(media1);


                var repository = CreateRepository(provider);
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"},
                        new Tag {Text = "tag3", Group = "test"}
                    }, false);

                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"},
                    }, false);

                repository.Assign(
                    media1.Id,
                    mediaType.PropertyTypes.Last().Id,
                    new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test1"}
                    }, false);

                var contentTestIds = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Content, "tag1").ToArray();
                //there are two content items tagged against the 'tag1' tag
                Assert.AreEqual(2, contentTestIds.Length);
                //there are a total of two property types tagged against the 'tag1' tag
                Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).Count());
                //there are a total of 1 tags since we're only looking against one tag
                Assert.AreEqual(1, contentTestIds.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                var contentTest1Ids = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Content, "tag3").ToArray();
                //there are 1 content items tagged against the 'tag3' tag
                Assert.AreEqual(1, contentTest1Ids.Length);
                //there are a total of two property types tagged against the 'tag3' tag
                Assert.AreEqual(1, contentTest1Ids.SelectMany(x => x.TaggedProperties).Count());
                //there are a total of 1 tags since we're only looking against one tag
                Assert.AreEqual(1, contentTest1Ids.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                var mediaTestIds = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Media, "tag1");
                Assert.AreEqual(1, mediaTestIds.Count());
            }
        }

        private TagRepository CreateRepository(IScopeProvider provider)
        {
            return new TagRepository((IScopeAccessor) provider, AppCaches.Disabled, Logger);
        }

        private DocumentRepository CreateContentRepository(IScopeProvider provider, out ContentTypeRepository contentTypeRepository)
        {
            var accessor = (IScopeAccessor) provider;
            var templateRepository = new TemplateRepository(accessor, AppCaches.Disabled, Logger, TestObjects.GetFileSystemsMock());
            var tagRepository = new TagRepository(accessor, AppCaches.Disabled, Logger);
            contentTypeRepository = new ContentTypeRepository(accessor, AppCaches.Disabled, Logger, templateRepository);
            var languageRepository = new LanguageRepository(accessor, AppCaches.Disabled, Logger);
            var repository = new DocumentRepository(accessor, AppCaches.Disabled, Logger, contentTypeRepository, templateRepository, tagRepository, languageRepository, Mock.Of<IContentSection>());
            return repository;
        }

        private MediaRepository CreateMediaRepository(IScopeProvider provider, out MediaTypeRepository mediaTypeRepository)
        {
            var accessor = (IScopeAccessor) provider;
            var tagRepository = new TagRepository(accessor, AppCaches.Disabled, Logger);
            mediaTypeRepository = new MediaTypeRepository(accessor, AppCaches.Disabled, Logger);
            var repository = new MediaRepository(accessor, AppCaches.Disabled, Logger, mediaTypeRepository, tagRepository, Mock.Of<IContentSection>(), Mock.Of<ILanguageRepository>());
            return repository;
        }
    }
}

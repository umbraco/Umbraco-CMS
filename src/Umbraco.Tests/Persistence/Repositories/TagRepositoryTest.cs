using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class TagRepositoryTest : BaseDatabaseFactoryTest
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

        private TagsRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            var tagRepository = new TagsRepository(unitOfWork, NullCacheProvider.Current);
            return tagRepository;
        }

        [Test]
        public void Can_Instantiate_Repository_From_Resolver()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = RepositoryResolver.Current.ResolveByType<ITagsRepository>(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var tag = new Tag()
                    {
                        Group = "Test",
                        Text = "Test"
                    };

                // Act
                repository.AddOrUpdate(tag);
                unitOfWork.Commit();

                // Assert
                Assert.That(tag.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var tag = new Tag()
                    {
                        Group = "Test",
                        Text = "Test"
                    };

                // Act
                repository.AddOrUpdate(tag);
                unitOfWork.Commit();

                var tag2 = new Tag()
                    {
                        Group = "Test",
                        Text = "Test2"
                    };
                repository.AddOrUpdate(tag2);
                unitOfWork.Commit();

                // Assert
                Assert.That(tag.HasIdentity, Is.True);
                Assert.That(tag2.HasIdentity, Is.True);
                Assert.AreNotEqual(tag.Id, tag2.Id);
            }

        }

        [Test]
        public void Can_Create_Tag_Relations()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
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
        }

        [Test]
        public void Can_Append_Tag_Relations()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    repository.AssignTagsToProperty(
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
        }

        [Test]
        public void Can_Replace_Tag_Relations()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"},
                            }, true);

                    var result = repository.GetTagsForEntity(content.Id);
                    Assert.AreEqual(2, result.Count());
                    Assert.AreEqual("tag3", result.ElementAt(0).Text);
                    Assert.AreEqual("tag4", result.ElementAt(1).Text);
                }
            }
        }

        [Test]
        public void Can_Merge_Tag_Relations()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    repository.AssignTagsToProperty(
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
        }

        [Test]
        public void Can_Clear_Tag_Relations()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Id,
                        Enumerable.Empty<ITag>(), true);                    

                    var result = repository.GetTagsForEntity(content.Id);
                    Assert.AreEqual(0, result.Count());
                }
            }
        }

        [Test]
        public void Can_Remove_Specific_Tags_From_Property()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"}
                            }, false);

                    repository.RemoveTagsFromProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"}                             
                            });

                    var result = repository.GetTagsForEntity(content.Id);
                    Assert.AreEqual(2, result.Count());
                    Assert.AreEqual("tag1", result.ElementAt(0).Text);
                    Assert.AreEqual("tag4", result.ElementAt(1).Text);
                }
            }
        }

        [Test]
        public void Can_Get_Tags_For_Content()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content1);
                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content2);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"}
                            }, false);

                    repository.AssignTagsToProperty(
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
        }

        [Test]
        public void Can_Get_Tags_For_Content_For_Group()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content1);
                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content2);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test1"}
                            }, false);

                    repository.AssignTagsToProperty(
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
        }

        [Test]
        public void Can_Get_Tags_For_Property()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content1);                
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"}
                            }, false);

                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.Last().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"}
                            }, false);

                    var result1 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.First().Alias).ToArray();
                    var result2 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.Last().Alias).ToArray();
                    Assert.AreEqual(4, result1.Count());
                    Assert.AreEqual(2, result2.Count());
                }
            }

        }

        [Test]
        public void Can_Get_Tags_For_Property_For_Group()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content1);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test1"}
                            }, false);

                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.Last().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"}
                            }, false);

                    var result1 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.First().Alias, "test1").ToArray();
                    var result2 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.Last().Alias, "test1").ToArray();

                    Assert.AreEqual(2, result1.Count());
                    Assert.AreEqual(1, result2.Count());

                }
            }

        }

        [Test]
        public void Can_Get_Tags_For_Entity_Type()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MediaTypeRepository mediaTypeRepository;
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            using (var mediaRepository = CreateMediaRepository(unitOfWork, out mediaTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content1);
                unitOfWork.Commit();
                var mediaType = MockedContentTypes.CreateImageMediaType("image2");
                mediaTypeRepository.AddOrUpdate(mediaType);
                unitOfWork.Commit();
                var media1 = MockedMedia.CreateMediaImage(mediaType, -1);
                mediaRepository.AddOrUpdate(media1);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"},
                                new Tag {Text = "tag3", Group = "test"}
                            }, false);

                    repository.AssignTagsToProperty(
                        media1.Id,
                        mediaType.PropertyTypes.Last().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"}
                            }, false);

                    var result1 = repository.GetTagsForEntityType(TaggableObjectTypes.Content).ToArray();
                    var result2 = repository.GetTagsForEntityType(TaggableObjectTypes.Media).ToArray();

                    Assert.AreEqual(3, result1.Count());
                    Assert.AreEqual(2, result2.Count());

                }
            }

        }

        [Test]
        public void Can_Get_Tags_For_Entity_Type_For_Group()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MediaTypeRepository mediaTypeRepository;
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            using (var mediaRepository = CreateMediaRepository(unitOfWork, out mediaTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content1);
                unitOfWork.Commit();
                var mediaType = MockedContentTypes.CreateImageMediaType("image2");
                mediaTypeRepository.AddOrUpdate(mediaType);
                unitOfWork.Commit();
                var media1 = MockedMedia.CreateMediaImage(mediaType, -1);
                mediaRepository.AddOrUpdate(media1);
                unitOfWork.Commit();
                
                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.First().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test1"}
                            }, false);

                    repository.AssignTagsToProperty(
                        media1.Id,
                        mediaType.PropertyTypes.Last().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"}
                            }, false);

                    var result1 = repository.GetTagsForEntityType(TaggableObjectTypes.Content,  "test1").ToArray();
                    var result2 = repository.GetTagsForEntityType(TaggableObjectTypes.Media, "test1").ToArray();

                    Assert.AreEqual(2, result1.Count());
                    Assert.AreEqual(1, result2.Count());

                }
            }

        }

        [Test]
        public void Cascade_Deletes_Tag_Relations()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var contentRepository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                //create data to relate to
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();
                var content1 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content1);
                unitOfWork.Commit();

                using (var repository = CreateRepository(unitOfWork))
                {
                    repository.AssignTagsToProperty(
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

                    unitOfWork.Commit();

                    Assert.AreEqual(0, DatabaseContext.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                        new { nodeId = content1.Id, propTypeId = contentType.PropertyTypes.First().Id }));
                }
            }

        }

        private ContentRepository CreateContentRepository(IDatabaseUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository)
        {
            var templateRepository = new TemplateRepository(unitOfWork, NullCacheProvider.Current);
            var tagRepository = new TagsRepository(unitOfWork, NullCacheProvider.Current);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, NullCacheProvider.Current, templateRepository);
            var repository = new ContentRepository(unitOfWork, NullCacheProvider.Current, contentTypeRepository, templateRepository, tagRepository, CacheHelper.CreateDisabledCacheHelper());
            return repository;
        }

        private MediaRepository CreateMediaRepository(IDatabaseUnitOfWork unitOfWork, out MediaTypeRepository mediaTypeRepository)
        {
            var tagRepository = new TagsRepository(unitOfWork, NullCacheProvider.Current);
            mediaTypeRepository = new MediaTypeRepository(unitOfWork, NullCacheProvider.Current);
            var repository = new MediaRepository(unitOfWork, NullCacheProvider.Current, mediaTypeRepository, tagRepository);
            return repository;
        }
    }
}
using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

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

        private TagRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            var tagRepository = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            return tagRepository;
        }

        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
        public void Can_Get_All()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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

                    var result = repository.GetAll();
                    Assert.AreEqual(4, result.Count());
                }
            }
        }

        [Test]
        public void Can_Get_All_With_Ids()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
                    var tags = new[]
                    {
                        new Tag {Text = "tag1", Group = "test"},
                        new Tag {Text = "tag2", Group = "test"},
                        new Tag {Text = "tag3", Group = "test"},
                        new Tag {Text = "tag4", Group = "test"}
                    };
                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.First().Id,
                        tags, false);

                    //TODO: This would be nice to be able to map the ids back but unfortunately we are not doing this
                    //var result = repository.GetAll(new[] {tags[0].Id, tags[1].Id, tags[2].Id});
                    var all = repository.GetAll().ToArray();

                    var result = repository.GetAll(new[] { all[0].Id, all[1].Id, all[2].Id });
                    Assert.AreEqual(3, result.Count());
                }
            }
        }

        [Test]
        public void Can_Get_Tags_For_Content_For_Group()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
                                new Tag {Text = "tag4", Group = "test1"}
                            }, false);

                    var result1 = repository.GetTagsForEntityType(TaggableObjectTypes.Content).ToArray();
                    var result2 = repository.GetTagsForEntityType(TaggableObjectTypes.Media).ToArray();
                    var result3 = repository.GetTagsForEntityType(TaggableObjectTypes.All).ToArray();

                    Assert.AreEqual(3, result1.Count());
                    Assert.AreEqual(2, result2.Count());
                    Assert.AreEqual(4, result3.Count());

                    Assert.AreEqual(1, result1.Single(x => x.Text == "tag1").NodeCount);
                    Assert.AreEqual(2, result3.Single(x => x.Text == "tag1").NodeCount);
                    Assert.AreEqual(1, result3.Single(x => x.Text == "tag4").NodeCount);

                }
            }

        }

        [Test]
        public void Can_Get_Tags_For_Entity_Type_For_Group()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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

        [Test]
        public void Can_Get_Tagged_Entities_For_Tag_Group()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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

                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content2);
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
                        content2.Id,
                        contentType.PropertyTypes.Last().Id,
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

                    var contentTestIds = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Content, "test").ToArray();
                    //there are two content items tagged against the 'test' group
                    Assert.AreEqual(2, contentTestIds.Count());
                    //there are a total of two property types tagged against the 'test' group
                    Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).Count());
                    //there are a total of 2 tags tagged against the 'test' group
                    Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                    var contentTest1Ids = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Content, "test1").ToArray();
                    //there are two content items tagged against the 'test1' group
                    Assert.AreEqual(2, contentTest1Ids.Count());
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

        }

        [Test]
        public void Can_Get_Tagged_Entities_For_Tag()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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

                var content2 = MockedContent.CreateSimpleContent(contentType);
                contentRepository.AddOrUpdate(content2);
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
                        content2.Id,
                        contentType.PropertyTypes.Last().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"},
                            }, false);

                    repository.AssignTagsToProperty(
                        media1.Id,
                        mediaType.PropertyTypes.Last().Id,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test1"}
                            }, false);

                    var contentTestIds = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Content, "tag1").ToArray();
                    //there are two content items tagged against the 'tag1' tag
                    Assert.AreEqual(2, contentTestIds.Count());
                    //there are a total of two property types tagged against the 'tag1' tag
                    Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).Count());
                    //there are a total of 1 tags since we're only looking against one tag
                    Assert.AreEqual(1, contentTestIds.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                    var contentTest1Ids = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Content, "tag3").ToArray();
                    //there are 1 content items tagged against the 'tag3' tag
                    Assert.AreEqual(1, contentTest1Ids.Count());
                    //there are a total of two property types tagged against the 'tag3' tag
                    Assert.AreEqual(1, contentTest1Ids.SelectMany(x => x.TaggedProperties).Count());
                    //there are a total of 1 tags since we're only looking against one tag
                    Assert.AreEqual(1, contentTest1Ids.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                    var mediaTestIds = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Media, "tag1");
                    Assert.AreEqual(1, mediaTestIds.Count());
                    
                }
            }

        }

        private ContentRepository CreateContentRepository(IDatabaseUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository)
        {
            var templateRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var tagRepository = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, templateRepository);
            var repository = new ContentRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, contentTypeRepository, templateRepository, tagRepository);
            return repository;
        }

        private MediaRepository CreateMediaRepository(IDatabaseUnitOfWork unitOfWork, out MediaTypeRepository mediaTypeRepository)
        {
            var tagRepository = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            mediaTypeRepository = new MediaTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            var repository = new MediaRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, mediaTypeRepository, tagRepository);
            return repository;
        }
    }
}
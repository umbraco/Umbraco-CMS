using System;
using System.Linq;
using NUnit.Framework;
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
        public void Cannot_Assign_Tags_To_Non_Existing_Property()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                Assert.Throws<InvalidOperationException>(() => repository.AssignTagsToProperty(1234, "hello", Enumerable.Empty<ITag>(), true));
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
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    Assert.AreEqual(2, repository.GetTagsForContent(content.Id).Count());
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
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"},
                            }, false);

                    Assert.AreEqual(4, repository.GetTagsForContent(content.Id).Count());
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
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"},
                            }, true);

                    var result = repository.GetTagsForContent(content.Id);
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
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"},
                            }, false);

                    var result = repository.GetTagsForContent(content.Id);
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
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                            }, false);

                    repository.AssignTagsToProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Alias,
                        Enumerable.Empty<ITag>(), true);                    

                    var result = repository.GetTagsForContent(content.Id);
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
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"}
                            }, false);

                    repository.RemoveTagsFromProperty(
                        content.Id,
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"}                             
                            });

                    var result = repository.GetTagsForContent(content.Id);
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
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"}
                            }, false);

                    repository.AssignTagsToProperty(
                        content2.Id,
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"}
                            }, false);

                    var result = repository.GetTagsForContent(content2.Id);
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
                        contentType.PropertyTypes.First().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"},
                                new Tag {Text = "tag3", Group = "test"},
                                new Tag {Text = "tag4", Group = "test"}
                            }, false);

                    repository.AssignTagsToProperty(
                        content1.Id,
                        contentType.PropertyTypes.Last().Alias,
                        new[]
                            {
                                new Tag {Text = "tag1", Group = "test"},
                                new Tag {Text = "tag2", Group = "test"}
                            }, false);

                    var result1 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.First().Alias).ToArray();
                    var result2 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.Last().Alias).ToArray();
                    Assert.AreEqual(2, result2.Count());
                }
            }

        }

        private ContentRepository CreateContentRepository(IDatabaseUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository)
        {
            var templateRepository = new TemplateRepository(unitOfWork, NullCacheProvider.Current);
            var tagRepository = new TagsRepository(unitOfWork, NullCacheProvider.Current);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, NullCacheProvider.Current, templateRepository);
            var repository = new ContentRepository(unitOfWork, NullCacheProvider.Current, contentTypeRepository, templateRepository, tagRepository);
            return repository;
        }
    }
}
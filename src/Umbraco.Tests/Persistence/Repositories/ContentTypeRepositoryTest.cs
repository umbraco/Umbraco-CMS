using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [RequiresAutoMapperMappings]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class ContentTypeRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        private ContentRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository)
        {
            var templateRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var tagRepository = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, templateRepository);
            var repository = new ContentRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, contentTypeRepository, templateRepository, tagRepository, Mock.Of<IContentSection>());
            return repository;
        }

        private ContentTypeRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            var templateRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, templateRepository);
            return contentTypeRepository;
        }

        private MediaTypeRepository CreateMediaTypeRepository(IDatabaseUnitOfWork unitOfWork)
        {
            var contentTypeRepository = new MediaTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            return contentTypeRepository;
        }

        private EntityContainerRepository CreateContainerRepository(IDatabaseUnitOfWork unitOfWork, Guid containerEntityType)
        {
            return new EntityContainerRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, containerEntityType);
        }

        //TODO Add test to verify SetDefaultTemplates updates both AllowedTemplates and DefaultTemplate(id).


        [Test]
        public void Maps_Templates_Correctly()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var templateRepo = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>()))
            using (var repository = CreateRepository(unitOfWork))
            {
                var templates = new[]
                {
                    new Template("test1.cshtml", "test1", "test1"),
                    new Template("test2.cshtml", "test2", "test2"),
                    new Template("test3.cshtml", "test3", "test3")
                };
                foreach (var template in templates)
                {
                    templateRepo.AddOrUpdate(template);
                }
                unitOfWork.Commit();

                var contentType = MockedContentTypes.CreateSimpleContentType();
                contentType.AllowedTemplates = new[] {templates[0], templates[1]};
                contentType.SetDefaultTemplate(templates[0]);
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                //re-get
                var result = repository.Get(contentType.Id);

                Assert.AreEqual(2, result.AllowedTemplates.Count());
                Assert.AreEqual(templates[0].Id, result.DefaultTemplate.Id);
            }

        }

        [Test]
        public void Can_Move()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.DocumentTypeContainerGuid))
            using (var repository = CreateRepository(unitOfWork))
            {
                var container1 = new EntityContainer(Constants.ObjectTypes.DocumentTypeGuid) { Name = "blah1" };
                containerRepository.AddOrUpdate(container1);
                unitOfWork.Commit();

                var container2 = new EntityContainer(Constants.ObjectTypes.DocumentTypeGuid) { Name = "blah2", ParentId = container1.Id };
                containerRepository.AddOrUpdate(container2);
                unitOfWork.Commit();

                var contentType = (IContentType) MockedContentTypes.CreateBasicContentType("asdfasdf");
                contentType.ParentId = container2.Id;
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                //create a
                var contentType2 = (IContentType)new ContentType(contentType, "hello")
                {
                    Name = "Blahasdfsadf"
                };
                contentType.ParentId = contentType.Id;
                repository.AddOrUpdate(contentType2);
                unitOfWork.Commit();

                var result = repository.Move(contentType, container1).ToArray();
                unitOfWork.Commit();

                Assert.AreEqual(2, result.Count());

                //re-get
                contentType = repository.Get(contentType.Id);
                contentType2 = repository.Get(contentType2.Id);

                Assert.AreEqual(container1.Id, contentType.ParentId);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType.Id).OriginalPath, contentType.Path);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType2.Id).OriginalPath, contentType2.Path);
            }

        }

        [Test]
        public void Can_Create_Container()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            EntityContainer container;
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.DocumentTypeContainerGuid))
            {
                container = new EntityContainer(Constants.ObjectTypes.DocumentTypeGuid) { Name = "blah" };
                containerRepository.AddOrUpdate(container);
                unitOfWork.Commit();
                Assert.That(container.Id, Is.GreaterThan(0));
            }
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.DocumentTypeContainerGuid))
            {
                var found = containerRepository.Get(container.Id);
                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Can_Delete_Container()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            EntityContainer container;
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.DocumentTypeContainerGuid))
            {
                container = new EntityContainer(Constants.ObjectTypes.DocumentTypeGuid) { Name = "blah" };
                containerRepository.AddOrUpdate(container);
                unitOfWork.Commit();
            }
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.DocumentTypeContainerGuid))
            {
                // Act
                containerRepository.Delete(container);
                unitOfWork.Commit();
            }
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.DocumentTypeContainerGuid))
            {
                var found = containerRepository.Get(container.Id);
                Assert.IsNull(found);
            }
        }

        [Test]
        public void Can_Create_Container_Containing_Media_Types()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.MediaTypeContainerGuid))
            using (var repository = CreateRepository(unitOfWork))
            {
                var container = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid) { Name = "blah" };
                containerRepository.AddOrUpdate(container);
                unitOfWork.Commit();

                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test", propertyGroupName: "testGroup");
                contentType.ParentId = container.Id;
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                Assert.AreEqual(container.Id, contentType.ParentId);
            }
        }

        [Test]
        public void Can_Delete_Container_Containing_Media_Types()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            EntityContainer container;
            IMediaType contentType;
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.MediaTypeContainerGuid))
            using (var repository = CreateMediaTypeRepository(unitOfWork))
            {
                container = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid) { Name = "blah" };
                containerRepository.AddOrUpdate(container);
                unitOfWork.Commit();

                contentType = MockedContentTypes.CreateSimpleMediaType("test", "Test", propertyGroupName: "testGroup");
                contentType.ParentId = container.Id;
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();
            }
            using (var containerRepository = CreateContainerRepository(unitOfWork, Constants.ObjectTypes.MediaTypeContainerGuid))
            using (var repository = CreateMediaTypeRepository(unitOfWork))
            {
                // Act
                containerRepository.Delete(container);
                unitOfWork.Commit();

                var found = containerRepository.Get(container.Id);
                Assert.IsNull(found);

                contentType = repository.Get(contentType.Id);
                Assert.IsNotNull(contentType);
                Assert.AreEqual(-1, contentType.ParentId);
            }
        }

        [Test]
        public void Can_Perform_Add_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var contentType = MockedContentTypes.CreateSimpleContentType("test", "Test", propertyGroupName: "testGroup");
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.PropertyTypes.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.Path.Contains(","), Is.True);
                Assert.That(contentType.SortOrder, Is.GreaterThan(0));

                Assert.That(contentType.PropertyGroups.ElementAt(0).Name == "testGroup", Is.True);
                var groupId = contentType.PropertyGroups.ElementAt(0).Id;
                Assert.That(contentType.PropertyTypes.All(x => x.PropertyGroupId.Value == groupId), Is.True);

                foreach (var propertyType in contentType.PropertyTypes)
                {
                    Assert.AreNotEqual(propertyType.Key, Guid.Empty);
                }
            }
        }

        [Test]
        public void Can_Perform_Add_On_ContentTypeRepository_After_Model_Mapping()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var contentType = (IContentType)MockedContentTypes.CreateSimpleContentType2("test", "Test", propertyGroupName: "testGroup");

                Assert.AreEqual(4, contentType.PropertyTypes.Count());

                // there is NO mapping from display to contentType, but only from save
                // to contentType, so if we want to test, let's to it properly!
                var display = Mapper.Map<DocumentTypeDisplay>(contentType);
                var save = MapToContentTypeSave(display);
                var mapped = Mapper.Map<IContentType>(save);

                Assert.AreEqual(4, mapped.PropertyTypes.Count());

                repository.AddOrUpdate(mapped);
                unitOfWork.Commit();

                Assert.AreEqual(4, mapped.PropertyTypes.Count());

                //re-get
                contentType = repository.Get(mapped.Id);

                Assert.AreEqual(4, contentType.PropertyTypes.Count());

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.PropertyTypes.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.Path.Contains(","), Is.True);
                Assert.That(contentType.SortOrder, Is.GreaterThan(0));

                Assert.That(contentType.PropertyGroups.ElementAt(0).Name == "testGroup", Is.True);
                var groupId = contentType.PropertyGroups.ElementAt(0).Id;

                var propertyTypes = contentType.PropertyTypes.ToArray();
                Assert.AreEqual("gen", propertyTypes[0].Alias); // just to be sure
                Assert.IsNull(propertyTypes[0].PropertyGroupId);
                Assert.IsTrue(propertyTypes.Skip(1).All((x => x.PropertyGroupId.Value == groupId)));
            }

        }

        [Test]
        public void Can_Perform_Update_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                contentType.Thumbnail = "Doc2.png";
            contentType.PropertyGroups["Content"].PropertyTypes.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "subtitle")
                {
                    Name = "Subtitle",
                    Description = "Optional Subtitle",
                    Mandatory = false,
                    SortOrder = 1,
                    DataTypeDefinitionId = -88
                });
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                var dirty = ((ICanBeDirty)contentType).IsDirty();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(dirty, Is.False);
                Assert.That(contentType.Thumbnail, Is.EqualTo("Doc2.png"));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);
            }


        }

        // this is for tests only because it makes no sense at all to have such a
        // mapping defined, we only need it for the weird tests that use it
        private DocumentTypeSave MapToContentTypeSave(DocumentTypeDisplay display)
        {
            return new DocumentTypeSave
            {
                // EntityBasic
                Name = display.Name,
                Icon = display.Icon,
                Trashed = display.Trashed,
                Key = display.Key,
                ParentId = display.ParentId,
                //Alias = display.Alias,
                Path = display.Path,
                //AdditionalData = display.AdditionalData,

                // ContentTypeBasic
                Alias = display.Alias,
                UpdateDate = display.UpdateDate,
                CreateDate = display.CreateDate,
                Description = display.Description,
                Thumbnail = display.Thumbnail,

                // ContentTypeSave
                CompositeContentTypes = display.CompositeContentTypes,
                IsContainer = display.IsContainer,
                AllowAsRoot = display.AllowAsRoot,
                AllowedTemplates = display.AllowedTemplates.Select(x => x.Alias),
                AllowedContentTypes = display.AllowedContentTypes,
                DefaultTemplate = display.DefaultTemplate == null ? null : display.DefaultTemplate.Alias,
                Groups = display.Groups.Select(x => new PropertyGroupBasic<PropertyTypeBasic>
                {
                    Inherited = x.Inherited,
                    Id = x.Id,
                    Properties = x.Properties,
                    SortOrder = x.SortOrder,
                    Name = x.Name
                }).ToArray()
            };
        }

        [Test]
        public void Can_Perform_Update_On_ContentTypeRepository_After_Model_Mapping()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                // there is NO mapping from display to contentType, but only from save
                // to contentType, so if we want to test, let's to it properly!
                var display = Mapper.Map<DocumentTypeDisplay>(contentType);
                var save = MapToContentTypeSave(display);

                // modify...
                save.Thumbnail = "Doc2.png";
                var contentGroup = save.Groups.Single(x => x.Name == "Content");
                contentGroup.Properties = contentGroup.Properties.Concat(new[]
                {
                    new PropertyTypeBasic
                    {
                        Alias = "subtitle",
                        Label = "Subtitle",
                        Description = "Optional Subtitle",
                        Validation = new PropertyTypeValidation
                        {
                            Mandatory = false,
                            Pattern = ""
                        },
                        SortOrder = 1,
                        DataTypeId = -88
                    }
                });

                var mapped = Mapper.Map(save, contentType);

                // just making sure
                Assert.AreEqual(mapped.Thumbnail, "Doc2.png");
                Assert.IsTrue(mapped.PropertyTypes.Any(x => x.Alias == "subtitle"));

                repository.AddOrUpdate(mapped);
                unitOfWork.Commit();

                var dirty = mapped.IsDirty();

                //re-get
                contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(dirty, Is.False);
                Assert.That(contentType.Thumbnail, Is.EqualTo("Doc2.png"));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);
                foreach (var propertyType in contentType.PropertyTypes)
                {
                    Assert.IsTrue(propertyType.HasIdentity);
                    Assert.Greater(propertyType.Id, 0);
                }
            }
        }

        [Test]
        public void Can_Perform_Delete_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var contentType = MockedContentTypes.CreateSimpleContentType();
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                var contentType2 = repository.Get(contentType.Id);
                repository.Delete(contentType2);
                unitOfWork.Commit();

                var exists = repository.Exists(contentType.Id);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Delete_With_Heirarchy_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var ctMain = MockedContentTypes.CreateSimpleContentType();
                var ctChild1 = MockedContentTypes.CreateSimpleContentType("child1", "Child 1", ctMain, true);
                var ctChild2 = MockedContentTypes.CreateSimpleContentType("child2", "Child 2", ctChild1, true);

                repository.AddOrUpdate(ctMain);
                repository.AddOrUpdate(ctChild1);
                repository.AddOrUpdate(ctChild2);
                unitOfWork.Commit();

                // Act

                var resolvedParent = repository.Get(ctMain.Id);
                repository.Delete(resolvedParent);
                unitOfWork.Commit();

                // Assert
                Assert.That(repository.Exists(ctMain.Id), Is.False);
                Assert.That(repository.Exists(ctChild1.Id), Is.False);
                Assert.That(repository.Exists(ctChild2.Id), Is.False);
            }
        }

        [Test]
        public void Can_Perform_Query_On_ContentTypeRepository_Sort_By_Name()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);
                var child1 = MockedContentTypes.CreateSimpleContentType("aabc", "aabc", contentType, randomizeAliases: true);
                repository.AddOrUpdate(child1);
                var child3 = MockedContentTypes.CreateSimpleContentType("zyx", "zyx", contentType, randomizeAliases: true);
                repository.AddOrUpdate(child3);
                var child2 = MockedContentTypes.CreateSimpleContentType("a123", "a123", contentType, randomizeAliases: true);
                repository.AddOrUpdate(child2);
                unitOfWork.Commit();

                // Act
                var contentTypes = repository.GetByQuery(new Query<IContentType>().Where(x => x.ParentId == contentType.Id));

                // Assert
                Assert.That(contentTypes.Count(), Is.EqualTo(3));
                Assert.AreEqual("a123", contentTypes.ElementAt(0).Name);
                Assert.AreEqual("aabc", contentTypes.ElementAt(1).Name);
                Assert.AreEqual("zyx", contentTypes.ElementAt(2).Name);
            }

        }

        [Test]
        public void Can_Perform_Get_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                // Assert
                Assert.That(contentType, Is.Not.Null);
                Assert.That(contentType.Id, Is.EqualTo(NodeDto.NodeIdSeed + 1));
            }
        }

        [Test]
        public void Can_Perform_Get_By_Guid_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);
                var childContentType = MockedContentTypes.CreateSimpleContentType("blah", "Blah", contentType, randomizeAliases:true);
                repository.AddOrUpdate(childContentType);
                unitOfWork.Commit();

                // Act
                var result = repository.Get(childContentType.Key);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(childContentType.Id));
            }
        }

        [Test]
        public void Can_Perform_Get_By_Missing_Guid_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                var result = repository.Get(Guid.NewGuid());

                // Assert
                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var contentTypes = repository.GetAll();
                int count =
                    DatabaseContext.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new {NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType)});

                // Assert
                Assert.That(contentTypes.Any(), Is.True);
                Assert.That(contentTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Guid_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var allGuidIds = repository.GetAll().Select(x => x.Key).ToArray();

                // Act
                var contentTypes = ((IReadRepository<Guid, IContentType>)repository).GetAll(allGuidIds);
                int count =
                    DatabaseContext.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new { NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType) });

                // Assert
                Assert.That(contentTypes.Any(), Is.True);
                Assert.That(contentTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_ContentTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var exists = repository.Exists(NodeDto.NodeIdSeed);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Update_ContentType_With_PropertyType_Removed()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                // Act
                contentType.PropertyGroups["Meta"].PropertyTypes.Remove("description");
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                var result = repository.Get(NodeDto.NodeIdSeed + 1);

                // Assert
                Assert.That(result.PropertyTypes.Any(x => x.Alias == "description"), Is.False);
                Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(result.PropertyGroups.Count));
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(result.PropertyTypes.Count()));
            }
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_SimpleTextpage()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var contentType = repository.Get(NodeDto.NodeIdSeed);

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
                Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_Textpage()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));
                Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Verify_PropertyType_With_No_Group()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                // Act
                var urlAlias = new PropertyType("test", DataTypeDatabaseType.Nvarchar, "urlAlias")
                    {
                        Name = "Url Alias",
                        Description = "",
                        Mandatory = false,
                        SortOrder = 1,
                        DataTypeDefinitionId = -88
                    };

                var addedPropertyType = contentType.AddPropertyType(urlAlias);
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                // Assert
                var updated = repository.Get(NodeDto.NodeIdSeed + 1);
                Assert.That(addedPropertyType, Is.True);
                Assert.That(updated.PropertyGroups.Count(), Is.EqualTo(2));
                Assert.That(updated.PropertyTypes.Count(), Is.EqualTo(5));
                Assert.That(updated.PropertyTypes.Any(x => x.Alias == "urlAlias"), Is.True);
                Assert.That(updated.PropertyTypes.First(x => x.Alias == "urlAlias").PropertyGroupId, Is.Null);
            }
        }

        [Test]
        public void Can_Verify_AllowedChildContentTypes_On_ContentType()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                var subpageContentType = MockedContentTypes.CreateSimpleContentType("umbSubpage", "Subpage");
                var simpleSubpageContentType = MockedContentTypes.CreateSimpleContentType("umbSimpleSubpage", "Simple Subpage");
                repository.AddOrUpdate(subpageContentType);
                repository.AddOrUpdate(simpleSubpageContentType);
                unitOfWork.Commit();

                // Act
                var contentType = repository.Get(NodeDto.NodeIdSeed);
                contentType.AllowedContentTypes = new List<ContentTypeSort>
                    {
                        new ContentTypeSort(new Lazy<int>(() => subpageContentType.Id), 0, subpageContentType.Alias),
                        new ContentTypeSort(new Lazy<int>(() => simpleSubpageContentType.Id), 1, simpleSubpageContentType.Alias)
                    };
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                //Assert
                var updated = repository.Get(NodeDto.NodeIdSeed);

                Assert.That(updated.AllowedContentTypes.Any(), Is.True);
                Assert.That(updated.AllowedContentTypes.Any(x => x.Alias == subpageContentType.Alias), Is.True);
                Assert.That(updated.AllowedContentTypes.Any(x => x.Alias == simpleSubpageContentType.Alias), Is.True);
            }
        }

        [Test]
        public void Can_Verify_Removal_Of_Used_PropertyType_From_ContentType()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository repository;
            using (var contentRepository = CreateRepository(unitOfWork, out repository))
            {
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                var subpage = MockedContent.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
                contentRepository.AddOrUpdate(subpage);
                unitOfWork.Commit();

                // Act
                contentType.RemovePropertyType("keywords");
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "keywords"), Is.False);
                Assert.That(subpage.Properties.First(x => x.Alias == "description").Value, Is.EqualTo("This is the meta description for a textpage"));
            }
        }

        [Test]
        public void Can_Verify_Addition_Of_PropertyType_After_ContentType_Is_Used()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository repository;
            using (var contentRepository = CreateRepository(unitOfWork, out repository))
            {
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                var subpage = MockedContent.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
                contentRepository.AddOrUpdate(subpage);
                unitOfWork.Commit();

                // Act
                var propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
                propertyGroup.PropertyTypes.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "metaAuthor") { Name = "Meta Author", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(5));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
            }

        }

        [Test]
        public void Can_Verify_Usage_Of_New_PropertyType_On_Content()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository repository;
            using (var contentRepository = CreateRepository(unitOfWork, out repository))
            {
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                var subpage = MockedContent.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
                contentRepository.AddOrUpdate(subpage);
                unitOfWork.Commit();

                var propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
                propertyGroup.PropertyTypes.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "metaAuthor") { Name = "Meta Author", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                // Act
                var content = contentRepository.Get(subpage.Id);
                content.SetValue("metaAuthor", "John Doe");
                contentRepository.AddOrUpdate(content);
                unitOfWork.Commit();

                //Assert
                var updated = contentRepository.Get(subpage.Id);
                Assert.That(updated.GetValue("metaAuthor").ToString(), Is.EqualTo("John Doe"));
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(5));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
            }
        }

        [Test]
        public void
            Can_Verify_That_A_Combination_Of_Adding_And_Deleting_PropertyTypes_Doesnt_Cause_Issues_For_Content_And_ContentType
            ()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository repository;
            using (var contentRepository = CreateRepository(unitOfWork, out repository))
            {
                var contentType = repository.Get(NodeDto.NodeIdSeed + 1);

                var subpage = MockedContent.CreateTextpageContent(contentType, "Text Page 1", contentType.Id);
                contentRepository.AddOrUpdate(subpage);
                unitOfWork.Commit();

                //Remove PropertyType
                contentType.RemovePropertyType("keywords");
                //Add PropertyType
                var propertyGroup = contentType.PropertyGroups.First(x => x.Name == "Meta");
                propertyGroup.PropertyTypes.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "metaAuthor") { Name = "Meta Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
                repository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                // Act
                var content = contentRepository.Get(subpage.Id);
                content.SetValue("metaAuthor", "John Doe");
                contentRepository.AddOrUpdate(content);
                unitOfWork.Commit();

                //Assert
                var updated = contentRepository.Get(subpage.Id);
                Assert.That(updated.GetValue("metaAuthor").ToString(), Is.EqualTo("John Doe"));
                Assert.That(updated.Properties.First(x => x.Alias == "description").Value, Is.EqualTo("This is the meta description for a textpage"));

                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(4));
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "metaAuthor"), Is.True);
                Assert.That(contentType.PropertyTypes.Any(x => x.Alias == "keywords"), Is.False);
            }
        }

        public void CreateTestData()
        {
            //Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
            ContentType simpleContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.ContentTypeService.Save(simpleContentType);

            //Create and Save ContentType "textPage" -> (NodeDto.NodeIdSeed + 1)
            ContentType textpageContentType = MockedContentTypes.CreateTextpageContentType();
            ServiceContext.ContentTypeService.Save(textpageContentType);
        }
    }
}
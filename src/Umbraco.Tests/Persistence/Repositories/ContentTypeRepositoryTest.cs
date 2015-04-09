using System;
using System.Collections.Generic;
using System.Linq;
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

using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
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
            var repository = new ContentRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, contentTypeRepository, templateRepository, tagRepository);
            return repository;
        }

        private ContentTypeRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            var templateRepository = new TemplateRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, templateRepository);
            return contentTypeRepository;
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
        public void Can_Perform_Add_On_ContentTypeRepository()
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

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.Path.Contains(","), Is.True);
                Assert.That(contentType.SortOrder, Is.GreaterThan(0));    
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
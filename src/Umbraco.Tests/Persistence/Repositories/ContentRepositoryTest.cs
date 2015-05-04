using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.editorControls.tinyMCE3;
using umbraco.interfaces;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class ContentRepositoryTest : BaseDatabaseFactoryTest
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
            var templateRepository = new TemplateRepository(unitOfWork, CacheHelper, Logger, SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var tagRepository = new TagRepository(unitOfWork, CacheHelper, Logger, SqlSyntax);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper, Logger, SqlSyntax, templateRepository);
            var repository = new ContentRepository(unitOfWork, CacheHelper, Logger, SqlSyntax, contentTypeRepository, templateRepository, tagRepository);
            return repository;
        }

        [Test]
        public void Rebuild_All_Xml_Structures()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {

                var contentType1 = MockedContentTypes.CreateSimpleContentType("Textpage1", "Textpage1");
                contentTypeRepository.AddOrUpdate(contentType1);
                var allCreated = new List<IContent>();

                for (var i = 0; i < 100; i++)
                {
                    //These will be non-published so shouldn't show up
                    var c1 = MockedContent.CreateSimpleContent(contentType1);
                    repository.AddOrUpdate(c1);
                    allCreated.Add(c1);
                }
                for (var i = 0; i < 100; i++)
                {
                    var c1 = MockedContent.CreateSimpleContent(contentType1);
                    c1.ChangePublishedState(PublishedState.Published);
                    repository.AddOrUpdate(c1);
                    allCreated.Add(c1);
                }
                unitOfWork.Commit();

                //now create some versions of this content - this shouldn't affect the xml structures saved
                for (int i = 0; i < allCreated.Count; i++)
                {
                    allCreated[i].Name = "blah" + i;
                    repository.AddOrUpdate(allCreated[i]);
                }
                unitOfWork.Commit();

                //delete all xml                 
                unitOfWork.Database.Execute("DELETE FROM cmsContentXml");
                Assert.AreEqual(0, unitOfWork.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContentXml"));

                repository.RebuildXmlStructures(media => new XElement("test"), 10);

                Assert.AreEqual(100, unitOfWork.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContentXml"));
            }
        }

        [Test]
        public void Rebuild_All_Xml_Structures_For_Content_Type()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                var contentType1 = MockedContentTypes.CreateSimpleContentType("Textpage1", "Textpage1");
                var contentType2 = MockedContentTypes.CreateSimpleContentType("Textpage2", "Textpage2");
                var contentType3 = MockedContentTypes.CreateSimpleContentType("Textpage3", "Textpage3");
                contentTypeRepository.AddOrUpdate(contentType1);
                contentTypeRepository.AddOrUpdate(contentType2);
                contentTypeRepository.AddOrUpdate(contentType3);

                var allCreated = new List<IContent>();

                for (var i = 0; i < 30; i++)
                {
                    //These will be non-published so shouldn't show up
                    var c1 = MockedContent.CreateSimpleContent(contentType1);                 
                    repository.AddOrUpdate(c1);
                    allCreated.Add(c1);
                }
                for (var i = 0; i < 30; i++)
                {
                    var c1 = MockedContent.CreateSimpleContent(contentType1);
                    c1.ChangePublishedState(PublishedState.Published);
                    repository.AddOrUpdate(c1);
                    allCreated.Add(c1);
                }
                for (var i = 0; i < 30; i++)
                {
                    var c1 = MockedContent.CreateSimpleContent(contentType2);
                    c1.ChangePublishedState(PublishedState.Published);
                    repository.AddOrUpdate(c1);
                    allCreated.Add(c1);
                }
                for (var i = 0; i < 30; i++)
                {
                    var c1 = MockedContent.CreateSimpleContent(contentType3);
                    c1.ChangePublishedState(PublishedState.Published);
                    repository.AddOrUpdate(c1);
                    allCreated.Add(c1);
                }
                unitOfWork.Commit();

                //now create some versions of this content - this shouldn't affect the xml structures saved
                for (int i = 0; i < allCreated.Count; i++)
                {
                    allCreated[i].Name = "blah" + i;
                    repository.AddOrUpdate(allCreated[i]);
                }
                unitOfWork.Commit();

                //delete all xml                 
                unitOfWork.Database.Execute("DELETE FROM cmsContentXml");
                Assert.AreEqual(0, unitOfWork.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContentXml"));

                repository.RebuildXmlStructures(media => new XElement("test"), 10, contentTypeIds: new[] { contentType1.Id, contentType2.Id });

                Assert.AreEqual(60, unitOfWork.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContentXml"));
            }
        }

        [Test]
        public void Ensures_Permissions_Are_Set_If_Parent_Entity_Permissions_Exist()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                contentType.AllowedContentTypes = new List<ContentTypeSort>
                {
                    new ContentTypeSort(new Lazy<int>(() => contentType.Id), 0, contentType.Alias)
                };
                var parentPage = MockedContent.CreateSimpleContent(contentType);
                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(parentPage);
                unitOfWork.Commit();

                // Act
                repository.AssignEntityPermission(parentPage, 'A', new int[] { 0 });
                var childPage = MockedContent.CreateSimpleContent(contentType, "child", parentPage);
                repository.AddOrUpdate(childPage);
                unitOfWork.Commit();

                // Assert
                var permissions = repository.GetPermissionsForEntity(childPage.Id);
                Assert.AreEqual(1, permissions.Count());
                Assert.AreEqual("A", permissions.Single().AssignedPermissions.First());
            }

        }

        [Test]
        public void Can_Perform_Add_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage2", "Textpage");
                Content textpage = MockedContent.CreateSimpleContent(contentType);

                // Act
                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(textpage);
                unitOfWork.Commit();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(textpage.HasIdentity, Is.True);
            }
        }

        //Covers issue U4-2791 and U4-2607
        [Test]
        public void Can_Save_Content_With_AtSign_In_Name_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Commit();

                var textpage = MockedContent.CreateSimpleContent(contentType, "test@umbraco.org", -1);
                var anotherTextpage = MockedContent.CreateSimpleContent(contentType, "@lightgiants", -1);

                // Act

                repository.AddOrUpdate(textpage);
                repository.AddOrUpdate(anotherTextpage);
                unitOfWork.Commit();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(textpage.HasIdentity, Is.True);

                var content = repository.Get(textpage.Id);
                Assert.That(content.Name, Is.EqualTo(textpage.Name));

                var content2 = repository.Get(anotherTextpage.Id);
                Assert.That(content2.Name, Is.EqualTo(anotherTextpage.Name));
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                Content textpage = MockedContent.CreateSimpleContent(contentType);

                // Act
                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(textpage);
                unitOfWork.Commit();

                Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
                repository.AddOrUpdate(subpage);
                unitOfWork.Commit();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(textpage.HasIdentity, Is.True);
                Assert.That(subpage.HasIdentity, Is.True);
                Assert.That(textpage.Id, Is.EqualTo(subpage.ParentId));
            }

        }


        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var content = repository.Get(NodeDto.NodeIdSeed + 3);
                bool dirty = ((Content)content).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Update_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var content = repository.Get(NodeDto.NodeIdSeed + 2);
                content.Name = "About 2";
                repository.AddOrUpdate(content);
                unitOfWork.Commit();
                var updatedContent = repository.Get(NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(updatedContent.Id, Is.EqualTo(content.Id));
                Assert.That(updatedContent.Name, Is.EqualTo(content.Name));
            }

        }

        [Test]
        public void Can_Perform_Delete_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                var contentType = contentTypeRepository.Get(NodeDto.NodeIdSeed);
                var content = new Content("Textpage 2 Child Node", NodeDto.NodeIdSeed + 3, contentType);
                content.CreatorId = 0;
                content.WriterId = 0;

                // Act
                repository.AddOrUpdate(content);
                unitOfWork.Commit();
                var id = content.Id;

                repository.Delete(content);
                unitOfWork.Commit();

                var content1 = repository.Get(id);

                // Assert
                Assert.That(content1, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var content = repository.Get(NodeDto.NodeIdSeed + 3);

                // Assert
                Assert.That(content.Id, Is.EqualTo(NodeDto.NodeIdSeed + 3));
                Assert.That(content.CreateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(content.UpdateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(content.ParentId, Is.Not.EqualTo(0));
                Assert.That(content.Name, Is.EqualTo("Text Page 2"));
                Assert.That(content.SortOrder, Is.EqualTo(1));
                Assert.That(content.Version, Is.Not.EqualTo(Guid.Empty));
                Assert.That(content.ContentTypeId, Is.EqualTo(NodeDto.NodeIdSeed));
                Assert.That(content.Path, Is.Not.Empty);
                Assert.That(content.Properties.Any(), Is.True);
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var query = Query<IContent>.Builder.Where(x => x.Level == 2);
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Get_All_With_Many_Version()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {                
                var result = repository.GetAll().ToArray();
                foreach (var content in result)
                {
                    content.ChangePublishedState(PublishedState.Saved);
                    repository.AddOrUpdate(content);
                }
                unitOfWork.Commit();
                foreach (var content in result)
                {
                    content.ChangePublishedState(PublishedState.Published);
                    repository.AddOrUpdate(content);
                }
                unitOfWork.Commit();

                //re-get

                var result2 = repository.GetAll().ToArray();

                Assert.AreEqual(result.Count(), result2.Count());
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_ForFirstPage_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var query = Query<IContent>.Builder.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Ascending);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_ForSecondPage_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var query = Query<IContent>.Builder.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 1, 1, out totalRecords, "Name", Direction.Ascending);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WithSinglePage_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var query = Query<IContent>.Builder.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 2, out totalRecords, "Name", Direction.Ascending);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WithDescendingOrder_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var query = Query<IContent>.Builder.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Descending);

                // Assert
                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WithFilterMatchingSome_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var query = Query<IContent>.Builder.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Ascending, "Page 2");

                // Assert
                Assert.That(totalRecords, Is.EqualTo(1));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_WithFilterMatchingAll_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var query = Query<IContent>.Builder.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Ascending, "Page");

                // Assert
                Assert.That(totalRecords, Is.EqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var contents = repository.GetAll(NodeDto.NodeIdSeed + 2, NodeDto.NodeIdSeed + 3);

                // Assert
                Assert.That(contents, Is.Not.Null);
                Assert.That(contents.Any(), Is.True);
                Assert.That(contents.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var contents = repository.GetAll();

                // Assert
                Assert.That(contents, Is.Not.Null);
                Assert.That(contents.Any(), Is.True);
                Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(4));
            }


        }

        [Test]
        public void Can_Perform_Exists_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var exists = repository.Exists(NodeDto.NodeIdSeed + 1);

                // Assert
                Assert.That(exists, Is.True);
            }


        }

        [Test]
        public void Can_Perform_Count_On_ContentRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                int level = 2;
                var query = Query<IContent>.Builder.Where(x => x.Level == level);
                var result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void Can_Verify_Keys_Set()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var textpage = repository.Get(NodeDto.NodeIdSeed + 1);
                var subpage = repository.Get(NodeDto.NodeIdSeed + 2);
                var trashed = repository.Get(NodeDto.NodeIdSeed + 4);

                // Assert
                Assert.That(textpage.Key.ToString().ToUpper(), Is.EqualTo("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
                Assert.That(subpage.Key.ToString().ToUpper(), Is.EqualTo("FF11402B-7E53-4654-81A7-462AC2108059"));
                Assert.That(trashed.Key, Is.Not.EqualTo(Guid.Empty));
            }
        }

        [Test]
        public void Can_Get_Content_By_Guid_Key()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out contentTypeRepository))
            {
                // Act
                var query = Query<IContent>.Builder.Where(x => x.Key == new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
                var content = repository.GetByQuery(query).SingleOrDefault();

                // Assert
                Assert.That(content, Is.Not.Null);
                Assert.That(content.Id, Is.EqualTo(NodeDto.NodeIdSeed + 1));
            }

        }

        public void CreateTestData()
        {
            //Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
            ServiceContext.ContentTypeService.Save(contentType);

            //Create and Save Content "Homepage" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 1)
            Content textpage = MockedContent.CreateSimpleContent(contentType);
            textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
            ServiceContext.ContentService.Save(textpage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 2)
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
            subpage.Key = new Guid("FF11402B-7E53-4654-81A7-462AC2108059");
            ServiceContext.ContentService.Save(subpage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 3)
            Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Page 2", textpage.Id);
            ServiceContext.ContentService.Save(subpage2, 0);

            //Create and Save Content "Text Page Deleted" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 4)
            Content trashed = MockedContent.CreateSimpleContent(contentType, "Text Page Deleted", -20);
            trashed.Trashed = true;
            ServiceContext.ContentService.Save(trashed, 0);
        }
    }
}
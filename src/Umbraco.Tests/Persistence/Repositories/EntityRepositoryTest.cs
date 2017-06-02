using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class EntityRepositoryTest : BaseDatabaseFactoryTest
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
        
        private ContentRepository CreateContentRepository(IScopeUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository)
        {
            TemplateRepository tr;
            return CreateContentRepository(unitOfWork, out contentTypeRepository, out tr);
        }

        private ContentRepository CreateContentRepository(IScopeUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository, out TemplateRepository templateRepository)
        {
            templateRepository = new TemplateRepository(unitOfWork, CacheHelper, Logger, SqlSyntax, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var tagRepository = new TagRepository(unitOfWork, CacheHelper, Logger, SqlSyntax);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper, Logger, SqlSyntax, templateRepository);
            var repository = new ContentRepository(unitOfWork, CacheHelper, Logger, SqlSyntax, contentTypeRepository, templateRepository, tagRepository, Mock.Of<IContentSection>());
            return repository;
        }

        [Test]
        public void Deal_With_Corrupt_Duplicate_Newest_Published_Flags_Full_Model()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            IContent content1;            

            using (var repository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                var hasPropertiesContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                content1 = MockedContent.CreateSimpleContent(hasPropertiesContentType);

                contentTypeRepository.AddOrUpdate(hasPropertiesContentType);
                repository.AddOrUpdate(content1);
                unitOfWork.Commit();
            }

            var versionDtos = new List<ContentVersionDto>();

            //Now manually corrupt the data
            var versions = new[] {Guid.NewGuid(), Guid.NewGuid()};
            for (var index = 0; index < versions.Length; index++)
            {
                var version = versions[index];
                var versionDate = DateTime.Now.AddMinutes(index);
                var versionDto = new ContentVersionDto
                {
                    NodeId = content1.Id,
                    VersionDate = versionDate,
                    VersionId = version
                };
                this.DatabaseContext.Database.Insert(versionDto);
                versionDtos.Add(versionDto);
                this.DatabaseContext.Database.Insert(new DocumentDto
                {
                    Newest = true,
                    NodeId = content1.Id,
                    Published = true,
                    Text = content1.Name,
                    VersionId = version,
                    WriterUserId = 0,
                    UpdateDate = versionDate,
                    TemplateId = content1.Template == null || content1.Template.Id <= 0 ? null : (int?)content1.Template.Id
                });
            }

            // Assert
            using (var repository = new EntityRepository(unitOfWork))
            {
                var content = repository.GetByQuery(new Query<IUmbracoEntity>().Where(c => c.Id == content1.Id), Constants.ObjectTypes.DocumentGuid).ToArray();
                Assert.AreEqual(1, content.Length);
                Assert.AreEqual(versionDtos.Max(x => x.Id), content[0].AdditionalData["VersionId"]);

                content = repository.GetAll(Constants.ObjectTypes.DocumentGuid, content[0].Key).ToArray();
                Assert.AreEqual(1, content.Length);
                Assert.AreEqual(versionDtos.Max(x => x.Id), content[0].AdditionalData["VersionId"]);

                content = repository.GetAll(Constants.ObjectTypes.DocumentGuid, content[0].Id).ToArray();
                Assert.AreEqual(1, content.Length);
                Assert.AreEqual(versionDtos.Max(x => x.Id), content[0].AdditionalData["VersionId"]);

                var contentItem = repository.Get(content[0].Id, Constants.ObjectTypes.DocumentGuid);
                Assert.AreEqual(versionDtos.Max(x => x.Id), contentItem.AdditionalData["VersionId"]);

                contentItem = repository.GetByKey(content[0].Key, Constants.ObjectTypes.DocumentGuid);
                Assert.AreEqual(versionDtos.Max(x => x.Id), contentItem.AdditionalData["VersionId"]);
            }
        }

        /// <summary>
        /// The Slim model will test the EntityRepository when it doesn't know the object type so it will 
        /// make the simplest (slim) query
        /// </summary>
        [Test]
        public void Deal_With_Corrupt_Duplicate_Newest_Published_Flags_Slim_Model()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            ContentTypeRepository contentTypeRepository;
            IContent content1;

            using (var repository = CreateContentRepository(unitOfWork, out contentTypeRepository))
            {
                var hasPropertiesContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                content1 = MockedContent.CreateSimpleContent(hasPropertiesContentType);

                contentTypeRepository.AddOrUpdate(hasPropertiesContentType);
                repository.AddOrUpdate(content1);
                unitOfWork.Commit();
            }

            //Now manually corrupt the data
            var versions = new[] { Guid.NewGuid(), Guid.NewGuid() };
            for (var index = 0; index < versions.Length; index++)
            {
                var version = versions[index];
                var versionDate = DateTime.Now.AddMinutes(index);
                var versionDto = new ContentVersionDto
                {
                    NodeId = content1.Id,
                    VersionDate = versionDate,
                    VersionId = version
                };
                this.DatabaseContext.Database.Insert(versionDto);
                this.DatabaseContext.Database.Insert(new DocumentDto
                {
                    Newest = true,
                    NodeId = content1.Id,
                    Published = true,
                    Text = content1.Name,
                    VersionId = version,
                    WriterUserId = 0,
                    UpdateDate = versionDate,
                    TemplateId = content1.Template == null || content1.Template.Id <= 0 ? null : (int?)content1.Template.Id
                });
            }

            // Assert
            using (var repository = new EntityRepository(unitOfWork))
            {
                var content = repository.GetByQuery(new Query<IUmbracoEntity>().Where(c => c.Id == content1.Id)).ToArray();
                Assert.AreEqual(1, content.Length);
                var contentItem = repository.Get(content[0].Id);
                Assert.IsNotNull(contentItem);
                contentItem = repository.GetByKey(content[0].Key);
                Assert.IsNotNull(contentItem);
            }
        }
    }
}
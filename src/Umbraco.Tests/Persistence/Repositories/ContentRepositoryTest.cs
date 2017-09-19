using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ContentRepositoryTest : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();

            CreateTestData();

            VersionableRepositoryBase.ThrowOnWarning = true;
        }

        public override void TearDown()
        {
            VersionableRepositoryBase.ThrowOnWarning = false;

            base.TearDown();
        }

        private ContentRepository CreateRepository(IScopeUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository, out DataTypeDefinitionRepository dtdRepository, CacheHelper cacheHelper = null)
        {
            cacheHelper = cacheHelper ?? CacheHelper;

            TemplateRepository tr;
            var ctRepository = CreateRepository(unitOfWork, out contentTypeRepository, out tr);
            dtdRepository = new DataTypeDefinitionRepository(unitOfWork, cacheHelper, Logger, contentTypeRepository);
            return ctRepository;
        }

        private ContentRepository CreateRepository(IScopeUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository, CacheHelper cacheHelper = null)
        {
            TemplateRepository tr;
            return CreateRepository(unitOfWork, out contentTypeRepository, out tr, cacheHelper);
        }

        private ContentRepository CreateRepository(IScopeUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository, out TemplateRepository templateRepository, CacheHelper cacheHelper = null)
        {
            cacheHelper = cacheHelper ?? CacheHelper;

            templateRepository = new TemplateRepository(unitOfWork, cacheHelper, Logger, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var tagRepository = new TagRepository(unitOfWork, cacheHelper, Logger);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, cacheHelper, Logger, templateRepository);
            var repository = new ContentRepository(unitOfWork, cacheHelper, Logger, contentTypeRepository, templateRepository, tagRepository, Mock.Of<IContentSection>());
            return repository;
        }

        [Test]
        public void Cache_Active_By_Int_And_Guid()
        {
            ContentTypeRepository contentTypeRepository;

            var realCache = new CacheHelper(
                new ObjectCacheRuntimeCacheProvider(),
                new StaticCacheProvider(),
                new StaticCacheProvider(),
                new IsolatedRuntimeCache(t => new ObjectCacheRuntimeCacheProvider()));

            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out contentTypeRepository, cacheHelper: realCache);

                var udb = (UmbracoDatabase) unitOfWork.Database;

                udb.EnableSqlCount = false;

                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                var content = MockedContent.CreateSimpleContent(contentType);
                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(content);
                unitOfWork.Complete();

                udb.EnableSqlCount = true;

                //go get it, this should already be cached since the default repository key is the INT
                var found = repository.Get(content.Id);
                Assert.AreEqual(0, udb.SqlCount);
                //retrieve again, this should use cache
                found = repository.Get(content.Id);
                Assert.AreEqual(0, udb.SqlCount);

                //reset counter
                udb.EnableSqlCount = false;
                udb.EnableSqlCount = true;

                //now get by GUID, this won't be cached yet because the default repo key is not a GUID
                found = repository.Get(content.Key);
                var sqlCount = udb.SqlCount;
                Assert.Greater(sqlCount, 0);
                //retrieve again, this should use cache now
                found = repository.Get(content.Key);
                Assert.AreEqual(sqlCount, udb.SqlCount);
            }
        }

        [Test]
        public void Get_Always_Returns_Latest_Version()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out ContentTypeRepository contentTypeRepository, out DataTypeDefinitionRepository dataTypeDefinitionRepository);

                IContent content1;
                var versions = new List<Guid>();
                var hasPropertiesContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                content1 = MockedContent.CreateSimpleContent(hasPropertiesContentType);

                //save version
                contentTypeRepository.AddOrUpdate(hasPropertiesContentType);
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // the first version

                //publish version
                content1.ChangePublishedState(PublishedState.Publishing);
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // the first version got published, same id

                //change something and make a pending version
                content1.Name = "new name";
                if (content1.Published)
                    content1.ChangePublishedState(PublishedState.Saving);
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // the second version

                Assert.AreEqual(2, versions.Distinct().Count());

                var content = repository.GetByQuery(unitOfWork.Query<IContent>().Where(c => c.Id == content1.Id)).ToArray()[0];
                Assert.AreEqual(versions[2], content.Version);

                content = repository.Get(content1.Id);
                Assert.AreEqual(versions[2], content.Version);

                foreach (var version in versions)
                {
                    content = repository.GetByVersion(version);
                    Assert.IsNotNull(content);
                    Assert.AreEqual(version, content.Version);
                }
            }
        }

        [Test]
        public void Deal_With_Corrupt_Duplicate_Newest_Published_Flags()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out ContentTypeRepository contentTypeRepository, out DataTypeDefinitionRepository dataTypeDefinitionRepository);

                var hasPropertiesContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                IContent content1;
                content1 = MockedContent.CreateSimpleContent(hasPropertiesContentType);

                contentTypeRepository.AddOrUpdate(hasPropertiesContentType);
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();

                var versionDtos = new List<ContentVersionDto>();

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
                    unitOfWork.Database.Insert(versionDto);
                    versionDtos.Add(versionDto);
                    unitOfWork.Database.Insert(new DocumentDto
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

                    var content = repository.GetByQuery(unitOfWork.Query<IContent>().Where(c => c.Id == content1.Id)).ToArray();
                    Assert.AreEqual(1, content.Length);
                    Assert.AreEqual(content[0].Version, versionDtos.Single(x => x.Id == versionDtos.Max(y => y.Id)).VersionId);
                    Assert.AreEqual(content[0].UpdateDate.ToString(CultureInfo.InvariantCulture), versionDtos.Single(x => x.Id == versionDtos.Max(y => y.Id)).VersionDate.ToString(CultureInfo.InvariantCulture));

                    var contentItem = repository.GetByVersion(content1.Version);
                    Assert.IsNotNull(contentItem);

                    contentItem = repository.Get(content1.Id);
                    Assert.IsNotNull(contentItem);
                    Assert.AreEqual(contentItem.UpdateDate.ToString(CultureInfo.InvariantCulture), versionDtos.Single(x => x.Id == versionDtos.Max(y => y.Id)).VersionDate.ToString(CultureInfo.InvariantCulture));
                    Assert.AreEqual(contentItem.Version, versionDtos.Single(x => x.Id == versionDtos.Max(y => y.Id)).VersionId);

                    var allVersions = repository.GetAllVersions(content[0].Id);
                    var allKnownVersions = versionDtos.Select(x => x.VersionId).Union(new[] { content1.Version }).ToArray();
                    Assert.IsTrue(allKnownVersions.ContainsAll(allVersions.Select(x => x.Version)));
                    Assert.IsTrue(allVersions.Select(x => x.Version).ContainsAll(allKnownVersions));
                }
            }
        }

        /// <summary>
        /// This tests the regression issue of U4-9438
        /// </summary>
        /// <remarks>
        /// The problem was the iteration of the property data in VersionableRepositoryBase when a content item
        /// in the list actually doesn't have any property types, it would still skip over a property row.
        /// To test, we have 3 content items, the first has properties, the second doesn't and the third does.
        /// </remarks>
        [Test]
        public void Property_Data_Assigned_Correctly()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            var allContent = new List<IContent>();
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out ContentTypeRepository contentTypeRepository, out DataTypeDefinitionRepository dataTypeDefinitionRepository);

                var emptyContentType = MockedContentTypes.CreateBasicContentType();
                var hasPropertiesContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                var content1 = MockedContent.CreateSimpleContent(hasPropertiesContentType);
                var content2 = MockedContent.CreateBasicContent(emptyContentType);
                var content3 = MockedContent.CreateSimpleContent(hasPropertiesContentType);

                // Act
                contentTypeRepository.AddOrUpdate(emptyContentType);
                contentTypeRepository.AddOrUpdate(hasPropertiesContentType);
                repository.AddOrUpdate(content1);
                repository.AddOrUpdate(content2);
                repository.AddOrUpdate(content3);
                unitOfWork.Flush();

                allContent.Add(content1);
                allContent.Add(content2);
                allContent.Add(content3);

                //this will cause the GetPropertyCollection to execute and we need to ensure that
                // all of the properties and property types are all correct
                var result = repository.GetAll(allContent.Select(x => x.Id).ToArray()).ToArray();

                foreach (var content in result)
                {
                    foreach (var contentProperty in content.Properties)
                    {
                        //prior to the fix, the 2nd document iteration in the GetPropertyCollection would have caused
                        //the enumerator to move forward past the first property of the 3rd document which would have
                        //ended up not assiging a property to the 3rd document. This would have ended up with the 3rd
                        //document still having 3 properties but the last one would not have been assigned an identity
                        //because the property data would not have been assigned.
                        Assert.IsTrue(contentProperty.HasIdentity);
                    }
                }
            }
        }

        /// <summary>
        /// This test ensures that when property values using special database fields are saved, the actual data in the
        /// object being stored is also transformed in the same way as the data being stored in the database is.
        /// Before you would see that ex: a decimal value being saved as 100 or "100", would be that exact value in the
        /// object, but the value saved to the database was actually 100.000000.
        /// When querying the database for the value again - the value would then differ from what is in the object.
        /// This caused inconsistencies between saving+publishing and simply saving and then publishing, due to the former
        /// sending the non-transformed data directly on to publishing.
        /// </summary>
        [Test]
        public void Property_Values_With_Special_DatabaseTypes_Are_Equal_Before_And_After_Being_Persisted()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                DataTypeDefinitionRepository dataTypeDefinitionRepository;

                var repository = CreateRepository(unitOfWork, out contentTypeRepository, out dataTypeDefinitionRepository);

                // Setup
                var dtd = new DataTypeDefinition(-1, Constants.PropertyEditors.DecimalAlias) { Name = "test", DatabaseType = DataTypeDatabaseType.Decimal };
                dataTypeDefinitionRepository.AddOrUpdate(dtd);
                unitOfWork.Complete();

                const string decimalPropertyAlias = "decimalProperty";
                const string intPropertyAlias = "intProperty";
                const string dateTimePropertyAlias = "datetimeProperty";
                var dateValue = new DateTime(2016, 1, 6);

                var propertyTypeCollection = new PropertyTypeCollection(
                    new List<PropertyType>
                    {
                        MockedPropertyTypes.CreateDecimalProperty(decimalPropertyAlias, "Decimal property", dtd.Id),
                        MockedPropertyTypes.CreateIntegerProperty(intPropertyAlias, "Integer property"),
                        MockedPropertyTypes.CreateDateTimeProperty(dateTimePropertyAlias, "DateTime property")
                    });
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage", propertyTypeCollection);
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Complete();

                // Int and decimal values are passed in as strings as they would be from the backoffice UI
                var textpage = MockedContent.CreateSimpleContentWithSpecialDatabaseTypes(contentType, "test@umbraco.org", -1, "100", "150", dateValue);

                // Act
                repository.AddOrUpdate(textpage);
                unitOfWork.Complete();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(textpage.HasIdentity, Is.True);

                var persistedTextpage = repository.Get(textpage.Id);
                Assert.That(persistedTextpage.Name, Is.EqualTo(textpage.Name));
                Assert.AreEqual(100m, persistedTextpage.GetValue(decimalPropertyAlias));
                Assert.AreEqual(persistedTextpage.GetValue(decimalPropertyAlias), textpage.GetValue(decimalPropertyAlias));
                Assert.AreEqual(150, persistedTextpage.GetValue(intPropertyAlias));
                Assert.AreEqual(persistedTextpage.GetValue(intPropertyAlias), textpage.GetValue(intPropertyAlias));
                Assert.AreEqual(dateValue, persistedTextpage.GetValue(dateTimePropertyAlias));
                Assert.AreEqual(persistedTextpage.GetValue(dateTimePropertyAlias), textpage.GetValue(dateTimePropertyAlias));
            }
        }

        [Test]
        public void Can_Perform_Add_On_ContentRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage2", "Textpage");
                IContent textpage = MockedContent.CreateSimpleContent(contentType);

                // Act
                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(textpage);
                unitOfWork.Complete();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(textpage.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Add_With_Default_Template()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                TemplateRepository templateRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository, out templateRepository);

                var template = new Template("hello", "hello");
                templateRepository.AddOrUpdate(template);
                unitOfWork.Flush();

                ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage2", "Textpage");
                contentType.AllowedTemplates = Enumerable.Empty<ITemplate>(); // because CreateSimple... assigns one
                contentType.SetDefaultTemplate(template);
                Content textpage = MockedContent.CreateSimpleContent(contentType);

                // Act

                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(textpage);
                unitOfWork.Flush();

                var fetched = repository.Get(textpage.Id);

                // Assert
                Assert.That(textpage.Template, Is.Not.Null);
                Assert.That(textpage.Template, Is.EqualTo(contentType.DefaultTemplate));

                unitOfWork.Complete();

                TestHelper.AssertAllPropertyValuesAreEquals(textpage, fetched, "yyyy-MM-dd HH:mm:ss");
            }
        }

        //Covers issue U4-2791 and U4-2607
        [Test]
        public void Can_Save_Content_With_AtSign_In_Name_On_ContentRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Flush();

                var textpage = MockedContent.CreateSimpleContent(contentType, "test@umbraco.org", -1);
                var anotherTextpage = MockedContent.CreateSimpleContent(contentType, "@lightgiants", -1);

                // Act

                repository.AddOrUpdate(textpage);
                repository.AddOrUpdate(anotherTextpage);
                unitOfWork.Flush();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(textpage.HasIdentity, Is.True);

                var content = repository.Get(textpage.Id);
                Assert.That(content.Name, Is.EqualTo(textpage.Name));

                var content2 = repository.Get(anotherTextpage.Id);
                Assert.That(content2.Name, Is.EqualTo(anotherTextpage.Name));

                unitOfWork.Complete();
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_ContentRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                Content textpage = MockedContent.CreateSimpleContent(contentType);

                // Act
                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(textpage);
                unitOfWork.Flush();

                Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
                repository.AddOrUpdate(subpage);
                unitOfWork.Flush();

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var content = repository.Get(NodeDto.NodeIdSeed + 2);
                content.Name = "About 2";
                repository.AddOrUpdate(content);
                unitOfWork.Flush();
                var updatedContent = repository.Get(NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(updatedContent.Id, Is.EqualTo(content.Id));
                Assert.That(updatedContent.Name, Is.EqualTo(content.Name));
            }

        }

        [Test]
        public void Can_Update_With_Null_Template()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var content = repository.Get(NodeDto.NodeIdSeed + 2);
                content.Template = null;
                repository.AddOrUpdate(content);
                unitOfWork.Flush();
                var updatedContent = repository.Get(NodeDto.NodeIdSeed + 2);

                // Assert
                Assert.That(updatedContent.Template, Is.Null);
            }

        }

        [Test]
        public void Can_Perform_Delete_On_ContentRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                var contentType = contentTypeRepository.Get(NodeDto.NodeIdSeed);
                var content = new Content("Textpage 2 Child Node", NodeDto.NodeIdSeed + 3, contentType);
                content.CreatorId = 0;
                content.WriterId = 0;

                // Act
                repository.AddOrUpdate(content);
                unitOfWork.Flush();
                var id = content.Id;

                repository.Delete(content);
                unitOfWork.Flush();

                var content1 = repository.Get(id);

                // Assert
                Assert.That(content1, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_ContentRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var content = repository.Get(NodeDto.NodeIdSeed + 3);

                // Assert
                Assert.That(content.Id, Is.EqualTo(NodeDto.NodeIdSeed + 3));
                Assert.That(content.CreateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(content.UpdateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(content.ParentId, Is.Not.EqualTo(0));
                Assert.That(content.Name, Is.EqualTo("Text Page 2"));
                //Assert.That(content.SortOrder, Is.EqualTo(1));
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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Level == 2);
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_Get_All_With_Many_Version()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                var result = repository.GetAll().ToArray();
                foreach (var content in result)
                {
                    content.ChangePublishedState(PublishedState.Saving);
                    repository.AddOrUpdate(content);
                }
                unitOfWork.Flush();
                foreach (var content in result)
                {
                    content.ChangePublishedState(PublishedState.Publishing);
                    repository.AddOrUpdate(content);
                }
                unitOfWork.Flush();

                //re-get

                var result2 = repository.GetAll().ToArray();

                Assert.AreEqual(result.Count(), result2.Count());
            }
        }

        [Test]
        public void RegexAliasTest()
        {
            var regex = VersionableRepositoryBaseAliasRegex.For(new SqlServerSyntaxProvider(new Lazy<IScopeProvider>(() => null)));
            Assert.AreEqual(@"(\[\w+]\.\[\w+])\s+AS\s+(\[\w+])", regex.ToString());
            const string sql = "SELECT [table].[column1] AS [alias1], [table].[column2] AS [alias2] FROM [table];";
            var matches = regex.Matches(sql);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("[table].[column1]", matches[0].Groups[1].Value);
            Assert.AreEqual("[alias1]", matches[0].Groups[2].Value);
            Assert.AreEqual("[table].[column2]", matches[1].Groups[1].Value);
            Assert.AreEqual("[alias2]", matches[1].Groups[2].Value);
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_Sorting_On_Custom_Property()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Name.Contains("Text"));
                long totalRecords;

                try
                {
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlCount = true;

                    var result = repository.GetPagedResultsByQuery(query, 0, 2, out totalRecords, "title", Direction.Ascending, false);

                    Assert.AreEqual(3, totalRecords);
                    Assert.AreEqual(2, result.Count());

                    result = repository.GetPagedResultsByQuery(query, 1, 2, out totalRecords, "title", Direction.Ascending, false);

                    Assert.AreEqual(1, result.Count());
                }
                finally
                {
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlCount = false;
                }
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_ForFirstPage_On_ContentRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Level == 2);
                long totalRecords;

                try
                {
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlCount = true;
                    var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Ascending, true);

                    // Assert
                    Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                    Assert.That(result.Count(), Is.EqualTo(1));
                    Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
                }
                finally
                {
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlCount = false;
                }
            }
        }

        [Test]
        public void Can_Perform_GetPagedResultsByQuery_ForSecondPage_On_ContentRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 1, 1, out totalRecords, "Name", Direction.Ascending, true);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 2, out totalRecords, "Name", Direction.Ascending, true);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Level == 2);
                long totalRecords;
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Descending, true);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Level == 2);

                long totalRecords;

                var filterQuery = unitOfWork.Query<IContent>().Where(x => x.Name.Contains("Page 2"));
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Ascending, true, filterQuery);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Level == 2);

                long totalRecords;

                var filterQuery = unitOfWork.Query<IContent>().Where(x => x.Name.Contains("text"));
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out totalRecords, "Name", Direction.Ascending, true, filterQuery);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var contents = repository.GetAll();

                // Assert
                Assert.That(contents, Is.Not.Null);
                Assert.That(contents.Any(), Is.True);
                Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(4));

                contents = repository.GetAll(contents.Select(x => x.Id).ToArray());
                Assert.That(contents, Is.Not.Null);
                Assert.That(contents.Any(), Is.True);
                Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(4));

                contents = ((IReadRepository<Guid, IContent>)repository).GetAll(contents.Select(x => x.Key).ToArray());
                Assert.That(contents, Is.Not.Null);
                Assert.That(contents.Any(), Is.True);
                Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(4));
            }


        }

        [Test]
        public void Can_Perform_Exists_On_ContentRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                int level = 2;
                var query = repository.QueryT.Where(x => x.Level == level);
                var result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void Can_Verify_Keys_Set()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentTypeRepository contentTypeRepository;
                var repository = CreateRepository(unitOfWork, out contentTypeRepository);
                // Act
                var query = repository.QueryT.Where(x => x.Key == new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
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

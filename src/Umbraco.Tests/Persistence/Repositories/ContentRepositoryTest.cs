using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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
        public void CacheActiveForIntsAndGuids()
        {
            var realCache = new CacheHelper(
                new ObjectCacheRuntimeCacheProvider(),
                new StaticCacheProvider(),
                new StaticCacheProvider(),
                new IsolatedRuntimeCache(t => new ObjectCacheRuntimeCacheProvider()));

            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository, cacheHelper: realCache);

                var udb = (UmbracoDatabase) unitOfWork.Database;

                udb.EnableSqlCount = false;

                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.AddOrUpdate(contentType);
                var content = MockedContent.CreateSimpleContent(contentType);
                repository.AddOrUpdate(content);
                unitOfWork.Complete();

                udb.EnableSqlCount = true;

                //go get it, this should already be cached since the default repository key is the INT
                repository.Get(content.Id);
                Assert.AreEqual(0, udb.SqlCount);
                //retrieve again, this should use cache
                repository.Get(content.Id);
                Assert.AreEqual(0, udb.SqlCount);

                //reset counter
                udb.EnableSqlCount = false;
                udb.EnableSqlCount = true;

                //now get by GUID, this won't be cached yet because the default repo key is not a GUID
                repository.Get(content.Key);
                var sqlCount = udb.SqlCount;
                Assert.Greater(sqlCount, 0);
                //retrieve again, this should use cache now
                repository.Get(content.Key);
                Assert.AreEqual(sqlCount, udb.SqlCount);
            }
        }

        [Test]
        public void CreateVersions()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository, out DataTypeDefinitionRepository _);

                var versions = new List<Guid>();
                var hasPropertiesContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                ServiceContext.FileService.SaveTemplate(hasPropertiesContentType.DefaultTemplate); // else, FK violation on contentType!

                IContent content1 = MockedContent.CreateSimpleContent(hasPropertiesContentType);

                // save = create the initial version
                contentTypeRepository.AddOrUpdate(hasPropertiesContentType);
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // the first version

                // publish = new edit version
                content1.SetValue("title", "title");
                ((Content) content1).PublishValues();
                ((Content) content1).PublishedState = PublishedState.Publishing;
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // NEW VERSION

                // new edit version has been created
                Assert.AreNotEqual(versions[versions.Count - 2], versions[versions.Count - 1]);
                Assert.IsTrue(content1.Published);
                Assert.AreEqual(PublishedState.Published, ((Content) content1).PublishedState);
                Assert.AreEqual(versions[versions.Count - 1], repository.Get(content1.Id).Version);

                // misc checks
                Assert.AreEqual(true, unitOfWork.Database.ExecuteScalar<bool>("SELECT published FROM uDocument WHERE nodeId=@id", new { id = content1.Id }));
                Console.WriteLine(unitOfWork.Database.ExecuteScalar<DateTime>("SELECT updateDate FROM uContent WHERE nodeId=@id", new { id = content1.Id }));

                // change something
                // save = update the current (draft) version
                content1.Name = "name-1";
                content1.SetValue("title", "title-1");
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // the same version

                // no new version has been created
                Assert.AreEqual(versions[versions.Count - 2], versions[versions.Count - 1]);
                Assert.IsTrue(content1.Published);
                Assert.AreEqual(versions[versions.Count - 1], repository.Get(content1.Id).Version);

                // misc checks
                Assert.AreEqual(true, unitOfWork.Database.ExecuteScalar<bool>("SELECT published FROM uDocument WHERE nodeId=@id", new { id = content1.Id }));
                Console.WriteLine(unitOfWork.Database.ExecuteScalar<DateTime>("SELECT updateDate FROM uContent WHERE nodeId=@id", new { id = content1.Id }));

                // unpublish = no impact on versions
                ((Content) content1).PublishedState = PublishedState.Unpublishing;
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // the same version

                // no new version has been created
                Assert.AreEqual(versions[versions.Count - 2], versions[versions.Count - 1]);
                Assert.IsFalse(content1.Published);
                Assert.AreEqual(PublishedState.Unpublished, ((Content) content1).PublishedState);
                Assert.AreEqual(versions[versions.Count - 1], repository.Get(content1.Id).Version);

                // misc checks
                Assert.AreEqual(false, unitOfWork.Database.ExecuteScalar<bool>("SELECT published FROM uDocument WHERE nodeId=@id", new { id = content1.Id }));
                Console.WriteLine(unitOfWork.Database.ExecuteScalar<DateTime>("SELECT updateDate FROM uContent WHERE nodeId=@id", new { id = content1.Id }));

                // change something
                // save = update the current (draft) version
                content1.Name = "name-2";
                content1.SetValue("title", "title-2");
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // the same version

                // no new version has been created
                Assert.AreEqual(versions[versions.Count - 2], versions[versions.Count - 1]);
                Assert.AreEqual(versions[versions.Count - 1], repository.Get(content1.Id).Version);

                // misc checks
                Assert.AreEqual(false, unitOfWork.Database.ExecuteScalar<bool>("SELECT published FROM uDocument WHERE nodeId=@id", new { id = content1.Id }));
                Console.WriteLine(unitOfWork.Database.ExecuteScalar<DateTime>("SELECT updateDate FROM uContent WHERE nodeId=@id", new { id = content1.Id }));

                // publish = version
                ((Content) content1).PublishValues();
                ((Content) content1).PublishedState = PublishedState.Publishing;
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // NEW VERSION

                // new version has been created
                Assert.AreNotEqual(versions[versions.Count - 2], versions[versions.Count - 1]);
                Assert.IsTrue(content1.Published);
                Assert.AreEqual(PublishedState.Published, ((Content) content1).PublishedState);
                Assert.AreEqual(versions[versions.Count - 1], repository.Get(content1.Id).Version);

                // misc checks
                Assert.AreEqual(true, unitOfWork.Database.ExecuteScalar<bool>("SELECT published FROM uDocument WHERE nodeId=@id", new { id = content1.Id }));
                Console.WriteLine(unitOfWork.Database.ExecuteScalar<DateTime>("SELECT updateDate FROM uContent WHERE nodeId=@id", new { id = content1.Id }));

                // change something
                // save = update the current (draft) version
                content1.Name = "name-3";
                content1.SetValue("title", "title-3");

                //Thread.Sleep(2000); // force date change

                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // the same version

                // no new version has been created
                Assert.AreEqual(versions[versions.Count - 2], versions[versions.Count - 1]);
                Assert.AreEqual(versions[versions.Count - 1], repository.Get(content1.Id).Version);

                // misc checks
                Assert.AreEqual(true, unitOfWork.Database.ExecuteScalar<bool>("SELECT published FROM uDocument WHERE nodeId=@id", new { id = content1.Id }));
                Console.WriteLine(unitOfWork.Database.ExecuteScalar<DateTime>("SELECT updateDate FROM uContent WHERE nodeId=@id", new { id = content1.Id }));

                // publish = new version
                content1.Name = "name-4";
                content1.SetValue("title", "title-4");
                ((Content) content1).PublishValues();
                ((Content) content1).PublishedState = PublishedState.Publishing;
                repository.AddOrUpdate(content1);
                unitOfWork.Flush();
                versions.Add(content1.Version); // NEW VERSION

                // a new version has been created
                Assert.AreNotEqual(versions[versions.Count - 2], versions[versions.Count - 1]);
                Assert.IsTrue(content1.Published);
                Assert.AreEqual(PublishedState.Published, ((Content) content1).PublishedState);
                Assert.AreEqual(versions[versions.Count - 1], repository.Get(content1.Id).Version);

                // misc checks
                Assert.AreEqual(true, unitOfWork.Database.ExecuteScalar<bool>("SELECT published FROM uDocument WHERE nodeId=@id", new { id = content1.Id }));
                Console.WriteLine(unitOfWork.Database.ExecuteScalar<DateTime>("SELECT updateDate FROM uContent WHERE nodeId=@id", new { id = content1.Id }));

                // all versions
                var allVersions = repository.GetAllVersions(content1.Id).ToArray();
                Console.WriteLine();
                foreach (var v in versions)
                    Console.WriteLine(v);
                Console.WriteLine();
                foreach (var v in allVersions)
                {
                    var c = (Content) v;
                    Console.WriteLine($"{c.Id} {c.Version} {(c.Published ? "+" : "-")}pub pk={c.VersionPk} ppk={c.PublishedVersionPk} name=\"{c.Name}\" pname=\"{c.PublishName}\"");
                }

                // get older version
                var content = repository.GetByVersion(versions[versions.Count - 4]);
                Assert.IsNotNull(content.Version);
                Assert.AreEqual(versions[versions.Count - 4], content.Version);
                Assert.AreEqual("name-4", content1.Name);
                Assert.AreEqual("title-4", content1.GetValue("title"));
                Assert.AreEqual("name-2", content.Name);
                Assert.AreEqual("title-2", content.GetValue("title"));

                // get all versions - most recent first
                allVersions = repository.GetAllVersions(content1.Id).ToArray();
                var expVersions = versions.Distinct().Reverse().ToArray();
                Assert.AreEqual(expVersions.Length, allVersions.Length);
                for (var i = 0; i < expVersions.Length; i++)
                    Assert.AreEqual(expVersions[i], allVersions[i].Version);
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
        public void PropertyDataAssignedCorrectly()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository, out DataTypeDefinitionRepository _);

                var emptyContentType = MockedContentTypes.CreateBasicContentType();
                var hasPropertiesContentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                ServiceContext.FileService.SaveTemplate(hasPropertiesContentType.DefaultTemplate); // else, FK violation on contentType!
                var content1 = MockedContent.CreateSimpleContent(hasPropertiesContentType);
                var content2 = MockedContent.CreateBasicContent(emptyContentType);
                var content3 = MockedContent.CreateSimpleContent(hasPropertiesContentType);

                contentTypeRepository.AddOrUpdate(emptyContentType);
                contentTypeRepository.AddOrUpdate(hasPropertiesContentType);
                repository.AddOrUpdate(content1);
                repository.AddOrUpdate(content2);
                repository.AddOrUpdate(content3);
                unitOfWork.Flush();

                // this will cause the GetPropertyCollection to execute and we need to ensure that
                // all of the properties and property types are all correct
                var result = repository.GetAll(content1.Id, content2.Id, content3.Id).ToArray();
                var n1 = result[0];
                var n2 = result[1];
                var n3 = result[2];

                Assert.AreEqual(content1.Id, n1.Id);
                Assert.AreEqual(content2.Id, n2.Id);
                Assert.AreEqual(content3.Id, n3.Id);

                // compare everything including properties and their values
                // this ensures that they have been properly retrieved
                TestHelper.AssertPropertyValuesAreEqual(content1, n1);
                TestHelper.AssertPropertyValuesAreEqual(content2, n2);
                TestHelper.AssertPropertyValuesAreEqual(content3, n3);
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
        public void PropertyValuesWithSpecialTypes()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository, out DataTypeDefinitionRepository dataTypeDefinitionRepository);

                var dtd = new DataTypeDefinition(-1, Constants.PropertyEditors.DecimalAlias) { Name = "test", DatabaseType = DataTypeDatabaseType.Decimal };
                dataTypeDefinitionRepository.AddOrUpdate(dtd);
                unitOfWork.Complete();

                const string decimalPropertyAlias = "decimalProperty";
                const string intPropertyAlias = "intProperty";
                const string dateTimePropertyAlias = "datetimeProperty";
                var dateValue = new DateTime(2016, 1, 6);

                var propertyTypeCollection = new PropertyTypeCollection(true,
                    new List<PropertyType>
                    {
                        MockedPropertyTypes.CreateDecimalProperty(decimalPropertyAlias, "Decimal property", dtd.Id),
                        MockedPropertyTypes.CreateIntegerProperty(intPropertyAlias, "Integer property"),
                        MockedPropertyTypes.CreateDateTimeProperty(dateTimePropertyAlias, "DateTime property")
                    });
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage", propertyTypeCollection);
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Complete();

                // int and decimal values are passed in as strings as they would be from the backoffice UI
                var textpage = MockedContent.CreateSimpleContentWithSpecialDatabaseTypes(contentType, "test@umbraco.org", -1, "100", "150", dateValue);

                repository.AddOrUpdate(textpage);
                unitOfWork.Complete();

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
        public void SaveContent()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository);
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage2", "Textpage");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                IContent textpage = MockedContent.CreateSimpleContent(contentType);

                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(textpage);
                unitOfWork.Complete();

                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(textpage.HasIdentity, Is.True);
            }
        }

        [Test]
        public void SaveContentWithDefaultTemplate()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository, out TemplateRepository templateRepository);

                var template = new Template("hello", "hello");
                templateRepository.AddOrUpdate(template);
                unitOfWork.Flush();

                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage2", "Textpage");
                contentType.AllowedTemplates = Enumerable.Empty<ITemplate>(); // because CreateSimpleContentType assigns one already
                contentType.SetDefaultTemplate(template);
                var textpage = MockedContent.CreateSimpleContent(contentType);

                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(textpage);
                unitOfWork.Flush();

                var fetched = repository.Get(textpage.Id);

                Assert.NotNull(textpage.Template);
                Assert.AreEqual(textpage.Template, contentType.DefaultTemplate);

                unitOfWork.Complete();

                TestHelper.AssertPropertyValuesAreEqual(textpage, fetched, "yyyy-MM-dd HH:mm:ss");
            }
        }

        //Covers issue U4-2791 and U4-2607
        [Test]
        public void SaveContentWithAtSignInName()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository);
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.AddOrUpdate(contentType);
                unitOfWork.Flush();

                var textpage = MockedContent.CreateSimpleContent(contentType, "test@umbraco.org");
                var anotherTextpage = MockedContent.CreateSimpleContent(contentType, "@lightgiants");

                repository.AddOrUpdate(textpage);
                repository.AddOrUpdate(anotherTextpage);
                unitOfWork.Flush();


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
        public void SaveContentMultiple()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository);
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage1", "Textpage");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                var textpage = MockedContent.CreateSimpleContent(contentType);

                contentTypeRepository.AddOrUpdate(contentType);
                repository.AddOrUpdate(textpage);
                unitOfWork.Flush();

                var subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
                repository.AddOrUpdate(subpage);
                unitOfWork.Flush();

                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(textpage.HasIdentity, Is.True);
                Assert.That(subpage.HasIdentity, Is.True);
                Assert.That(textpage.Id, Is.EqualTo(subpage.ParentId));
            }

        }


        [Test]
        public void GetContentIsNotDirty()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var content = repository.Get(NodeDto.NodeIdSeed + 3);
                var dirty = ((Content) content).IsDirty();

                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void UpdateContent()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var content = repository.Get(NodeDto.NodeIdSeed + 2);
                content.Name = "About 2";
                repository.AddOrUpdate(content);
                unitOfWork.Flush();
                var updatedContent = repository.Get(NodeDto.NodeIdSeed + 2);

                Assert.AreEqual(content.Id, updatedContent.Id);
                Assert.AreEqual(content.Name, updatedContent.Name);
                Assert.AreEqual(content.Version, updatedContent.Version);

                Assert.AreEqual(content.GetValue("title"), "Welcome to our Home page");
                content.SetValue("title", "toot");
                repository.AddOrUpdate(content);
                unitOfWork.Flush();
                updatedContent = repository.Get(NodeDto.NodeIdSeed + 2);

                Assert.AreEqual("toot", updatedContent.GetValue("title"));
                Assert.AreEqual(content.Version, updatedContent.Version);
            }
        }

        [Test]
        public void UpdateContentWithNullTemplate()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var content = repository.Get(NodeDto.NodeIdSeed + 2);
                content.Template = null;
                repository.AddOrUpdate(content);
                unitOfWork.Flush();
                var updatedContent = repository.Get(NodeDto.NodeIdSeed + 2);

                Assert.IsNull(updatedContent.Template);
            }

        }

        [Test]
        public void DeleteContent()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out var contentTypeRepository);
                var contentType = contentTypeRepository.Get(NodeDto.NodeIdSeed + 1);
                var content = new Content("Textpage 2 Child Node", NodeDto.NodeIdSeed + 4, contentType);
                content.CreatorId = 0;
                content.WriterId = 0;

                repository.AddOrUpdate(content);
                unitOfWork.Flush();
                var id = content.Id;

                repository.Delete(content);
                unitOfWork.Flush();

                var content1 = repository.Get(id);
                Assert.IsNull(content1);
            }
        }

        [Test]
        public void GetContent()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);
                var content = repository.Get(NodeDto.NodeIdSeed + 4);

                Assert.AreEqual(NodeDto.NodeIdSeed + 4, content.Id);
                Assert.That(content.CreateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(content.UpdateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.AreNotEqual(0, content.ParentId);
                Assert.AreEqual("Text Page 2", content.Name);
                Assert.AreNotEqual(Guid.Empty, content.Version);
                Assert.AreEqual(NodeDto.NodeIdSeed + 1, content.ContentTypeId);
                Assert.That(content.Path, Is.Not.Empty);
                Assert.That(content.Properties.Any(), Is.True);
            }
        }

        [Test]
        public void QueryContent()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Level == 2);
                var result = repository.GetByQuery(query);

                Assert.GreaterOrEqual(2, result.Count());
            }
        }

        [Test]
        public void GetAllContentManyVersions()
        {
            IContent[] result;

            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);
                result = repository.GetAll().ToArray();

                // save them all
                foreach (var content in result)
                {
                    content.SetValue("title", content.GetValue<string>("title") + "x");
                    repository.AddOrUpdate(content);
                }
                unitOfWork.Flush();

                // publish them all
                foreach (var content in result)
                {
                    ((Content) content).PublishAllValues();
                    ((Content) content).PublishedState = PublishedState.Publishing;
                    repository.AddOrUpdate(content);
                }
                unitOfWork.Flush();

                unitOfWork.Complete();
            }

            // get them all again
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);
                var result2 = repository.GetAll().ToArray();

                Assert.AreEqual(result.Length, result2.Length);
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
        public void GetPagedResultsByQuery_CustomPropertySort()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Name.Contains("Text"));

                try
                {
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlCount = true;

                    var result = repository.GetPagedResultsByQuery(query, 0, 2, out var totalRecords, "title", Direction.Ascending, false);

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
        public void GetPagedResultsByQuery_FirstPage()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Level == 2);

                try
                {
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                    unitOfWork.Database.AsUmbracoDatabase().EnableSqlCount = true;

                    var result = repository.GetPagedResultsByQuery(query, 0, 1, out var totalRecords, "Name", Direction.Ascending, true);

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
        public void GetPagedResultsByQuery_SecondPage()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Level == 2);
                var result = repository.GetPagedResultsByQuery(query, 1, 1, out var totalRecords, "Name", Direction.Ascending, true);

                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_SinglePage()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Level == 2);
                var result = repository.GetPagedResultsByQuery(query, 0, 2, out var totalRecords, "Name", Direction.Ascending, true);

                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_DescendingOrder()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Level == 2);
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out var totalRecords, "Name", Direction.Descending, true);

                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_FilterMatchingSome()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Level == 2);

                var filterQuery = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Name.Contains("Page 2"));
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out var totalRecords, "Name", Direction.Ascending, true, filterQuery);

                Assert.That(totalRecords, Is.EqualTo(1));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
            }
        }

        [Test]
        public void GetPagedResultsByQuery_FilterMatchingAll()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Level == 2);

                var filterQuery = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Name.Contains("text"));
                var result = repository.GetPagedResultsByQuery(query, 0, 1, out var totalRecords, "Name", Direction.Ascending, true, filterQuery);

                Assert.That(totalRecords, Is.EqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
            }
        }

        [Test]
        public void GetAllContentByIds()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var contents = repository.GetAll(NodeDto.NodeIdSeed + 2, NodeDto.NodeIdSeed + 3);


                Assert.That(contents, Is.Not.Null);
                Assert.That(contents.Any(), Is.True);
                Assert.That(contents.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void GetAllContent()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var contents = repository.GetAll();

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
        public void ExistContent()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var exists = repository.Exists(NodeDto.NodeIdSeed + 2);

                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void CountContent()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Level == 2);
                var result = repository.Count(query);

                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void QueryContentByUniqueId()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork, out _);

                var query = unitOfWork.SqlContext.Query<IContent>().Where(x => x.Key == new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
                var content = repository.GetByQuery(query).SingleOrDefault();

                Assert.IsNotNull(content);
                Assert.AreEqual(NodeDto.NodeIdSeed + 2, content.Id);
            }
        }

        public void CreateTestData()
        {
            //Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
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

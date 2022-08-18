// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUpData()
    {
        CreateTestData();

        ContentRepositoryBase.ThrowOnWarning = true;
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private ContentType _contentType;
    private Content _textpage;
    private Content _subpage;
    private Content _subpage2;
    private Content _trashed;

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private FileSystems FileSystems => GetRequiredService<FileSystems>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    public void CreateTestData()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        // Create and Save ContentType "umbTextpage" -> (_contentType.Id)
        _contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        _contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
        ContentTypeService.Save(_contentType);

        // Create and Save Content "Homepage" based on "umbTextpage" -> (_textpage.Id)
        _textpage = ContentBuilder.CreateSimpleContent(_contentType);
        _textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
        ContentService.Save(_textpage, 0);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> (_subpage.Id)
        _subpage = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 1", _textpage.Id);
        _subpage.Key = new Guid("FF11402B-7E53-4654-81A7-462AC2108059");
        ContentService.Save(_subpage, 0);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> (_subpage2.Id)
        _subpage2 = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 2", _textpage.Id);
        ContentService.Save(_subpage2, 0);

        // Create and Save Content "Text Page Deleted" based on "umbTextpage" -> (_trashed.Id)
        _trashed = ContentBuilder.CreateSimpleContent(_contentType, "Text Page Deleted", -20);
        _trashed.Trashed = true;
        ContentService.Save(_trashed, 0);
    }

    private DocumentRepository CreateRepository(IScopeAccessor scopeAccessor, out ContentTypeRepository contentTypeRepository, out DataTypeRepository dtdRepository, AppCaches appCaches = null)
    {
        appCaches ??= AppCaches;

        var ctRepository = CreateRepository(scopeAccessor, out contentTypeRepository, out TemplateRepository tr);
        var editors = new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
        dtdRepository = new DataTypeRepository(scopeAccessor, appCaches, editors, LoggerFactory.CreateLogger<DataTypeRepository>(), LoggerFactory, ConfigurationEditorJsonSerializer);
        return ctRepository;
    }

    private DocumentRepository CreateRepository(IScopeAccessor scopeAccessor, out ContentTypeRepository contentTypeRepository, AppCaches appCaches = null) =>
        CreateRepository(scopeAccessor, out contentTypeRepository, out TemplateRepository tr, appCaches);

    private DocumentRepository CreateRepository(IScopeAccessor scopeAccessor, out ContentTypeRepository contentTypeRepository, out TemplateRepository templateRepository, AppCaches appCaches = null)
    {
        appCaches ??= AppCaches;

        var runtimeSettingsMock = new Mock<IOptionsMonitor<RuntimeSettings>>();
        runtimeSettingsMock.Setup(x => x.CurrentValue).Returns(new RuntimeSettings());

        templateRepository = new TemplateRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<TemplateRepository>(), FileSystems, IOHelper, ShortStringHelper, Mock.Of<IViewHelper>(), runtimeSettingsMock.Object);
        var tagRepository = new TagRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<TagRepository>());
        var commonRepository =
            new ContentTypeCommonRepository(scopeAccessor, templateRepository, appCaches, ShortStringHelper);
        var languageRepository =
            new LanguageRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<LanguageRepository>());
        contentTypeRepository = new ContentTypeRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<ContentTypeRepository>(), commonRepository, languageRepository, ShortStringHelper);
        var relationTypeRepository = new RelationTypeRepository(scopeAccessor, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>());
        var entityRepository = new EntityRepository(scopeAccessor, AppCaches.Disabled);
        var relationRepository = new RelationRepository(scopeAccessor, LoggerFactory.CreateLogger<RelationRepository>(), relationTypeRepository, entityRepository);
        var propertyEditors =
            new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
        var dataValueReferences =
            new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>());
        var repository = new DocumentRepository(
            scopeAccessor,
            appCaches,
            LoggerFactory.CreateLogger<DocumentRepository>(),
            LoggerFactory,
            contentTypeRepository,
            templateRepository,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferences,
            DataTypeService,
            ConfigurationEditorJsonSerializer,
            Mock.Of<IEventAggregator>());
        return repository;
    }

    [Test]
    public void CacheActiveForIntsAndGuids()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, realCache);

            var udb = scopeAccessor.AmbientScope.Database;

            udb.EnableSqlCount = false;

            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);
            var contentType =
                ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);

            contentTypeRepository.Save(contentType);
            var content = ContentBuilder.CreateSimpleContent(contentType);
            repository.Save(content);

            udb.EnableSqlCount = true;

            // go get it, this should already be cached since the default repository key is the INT
            repository.Get(content.Id);
            Assert.AreEqual(0, udb.SqlCount);

            // retrieve again, this should use cache
            repository.Get(content.Id);
            Assert.AreEqual(0, udb.SqlCount);

            // reset counter
            udb.EnableSqlCount = false;
            udb.EnableSqlCount = true;

            // now get by GUID, this won't be cached yet because the default repo key is not a GUID
            repository.Get(content.Key);
            var sqlCount = udb.SqlCount;
            Assert.Greater(sqlCount, 0);

            // retrieve again, this should use cache now
            repository.Get(content.Key);
            Assert.AreEqual(sqlCount, udb.SqlCount);
        }
    }

    [Test]
    public void CreateVersions()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
            var versions = new List<int>();
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);
            var hasPropertiesContentType =
                ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);

            contentTypeRepository.Save(hasPropertiesContentType);

            IContent content1 = ContentBuilder.CreateSimpleContent(hasPropertiesContentType);

            // save = create the initial version
            repository.Save(content1);

            versions.Add(content1.VersionId); // the first version

            // publish = new edit version
            content1.SetValue("title", "title");
            content1.PublishCulture(CultureImpact.Invariant);
            content1.PublishedState = PublishedState.Publishing;
            repository.Save(content1);

            versions.Add(content1.VersionId); // NEW VERSION

            // new edit version has been created
            Assert.AreNotEqual(versions[^2], versions[^1]);
            Assert.IsTrue(content1.Published);
            Assert.AreEqual(PublishedState.Published, ((Content)content1).PublishedState);
            Assert.AreEqual(versions[^1], repository.Get(content1.Id).VersionId);

            // misc checks
            Assert.AreEqual(true, ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Document} WHERE nodeId=@id",
                    new { id = content1.Id }));

            // change something
            // save = update the current (draft) version
            content1.Name = "name-1";
            content1.SetValue("title", "title-1");
            repository.Save(content1);

            versions.Add(content1.VersionId); // the same version

            // no new version has been created
            Assert.AreEqual(versions[^2], versions[^1]);
            Assert.IsTrue(content1.Published);
            Assert.AreEqual(versions[^1], repository.Get(content1.Id).VersionId);

            // misc checks
            Assert.AreEqual(
                true,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Document} WHERE nodeId=@id",
                    new { id = content1.Id }));

            // unpublish = no impact on versions
            ((Content)content1).PublishedState = PublishedState.Unpublishing;
            repository.Save(content1);

            versions.Add(content1.VersionId); // the same version

            // no new version has been created
            Assert.AreEqual(versions[^2], versions[^1]);
            Assert.IsFalse(content1.Published);
            Assert.AreEqual(PublishedState.Unpublished, ((Content)content1).PublishedState);
            Assert.AreEqual(versions[^1], repository.Get(content1.Id).VersionId);

            // misc checks
            Assert.AreEqual(
                false,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Document} WHERE nodeId=@id",
                    new { id = content1.Id }));

            // change something
            // save = update the current (draft) version
            content1.Name = "name-2";
            content1.SetValue("title", "title-2");
            repository.Save(content1);

            versions.Add(content1.VersionId); // the same version

            // no new version has been created
            Assert.AreEqual(versions[^2], versions[^1]);
            Assert.AreEqual(versions[^1], repository.Get(content1.Id).VersionId);

            // misc checks
            Assert.AreEqual(
                false,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Document} WHERE nodeId=@id",
                    new { id = content1.Id }));

            // publish = version
            content1.PublishCulture(CultureImpact.Invariant);
            content1.PublishedState = PublishedState.Publishing;
            repository.Save(content1);

            versions.Add(content1.VersionId); // NEW VERSION

            // new version has been created
            Assert.AreNotEqual(versions[^2], versions[^1]);
            Assert.IsTrue(content1.Published);
            Assert.AreEqual(PublishedState.Published, ((Content)content1).PublishedState);
            Assert.AreEqual(versions[^1], repository.Get(content1.Id).VersionId);

            // misc checks
            Assert.AreEqual(
                true,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Document} WHERE nodeId=@id",
                    new { id = content1.Id }));

            // change something
            // save = update the current (draft) version
            content1.Name = "name-3";
            content1.SetValue("title", "title-3");

            //// Thread.Sleep(2000); // force date change

            repository.Save(content1);

            versions.Add(content1.VersionId); // the same version

            // no new version has been created
            Assert.AreEqual(versions[^2], versions[^1]);
            Assert.AreEqual(versions[^1], repository.Get(content1.Id).VersionId);

            // misc checks
            Assert.AreEqual(
                true,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Document} WHERE nodeId=@id",
                    new { id = content1.Id }));

            // publish = new version
            content1.Name = "name-4";
            content1.SetValue("title", "title-4");
            content1.PublishCulture(CultureImpact.Invariant);
            content1.PublishedState = PublishedState.Publishing;
            repository.Save(content1);

            versions.Add(content1.VersionId); // NEW VERSION

            // a new version has been created
            Assert.AreNotEqual(versions[^2], versions[^1]);
            Assert.IsTrue(content1.Published);
            Assert.AreEqual(PublishedState.Published, ((Content)content1).PublishedState);
            Assert.AreEqual(versions[^1], repository.Get(content1.Id).VersionId);

            // misc checks
            Assert.AreEqual(
                true,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Document} WHERE nodeId=@id",
                    new { id = content1.Id }));

            // all versions
            var allVersions = repository.GetAllVersions(content1.Id).ToArray();
            Console.WriteLine();
            foreach (var v in versions)
            {
                Console.WriteLine(v);
            }

            Console.WriteLine();
            foreach (var v in allVersions)
            {
                var c = (Content)v;
                Console.WriteLine(
                    $"{c.Id} {c.VersionId} {(c.Published ? "+" : "-")}pub pk={c.VersionId} ppk={c.PublishedVersionId} name=\"{c.Name}\" pname=\"{c.PublishName}\"");
            }

            // get older version
            var content = repository.GetVersion(versions[^4]);
            Assert.AreNotEqual(0, content.VersionId);
            Assert.AreEqual(versions[^4], content.VersionId);
            Assert.AreEqual("name-4", content1.Name);
            Assert.AreEqual("title-4", content1.GetValue("title"));
            Assert.AreEqual("name-2", content.Name);
            Assert.AreEqual("title-2", content.GetValue("title"));

            // get all versions - most recent first
            allVersions = repository.GetAllVersions(content1.Id).ToArray();
            var expVersions = versions.Distinct().Reverse().ToArray();
            Assert.AreEqual(expVersions.Length, allVersions.Length);
            for (var i = 0; i < expVersions.Length; i++)
            {
                Assert.AreEqual(expVersions[i], allVersions[i].VersionId);
            }
        }
    }

    /// <summary>
    ///     This tests the regression issue of U4-9438
    /// </summary>
    /// <remarks>
    ///     The problem was the iteration of the property data in VersionableRepositoryBase when a content item
    ///     in the list actually doesn't have any property types, it would still skip over a property row.
    ///     To test, we have 3 content items, the first has properties, the second doesn't and the third does.
    /// </remarks>
    [Test]
    public void PropertyDataAssignedCorrectly()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);

            var emptyContentType = ContentTypeBuilder.CreateBasicContentType();
            var hasPropertiesContentType =
                ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);
            contentTypeRepository.Save(emptyContentType);
            contentTypeRepository.Save(hasPropertiesContentType);

            var content1 = ContentBuilder.CreateSimpleContent(hasPropertiesContentType);
            var content2 = ContentBuilder.CreateBasicContent(emptyContentType);
            var content3 = ContentBuilder.CreateSimpleContent(hasPropertiesContentType);

            repository.Save(content1);
            repository.Save(content2);
            repository.Save(content3);

            // this will cause the GetPropertyCollection to execute and we need to ensure that
            // all of the properties and property types are all correct
            var result = repository.GetMany(content1.Id, content2.Id, content3.Id).ToArray();
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

    //// /// <summary>
    //// /// This test ensures that when property values using special database fields are saved, the actual data in the
    //// /// object being stored is also transformed in the same way as the data being stored in the database is.
    //// /// Before you would see that ex: a decimal value being saved as 100 or "100", would be that exact value in the
    //// /// object, but the value saved to the database was actually 100.000000.
    //// /// When querying the database for the value again - the value would then differ from what is in the object.
    //// /// This caused inconsistencies between saving+publishing and simply saving and then publishing, due to the former
    //// /// sending the non-transformed data directly on to publishing.
    //// /// </summary>
    //// [Test]
    //// public void PropertyValuesWithSpecialTypes()
    //// {
    ////     var provider = ScopeProvider;
    ////     using (var scope = provider.CreateScope())
    ////     {
    ////         var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository dataTypeDefinitionRepository);
    ////
    ////         var editor = new DecimalPropertyEditor(LoggerFactory, DataTypeService, LocalizationService, LocalizedTextService, ShortStringHelper);
    ////         var dtd = new DataType(editor) { Name = "test", DatabaseType = ValueStorageType.Decimal };
    ////         dataTypeDefinitionRepository.Save(dtd);
    ////
    ////         const string decimalPropertyAlias = "decimalProperty";
    ////         const string intPropertyAlias = "intProperty";
    ////         const string dateTimePropertyAlias = "datetimeProperty";
    ////         var dateValue = new DateTime(2016, 1, 6);
    ////
    ////         var propertyTypeCollection = new PropertyTypeCollection(true,
    ////             new List<PropertyType>
    ////             {
    ////                 MockedPropertyTypes.CreateDecimalProperty(decimalPropertyAlias, "Decimal property", dtd.Id),
    ////                 MockedPropertyTypes.CreateIntegerProperty(intPropertyAlias, "Integer property"),
    ////                 MockedPropertyTypes.CreateDateTimeProperty(dateTimePropertyAlias, "DateTime property")
    ////             });
    ////         var contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", propertyTypeCollection);
    ////         contentTypeRepository.Save(contentType);
    ////
    ////         // int and decimal values are passed in as strings as they would be from the backoffice UI
    ////         var textpage = ContentBuilder.CreateSimpleContentWithSpecialDatabaseTypes(contentType, "test@umbraco.org", -1, "100", "150", dateValue);
    ////
    ////         repository.Save(textpage);
    ////         scope.Complete();
    ////
    ////         Assert.That(contentType.HasIdentity, Is.True);
    ////         Assert.That(textpage.HasIdentity, Is.True);
    ////
    ////         var persistedTextpage = repository.Get(textpage.Id);
    ////         Assert.That(persistedTextpage.Name, Is.EqualTo(textpage.Name));
    ////         Assert.AreEqual(100m, persistedTextpage.GetValue(decimalPropertyAlias));
    ////         Assert.AreEqual(persistedTextpage.GetValue(decimalPropertyAlias), textpage.GetValue(decimalPropertyAlias));
    ////         Assert.AreEqual(150, persistedTextpage.GetValue(intPropertyAlias));
    ////         Assert.AreEqual(persistedTextpage.GetValue(intPropertyAlias), textpage.GetValue(intPropertyAlias));
    ////         Assert.AreEqual(dateValue, persistedTextpage.GetValue(dateTimePropertyAlias));
    ////         Assert.AreEqual(persistedTextpage.GetValue(dateTimePropertyAlias), textpage.GetValue(dateTimePropertyAlias));
    ////     }
    //// }

    [Test]
    public void SaveContent()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);
            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository);
            var contentType =
                ContentTypeBuilder.CreateSimpleContentType("umbTextpage2", "Textpage", defaultTemplateId: template.Id);
            contentTypeRepository.Save(contentType);

            IContent textpage = ContentBuilder.CreateSimpleContent(contentType);

            repository.Save(textpage);
            scope.Complete();

            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(textpage.HasIdentity, Is.True);
        }
    }

    [Test]
    public void SaveContentWithDefaultTemplate()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out TemplateRepository templateRepository);

            var template = new Template(ShortStringHelper, "hello", "hello");
            templateRepository.Save(template);

            var contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage2", "Textpage");
            contentType.AllowedTemplates =
                Enumerable.Empty<ITemplate>(); // because CreateSimpleContentType assigns one already
            contentType.SetDefaultTemplate(template);
            contentTypeRepository.Save(contentType);

            var textpage = ContentBuilder.CreateSimpleContent(contentType);
            repository.Save(textpage);

            var fetched = repository.Get(textpage.Id);

            Assert.True(textpage.TemplateId.HasValue);
            Assert.NotZero(textpage.TemplateId.Value);
            Assert.AreEqual(textpage.TemplateId, contentType.DefaultTemplate.Id);

            scope.Complete();

            TestHelper.AssertPropertyValuesAreEqual(textpage, fetched);
        }
    }

    // Covers issue U4-2791 and U4-2607
    [Test]
    public void SaveContentWithAtSignInName()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);
            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository);
            var contentType =
                ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);
            contentTypeRepository.Save(contentType);

            var textpage = ContentBuilder.CreateSimpleContent(contentType, "test@umbraco.org");
            var anotherTextpage = ContentBuilder.CreateSimpleContent(contentType, "@lightgiants");

            repository.Save(textpage);
            repository.Save(anotherTextpage);

            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(textpage.HasIdentity, Is.True);

            var content = repository.Get(textpage.Id);
            Assert.That(content.Name, Is.EqualTo(textpage.Name));

            var content2 = repository.Get(anotherTextpage.Id);
            Assert.That(content2.Name, Is.EqualTo(anotherTextpage.Name));

            scope.Complete();
        }
    }

    [Test]
    public void SaveContentMultiple()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);
            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository);
            var contentType =
                ContentTypeBuilder.CreateSimpleContentType("umbTextpage1", "Textpage", defaultTemplateId: template.Id);
            contentTypeRepository.Save(contentType);

            var textpage = ContentBuilder.CreateSimpleContent(contentType);

            repository.Save(textpage);

            var subpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
            repository.Save(subpage);

            Assert.That(contentType.HasIdentity, Is.True);
            Assert.That(textpage.HasIdentity, Is.True);
            Assert.That(subpage.HasIdentity, Is.True);
            Assert.That(textpage.Id, Is.EqualTo(subpage.ParentId));
        }
    }

    [Test]
    public void GetContentIsNotDirty()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var content = repository.Get(_subpage2.Id);
            var dirty = ((Content)content).IsDirty();

            Assert.That(dirty, Is.False);
        }
    }

    [Test]
    public void UpdateContent()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var content = repository.Get(_subpage.Id);
            content.Name = "About 2";
            repository.Save(content);

            var updatedContent = repository.Get(_subpage.Id);

            Assert.AreEqual(content.Id, updatedContent.Id);
            Assert.AreEqual(content.Name, updatedContent.Name);
            Assert.AreEqual(content.VersionId, updatedContent.VersionId);

            Assert.AreEqual(content.GetValue("title"), "Welcome to our Home page");
            content.SetValue("title", "toot");
            repository.Save(content);

            updatedContent = repository.Get(_subpage.Id);

            Assert.AreEqual("toot", updatedContent.GetValue("title"));
            Assert.AreEqual(content.VersionId, updatedContent.VersionId);
        }
    }

    [Test]
    public void UpdateContentWithNullTemplate()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var content = repository.Get(_subpage.Id);
            content.TemplateId = null;
            repository.Save(content);

            var updatedContent = repository.Get(_subpage.Id);

            Assert.False(updatedContent.TemplateId.HasValue);
        }
    }

    [Test]
    public void DeleteContent()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository);
            var contentType = contentTypeRepository.Get(_contentType.Id);
            var content = new Content("Textpage 2 Child Node", _trashed.Id, contentType) { CreatorId = 0, WriterId = 0 };

            repository.Save(content);

            var id = content.Id;

            repository.Delete(content);

            var content1 = repository.Get(id);
            Assert.IsNull(content1);
        }
    }

    [Test]
    public void GetContent()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);
            var content = repository.Get(_subpage2.Id);

            Assert.AreEqual(_subpage2.Id, content.Id);
            Assert.That(content.CreateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(content.UpdateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.AreNotEqual(0, content.ParentId);
            Assert.AreEqual("Text Page 2", content.Name);
            Assert.AreNotEqual(0, content.VersionId);
            Assert.AreEqual(_contentType.Id, content.ContentTypeId);
            Assert.That(content.Path, Is.Not.Empty);
            Assert.That(content.Properties.Any(), Is.True);
        }
    }

    [Test]
    public void QueryContent()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Level == 2);
            var result = repository.Get(query);

            Assert.GreaterOrEqual(2, result.Count());
        }
    }

    [Test]
    public void GetAllContentManyVersions()
    {
        IContent[] result;

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);
            result = repository.GetMany().ToArray();

            // save them all
            foreach (var content in result)
            {
                content.SetValue("title", content.GetValue<string>("title") + "x");
                repository.Save(content);
            }

            // publish them all
            foreach (var content in result)
            {
                content.PublishCulture(CultureImpact.Invariant);
                repository.Save(content);
            }

            scope.Complete();
        }

        // get them all again
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);
            var result2 = repository.GetMany().ToArray();

            Assert.AreEqual(result.Length, result2.Length);
        }
    }

    [Test]
    public void AliasRegexTest()
    {
        var regex = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())).AliasRegex;
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
    public void GetPagedResultsByQuery_With_Variant_Names()
    {
        // One invariant content type named "umbInvariantTextPage"
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var invariantCt = ContentTypeBuilder.CreateSimpleContentType("umbInvariantTextpage", "Invariant Textpage", defaultTemplateId: template.Id);
        invariantCt.Variations = ContentVariation.Nothing;
        foreach (var p in invariantCt.PropertyTypes)
        {
            p.Variations = ContentVariation.Nothing;
        }

        ContentTypeService.Save(invariantCt);

        // One variant (by culture) content type named "umbVariantTextPage"
        // with properties, every 2nd one being variant (by culture), the other being invariant
        var variantCt = ContentTypeBuilder.CreateSimpleContentType("umbVariantTextpage", "Variant Textpage", defaultTemplateId: template.Id);
        variantCt.Variations = ContentVariation.Culture;
        var propTypes = variantCt.PropertyTypes.ToList();
        for (var i = 0; i < propTypes.Count; i++)
        {
            var p = propTypes[i];
            p.Variations = i % 2 == 0 ? ContentVariation.Culture : ContentVariation.Nothing;
        }

        ContentTypeService.Save(variantCt);

        invariantCt.AllowedContentTypes =
            new[] { new ContentTypeSort(invariantCt.Id, 0), new ContentTypeSort(variantCt.Id, 1) };
        ContentTypeService.Save(invariantCt);

        // Create content
        var root = ContentBuilder.CreateSimpleContent(invariantCt);
        ContentService.Save(root);

        var children = new List<IContent>();

        for (var i = 0; i < 25; i++)
        {
            var isInvariant = i % 2 == 0;
            var name = (isInvariant ? "INV" : "VAR") + "_" + Guid.NewGuid();
            var culture = isInvariant ? null : "en-US";

            var child = ContentBuilder.CreateSimpleContent(
                isInvariant ? invariantCt : variantCt,
                name,
                root,
                culture,
                setPropertyValues: isInvariant);

            if (!isInvariant)
            {
                // manually set the property values since we have mixed variant/invariant property types
                child.SetValue("title", name + " Subpage", culture);
                child.SetValue("bodyText", "This is a subpage"); // this one is invariant
                child.SetValue("author", "John Doe", culture);
            }

            ContentService.Save(child);
            children.Add(child);
        }

        var child1 = children[1];
        Assert.IsTrue(child1.ContentType.VariesByCulture());
        Assert.IsTrue(child1.Name.StartsWith("VAR"));
        Assert.IsTrue(child1.GetCultureName("en-US").StartsWith("VAR"));

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var child = repository.Get(children[1].Id); // 1 is variant
            Assert.IsTrue(child.ContentType.VariesByCulture());
            Assert.IsTrue(child.Name.StartsWith("VAR"));
            Assert.IsTrue(child.GetCultureName("en-US").StartsWith("VAR"));

            try
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.ParentId == root.Id);
                var result = repository.GetPage(query, 0, 20, out var totalRecords, null, Ordering.By("UpdateDate"));

                Assert.AreEqual(25, totalRecords);
                foreach (var r in result)
                {
                    var isInvariant = r.ContentType.Alias == "umbInvariantTextpage";
                    var name = isInvariant ? r.Name : r.CultureInfos["en-US"].Name;
                    var namePrefix = isInvariant ? "INV" : "VAR";

                    // ensure the correct name (invariant vs variant) is in the result
                    Assert.IsTrue(name.StartsWith(namePrefix));

                    foreach (var p in r.Properties)
                    {
                        // ensure there is a value for the correct variant/invariant property
                        var value = p.GetValue(p.PropertyType.Variations.VariesByNothing() ? null : "en-US");
                        Assert.IsNotNull(value);
                    }
                }
            }
            finally
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = false;
            }
        }
    }

    [Test]
    public void GetPagedResultsByQuery_CustomPropertySort()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Name.Contains("Text"));

            try
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                var result = repository.GetPage(query, 0, 2, out var totalRecords, null, Ordering.By("title", isCustomField: true));

                Assert.AreEqual(3, totalRecords);
                Assert.AreEqual(2, result.Count());

                result = repository.GetPage(query, 1, 2, out totalRecords, null, Ordering.By("title", isCustomField: true));

                Assert.AreEqual(1, result.Count());
            }
            finally
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = false;
            }
        }
    }

    [Test]
    public void GetPagedResultsByQuery_FirstPage()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Level == 2);

            try
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                var result = repository.GetPage(query, 0, 1, out var totalRecords, null, Ordering.By("Name"));

                Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
            }
            finally
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = false;
            }
        }
    }

    [Test]
    public void GetPagedResultsByQuery_SecondPage()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Level == 2);
            var result = repository.GetPage(query, 1, 1, out var totalRecords, null, Ordering.By("Name")).ToArray();

            Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
        }
    }

    [Test]
    public void GetPagedResultsByQuery_SinglePage()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Level == 2);
            var result = repository.GetPage(query, 0, 2, out var totalRecords, null, Ordering.By("Name")).ToArray();

            Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
        }
    }

    [Test]
    public void GetPagedResultsByQuery_DescendingOrder()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Level == 2);
            var result = repository.GetPage(query, 0, 1, out var totalRecords, null, Ordering.By("Name", Direction.Descending)).ToArray();

            Assert.That(totalRecords, Is.GreaterThanOrEqualTo(2));
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
        }
    }

    [Test]
    public void GetPagedResultsByQuery_FilterMatchingSome()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Level == 2);

            var filterQuery = ScopeProvider.CreateQuery<IContent>().Where(x => x.Name.Contains("Page 2"));
            var result = repository.GetPage(query, 0, 1, out var totalRecords, filterQuery, Ordering.By("Name")).ToArray();

            Assert.That(totalRecords, Is.EqualTo(1));
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Text Page 2"));
        }
    }

    [Test]
    public void GetPagedResultsByQuery_FilterMatchingAll()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Level == 2);

            var filterQuery = ScopeProvider.CreateQuery<IContent>().Where(x => x.Name.Contains("text"));
            var result = repository.GetPage(query, 0, 1, out var totalRecords, filterQuery, Ordering.By("Name")).ToArray();

            Assert.That(totalRecords, Is.EqualTo(2));
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Text Page 1"));
        }
    }

    [Test]
    public void GetAllContentByIds()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var contents = repository.GetMany(_subpage.Id, _subpage2.Id).ToArray();

            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public void GetAllContent()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var contents = repository.GetMany().ToArray();

            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(4));

            contents = repository.GetMany(contents.Select(x => x.Id).ToArray()).ToArray();
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(4));

            contents = ((IReadRepository<Guid, IContent>)repository).GetMany(contents.Select(x => x.Key).ToArray()).ToArray();
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.GreaterThanOrEqualTo(4));
        }
    }

    [Test]
    public void ExistContent()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var exists = repository.Exists(_subpage.Id);

            Assert.That(exists, Is.True);
        }
    }

    [Test]
    public void CountContent()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>().Where(x => x.Level == 2);
            var result = repository.Count(query);

            Assert.That(result, Is.GreaterThanOrEqualTo(2));
        }
    }

    [Test]
    public void QueryContentByUniqueId()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _);

            var query = ScopeProvider.CreateQuery<IContent>()
                .Where(x => x.Key == new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0"));
            var content = repository.Get(query).SingleOrDefault();

            Assert.IsNotNull(content);
            Assert.AreEqual(_textpage.Id, content.Id);
        }
    }
}

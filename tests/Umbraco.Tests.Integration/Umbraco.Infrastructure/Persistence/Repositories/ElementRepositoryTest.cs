// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ElementRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUpData()
    {
        ContentRepositoryBase.ThrowOnWarning = true;
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private ContentType _contentType;

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    private ElementRepository CreateRepository(IScopeAccessor scopeAccessor, out ContentTypeRepository contentTypeRepository, out DataTypeRepository dtdRepository, AppCaches appCaches = null)
    {
        appCaches ??= AppCaches;

        var ctRepository = CreateRepository(scopeAccessor, out contentTypeRepository);
        var editors = new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
        dtdRepository = new DataTypeRepository(
            scopeAccessor,
            appCaches,
            editors,
            LoggerFactory.CreateLogger<DataTypeRepository>(),
            LoggerFactory,
            ConfigurationEditorJsonSerializer,
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>(),
            DataValueEditorFactory);
        return ctRepository;
    }

    private ElementRepository CreateRepository(IScopeAccessor scopeAccessor, out ContentTypeRepository contentTypeRepository, AppCaches appCaches = null)
    {
        appCaches ??= AppCaches;

        var runtimeSettingsMock = new Mock<IOptionsMonitor<RuntimeSettings>>();
        runtimeSettingsMock.Setup(x => x.CurrentValue).Returns(new RuntimeSettings());

        var templateRepository = new TemplateRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<TemplateRepository>(), GetRequiredService<FileSystems>(), ShortStringHelper, Mock.Of<IViewHelper>(), runtimeSettingsMock.Object,  Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
        var tagRepository = new TagRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<TagRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
        var commonRepository =
            new ContentTypeCommonRepository(scopeAccessor, templateRepository, appCaches, ShortStringHelper);
        var languageRepository =
            new LanguageRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<LanguageRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
        contentTypeRepository = new ContentTypeRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<ContentTypeRepository>(), commonRepository, languageRepository, ShortStringHelper, Mock.Of<IRepositoryCacheVersionService>(), IdKeyMap, Mock.Of<ICacheSyncService>());
        var relationTypeRepository = new RelationTypeRepository(scopeAccessor, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
        var entityRepository = new EntityRepository(scopeAccessor, AppCaches.Disabled);
        var relationRepository = new RelationRepository(scopeAccessor, LoggerFactory.CreateLogger<RelationRepository>(), relationTypeRepository, entityRepository, Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
        var propertyEditors =
            new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
        var dataValueReferences =
            new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>(), new NullLogger<DataValueReferenceFactoryCollection>());
        var repository = new ElementRepository(
            scopeAccessor,
            appCaches,
            LoggerFactory.CreateLogger<ElementRepository>(),
            LoggerFactory,
            contentTypeRepository,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferences,
            DataTypeService,
            ConfigurationEditorJsonSerializer,
            Mock.Of<IEventAggregator>(),
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());
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

            var contentType = ContentTypeBuilder.CreateBasicElementType();
            contentTypeRepository.Save(contentType);
            var content = ElementBuilder.CreateBasicElement(contentType);
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
            var hasPropertiesContentType = ContentTypeBuilder.CreateSimpleElementType();
            contentTypeRepository.Save(hasPropertiesContentType);

            IElement element1 = ElementBuilder.CreateSimpleElement(hasPropertiesContentType);

            // save = create the initial version
            repository.Save(element1);

            versions.Add(element1.VersionId); // the first version

            // publish = new edit version
            element1.SetValue("title", "title");
            element1.PublishCulture(CultureImpact.Invariant, DateTime.Now, PropertyEditorCollection);
            element1.PublishedState = PublishedState.Publishing;
            repository.Save(element1);

            versions.Add(element1.VersionId); // NEW VERSION

            // new edit version has been created
            Assert.AreNotEqual(versions[^2], versions[^1]);
            Assert.IsTrue(element1.Published);
            Assert.AreEqual(PublishedState.Published, element1.PublishedState);
            Assert.AreEqual(versions[^1], repository.Get(element1.Id)!.VersionId);

            // misc checks
            Assert.AreEqual(true, ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Element} WHERE nodeId=@id",
                    new { id = element1.Id }));

            // change something
            // save = update the current (draft) version
            element1.Name = "name-1";
            element1.SetValue("title", "title-1");
            repository.Save(element1);

            versions.Add(element1.VersionId); // the same version

            // no new version has been created
            Assert.AreEqual(versions[^2], versions[^1]);
            Assert.IsTrue(element1.Published);
            Assert.AreEqual(versions[^1], repository.Get(element1.Id)!.VersionId);

            // misc checks
            Assert.AreEqual(
                true,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Element} WHERE nodeId=@id",
                    new { id = element1.Id }));

            // unpublish = no impact on versions
            element1.PublishedState = PublishedState.Unpublishing;
            repository.Save(element1);

            versions.Add(element1.VersionId); // the same version

            // no new version has been created
            Assert.AreEqual(versions[^2], versions[^1]);
            Assert.IsFalse(element1.Published);
            Assert.AreEqual(PublishedState.Unpublished, element1.PublishedState);
            Assert.AreEqual(versions[^1], repository.Get(element1.Id)!.VersionId);

            // misc checks
            Assert.AreEqual(
                false,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Element} WHERE nodeId=@id",
                    new { id = element1.Id }));

            // change something
            // save = update the current (draft) version
            element1.Name = "name-2";
            element1.SetValue("title", "title-2");
            repository.Save(element1);

            versions.Add(element1.VersionId); // the same version

            // no new version has been created
            Assert.AreEqual(versions[^2], versions[^1]);
            Assert.AreEqual(versions[^1], repository.Get(element1.Id)!.VersionId);

            // misc checks
            Assert.AreEqual(
                false,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Element} WHERE nodeId=@id",
                    new { id = element1.Id }));

            // publish = version
            element1.PublishCulture(CultureImpact.Invariant, DateTime.Now, PropertyEditorCollection);
            element1.PublishedState = PublishedState.Publishing;
            repository.Save(element1);

            versions.Add(element1.VersionId); // NEW VERSION

            // new version has been created
            Assert.AreNotEqual(versions[^2], versions[^1]);
            Assert.IsTrue(element1.Published);
            Assert.AreEqual(PublishedState.Published, element1.PublishedState);
            Assert.AreEqual(versions[^1], repository.Get(element1.Id)!.VersionId);

            // misc checks
            Assert.AreEqual(
                true,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Element} WHERE nodeId=@id",
                    new { id = element1.Id }));

            // change something
            // save = update the current (draft) version
            element1.Name = "name-3";
            element1.SetValue("title", "title-3");

            //// Thread.Sleep(2000); // force date change

            repository.Save(element1);

            versions.Add(element1.VersionId); // the same version

            // no new version has been created
            Assert.AreEqual(versions[^2], versions[^1]);
            Assert.AreEqual(versions[^1], repository.Get(element1.Id)!.VersionId);

            // misc checks
            Assert.AreEqual(
                true,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Element} WHERE nodeId=@id",
                    new { id = element1.Id }));

            // publish = new version
            element1.Name = "name-4";
            element1.SetValue("title", "title-4");
            element1.PublishCulture(CultureImpact.Invariant, DateTime.Now, PropertyEditorCollection);
            element1.PublishedState = PublishedState.Publishing;
            repository.Save(element1);

            versions.Add(element1.VersionId); // NEW VERSION

            // a new version has been created
            Assert.AreNotEqual(versions[^2], versions[^1]);
            Assert.IsTrue(element1.Published);
            Assert.AreEqual(PublishedState.Published, element1.PublishedState);
            Assert.AreEqual(versions[^1], repository.Get(element1.Id)!.VersionId);

            // misc checks
            Assert.AreEqual(
                true,
                ScopeAccessor.AmbientScope.Database.ExecuteScalar<bool>(
                    $"SELECT published FROM {Constants.DatabaseSchema.Tables.Element} WHERE nodeId=@id",
                    new { id = element1.Id }));

            // all versions
            var allVersions = repository.GetAllVersions(element1.Id).ToArray();
            Assert.Multiple(() =>
            {
                Assert.AreEqual(4, allVersions.Length);
                Assert.IsTrue(allVersions.All(v => v.PublishedVersionId == 3));
                Assert.AreEqual(4, allVersions.DistinctBy(v => v.VersionId).Count());
                for (var versionId = 1; versionId <= 4; versionId++)
                {
                    Assert.IsNotNull(allVersions.FirstOrDefault(v => v.VersionId == versionId));
                }
            });

            // Console.WriteLine();
            // foreach (var v in versions)
            // {
            //     Console.WriteLine(v);
            // }
            //
            // Console.WriteLine();
            // foreach (var v in allVersions)
            // {
            //     Console.WriteLine($"{v.Id} {v.VersionId} {(v.Published ? "+" : "-")}pub pk={v.VersionId} ppk={v.PublishedVersionId} name=\"{v.Name}\" pname=\"{v.PublishName}\"");
            // }

            // get older version
            var element = repository.GetVersion(versions[^4]);
            Assert.AreNotEqual(0, element.VersionId);
            Assert.AreEqual(versions[^4], element.VersionId);
            Assert.AreEqual("name-4", element1.Name);
            Assert.AreEqual("title-4", element1.GetValue("title"));
            Assert.AreEqual("name-2", element.Name);
            Assert.AreEqual("title-2", element.GetValue("title"));

            // get all versions - most recent first
            allVersions = repository.GetAllVersions(element1.Id).ToArray();
            var expVersions = versions.Distinct().Reverse().ToArray();
            Assert.AreEqual(expVersions.Length, allVersions.Length);
            for (var i = 0; i < expVersions.Length; i++)
            {
                Assert.AreEqual(expVersions[i], allVersions[i].VersionId);
            }
        }
    }

    // TODO ELEMENTS: port over all relevant tests from DocumentRepositoryTest
}

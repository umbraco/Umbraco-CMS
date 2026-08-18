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
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Attributes;
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

        var templateRepository = new TemplateRepository(scopeAccessor, appCaches, LoggerFactory.CreateLogger<TemplateRepository>(), LoggerFactory, GetRequiredService<FileSystems>(), ShortStringHelper, Mock.Of<IViewHelper>(), runtimeSettingsMock.Object,  Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
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
            IdKeyMap,
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

            // now get by GUID, this will also be cached because of the sub-repo-by-key pattern in the entity service
            repository.Get(content.Key);
            var sqlCount = udb.SqlCount;
            Assert.AreEqual(sqlCount, 0);

            // retrieve again, this should use cache now
            repository.Get(content.Key);
            Assert.AreEqual(sqlCount, udb.SqlCount);
        }
    }

    [Test]
    public void Retrieval_By_Key_After_Retrieval_By_Id_Is_Cached()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var element = CreateElement(repository, contentTypeRepository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for IElement so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IElement>();

        // Initial request by ID should hit the database.
        repository.Get(element.Id);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache, since the cache by Id and Key was populated on retrieval.
        repository.Get(element.Id);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(element.Key);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Id_After_Retrieval_By_Key_Is_Cached()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var element = CreateElement(repository, contentTypeRepository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for IElement so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<IElement>();

        // Initial request by key should hit the database.
        repository.Get(element.Key);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache, since the cache by Id and Key was populated on retrieval.
        repository.Get(element.Key);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(element.Id);
        Assert.AreEqual(0, database.SqlCount);
    }

    private IElement CreateElement(ElementRepository repository, ContentTypeRepository contentTypeRepository)
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);
        var element = ElementBuilder.CreateSimpleElement(elementType);
        repository.Save(element);
        return element;
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

    [Test]
    [LongRunning]
    public void GetAllElementsManyVersions()
    {
        var provider = ScopeProvider;
        IElement[] result;

        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
            var elementType = ContentTypeBuilder.CreateSimpleElementType();
            contentTypeRepository.Save(elementType);

            for (var i = 0; i < 4; i++)
            {
                repository.Save(ElementBuilder.CreateSimpleElement(elementType, "Element " + i));
            }

            result = repository.GetMany().ToArray();

            // save them all
            foreach (var element in result)
            {
                element.SetValue("title", element.GetValue<string>("title") + "x");
                repository.Save(element);
            }

            // publish them all
            foreach (var element in result)
            {
                element.PublishCulture(CultureImpact.Invariant, DateTime.Now, PropertyEditorCollection);
                repository.Save(element);
            }

            scope.Complete();
        }

        // get them all again
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository((IScopeAccessor)provider, out _, out DataTypeRepository _);
            var result2 = repository.GetMany().ToArray();

            Assert.AreEqual(result.Length, result2.Length);
        }
    }

    [Test]
    public void GetPagedResultsByQuery_FilterMatchingSome()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);
        var otherElementType = ContentTypeBuilder.CreateSimpleElementType("otherElementType", "Other Element Type");
        contentTypeRepository.Save(otherElementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element Two");
        var otherTypeElement = ElementBuilder.CreateSimpleElement(otherElementType, "Element Two"); // same name, different type - must not match the type-scoped query
        repository.Save(element1);
        repository.Save(element2);
        repository.Save(otherTypeElement);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.ContentTypeId == elementType.Id);
        var filterQuery = ScopeProvider.CreateQuery<IElement>().Where(x => x.Name.Contains("Two"));
        var result = repository.GetPage(query, 0, 1, out var totalRecords, propertyAliases: null, filter: filterQuery, ordering: Ordering.By("Name")).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, totalRecords);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Element Two", result.First().Name);
        });
    }

    [Test]
    public void GetPagedResultsByQuery_FilterMatchingAll()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);
        var otherElementType = ContentTypeBuilder.CreateSimpleElementType("otherElementType", "Other Element Type");
        contentTypeRepository.Save(otherElementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element Two");
        var otherTypeElement = ElementBuilder.CreateSimpleElement(otherElementType, "Element Three"); // different type - must not match the type-scoped query
        repository.Save(element1);
        repository.Save(element2);
        repository.Save(otherTypeElement);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.ContentTypeId == elementType.Id);
        var filterQuery = ScopeProvider.CreateQuery<IElement>().Where(x => x.Name.Contains("Element"));
        var result = repository.GetPage(query, 0, 1, out var totalRecords, propertyAliases: null, filter: filterQuery, ordering: Ordering.By("Name")).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, totalRecords);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Element One", result.First().Name);
        });
    }

    [Test]
    public void GetPagedResultsByQuery_FirstPage()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element Two");
        repository.Save(element1);
        repository.Save(element2);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.ContentTypeId == elementType.Id);
        var result = repository.GetPage(query, 0, 1, out var totalRecords, propertyAliases: null, filter: null, ordering: Ordering.By("Name")).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, totalRecords);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Element One", result.First().Name);
        });
    }

    [Test]
    public void GetPagedResultsByQuery_SecondPage()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element Two");
        repository.Save(element1);
        repository.Save(element2);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.ContentTypeId == elementType.Id);
        var result = repository.GetPage(query, 1, 1, out var totalRecords, propertyAliases: null, filter: null, ordering: Ordering.By("Name")).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, totalRecords);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Element Two", result.First().Name);
        });
    }

    [Test]
    public void GetPagedResultsByQuery_SinglePage()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element Two");
        repository.Save(element1);
        repository.Save(element2);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.ContentTypeId == elementType.Id);
        var result = repository.GetPage(query, 0, 2, out var totalRecords, propertyAliases: null, filter: null, ordering: Ordering.By("Name")).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, totalRecords);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("Element One", result.First().Name);
        });
    }

    [Test]
    public void GetPagedResultsByQuery_DescendingOrder()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);
        var otherElementType = ContentTypeBuilder.CreateSimpleElementType("otherElementType", "Other Element Type");
        contentTypeRepository.Save(otherElementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element A");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element B");
        var otherTypeElement = ElementBuilder.CreateSimpleElement(otherElementType, "Element C"); // different type - must be excluded by the type-scoped query
        repository.Save(element1);
        repository.Save(element2);
        repository.Save(otherTypeElement);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.ContentTypeId == elementType.Id);
        var result = repository.GetPage(query, 0, 1, out var totalRecords, propertyAliases: null, filter: null, ordering: Ordering.By("Name", Direction.Descending)).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, totalRecords);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Element B", result.First().Name);
        });
    }

    [Test]
    public void GetAllElementsByIds()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element Two");
        var element3 = ElementBuilder.CreateSimpleElement(elementType, "Element Three");
        repository.Save(element1);
        repository.Save(element2);
        repository.Save(element3);

        var elements = repository.GetMany(element1.Id, element2.Id).ToArray();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(elements);
            Assert.AreEqual(2, elements.Length);
            CollectionAssert.AreEquivalent(new[] { element1.Id, element2.Id }, elements.Select(e => e.Id));
        });
    }

    [Test]
    public void GetAllElements()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        repository.Save(ElementBuilder.CreateSimpleElement(elementType, "Element One"));
        repository.Save(ElementBuilder.CreateSimpleElement(elementType, "Element Two"));

        var elements = repository.GetMany().ToArray();
        Assert.That(elements, Is.Not.Null);
        Assert.That(elements.Any(), Is.True);
        Assert.That(elements.Length, Is.GreaterThanOrEqualTo(2));

        elements = repository.GetMany(elements.Select(x => x.Id).ToArray()).ToArray();
        Assert.That(elements, Is.Not.Null);
        Assert.That(elements.Any(), Is.True);
        Assert.That(elements.Length, Is.GreaterThanOrEqualTo(2));

        elements = ((IReadRepository<Guid, IElement>)repository).GetMany(elements.Select(x => x.Key).ToArray()).ToArray();
        Assert.That(elements, Is.Not.Null);
        Assert.That(elements.Any(), Is.True);
        Assert.That(elements.Length, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void GetElement()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var saved = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        repository.Save(saved);

        var element = repository.Get(saved.Id);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(saved.Id, element.Id);
            Assert.That(element.CreateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.That(element.UpdateDate, Is.GreaterThan(DateTime.MinValue));
            Assert.AreNotEqual(0, element.ParentId);
            Assert.AreEqual("Element One", element.Name);
            Assert.AreNotEqual(0, element.VersionId);
            Assert.AreEqual(elementType.Id, element.ContentTypeId);
            Assert.That(element.Path, Is.Not.Empty);
            Assert.That(element.Properties.Any(), Is.True);
        });
    }

    [Test]
    public void QueryElement()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element Two");
        repository.Save(element1);
        repository.Save(element2);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.Level == element1.Level);
        var result = repository.Get(query);

        Assert.GreaterOrEqual(result.Count(), 2);
    }

    [Test]
    public void ExistElement()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        repository.Save(element);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(repository.Exists(element.Id));
            Assert.IsFalse(repository.Exists(element.Id + 1));
        });
    }

    [Test]
    public void CountElement()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);
        var otherElementType = ContentTypeBuilder.CreateSimpleElementType("otherElementType", "Other Element Type");
        contentTypeRepository.Save(otherElementType);

        repository.Save(ElementBuilder.CreateSimpleElement(elementType, "Element One"));
        repository.Save(ElementBuilder.CreateSimpleElement(elementType, "Element Two"));
        repository.Save(ElementBuilder.CreateSimpleElement(otherElementType, "Element Three")); // different type - must be excluded by the type-scoped query

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.ContentTypeId == elementType.Id);
        var result = repository.Count(query);

        Assert.AreEqual(2, result);
    }

    [Test]
    public void PropertyDataAssignedCorrectly()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);

        var emptyContentType = ContentTypeBuilder.CreateBasicElementType();
        var hasPropertiesContentType = ContentTypeBuilder.CreateSimpleElementType("elementTypeWithProps", "Element With Props");
        contentTypeRepository.Save(emptyContentType);
        contentTypeRepository.Save(hasPropertiesContentType);

        var element1 = ElementBuilder.CreateSimpleElement(hasPropertiesContentType, "Element One");
        var element2 = ElementBuilder.CreateBasicElement(emptyContentType);
        var element3 = ElementBuilder.CreateSimpleElement(hasPropertiesContentType, "Element Three");

        repository.Save(element1);
        repository.Save(element2);
        repository.Save(element3);

        var result = repository.GetMany(element1.Id, element2.Id, element3.Id).ToArray();
        var n1 = result[0];
        var n2 = result[1];
        var n3 = result[2];

        Assert.Multiple(() =>
        {
            Assert.AreEqual(element1.Id, n1.Id);
            Assert.AreEqual(element2.Id, n2.Id);
            Assert.AreEqual(element3.Id, n3.Id);
        });

        TestHelper.AssertPropertyValuesAreEqual(element1, n1);
        TestHelper.AssertPropertyValuesAreEqual(element2, n2);
        TestHelper.AssertPropertyValuesAreEqual(element3, n3);
    }

    [Test]
    public void DeleteElement()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        repository.Save(element);
        var id = element.Id;

        repository.Delete(element);

        Assert.IsNull(repository.Get(id));
    }

    [Test]
    public void QueryElementByUniqueId()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        element.Key = new Guid("A5C3A9D2-6B0E-4F1A-9E7C-3D8B2C1E4F60");
        repository.Save(element);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.Key == new Guid("A5C3A9D2-6B0E-4F1A-9E7C-3D8B2C1E4F60"));
        var result = repository.Get(query).SingleOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(element.Id, result.Id);
    }

    [Test]
    public void GetPagedResultsByQuery_With_Variant_Names()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);

        // One invariant element type
        var invariantElementType = (ContentType)ContentTypeBuilder.CreateSimpleElementType("invariantElementType", "Invariant Element Type");
        invariantElementType.Variations = ContentVariation.Nothing;
        foreach (var propertyType in invariantElementType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Nothing;
        }

        contentTypeRepository.Save(invariantElementType);

        // One variant (by culture) element type, every 2nd property variant by culture, the rest invariant
        var variantElementType = (ContentType)ContentTypeBuilder.CreateSimpleElementType("variantElementType", "Variant Element Type");
        variantElementType.Variations = ContentVariation.Culture;
        var propertyTypes = variantElementType.PropertyTypes.ToList();
        for (var i = 0; i < propertyTypes.Count; i++)
        {
            propertyTypes[i].Variations = i % 2 == 0 ? ContentVariation.Culture : ContentVariation.Nothing;
        }

        contentTypeRepository.Save(variantElementType);

        var elements = new List<IElement>();
        for (var i = 0; i < 10; i++)
        {
            var isInvariant = i % 2 == 0;
            var name = (isInvariant ? "INV" : "VAR") + "_" + Guid.NewGuid();

            IElement element = isInvariant
                ? ElementBuilder.CreateSimpleElement(invariantElementType, name)
                : ElementBuilder.CreateBasicElement(variantElementType);

            if (!isInvariant)
            {
                element.SetCultureName(name, "en-US");
                element.SetValue("title", name + " Subpage", "en-US");
                element.SetValue("bodyText", "This is a subpage"); // this one is invariant
                element.SetValue("author", "John Doe", "en-US");
            }

            repository.Save(element);
            elements.Add(element);
        }

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.ParentId == Constants.System.Root);
        var result = repository.GetPage(query, 0, 20, out var totalRecords, propertyAliases: null, filter: null, ordering: Ordering.By("UpdateDate")).ToArray();

        Assert.AreEqual(10, totalRecords);
        foreach (var r in result)
        {
            var isInvariant = r.ContentType.Alias == "invariantElementType";
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

    /// <summary>
    /// Verifies that retrieving all elements from the GUID-based repository returns all items when the cache is
    /// populated.
    /// </summary>
    /// <remarks>
    /// Verifies the fix for https://github.com/umbraco/Umbraco-CMS/issues/21756 as this test fails before
    /// the fix is applied.
    /// </remarks>
    [Test]
    public void GetMany_By_Guid_With_Warm_Cache_Returns_All()
    {
        var realCache = new AppCaches(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

        var provider = ScopeProvider;

        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _, realCache);

        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);
        var element = ElementBuilder.CreateSimpleElement(elementType);
        repository.Save(element);

        var guidRepo = (IReadRepository<Guid, IElement>)repository;

        var result = guidRepo.GetMany().ToArray();
        Assert.IsNotEmpty(result);
        Assert.IsTrue(result.Any(e => e.Key == element.Key));
    }

    [Test]
    public void UpdateElement()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        repository.Save(element);

        var content = repository.Get(element.Id);
        content.Name = "Updated Name";
        repository.Save(content);

        var updatedContent = repository.Get(element.Id);

        Assert.AreEqual(content.Id, updatedContent.Id);
        Assert.AreEqual("Updated Name", updatedContent.Name);
        Assert.AreEqual(content.VersionId, updatedContent.VersionId);

        Assert.AreEqual("This is the element title", content.GetValue("title"));
        content.SetValue("title", "Updated Title");
        repository.Save(content);

        updatedContent = repository.Get(element.Id);

        Assert.AreEqual("Updated Title", updatedContent.GetValue("title"));
        Assert.AreEqual(content.VersionId, updatedContent.VersionId);
    }

    [Test]
    public void ElementIsNotDirtyAfterSave()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        element.SetValue("title", "Dirty Title");

        Assert.IsTrue(((Element)element).IsDirty());

        repository.Save(element);

        Assert.IsFalse(((Element)element).IsDirty());

        var content = repository.Get(element.Id);
        Assert.IsFalse(((Element)content).IsDirty());
    }

    [Test]
    public void SaveElement()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        repository.Save(element);

        Assert.Multiple(() =>
        {
            Assert.That(elementType.HasIdentity, Is.True);
            Assert.That(element.HasIdentity, Is.True);
        });
    }

    // Covers issue U4-2791 and U4-2607
    [Test]
    public void SaveElementWithAtSignInName()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "test@umbraco.org");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "@lightgiants");

        repository.Save(element1);
        repository.Save(element2);

        Assert.IsTrue(element1.HasIdentity);
        Assert.IsTrue(element2.HasIdentity);

        var result1 = repository.Get(element1.Id);
        Assert.AreEqual(element1.Name, result1.Name);

        var result2 = repository.Get(element2.Id);
        Assert.AreEqual(element2.Name, result2.Name);
    }

    [Test]
    public void GetPagedResultsByQuery_CustomPropertySort()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository((IScopeAccessor)provider, out var contentTypeRepository, out DataTypeRepository _);
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        contentTypeRepository.Save(elementType);

        var element1 = ElementBuilder.CreateSimpleElement(elementType, "Element One");
        element1.SetValue("title", "Bravo");
        var element2 = ElementBuilder.CreateSimpleElement(elementType, "Element Two");
        element2.SetValue("title", "Alpha");
        var element3 = ElementBuilder.CreateSimpleElement(elementType, "Element Three");
        element3.SetValue("title", "Charlie");

        repository.Save(element1);
        repository.Save(element2);
        repository.Save(element3);

        var query = ScopeProvider.CreateQuery<IElement>().Where(x => x.Name.Contains("Element"));

        var result = repository.GetPage(query, 0, 2, out var totalRecords, propertyAliases: null, filter: null, ordering: Ordering.By("title", isCustomField: true)).ToArray();

        Assert.AreEqual(3, totalRecords);
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(element2.Id, result[0].Id); // "Alpha"
        Assert.AreEqual(element1.Id, result[1].Id); // "Bravo"

        result = repository.GetPage(query, 1, 2, out totalRecords, propertyAliases: null, filter: null, ordering: Ordering.By("title", isCustomField: true)).ToArray();

        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(element3.Id, result[0].Id); // "Charlie"
    }
}

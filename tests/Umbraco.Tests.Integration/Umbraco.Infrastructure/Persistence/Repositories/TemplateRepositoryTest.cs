// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class TemplateRepositoryTest : UmbracoIntegrationTest
{
    [TearDown]
    public void TearDown()
    {
        var testHelper = new TestHelper();

        // Delete all files
        var fsViews = new PhysicalFileSystem(
            IOHelper,
            HostingEnvironment,
            LoggerFactory.CreateLogger<PhysicalFileSystem>(),
            HostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MvcViews),
            HostingEnvironment.ToAbsolute(Constants.SystemDirectories.MvcViews));
        var views = fsViews.GetFiles(string.Empty, "*.cshtml");
        foreach (var file in views)
        {
            fsViews.DeleteFile(file);
        }
    }

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private FileSystems FileSystems => GetRequiredService<FileSystems>();

    private IViewHelper ViewHelper => GetRequiredService<IViewHelper>();

    private IOptionsMonitor<RuntimeSettings> RuntimeSettings => GetRequiredService<IOptionsMonitor<RuntimeSettings>>();

    private ITemplateRepository CreateRepository(IScopeProvider provider, AppCaches? appCaches = null) =>
        new TemplateRepository((IScopeAccessor)provider, appCaches ?? AppCaches.Disabled, LoggerFactory.CreateLogger<TemplateRepository>(), LoggerFactory, FileSystems, ShortStringHelper, ViewHelper, RuntimeSettings, Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());

    private ITemplateRepository CreateRepository(IScopeProvider provider, IOptionsMonitor<RuntimeSettings> runtimeSettings) =>
        new TemplateRepository((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<TemplateRepository>(), FileSystems, ShortStringHelper, ViewHelper, runtimeSettings, Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());

    private static IOptionsMonitor<RuntimeSettings> CreateRuntimeSettingsMonitor(RuntimeMode mode)
    {
        var settings = new RuntimeSettings { Mode = mode };
        var monitor = new Mock<IOptionsMonitor<RuntimeSettings>>();
        monitor.Setup(m => m.CurrentValue).Returns(settings);
        return monitor.Object;
    }

    [Test]
    public void Can_Instantiate_Repository()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }
    }

    [Test]
    public void Retrieval_By_Id_After_Retrieval_By_Id_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var template = CreateTemplate(repository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for ITemplate so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<ITemplate>();

        // Initial request by Id should hit the database.
        repository.Get(template.Id);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache.
        repository.Get(template.Id);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Key_After_Retrieval_By_Key_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var template = CreateTemplate(repository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for ITemplate so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<ITemplate>();

        // Initial request by key should hit the database.
        repository.Get(template.Key);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache.
        repository.Get(template.Key);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrievals_By_Id_And_Key_After_Save_Are_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var template = CreateTemplate(repository);

        database.EnableSqlCount = true;

        // Initial and subsequent requests should use the cache, since the cache by Id and Key was populated on save.
        repository.Get(template.Id);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(template.Id);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(template.Key);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(template.Key);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Key_After_Retrieval_By_Id_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var template = CreateTemplate(repository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for ITemplate so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<ITemplate>();

        // Initial request by ID should hit the database.
        repository.Get(template.Id);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache, since the cache by Id and Key was populated on retrieval.
        repository.Get(template.Id);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(template.Key);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Id_After_Retrieval_By_Key_Is_Cached()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;
        var scopeAccessor = ScopeAccessor;

        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider, realCache);

        var database = scopeAccessor.AmbientScope.Database;

        database.EnableSqlCount = false;

        var template = CreateTemplate(repository);

        database.EnableSqlCount = true;

        // Clear the isolated cache for ITemplate so the next retrieval hits the database
        realCache.IsolatedCaches.ClearCache<ITemplate>();

        // Initial request by key should hit the database.
        repository.Get(template.Key);
        Assert.Greater(database.SqlCount, 0);

        // Reset counter.
        database.EnableSqlCount = false;
        database.EnableSqlCount = true;

        // Subsequent requests should use the cache, since the cache by Id and Key was populated on retrieval.
        repository.Get(template.Key);
        Assert.AreEqual(0, database.SqlCount);

        repository.Get(template.Id);
        Assert.AreEqual(0, database.SqlCount);
    }

    [Test]
    public void Retrieval_By_Id_After_Deletion_Returns_Null()
    {
        var realCache = CreateAppCaches();

        var provider = ScopeProvider;

        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider, realCache);

        var template = CreateTemplate(repository);
        var retrievedTemplate = repository.Get(template.Key);
        Assert.IsNotNull(retrievedTemplate);

        repository.Delete(template);

        retrievedTemplate = repository.Get(template.Key);
        Assert.IsNull(retrievedTemplate);
    }

    private static AppCaches CreateAppCaches() =>
        new(
            new ObjectCacheAppCache(),
            new DictionaryAppCache(),
            new IsolatedCaches(t => new ObjectCacheAppCache()));

    private static ITemplate CreateTemplate(ITemplateRepository repository)
    {
        var templateBuilder = new TemplateBuilder();
        var template = templateBuilder
            .WithId(0)
            .WithAlias("testTemplate")
            .WithName("Test Template")
            .Build();

        repository.Save(template);
        return template;
    }

    [Test]
    public void Can_Perform_Add_View()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var template = new Template(ShortStringHelper, "test", "test");
            repository.Save(template);

            // Assert
            Assert.That(repository.Get("test"), Is.Not.Null);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.True);
        }
    }

    [Test]
    public void Can_Perform_Add_View_With_Default_Content()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var template = new Template(ShortStringHelper, "test", "test") { Content = "mock-content" };
            repository.Save(template);

            // Assert
            Assert.That(repository.Get("test"), Is.Not.Null);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.True);
            Assert.AreEqual("mock-content", template.Content.StripWhitespace());
        }
    }

    [Test]
    public void Can_Perform_Add_View_With_Default_Content_With_Parent()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // NOTE: This has to be persisted first
            var template = new Template(ShortStringHelper, "test", "test");
            repository.Save(template);

            // Act
            var template2 = new Template(ShortStringHelper, "test2", "test2");
            template2.SetMasterTemplate(template);
            repository.Save(template2);

            // Assert
            Assert.That(repository.Get("test2"), Is.Not.Null);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("test2.cshtml"), Is.True);
            Assert.AreEqual(
                "@usingUmbraco.Cms.Web.Common.PublishedModels;@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage @{ Layout = \"test.cshtml\";}"
                    .StripWhitespace(),
                template2.Content.StripWhitespace());
        }
    }

    [Test]
    public void Can_Perform_Add_Unique_Alias()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var template = new Template(ShortStringHelper, "test", "test") { Content = "mock-content" };
            repository.Save(template);

            var template2 = new Template(ShortStringHelper, "test", "test") { Content = "mock-content" };
            repository.Save(template2);

            // Assert
            Assert.AreEqual("test1", template2.Alias);
        }
    }

    [Test]
    public void Can_Perform_Update_Unique_Alias()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var template = new Template(ShortStringHelper, "test", "test") { Content = "mock-content" };
            repository.Save(template);

            var template2 = new Template(ShortStringHelper, "test1", "test1") { Content = "mock-content" };
            repository.Save(template2);

            template.Alias = "test1";
            repository.Save(template);

            // Assert
            Assert.AreEqual("test11", template.Alias);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("test11.cshtml"), Is.True);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.False);
        }
    }

    [Test]
    public void Can_Perform_Update_View()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var template = new Template(ShortStringHelper, "test", "test") { Content = "mock-content" };
            repository.Save(template);

            template.Content += "<html></html>";
            repository.Save(template);

            var updated = repository.Get("test");

            // Assert
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.True);
            Assert.That(updated.Content, Is.EqualTo("mock-content" + "<html></html>"));
        }
    }

    [Test]
    public void Can_Perform_Delete_View()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var template = new Template(ShortStringHelper, "test", "test") { Content = "mock-content" };
            repository.Save(template);

            // Act
            var templates = repository.Get("test");
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.True);
            repository.Delete(templates);

            // Assert
            Assert.IsNull(repository.Get("test"));
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.False);
        }
    }

    [Test]
    public void Can_Perform_Delete_When_Assigned_To_Doc()
    {
        // Arrange
        var provider = ScopeProvider;
        var scopeAccessor = (IScopeAccessor)provider;
        var dataTypeService = GetRequiredService<IDataTypeService>();
        var fileService = GetRequiredService<IFileService>();

        using (provider.CreateScope())
        {
            var templateRepository = CreateRepository(provider);
            var globalSettings = new GlobalSettings();
            var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
            var tagRepository = new TagRepository(scopeAccessor, AppCaches.Disabled, LoggerFactory.CreateLogger<TagRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
            var commonRepository =
                new ContentTypeCommonRepository(scopeAccessor, templateRepository, AppCaches, ShortStringHelper);
            var languageRepository = new LanguageRepository(scopeAccessor, AppCaches.Disabled, LoggerFactory.CreateLogger<LanguageRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
            var contentTypeRepository = new ContentTypeRepository(scopeAccessor, AppCaches.Disabled, LoggerFactory.CreateLogger<ContentTypeRepository>(), commonRepository, languageRepository, ShortStringHelper, Mock.Of<IRepositoryCacheVersionService>(), IdKeyMap, Mock.Of<ICacheSyncService>());
            var relationTypeRepository = new RelationTypeRepository(scopeAccessor, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
            var entityRepository = new EntityRepository(scopeAccessor, AppCaches.Disabled);
            var relationRepository = new RelationRepository(scopeAccessor, LoggerFactory.CreateLogger<RelationRepository>(), relationTypeRepository, entityRepository, Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
            var propertyEditors =
                new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
            var dataValueReferences =
                new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>(), new NullLogger<DataValueReferenceFactoryCollection>());
            var contentRepo = new DocumentRepository(
                scopeAccessor,
                AppCaches.Disabled,
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
                dataTypeService,
                serializer,
                Mock.Of<IEventAggregator>());

            var template = TemplateBuilder.CreateTextPageTemplate();
            fileService.SaveTemplate(template); // else, FK violation on contentType!

            var contentType =
                ContentTypeBuilder.CreateSimpleContentType("umbTextpage2", "Textpage", defaultTemplateId: template.Id);
            contentTypeRepository.Save(contentType);

            var textpage = ContentBuilder.CreateSimpleContent(contentType);
            contentRepo.Save(textpage);

            textpage.TemplateId = template.Id;
            contentRepo.Save(textpage);

            // Act
            var templates = templateRepository.Get("textPage");
            templateRepository.Delete(templates);

            // Assert
            Assert.IsNull(templateRepository.Get("textPage"));
        }
    }

    [Test]
    public void Can_Perform_Delete_On_Nested_Templates()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var parent = new Template(ShortStringHelper, "parent", "parent")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };
            var child = new Template(ShortStringHelper, "child", "child") { Content = @"<%@ Master Language=""C#"" %>" };
            var baby = new Template(ShortStringHelper, "baby", "baby") { Content = @"<%@ Master Language=""C#"" %>" };
            child.MasterTemplateAlias = parent.Alias;
            child.MasterTemplateId = new Lazy<int>(() => parent.Id);
            baby.MasterTemplateAlias = child.Alias;
            baby.MasterTemplateId = new Lazy<int>(() => child.Id);
            repository.Save(parent);
            repository.Save(child);
            repository.Save(baby);

            // Act
            var templates = repository.Get("parent");
            repository.Delete(templates);

            // Assert
            Assert.IsNull(repository.Get("test"));
        }
    }

    [Test]
    public void Can_Get_All()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var created = CreateHierarchy(repository).ToArray();

            // Act
            var all = repository.GetAll().ToArray();
            var allByAlias = repository.GetAll("parent", "child2", "baby2", "notFound").ToArray();
            var allById = repository.GetMany(created[0].Id, created[2].Id, created[4].Id, created[5].Id, 999999).ToArray();

            // Assert
            Assert.AreEqual(9, all.Count());
            Assert.AreEqual(9, all.DistinctBy(x => x.Id).Count());

            Assert.AreEqual(3, allByAlias.Count());
            Assert.AreEqual(3, allByAlias.DistinctBy(x => x.Id).Count());

            Assert.AreEqual(4, allById.Count());
            Assert.AreEqual(4, allById.DistinctBy(x => x.Id).Count());
        }
    }

    [Test]
    public void Can_Get_Children()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var created = CreateHierarchy(repository).ToArray();

            // Act
            var childrenById = repository.GetChildren(created[1].Id).ToArray();

            // Assert
            Assert.AreEqual(2, childrenById.Count());
            Assert.AreEqual(2, childrenById.DistinctBy(x => x.Id).Count());
        }
    }

    [Test]
    public void Can_Get_Children_At_Root()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            CreateHierarchy(repository).ToArray();

            // Act
            var children = repository.GetChildren(-1).ToArray();

            // Assert
            Assert.AreEqual(1, children.Count());
            Assert.AreEqual(1, children.DistinctBy(x => x.Id).Count());
        }
    }

    [Test]
    public void Can_Get_Descendants()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);
            var created = CreateHierarchy(repository).ToArray();

            // Act
            var descendantsById = repository.GetDescendants(created[1].Id).ToArray();

            // Assert
            Assert.AreEqual(3, descendantsById.Count());
            Assert.AreEqual(3, descendantsById.DistinctBy(x => x.Id).Count());
        }
    }

    [Test]
    public void Path_Is_Set_Correctly_On_Creation()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var parent = new Template(ShortStringHelper, "parent", "parent");
            var child1 = new Template(ShortStringHelper, "child1", "child1");
            var toddler1 = new Template(ShortStringHelper, "toddler1", "toddler1");
            var toddler2 = new Template(ShortStringHelper, "toddler2", "toddler2");
            var baby1 = new Template(ShortStringHelper, "baby1", "baby1");
            var child2 = new Template(ShortStringHelper, "child2", "child2");
            var toddler3 = new Template(ShortStringHelper, "toddler3", "toddler3");
            var toddler4 = new Template(ShortStringHelper, "toddler4", "toddler4");
            var baby2 = new Template(ShortStringHelper, "baby2", "baby2");

            child1.MasterTemplateAlias = parent.Alias;
            child1.MasterTemplateId = new Lazy<int>(() => parent.Id);
            child2.MasterTemplateAlias = parent.Alias;
            child2.MasterTemplateId = new Lazy<int>(() => parent.Id);
            toddler1.MasterTemplateAlias = child1.Alias;
            toddler1.MasterTemplateId = new Lazy<int>(() => child1.Id);
            toddler2.MasterTemplateAlias = child1.Alias;
            toddler2.MasterTemplateId = new Lazy<int>(() => child1.Id);
            toddler3.MasterTemplateAlias = child2.Alias;
            toddler3.MasterTemplateId = new Lazy<int>(() => child2.Id);
            toddler4.MasterTemplateAlias = child2.Alias;
            toddler4.MasterTemplateId = new Lazy<int>(() => child2.Id);
            baby1.MasterTemplateAlias = toddler2.Alias;
            baby1.MasterTemplateId = new Lazy<int>(() => toddler2.Id);
            baby2.MasterTemplateAlias = toddler4.Alias;
            baby2.MasterTemplateId = new Lazy<int>(() => toddler4.Id);

            // Act
            repository.Save(parent);
            repository.Save(child1);
            repository.Save(child2);
            repository.Save(toddler1);
            repository.Save(toddler2);
            repository.Save(toddler3);
            repository.Save(toddler4);
            repository.Save(baby1);
            repository.Save(baby2);

            // Assert
            Assert.AreEqual(string.Format("-1,{0}", parent.Id), parent.Path);
            Assert.AreEqual(string.Format("-1,{0},{1}", parent.Id, child1.Id), child1.Path);
            Assert.AreEqual(string.Format("-1,{0},{1}", parent.Id, child2.Id), child2.Path);
            Assert.AreEqual(string.Format("-1,{0},{1}", parent.Id, child2.Id), child2.Path);
            Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child1.Id, toddler1.Id), toddler1.Path);
            Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child1.Id, toddler2.Id), toddler2.Path);
            Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child2.Id, toddler3.Id), toddler3.Path);
            Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child2.Id, toddler4.Id), toddler4.Path);
            Assert.AreEqual(string.Format("-1,{0},{1},{2},{3}", parent.Id, child1.Id, toddler2.Id, baby1.Id), baby1.Path);
            Assert.AreEqual(string.Format("-1,{0},{1},{2},{3}", parent.Id, child2.Id, toddler4.Id, baby2.Id), baby2.Path);
        }
    }

    [Test]
    public void Path_Is_Set_Correctly_On_Update()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var parent = new Template(ShortStringHelper, "parent", "parent");
            var child1 = new Template(ShortStringHelper, "child1", "child1");
            var child2 = new Template(ShortStringHelper, "child2", "child2");
            var toddler1 = new Template(ShortStringHelper, "toddler1", "toddler1");
            var toddler2 = new Template(ShortStringHelper, "toddler2", "toddler2");

            child1.MasterTemplateAlias = parent.Alias;
            child1.MasterTemplateId = new Lazy<int>(() => parent.Id);
            child2.MasterTemplateAlias = parent.Alias;
            child2.MasterTemplateId = new Lazy<int>(() => parent.Id);
            toddler1.MasterTemplateAlias = child1.Alias;
            toddler1.MasterTemplateId = new Lazy<int>(() => child1.Id);
            toddler2.MasterTemplateAlias = child1.Alias;
            toddler2.MasterTemplateId = new Lazy<int>(() => child1.Id);

            repository.Save(parent);
            repository.Save(child1);
            repository.Save(child2);
            repository.Save(toddler1);
            repository.Save(toddler2);

            // Act
            toddler2.SetMasterTemplate(child2);
            repository.Save(toddler2);

            // Assert
            Assert.AreEqual($"-1,{parent.Id},{child2.Id},{toddler2.Id}", toddler2.Path);
        }
    }

    [Test]
    public void Path_Is_Set_Correctly_On_Update_With_Master_Template_Removal()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var parent = new Template(ShortStringHelper, "parent", "parent");
            var child1 = new Template(ShortStringHelper, "child1", "child1")
            {
                MasterTemplateAlias = parent.Alias,
                MasterTemplateId = new Lazy<int>(() => parent.Id)
            };

            repository.Save(parent);
            repository.Save(child1);

            // Act
            child1.SetMasterTemplate(null);
            repository.Save(child1);

            // Assert
            Assert.AreEqual($"-1,{child1.Id}", child1.Path);
        }
    }

    [Test]
    public void Save_In_Production_Mode_Does_Not_Write_New_File()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var productionRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.Production);
            var repository = CreateRepository(provider, productionRuntimeSettings);

            // Act
            var template = new Template(ShortStringHelper, "productionTestNew", "productionTestNew") { Content = "mock-content" };
            repository.Save(template);

            // Assert - database record should be created but file should NOT be created in production mode.
            Assert.That(repository.Get("productionTestNew"), Is.Not.Null);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("productionTestNew.cshtml"), Is.False);
        }
    }

    [Test]
    public void Save_In_Production_Mode_Does_Not_Update_Existing_File()
    {
        // Arrange - create template in development mode first.
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var developmentRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.Development);
            var developmentRepository = CreateRepository(provider, developmentRuntimeSettings);

            var template = new Template(ShortStringHelper, "productionTestUpdate", "productionTestUpdate") { Content = "original-content" };
            developmentRepository.Save(template);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("productionTestUpdate.cshtml"), Is.True);

            // Read original content.
            using var originalStream = FileSystems.MvcViewsFileSystem.OpenFile("productionTestUpdate.cshtml");
            using var originalReader = new StreamReader(originalStream);
            var originalContent = originalReader.ReadToEnd();
            Assert.That(originalContent, Does.Contain("original-content"));

            // Act - try to update in production mode.
            var productionRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.Production);
            var productionRepository = CreateRepository(provider, productionRuntimeSettings);

            var updatedTemplate = productionRepository.Get("productionTestUpdate");
            Assert.IsNotNull(updatedTemplate);

            // Modify and try to save.
            updatedTemplate.Content = "modified-content";
            productionRepository.Save(updatedTemplate);

            // Assert - file should still have original content.
            using var updatedStream = FileSystems.MvcViewsFileSystem.OpenFile("productionTestUpdate.cshtml");
            using var updatedReader = new StreamReader(updatedStream);
            var updatedContent = updatedReader.ReadToEnd();
            Assert.That(updatedContent, Does.Contain("original-content"));
            Assert.That(updatedContent, Does.Not.Contain("modified-content"));
        }
    }

    [Test]
    public void Save_In_Development_Mode_Writes_File()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var developmentRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.Development);
            var repository = CreateRepository(provider, developmentRuntimeSettings);

            // Act
            var template = new Template(ShortStringHelper, "developmentTest", "developmentTest") { Content = "mock-content" };
            repository.Save(template);

            // Assert - file should be created in development mode.
            Assert.That(repository.Get("developmentTest"), Is.Not.Null);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("developmentTest.cshtml"), Is.True);
        }
    }

    [Test]
    public void Save_In_BackofficeDevelopment_Mode_Writes_File()
    {
        // Arrange
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var backofficeDevelopmentRuntimeSettings = CreateRuntimeSettingsMonitor(RuntimeMode.BackofficeDevelopment);
            var repository = CreateRepository(provider, backofficeDevelopmentRuntimeSettings);

            // Act
            var template = new Template(ShortStringHelper, "backofficeDevelopmentTest", "backofficeDevelopmentTest") { Content = "mock-content" };
            repository.Save(template);

            // Assert - file should be created in backoffice development mode.
            Assert.That(repository.Get("backofficeDevelopmentTest"), Is.Not.Null);
            Assert.That(FileSystems.MvcViewsFileSystem.FileExists("backofficeDevelopmentTest.cshtml"), Is.True);
        }
    }

    private IEnumerable<ITemplate> CreateHierarchy(ITemplateRepository repository)
    {
        var parent = new Template(ShortStringHelper, "parent", "parent") { Content = @"<%@ Master Language=""C#"" %>" };

        var child1 = new Template(ShortStringHelper, "child1", "child1") { Content = @"<%@ Master Language=""C#"" %>" };
        var toddler1 = new Template(ShortStringHelper, "toddler1", "toddler1")
        {
            Content = @"<%@ Master Language=""C#"" %>"
        };
        var toddler2 = new Template(ShortStringHelper, "toddler2", "toddler2")
        {
            Content = @"<%@ Master Language=""C#"" %>"
        };
        var baby1 = new Template(ShortStringHelper, "baby1", "baby1") { Content = @"<%@ Master Language=""C#"" %>" };

        var child2 = new Template(ShortStringHelper, "child2", "child2") { Content = @"<%@ Master Language=""C#"" %>" };
        var toddler3 = new Template(ShortStringHelper, "toddler3", "toddler3")
        {
            Content = @"<%@ Master Language=""C#"" %>"
        };
        var toddler4 = new Template(ShortStringHelper, "toddler4", "toddler4")
        {
            Content = @"<%@ Master Language=""C#"" %>"
        };
        var baby2 = new Template(ShortStringHelper, "baby2", "baby2") { Content = @"<%@ Master Language=""C#"" %>" };

        child1.MasterTemplateAlias = parent.Alias;
        child1.MasterTemplateId = new Lazy<int>(() => parent.Id);
        child2.MasterTemplateAlias = parent.Alias;
        child2.MasterTemplateId = new Lazy<int>(() => parent.Id);

        toddler1.MasterTemplateAlias = child1.Alias;
        toddler1.MasterTemplateId = new Lazy<int>(() => child1.Id);
        toddler2.MasterTemplateAlias = child1.Alias;
        toddler2.MasterTemplateId = new Lazy<int>(() => child1.Id);

        toddler3.MasterTemplateAlias = child2.Alias;
        toddler3.MasterTemplateId = new Lazy<int>(() => child2.Id);
        toddler4.MasterTemplateAlias = child2.Alias;
        toddler4.MasterTemplateId = new Lazy<int>(() => child2.Id);

        baby1.MasterTemplateAlias = toddler2.Alias;
        baby1.MasterTemplateId = new Lazy<int>(() => toddler2.Id);

        baby2.MasterTemplateAlias = toddler4.Alias;
        baby2.MasterTemplateId = new Lazy<int>(() => toddler4.Id);

        repository.Save(parent);
        repository.Save(child1);
        repository.Save(child2);
        repository.Save(toddler1);
        repository.Save(toddler2);
        repository.Save(toddler3);
        repository.Save(toddler4);
        repository.Save(baby1);
        repository.Save(baby2);

        return new[] { parent, child1, child2, toddler1, toddler2, toddler3, toddler4, baby1, baby2 };
    }
}

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class HybridCachingDocumentTests : UmbracoIntegrationTestWithContent
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddHybridCache();
        services.AddSingleton<IPublishedHybridCache, ContentCache>();
        services.AddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IContentCacheDataSerializerFactory, MsgPackContentNestedDataSerializerFactory>();
        services.AddSingleton<IPropertyCacheCompressionOptions, NoopPropertyCacheCompressionOptions>();
        services.AddNotificationAsyncHandler<ContentRefreshNotification, CacheRefreshingNotificationHandler>();
        services.AddTransient<IPublishedContentFactory, PublishedContentFactory>();
    }

    private IPublishedHybridCache PublishedHybridCache => GetRequiredService<IPublishedHybridCache>();
    public IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();


    // Create CRUD Tests for Content, Also cultures.

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
    {
        var textPage = await PublishedHybridCache.GetById(Textpage.Id, true);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Draft_Content_By_Key()
    {
        var textPage = await PublishedHybridCache.GetById(Textpage.Key, true);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Id()
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        var textPage = await PublishedHybridCache.GetById(Textpage.Id);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Key()
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        var textPage = await PublishedHybridCache.GetById(Textpage.Key);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_By_Id()
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        var textPage = await PublishedHybridCache.GetById(Textpage.Id, true);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_By_Key()
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        var textPage = await PublishedHybridCache.GetById(Textpage.Key, true);
        AssertTextPage(textPage);
    }


    [Test]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task Can_Get_Updated_Draft_Content_By_Id(bool preview, bool result)
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        string newName = "New Name";
        Textpage.Name = newName;
        ContentService.Save(Textpage, -1);
        var textPage = await PublishedHybridCache.GetById(Textpage.Id, preview);
        Assert.AreEqual(result, newName.Equals(textPage.Name));
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task Can_Get_Updated_Draft_Content_By_Key(bool preview, bool result)
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        string newName = "New Name";
        Textpage.Name = newName;
        ContentService.Save(Textpage, -1);
        var textPage = await PublishedHybridCache.GetById(Textpage.Key, preview);
        Assert.AreEqual(result, newName.Equals(textPage.Name));
    }

    [Test]
    public async Task Can_Get_Draft_Property_By_Id()
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        var titleValue = Textpage.GetValue("title");
        var textPage = await PublishedHybridCache.GetById(Textpage.Id, true);

        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Property_By_Key()
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        var titleValue = Textpage.GetValue("title");
        var textPage = await PublishedHybridCache.GetById(Textpage.Key, true);

        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Property_By_Id()
    {
        // Arrange
        ContentService.Publish(Textpage, Array.Empty<string>());
        string newTitle = "New Name";

        // Act
        Textpage.SetValue("title", newTitle);
        ContentService.Save(Textpage, -1);
        var textPage = await PublishedHybridCache.GetById(Textpage.Id, true);
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();

        // Assert
        Assert.AreEqual(newTitle, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Property_By_Key()
    {
        // Arrange
        ContentService.Publish(Textpage, Array.Empty<string>());
        string newTitle = "New Name";

        // Act
        Textpage.SetValue("title", newTitle);
        ContentService.Save(Textpage, -1);
        var textPage = await PublishedHybridCache.GetById(Textpage.Key, true);
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();

        // Assert
        Assert.AreEqual(newTitle, textPage.Value("title"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Not_Get_Deleted_Content_By_Id(bool preview)
    {
        // Act
        ContentService.Delete(Textpage);

        // Assert
        var textPageDeleted = await PublishedHybridCache.GetById(Textpage.Id, true);
        Assert.AreEqual(null, textPageDeleted);
    }

    // TestCases for published and draft
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Not_Get_Deleted_Content_By_Key(bool preview)
    {
        // Act
        ContentService.Delete(Textpage);

        // Assert
        var textPage = await PublishedHybridCache.GetById(Textpage.Key, preview);
        Assert.AreEqual(null, textPage);
    }

    private void AssertTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(textPage);
            Assert.AreEqual(Textpage.Name, textPage.Name);
            Assert.AreEqual(Textpage.Published, textPage.IsPublished());
        });
        AssertProperties(Textpage.Properties, textPage.Properties);
    }

    private void AssertProperties(IPropertyCollection propertyCollection, IEnumerable<IPublishedProperty> publishedProperties)
    {
        foreach (var prop in propertyCollection)
        {
            AssertProperty(prop, publishedProperties.First(x => x.Alias == prop.Alias));
        }
    }

    private void AssertProperty(IProperty property, IPublishedProperty publishedProperty)
    {
        Assert.Multiple(() =>
        {
            Assert.AreEqual(property.Alias, publishedProperty.Alias);
            Assert.AreEqual(property.PropertyType.Alias, publishedProperty.PropertyType.Alias);
        });
    }
}

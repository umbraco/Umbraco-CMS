using NUnit.Framework;
using OpenIddict.Validation.AspNetCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentHybridCacheAncestryTests : UmbracoIntegrationTestWithContent
{
    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IPublishStatusManagementService PublishStatusManagementService => GetRequiredService<IPublishStatusManagementService>();

    private IDocumentNavigationManagementService DocumentNavigationManagementService => GetRequiredService<IDocumentNavigationManagementService>();

    private Content SubSubPage;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    public override void Setup()
    {
        base.Setup();
        // Publish documents
        SubSubPage = ContentBuilder.CreateSimpleContent(ContentType, "SubSubPage", Subpage.Id);
        SubSubPage.Key = Guid.Parse("E4C369B5-CCCA-4981-ADAC-389824CF6B0B");
        ContentService.Save(SubSubPage, -1);
    }

    [Test]
    public async Task CantGetPublishedContentIfParentIsUnpublished()
    {
        // Text Page
          // Sub Page <-- Unpublished
                // Sub Sub Page
        await ContentPublishingService.PublishBranchAsync(Textpage.Key, Array.Empty<string>(), true, Constants.Security.SuperUserKey);
        await ContentPublishingService.UnpublishAsync(Subpage.Key, null, Constants.Security.SuperUserKey);

        var published = await PublishedContentCache.GetByIdAsync(SubSubPage.Key);
        Assert.IsNull(published);
    }

    [Test]
    public async Task CanGetPublishedContentIfParentIsPublished()
    {
        await ContentPublishingService.PublishBranchAsync(Textpage.Key, Array.Empty<string>(), true, Constants.Security.SuperUserKey);

        var published = await PublishedContentCache.GetByIdAsync(SubSubPage.Key);
        AssertSubSubPage(published);
    }

    private void AssertSubSubPage(IPublishedContent subSubPage, bool isPublished = true)
    {
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(subSubPage);
            Assert.That(subSubPage.Name, Is.EqualTo(SubSubPage.Name));
            Assert.That(subSubPage.IsPublished(), Is.EqualTo(isPublished));
        });

        AssertProperties(SubSubPage.Properties, subSubPage.Properties);
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

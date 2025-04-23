using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private const string NewName = "New Name";
    private const string NewTitle = "New Title";

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        // Assert
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Draft_Content_By_Key()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        // Assert
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Id()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId);

        // Assert
        AssertPublishedTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Key()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value);

        // Assert
        AssertPublishedTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_By_Id()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, true);

        // Assert
        AssertPublishedTextPage(textPage);
        Assert.IsFalse(textPage.IsPublished());
    }

    [Test]
    public async Task Cannot_get_unpublished_content()
    {
        // Arrange
        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(PublishedTextPage.Key.Value, null, Constants.Security.SuperUserKey);

        //Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, false);

        // Assert
        Assert.IsNull(textPage);
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_By_Key()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        AssertPublishedTextPage(textPage);
        Assert.IsFalse(textPage.IsPublished());
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Content_By_Id()
    {
        // Arrange
        Textpage.Variants = [new() { Name = NewName }];
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = Textpage.Properties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var updatedPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        // Assert
        Assert.AreEqual(NewName, updatedPage.Name);
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Content_By_Key()
    {
        // Arrange
        Textpage.Variants = [new() { Name = NewName }];
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = Textpage.Properties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var updatedPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        // Assert
        Assert.AreEqual(NewName, updatedPage.Name);
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(false, false)]
    // BETTER NAMING, CURRENTLY THIS IS TESTING BOTH THE PUBLISHED AND THE DRAFT OF THE PUBLISHED.
    public async Task Can_Get_Updated_Draft_Published_Content_By_Id(bool preview, bool result)
    {
        // Arrange
        PublishedTextPage.Variants = [new() { Name = NewName }];
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = PublishedTextPage.Properties,
            Variants = PublishedTextPage.Variants,
            TemplateKey = PublishedTextPage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(PublishedTextPage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, preview);

        // Assert
        Assert.AreEqual(result, NewName.Equals(textPage.Name));
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(false, false)]
    // BETTER NAMING, CURRENTLY THIS IS TESTING BOTH THE PUBLISHED AND THE DRAFT OF THE PUBLISHED.
    public async Task Can_Get_Updated_Draft_Published_Content_By_Key(bool preview, bool result)
    {
        // Arrange
        PublishedTextPage.Variants = [new() { Name = NewName }];;
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = PublishedTextPage.Properties,
            Variants = PublishedTextPage.Variants,
            TemplateKey = PublishedTextPage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(PublishedTextPage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, preview);

        // Assert
        Assert.AreEqual(result, NewName.Equals(textPage.Name));
    }

    [Test]
    public async Task Can_Get_Draft_Content_Property_By_Id()
    {
        // Arrange
        var titleValue = Textpage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        // Assert
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Content_Property_By_Key()
    {
        // Arrange
        var titleValue = Textpage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        // Assert
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Published_Content_Property_By_Id()
    {
        // Arrange
        var titleValue = PublishedTextPage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, true);

        // Assert
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Published_Content_Property_By_Key()
    {
        // Arrange
        var titleValue = PublishedTextPage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_Property_By_Id()
    {
        // Arrange
        var titleValue = PublishedTextPage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, true);

        // Assert
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_Property_By_Key()
    {
        // Arrange
        var titleValue = PublishedTextPage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        Assert.AreEqual(titleValue, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Content_Property_By_Id()
    {
        // Arrange
        Textpage.Properties.First(x => x.Alias == "title").Value = NewTitle;
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = Textpage.Properties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        // Assert
        Assert.AreEqual(NewTitle, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Content_Property_By_Key()
    {
        // Arrange
        Textpage.Properties.First(x => x.Alias == "title").Value = NewTitle;
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = Textpage.Properties,
            Variants = Textpage.Variants,
            TemplateKey = Textpage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(Textpage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        // Assert
        Assert.AreEqual(NewTitle, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Published_Content_Property_By_Id()
    {
        // Arrange
        PublishedTextPage.Properties.First(x => x.Alias == "title").Value = NewTitle;
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = PublishedTextPage.Properties,
            Variants = PublishedTextPage.Variants,
            TemplateKey = PublishedTextPage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(PublishedTextPage.Key.Value, updateModel, Constants.Security.SuperUserKey);
        await ContentPublishingService.PublishAsync(PublishedTextPage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        Assert.AreEqual(NewTitle, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Get_Updated_Published_Content_Property_By_Key()
    {
        // Arrange
        PublishedTextPage.Properties.First(x => x.Alias == "title").Value = NewTitle;
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = PublishedTextPage.Properties,
            Variants = PublishedTextPage.Variants,
            TemplateKey = PublishedTextPage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(PublishedTextPage.Key.Value, updateModel, Constants.Security.SuperUserKey);
        await ContentPublishingService.PublishAsync(PublishedTextPage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value);

        // Assert
        Assert.AreEqual(NewTitle, textPage.Value("title"));
    }

    [Test]
    [TestCase(true, "New Title")]
    [TestCase(false, "Welcome to our Home page")]
    public async Task Can_Get_Updated_Draft_Of_Published_Content_Property_By_Id(bool preview, string titleName)
    {
        // Arrange
        PublishedTextPage.Properties.First(x => x.Alias == "title").Value = NewTitle;
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = PublishedTextPage.Properties,
            Variants = PublishedTextPage.Variants,
            TemplateKey = PublishedTextPage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(PublishedTextPage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, preview);

        // Assert
        Assert.AreEqual(titleName, textPage.Value("title"));
    }

    [Test]
    [TestCase(true, "New Name")]
    [TestCase(false, "Welcome to our Home page")]
    public async Task Can_Get_Updated_Draft_Of_Published_Content_Property_By_Key(bool preview, string titleName)
    {
        // Arrange
        PublishedTextPage.Properties.First(x => x.Alias == "title").Value = titleName;
        ContentUpdateModel updateModel = new ContentUpdateModel
        {
            Properties = PublishedTextPage.Properties,
            Variants = PublishedTextPage.Variants,
            TemplateKey = PublishedTextPage.TemplateKey,
        };
        await ContentEditingService.UpdateAsync(PublishedTextPage.Key.Value, updateModel, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        Assert.AreEqual(titleName, textPage.Value("title"));
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Content_By_Id()
    {
        // Arrange
        var content = await PublishedContentHybridCache.GetByIdAsync(Subpage1Id, true);
        Assert.IsNotNull(content);
        await ContentEditingService.DeleteAsync(Subpage1.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPagePublishedContent = await PublishedContentHybridCache.GetByIdAsync(Subpage1Id, false);

        var textPage = await PublishedContentHybridCache.GetByIdAsync(Subpage1Id, true);

        // Assert
        Assert.IsNull(textPage);
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Content_By_Key()
    {
        // Arrange
        await PublishedContentHybridCache.GetByIdAsync(Subpage1.Key.Value, true);
        var hasContent = await PublishedContentHybridCache.GetByIdAsync(Subpage1Id, true);
        Assert.IsNotNull(hasContent);
        await ContentEditingService.DeleteAsync(Subpage1.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Subpage1.Key.Value, true);

        // Assert
        Assert.IsNull(textPage);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Not_Get_Deleted_Published_Content_By_Id(bool preview)
    {
        // Arrange
        await ContentEditingService.DeleteAsync(PublishedTextPage.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, preview);

        // Assert
        Assert.IsNull(textPage);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Not_Get_Deleted_Published_Content_By_Key(bool preview)
    {
        // Arrange
        await ContentEditingService.DeleteAsync(PublishedTextPage.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, preview);

        // Assert
        Assert.IsNull(textPage);
    }

    private void AssertTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(textPage);
            Assert.AreEqual(Textpage.Key, textPage.Key);
            Assert.AreEqual(Textpage.ContentTypeKey, textPage.ContentType.Key);
            Assert.AreEqual(Textpage.Variants.Single().Name, textPage.Name);
        });

        AssertProperties(Textpage.Properties, textPage.Properties);
    }

    private void AssertPublishedTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(textPage);
            Assert.AreEqual(PublishedTextPage.Key, textPage.Key);
            Assert.AreEqual(PublishedTextPage.ContentTypeKey, textPage.ContentType.Key);
            Assert.AreEqual(PublishedTextPage.Variants.Single().Name, textPage.Name);
        });

        AssertProperties(PublishedTextPage.Properties, textPage.Properties);
    }

    private void AssertProperties(IEnumerable<PropertyValueModel> propertyCollection, IEnumerable<IPublishedProperty> publishedProperties)
    {
        foreach (var prop in propertyCollection)
        {
            AssertProperty(prop, publishedProperties.First(x => x.Alias == prop.Alias));
        }
    }

    private void AssertProperty(PropertyValueModel property, IPublishedProperty publishedProperty)
    {
        Assert.Multiple(() =>
        {
            Assert.AreEqual(property.Alias, publishedProperty.Alias);
            Assert.AreEqual(property.Value, publishedProperty.GetSourceValue());
        });
    }
}

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
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

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

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
        Assert.That(textPage.IsPublished(), Is.False);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Unpublished_Content_By_Key(bool preview)
    {
        // Arrange
        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(PublishedTextPage.Key.Value, null, Constants.Security.SuperUserKey);
        Assert.That(unpublishAttempt.Success, Is.True);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, preview);

        // Assert
        if (preview)
        {
            Assert.That(textPage, Is.Not.Null);
            Assert.That(textPage.IsPublished(), Is.False);
        }
        else
        {
            Assert.That(textPage, Is.Null);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Unpublished_Content_By_Id(bool preview)
    {
        // Arrange
        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(PublishedTextPage.Key.Value, null, Constants.Security.SuperUserKey);
        Assert.That(unpublishAttempt.Success, Is.True);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, preview);

        // Assert
        if (preview)
        {
            Assert.That(textPage, Is.Not.Null);
            Assert.That(textPage.IsPublished(), Is.False);
        }
        else
        {
            Assert.That(textPage, Is.Null);
        }
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_By_Key()
    {
        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        AssertPublishedTextPage(textPage);
        Assert.That(textPage.IsPublished(), Is.False);
    }

    [Test]
    public async Task Filtering_By_IsPublished_In_Preview_Mode_Returns_Published_Content()
    {
        // Arrange - Initialize the publish status service to simulate production state
        // (in production, this runs at startup via PostRuntimePremigrationsUpgradeNotification)
        var publishStatusManagementService = GetRequiredService<IPublishStatusManagementService>();
        await publishStatusManagementService.InitializeAsync(CancellationToken.None);

        // PublishedTextPage is published, Textpage is draft-only (from base class setup)
        // Load both in preview mode (simulating a backoffice user viewing the frontend)
        var publishedPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, true);
        var unpublishedPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        Assert.That(publishedPage, Is.Not.Null);
        Assert.That(unpublishedPage, Is.Not.Null);

        var allPages = new[] { publishedPage!, unpublishedPage! };

        // Act - filter by IsPublished (the exact pattern that fails in Razor templates)
        var filteredPages = allPages.Where(x => x.IsPublished()).ToList();

        // Assert - only the published page should pass the filter
        Assert.That(filteredPages, Has.Count.EqualTo(1));
        Assert.That(filteredPages[0].Key, Is.EqualTo(PublishedTextPage.Key!.Value));
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
        Assert.That(updatedPage.Name, Is.EqualTo(NewName));
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
        Assert.That(updatedPage.Name, Is.EqualTo(NewName));
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
        Assert.That(NewName.Equals(textPage.Name), Is.EqualTo(result));
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(false, false)]
    // BETTER NAMING, CURRENTLY THIS IS TESTING BOTH THE PUBLISHED AND THE DRAFT OF THE PUBLISHED.
    public async Task Can_Get_Updated_Draft_Published_Content_By_Key(bool preview, bool result)
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
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, preview);

        // Assert
        Assert.That(NewName.Equals(textPage.Name), Is.EqualTo(result));
    }

    [Test]
    public async Task Can_Get_Draft_Content_Property_By_Id()
    {
        // Arrange
        var titleValue = Textpage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);

        // Assert
        Assert.That(textPage.Value("title"), Is.EqualTo(titleValue));
    }

    [Test]
    public async Task Can_Get_Draft_Content_Property_By_Key()
    {
        // Arrange
        var titleValue = Textpage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        // Assert
        Assert.That(textPage.Value("title"), Is.EqualTo(titleValue));
    }

    [Test]
    public async Task Can_Get_Published_Content_Property_By_Id()
    {
        // Arrange
        var titleValue = PublishedTextPage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, true);

        // Assert
        Assert.That(textPage.Value("title"), Is.EqualTo(titleValue));
    }

    [Test]
    public async Task Can_Get_Published_Content_Property_By_Key()
    {
        // Arrange
        var titleValue = PublishedTextPage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        Assert.That(textPage.Value("title"), Is.EqualTo(titleValue));
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_Property_By_Id()
    {
        // Arrange
        var titleValue = PublishedTextPage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, true);

        // Assert
        Assert.That(textPage.Value("title"), Is.EqualTo(titleValue));
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Content_Property_By_Key()
    {
        // Arrange
        var titleValue = PublishedTextPage.Properties.First(x => x.Alias == "title").Value;

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        Assert.That(textPage.Value("title"), Is.EqualTo(titleValue));
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
        Assert.That(textPage.Value("title"), Is.EqualTo(NewTitle));
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
        Assert.That(textPage.Value("title"), Is.EqualTo(NewTitle));
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
        await ContentPublishingService.PublishAsync(
            PublishedTextPage.Key.Value,
            [new CulturePublishScheduleModel { Culture = "*" }],
            Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);

        // Assert
        Assert.That(textPage.Value("title"), Is.EqualTo(NewTitle));
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
        await ContentPublishingService.PublishAsync(
            PublishedTextPage.Key.Value,
            [new CulturePublishScheduleModel { Culture = "*" }],
            Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value);

        // Assert
        Assert.That(textPage.Value("title"), Is.EqualTo(NewTitle));
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
        Assert.That(textPage.Value("title"), Is.EqualTo(titleName));
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
        Assert.That(textPage.Value("title"), Is.EqualTo(titleName));
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Content_By_Id()
    {
        // Arrange
        var content = await PublishedContentHybridCache.GetByIdAsync(Subpage1Id, true);
        Assert.That(content, Is.Not.Null);
        await ContentEditingService.DeleteAsync(Subpage1.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPagePublishedContent = await PublishedContentHybridCache.GetByIdAsync(Subpage1Id, false);

        var textPage = await PublishedContentHybridCache.GetByIdAsync(Subpage1Id, true);

        // Assert
        Assert.That(textPage, Is.Null);
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Content_By_Key()
    {
        // Arrange
        await PublishedContentHybridCache.GetByIdAsync(Subpage1.Key.Value, true);
        var hasContent = await PublishedContentHybridCache.GetByIdAsync(Subpage1Id, true);
        Assert.That(hasContent, Is.Not.Null);
        await ContentEditingService.DeleteAsync(Subpage1.Key.Value, Constants.Security.SuperUserKey);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Subpage1.Key.Value, true);

        // Assert
        Assert.That(textPage, Is.Null);
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
        Assert.That(textPage, Is.Null);
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
        Assert.That(textPage, Is.Null);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Id_After_Previous_Check_Where_Not_Found()
    {
        // Arrange
        var testPageKey = Guid.NewGuid();

        // Act & Assert
        // - assert we cannot get the content that doesn't yet exist from the cache
        var testPage = await PublishedContentHybridCache.GetByIdAsync(testPageKey);
        Assert.That(testPage, Is.Null);

        testPage = await PublishedContentHybridCache.GetByIdAsync(testPageKey);
        Assert.That(testPage, Is.Null);

        // - create and publish the content
        var testPageContent = ContentEditingBuilder.CreateBasicContent(ContentType.Key, testPageKey);
        var createResult = await ContentEditingService.CreateAsync(testPageContent, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        var publishResult = await ContentPublishingService.PublishAsync(testPageKey, CultureAndSchedule, Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        // - assert we can now get the content from the cache
        testPage = await PublishedContentHybridCache.GetByIdAsync(testPageKey);
        Assert.That(testPage, Is.Not.Null);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Id_After_Previous_Exists_Check()
    {
        // Act
        var hasContentForTextPageCached = await DocumentCacheService.HasContentByIdAsync(PublishedTextPageId);
        Assert.That(hasContentForTextPageCached, Is.True);
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId);

        // Assert
        AssertPublishedTextPage(textPage);
    }

    [Test]
    public async Task Can_Do_Exists_Check_On_Created_Published_Content()
    {
        var testPageKey = Guid.NewGuid();
        var testPageContent = ContentEditingBuilder.CreateBasicContent(ContentType.Key, testPageKey);
        var createResult = await ContentEditingService.CreateAsync(testPageContent, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        var publishResult = await ContentPublishingService.PublishAsync(testPageKey, CultureAndSchedule, Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var testPage = await PublishedContentHybridCache.GetByIdAsync(testPageKey);
        Assert.That(testPage, Is.Not.Null);

        var hasContentForTextPageCached = await DocumentCacheService.HasContentByIdAsync(testPage.Id);
        Assert.That(hasContentForTextPageCached, Is.True);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Get_Trashed_Content_By_Key(bool preview)
    {
        // Arrange - Verify published content is in cache
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, preview);
        Assert.That(textPage, Is.Not.Null, "Content should be in cache before trashing");

        // Act - Trash the document (move to recycle bin)
        var trashResult = await ContentEditingService.MoveToRecycleBinAsync(PublishedTextPage.Key.Value, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        // Assert - Content should no longer be in the cache
        var trashedPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, preview);
        Assert.That(trashedPage, Is.Null, "Trashed content should not be in cache");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Get_Trashed_Content_By_Id(bool preview)
    {
        // Arrange - Verify published content is in cache
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, preview);
        Assert.That(textPage, Is.Not.Null, "Content should be in cache before trashing");

        // Act - Trash the document (move to recycle bin)
        var trashResult = await ContentEditingService.MoveToRecycleBinAsync(PublishedTextPage.Key.Value, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        // Assert - Content should no longer be in the cache
        var trashedPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPageId, preview);
        Assert.That(trashedPage, Is.Null, "Trashed content should not be in cache");
    }

    [Test]
    public async Task Restored_Content_Is_Available_In_Draft_Cache()
    {
        // Arrange - Verify published content is in cache, then trash it
        var textPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);
        Assert.That(textPage, Is.Not.Null, "Content should be in cache before trashing");

        var trashResult = await ContentEditingService.MoveToRecycleBinAsync(PublishedTextPage.Key.Value, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        var trashedPage = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);
        Assert.That(trashedPage, Is.Null, "Trashed content should not be in cache");

        // Act - Restore to root (original location)
        var restoreResult = await ContentEditingService.RestoreAsync(PublishedTextPage.Key.Value, null, Constants.Security.SuperUserKey);
        Assert.That(restoreResult.Success, Is.True);

        // Assert - Restored content should be back in the draft cache, but not republished automatically
        var restoredDraft = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, true);
        Assert.That(restoredDraft, Is.Not.Null, "Restored content should be in the draft cache");

        var restoredPublished = await PublishedContentHybridCache.GetByIdAsync(PublishedTextPage.Key.Value, false);
        Assert.That(restoredPublished, Is.Null, "Restored content should not be in the published cache until it is republished");
    }

    private void AssertTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.That(textPage, Is.Not.Null);
            Assert.That(textPage.Key, Is.EqualTo(Textpage.Key));
            Assert.That(textPage.ContentType.Key, Is.EqualTo(Textpage.ContentTypeKey));
            Assert.That(textPage.Name, Is.EqualTo(Textpage.Variants.Single().Name));
        });

        AssertProperties(Textpage.Properties, textPage.Properties);
    }

    private void AssertPublishedTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.That(textPage, Is.Not.Null);
            Assert.That(textPage.Key, Is.EqualTo(PublishedTextPage.Key));
            Assert.That(textPage.ContentType.Key, Is.EqualTo(PublishedTextPage.ContentTypeKey));
            Assert.That(textPage.Name, Is.EqualTo(PublishedTextPage.Variants.Single().Name));
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
            Assert.That(publishedProperty.Alias, Is.EqualTo(property.Alias));
            Assert.That(publishedProperty.GetSourceValue(), Is.EqualTo(property.Value));
        });
    }
}

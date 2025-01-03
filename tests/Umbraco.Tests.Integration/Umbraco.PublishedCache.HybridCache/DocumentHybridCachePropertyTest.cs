using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentHybridCachePropertyTest : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private ICacheManager CacheManager => GetRequiredService<ICacheManager>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    [Test]
    public async Task Can_Get_Value_From_ContentPicker()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var textPage = await CreateTextPageDocument(template.Key);
        var contentPickerDocument = await CreateContentPickerDocument(template.Key, textPage.Key);

        // Act
        var contentPickerPage = await CacheManager.Content.GetByIdAsync(contentPickerDocument.Id);

        // Assert
        IPublishedContent contentPickerValue = (IPublishedContent)contentPickerPage.Value("contentPicker");
        Assert.AreEqual(textPage.Key, contentPickerValue.Key);
        Assert.AreEqual(textPage.Id, contentPickerValue.Id);
        Assert.AreEqual(textPage.Name, contentPickerValue.Name);
        Assert.AreEqual("The title value", contentPickerValue.Properties.First(x => x.Alias == "title").GetValue());
    }

    [Test]
    public async Task Can_Get_Value_From_Updated_ContentPicker()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var textPage = await CreateTextPageDocument(template.Key);
        var contentPickerDocument = await CreateContentPickerDocument(template.Key, textPage.Key);

        // Get for caching
        var notUpdatedContent = await CacheManager.Content.GetByIdAsync(contentPickerDocument.Id);

        IPublishedContent contentPickerValue = (IPublishedContent)notUpdatedContent.Value("contentPicker");
        Assert.AreEqual("The title value", contentPickerValue.Properties.First(x => x.Alias == "title").GetValue());

        // Update content
        var updateModel = new ContentUpdateModel
        {
            InvariantName = "Root Create",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "Updated title" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body text" }
            },
        };

        var updateResult = await ContentEditingService.UpdateAsync(textPage.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(updateResult.Success);

        var publishResult = await ContentPublishingService.PublishAsync(
            updateResult.Result.Content!.Key,
            new CultureAndScheduleModel()
            {
                CulturesToPublishImmediately = new HashSet<string> { "*" },
                Schedules = new ContentScheduleCollection(),
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishResult);

        // Act
        var contentPickerPage = await CacheManager.Content.GetByIdAsync(contentPickerDocument.Id);

        // Assert
        IPublishedContent updatedPickerValue = (IPublishedContent)contentPickerPage.Value("contentPicker");
        Assert.AreEqual(textPage.Key, updatedPickerValue.Key);
        Assert.AreEqual(textPage.Id, updatedPickerValue.Id);
        Assert.AreEqual(textPage.Name, updatedPickerValue.Name);
        Assert.AreEqual("Updated title", updatedPickerValue.Properties.First(x => x.Alias == "title").GetValue());
    }

    private async Task<IContent> CreateContentPickerDocument(Guid templateKey, Guid textPageKey)
    {
        var pickerContentType = ContentTypeEditingBuilder.CreateContentTypeWithContentPicker(templateKey: templateKey);
        await ContentTypeEditingService.CreateAsync(pickerContentType, Constants.Security.SuperUserKey);

        var createOtherModel = ContentEditingBuilder.CreateContentWithOneInvariantProperty(pickerContentType.Key.Value,
            "Test Create", "contentPicker", textPageKey);
        var result = await ContentEditingService.CreateAsync(createOtherModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        var publishResult = await ContentPublishingService.PublishAsync(
            result.Result.Content!.Key,
            new CultureAndScheduleModel()
            {
                CulturesToPublishImmediately = new HashSet<string> { "*" },
                Schedules = new ContentScheduleCollection(),
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishResult.Success);
        return result.Result.Content;
    }

    private async Task<IContent> CreateTextPageDocument(Guid templateKey)
    {
        var textContentType = ContentTypeEditingBuilder.CreateTextPageContentType(defaultTemplateKey: templateKey);
        await ContentTypeEditingService.CreateAsync(textContentType, Constants.Security.SuperUserKey);

        var contentCreateModel = ContentEditingBuilder.CreateContentWithTwoInvariantProperties(
            textContentType.Key.Value, "Root Create", "title", "The title value", "bodyText", "The body text",
            Constants.System.RootKey);

        var createResult = await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var publishResult = await ContentPublishingService.PublishAsync(
            createResult.Result.Content!.Key,
            new CultureAndScheduleModel()
            {
                CulturesToPublishImmediately = new HashSet<string> { "*" },
                Schedules = new ContentScheduleCollection(),
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishResult.Success);
        return createResult.Result.Content;
    }
}

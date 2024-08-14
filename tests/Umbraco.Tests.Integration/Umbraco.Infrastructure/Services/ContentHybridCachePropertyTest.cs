using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentHybridCachePropertyTest : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentHybridCache PublishedContentHybridCache => GetRequiredService<IPublishedContentHybridCache>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();


    [Test]
    public async Task Can_Get_Correct_Document_From_Updated_Content_Picker()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var textPage = await CreateTextPageDocument(template.Id);
        var contentPickerDocument = await CreateContentPickerDocument(template.Id, textPage.Key);

        var contentPickerPage = await PublishedContentHybridCache.GetById(contentPickerDocument.Id);

        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(textPage.Key, contentPickerPage.Value("contentPicker"));
    }

    private async Task<IContent> CreateContentPickerDocument(int templateId, Guid textPageKey)
    {
        var builder = new ContentTypeBuilder();
        var pickerContentType = (ContentType)builder
            .WithAlias("test")
            .WithName("TestName")
            .AddAllowedTemplate()
            .WithId(templateId)
            .Done()
            .AddPropertyGroup()
            .WithName("Content")
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("contentPicker")
            .WithName("Content Picker")
            .WithDataTypeId(1046)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithSortOrder(16)
            .Done()
            .Done()
            .Build();

        pickerContentType.AllowedAsRoot = true;
        ContentTypeService.Save(pickerContentType);


        var createOtherModel = new ContentCreateModel
        {
            ContentTypeKey = pickerContentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create",
            InvariantProperties = new[] { new PropertyValueModel { Alias = "contentPicker", Value = textPageKey }, },
        };

        var result = await ContentEditingService.CreateAsync(createOtherModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        var publishResult = await ContentPublishingService.PublishAsync(
            result.Result.Content!.Key,
            new CultureAndScheduleModel()
            {
                CulturesToPublishImmediately = new HashSet<string> {"*"},
                Schedules = new ContentScheduleCollection(),
            },
            Constants.Security.SuperUserKey);

        return result.Result.Content;
    }

    private async Task<IContent> CreateTextPageDocument(int templateId)
    {
        var textContentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: templateId);
        textContentType.AllowedAsRoot = true;
        ContentTypeService.Save(textContentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = textContentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Root Create",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body text" }
            },
        };

        var createResult = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var publishResult = await ContentPublishingService.PublishAsync(
            createResult.Result.Content!.Key,
            new CultureAndScheduleModel()
            {
                CulturesToPublishImmediately = new HashSet<string> {"*"},
                Schedules = new ContentScheduleCollection(),
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishResult.Success);
        return createResult.Result.Content;
    }
}

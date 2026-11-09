using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal class ContentVersionServiceTests : UmbracoIntegrationTest
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentVersionService ContentVersionService => GetRequiredService<IContentVersionService>();

    [Test]
    public async Task Can_Roll_Back_Culture_Invariant_Content_When_No_Culture_Is_Specified()
    {
        IContent content = await CreateInvariantContentWithTwoVersionsAsync();
        Guid versionId = await GetHistoricVersionIdAsync(content, culture: null);

        Attempt<ContentVersionOperationStatus> result =
            await ContentVersionService.RollBackAsync(versionId, null, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Original title", (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!.GetValue<string>("title"));
    }

    [Test]
    public async Task Cannot_Roll_Back_Culture_Invariant_Content_When_A_Culture_Is_Specified()
    {
        IContent content = await CreateInvariantContentWithTwoVersionsAsync();
        Guid versionId = await GetHistoricVersionIdAsync(content, culture: null);

        Attempt<ContentVersionOperationStatus> result =
            await ContentVersionService.RollBackAsync(versionId, "en-US", Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentVersionOperationStatus.InvalidCulture, result.Result);
        Assert.AreEqual("Updated title", (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!.GetValue<string>("title"));
    }

    [Test]
    public async Task Can_Roll_Back_Culture_Variant_Content_When_A_Culture_Is_Specified()
    {
        IContent content = await CreateVariantContentWithTwoVersionsAsync();
        Guid versionId = await GetHistoricVersionIdAsync(content, "en-US");

        Attempt<ContentVersionOperationStatus> result =
            await ContentVersionService.RollBackAsync(versionId, "en-US", Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Original title", (await ContentService.GetByIdAsync(content.Key, CancellationToken.None))!.GetValue<string>("title", "en-US"));
    }

    private async Task<IContent> CreateInvariantContentWithTwoVersionsAsync()
    {
        IContentType contentType = await CreateContentTypeAsync(ContentVariation.Nothing);

        Content content = ContentBuilder.CreateSimpleContent(contentType);
        content.SetValue("title", "Original title");
        await ContentService.SaveAsync(content, Constants.Security.SuperUserKey, null, CancellationToken.None);
        await ContentService.PublishAsync(content, [], Constants.Security.SuperUserKey, CancellationToken.None);

        content.SetValue("title", "Updated title");
        await ContentService.SaveAsync(content, Constants.Security.SuperUserKey, null, CancellationToken.None);
        await ContentService.PublishAsync(content, [], Constants.Security.SuperUserKey, CancellationToken.None);

        return content;
    }

    private async Task<IContent> CreateVariantContentWithTwoVersionsAsync()
    {
        IContentType contentType = await CreateContentTypeAsync(ContentVariation.Culture);

        Content content = ContentBuilder.CreateSimpleContent(contentType, "Home", culture: "en-US");
        content.SetCultureName("Home", "en-US");
        content.SetValue("title", "Original title", "en-US");
        await ContentService.SaveAsync(content, Constants.Security.SuperUserKey, null, CancellationToken.None);
        await ContentService.PublishAsync(content, ["en-US"], Constants.Security.SuperUserKey, CancellationToken.None);

        content.SetValue("title", "Updated title", "en-US");
        await ContentService.SaveAsync(content, Constants.Security.SuperUserKey, null, CancellationToken.None);
        await ContentService.PublishAsync(content, ["en-US"], Constants.Security.SuperUserKey, CancellationToken.None);

        return content;
    }

    private async Task<IContentType> CreateContentTypeAsync(ContentVariation variation)
    {
        Template template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey, CancellationToken.None);

        ContentType contentType = ContentTypeBuilder.CreateSimpleContentType(
            "umbTextpage", "Textpage", defaultTemplateId: template.Id);
        contentType.Variations = variation;
        foreach (IPropertyType propertyType in contentType.PropertyTypes)
        {
            propertyType.Variations = variation;
        }

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return contentType;
    }

    private async Task<Guid> GetHistoricVersionIdAsync(IContent content, string? culture)
    {
        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> versions =
            await ContentVersionService.GetPagedContentVersionsAsync(content.Key, culture, 0, 100);

        Assert.IsTrue(versions.Success);

        ContentVersionMeta? historicVersion = versions.Result!.Items
            .FirstOrDefault(version => version.CurrentDraftVersion is false && version.CurrentPublishedVersion is false);

        Assert.IsNotNull(historicVersion, "Expected the content to have at least one historic version.");

        return historicVersion!.VersionId.ToGuid();
    }
}

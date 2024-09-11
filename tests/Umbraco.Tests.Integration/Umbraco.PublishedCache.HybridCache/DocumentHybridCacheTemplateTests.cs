using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentHybridCacheTemplateTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    [Test]
    public async Task Can_Get_Document_After_Removing_Template()
    {
        // Arrange
        var textPageBefore = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.AreEqual(textPageBefore.TemplateId, TemplateId);
        var updatedContentType = ContentTypeUpdateModel;
        updatedContentType.DefaultTemplateKey = null;
        updatedContentType.AllowedTemplateKeys = new List<Guid>();

        // Act
        var updatedContentTypeResult = await ContentTypeEditingService.UpdateAsync(ContentType, updatedContentType, Constants.Security.SuperUserKey);

        // Assert
        Assert.AreEqual(updatedContentTypeResult.Status, ContentTypeOperationStatus.Success);
        var textPageAfter = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        // Should this not be null?
        Assert.AreEqual(textPageAfter.TemplateId, null);
    }
}

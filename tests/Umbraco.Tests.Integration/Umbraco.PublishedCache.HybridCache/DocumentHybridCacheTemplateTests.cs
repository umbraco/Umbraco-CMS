using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[Platform("Linux", Reason = "This uses too much memory when running both caches, should be removed when nuchache is removed")]
public class DocumentHybridCacheTemplateTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    [Test]
    public async Task Can_Get_Document_After_Removing_Template()
    {
        // Arrange
        var textPageBefore = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.AreEqual(textPageBefore.TemplateId, TemplateId);
        var updateModel = new ContentUpdateModel();
        {
            updateModel.TemplateKey = null;
            updateModel.InvariantName = textPageBefore.Name;
        }

        // Act
        var updateContentResult = await ContentEditingService.UpdateAsync(textPageBefore.Key, updateModel, Constants.Security.SuperUserKey);

        // Assert
        Assert.AreEqual(updateContentResult.Status, ContentEditingOperationStatus.Success);
        var textPageAfter = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.AreEqual(textPageAfter.TemplateId, null);
    }
}

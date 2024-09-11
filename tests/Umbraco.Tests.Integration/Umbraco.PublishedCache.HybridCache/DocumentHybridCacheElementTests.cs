using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentHybridCacheElementTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private ICacheManager CacheManager => GetRequiredService<ICacheManager>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    [Test]
    public async Task Can_Get_Element_Type()
    {
        // Arrange
        var element = ContentTypeEditingBuilder.CreateElementType();
        var elementCreateAttempt = await ContentTypeEditingService.CreateAsync(element , Constants.Security.SuperUserKey);

        // Act
        // Should this not contain a value?
        var textPageElement = await CacheManager.Content.GetByIdAsync(elementCreateAttempt.Result.Id);

        // Assert
        Assert.IsNotNull(textPageElement);
    }
}

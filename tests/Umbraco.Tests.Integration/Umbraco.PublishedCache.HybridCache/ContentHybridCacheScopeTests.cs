using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentHybridCacheScopeTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private ICoreScopeProvider ICoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    [Test]
    public async Task Can_Get_Correct_Content_After_Rollback()
    {
        using (ICoreScopeProvider.CreateCoreScope())
        {
            await ContentPublishingService.PublishAsync(Textpage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);
        }

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId);

        // Published page should be in cache, as we rolled scope back.
        Assert.IsNull(textPage);
    }
}

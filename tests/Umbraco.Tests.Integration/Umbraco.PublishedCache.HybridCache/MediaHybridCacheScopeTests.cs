using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaHybridCacheScopeTests : UmbracoIntegrationTestWithMediaEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedMediaCache PublishedMediaHybridCache => GetRequiredService<IPublishedMediaCache>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();


    [Test]
    public async Task Can_Get_Media_After_Delete_Was_Rolled_back_With_Id()
    {
        // Arrange
        using (CoreScopeProvider.CreateCoreScope())
        {
            await MediaEditingService.DeleteAsync(SubFolder1.Key.Value, Constants.Security.SuperUserKey);
        }

        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubFolder1Id);

        // Assert
        // media should still be in cache, as we rolled scope back.
        Assert.IsNotNull(media);
    }

    [Test]
    public async Task Can_Get_Media_After_Delete_Was_Rolled_back_With_Key()
    {
        // Arrange
        using (CoreScopeProvider.CreateCoreScope())
        {
            await MediaEditingService.DeleteAsync(SubFolder1.Key.Value, Constants.Security.SuperUserKey);
        }

        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubFolder1.Key.Value);

        // Assert
        // Media should still be in cache, as we rolled scope back.
        Assert.IsNotNull(media);
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Media_After_Scope_Complete_With_Id()
    {
        // Arrange
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            await MediaEditingService.DeleteAsync(SubFolder1.Key.Value, Constants.Security.SuperUserKey);
            scope.Complete();
        }

        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubFolder1Id);

        // Assert
        // Media  should not be in cache, as we completed the scope for delete.
        Assert.IsNull(media);
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Media_After_Scope_Complete_With_Key()
    {
        // Arrange
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            await MediaEditingService.DeleteAsync(SubFolder1.Key.Value, Constants.Security.SuperUserKey);
            scope.Complete();
        }

        // Act
        var textPage = await PublishedMediaHybridCache.GetByIdAsync(SubFolder1.Key.Value);

        // Assert
        // Media  should not be in cache, as we completed the scope for delete.
        Assert.IsNull(textPage);
    }
}

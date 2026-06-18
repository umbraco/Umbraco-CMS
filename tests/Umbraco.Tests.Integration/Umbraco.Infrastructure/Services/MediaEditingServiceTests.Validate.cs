using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class MediaEditingServiceTests
{
    [Test]
    public async Task Cannot_Validate_Create_At_Root_When_Not_Allowed_As_Root()
    {
        var createModel = BuildTestMediaRootCreateModel(allowedAsRoot: false);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowTestMediaAtRoot))]
    public async Task Cannot_Validate_Create_At_Root_With_Content_Type_Filter()
    {
        var createModel = BuildTestMediaRootCreateModel(allowedAsRoot: true);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToAllowTestMediaAtRoot))]
    public async Task Can_Validate_Create_At_Root_With_Content_Type_Filter()
    {
        var createModel = BuildTestMediaRootCreateModel(allowedAsRoot: true);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    private MediaCreateModel BuildTestMediaRootCreateModel(bool allowedAsRoot)
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMedia", "Test Media");
        mediaType.AllowedAsRoot = allowedAsRoot;
        MediaTypeService.Save(mediaType);

        return new MediaCreateModel
        {
            ContentTypeKey = mediaType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create" }],
        };
    }

    public static void ConfigureContentTypeFilterToAllowTestMediaAtRoot(IUmbracoBuilder builder)
        => builder.ContentTypeFilters().Append<ContentTypeFilterForAllowedTestMediaAtRoot>();

    public static void ConfigureContentTypeFilterToDisallowTestMediaAtRoot(IUmbracoBuilder builder)
        => builder.ContentTypeFilters().Append<ContentTypeFilterForDisallowedTestMediaAtRoot>();

    private sealed class ContentTypeFilterForAllowedTestMediaAtRoot() : ContentTypeFilterForTestMediaAtRoot(true);

    private sealed class ContentTypeFilterForDisallowedTestMediaAtRoot() : ContentTypeFilterForTestMediaAtRoot(false);

    private abstract class ContentTypeFilterForTestMediaAtRoot(bool allowed) : IContentTypeFilter
    {
        public Task<IEnumerable<TItem>> FilterAllowedAtRootAsync<TItem>(IEnumerable<TItem> contentTypes)
            where TItem : IContentTypeComposition
            => Task.FromResult(contentTypes.Where(x => (allowed && x.Alias == "testMedia") || (!allowed && x.Alias != "testMedia")));
    }
}

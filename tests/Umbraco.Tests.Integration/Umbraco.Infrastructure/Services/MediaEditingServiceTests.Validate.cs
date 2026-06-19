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
        var createModel = await BuildTestMediaRootCreateModel(allowedAsRoot: false);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowTestMediaAtRoot))]
    public async Task Cannot_Validate_Create_At_Root_With_Content_Type_Filter()
    {
        var createModel = await BuildTestMediaRootCreateModel(allowedAsRoot: true);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToAllowTestMediaAtRoot))]
    public async Task Can_Validate_Create_At_Root_With_Content_Type_Filter()
    {
        var createModel = await BuildTestMediaRootCreateModel(allowedAsRoot: true);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Cannot_Validate_Create_As_Child_When_Not_Allowed_By_Parent()
    {
        var createModel = await BuildTestMediaChildCreateModel(parentAllowsChild: false);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
    }

    [Test]
    public async Task Can_Validate_Create_As_Child_When_Allowed_By_Parent()
    {
        var createModel = await BuildTestMediaChildCreateModel(parentAllowsChild: true);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowChildMediaAsChild))]
    public async Task Cannot_Validate_Create_As_Child_With_Content_Type_Filter()
    {
        // The parent allows the child type, so the only reason creation is disallowed is the content type filter.
        var createModel = await BuildTestMediaChildCreateModel(parentAllowsChild: true);

        var result = await MediaEditingService.ValidateCreateAsync(createModel);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
    }

    private async Task<MediaCreateModel> BuildTestMediaRootCreateModel(bool allowedAsRoot)
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMedia", "Test Media");
        mediaType.AllowedAsRoot = allowedAsRoot;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        return new MediaCreateModel
        {
            ContentTypeKey = mediaType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create" }],
        };
    }

    private async Task<MediaCreateModel> BuildTestMediaChildCreateModel(bool parentAllowsChild)
    {
        var childMediaType = MediaTypeBuilder.CreateSimpleMediaType("childMedia", "Child Media");
        childMediaType.AllowedAsRoot = false;
        await MediaTypeService.CreateAsync(childMediaType, Constants.Security.SuperUserKey);

        var parentMediaType = MediaTypeBuilder.CreateSimpleMediaType("parentMedia", "Parent Media");
        parentMediaType.AllowedAsRoot = true;
        if (parentAllowsChild)
        {
            parentMediaType.AllowedContentTypes = new[]
            {
                new ContentTypeSort(childMediaType.Key, 1, childMediaType.Alias)
            };
        }

        await MediaTypeService.CreateAsync(parentMediaType, Constants.Security.SuperUserKey);

        var parentKey = (await MediaEditingService.CreateAsync(
            new MediaCreateModel
            {
                ContentTypeKey = parentMediaType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = "Parent" }],
            },
            Constants.Security.SuperUserKey)).Result.Content!.Key;

        return new MediaCreateModel
        {
            ContentTypeKey = childMediaType.Key,
            ParentKey = parentKey,
            Variants = [new VariantModel { Name = "Test Create Child" }],
        };
    }

    public static void ConfigureContentTypeFilterToAllowTestMediaAtRoot(IUmbracoBuilder builder)
        => builder.ContentTypeFilters().Append<ContentTypeFilterForAllowedTestMediaAtRoot>();

    public static void ConfigureContentTypeFilterToDisallowTestMediaAtRoot(IUmbracoBuilder builder)
        => builder.ContentTypeFilters().Append<ContentTypeFilterForDisallowedTestMediaAtRoot>();

    public static void ConfigureContentTypeFilterToDisallowChildMediaAsChild(IUmbracoBuilder builder)
        => builder.ContentTypeFilters().Append<ContentTypeFilterForDisallowedChildMediaAsChild>();

    private sealed class ContentTypeFilterForDisallowedChildMediaAsChild : IContentTypeFilter
    {
        public Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentContentTypeKey, Guid? parentContentKey)
            => Task.FromResult(contentTypes.Where(x => x.Alias != "childMedia"));
    }

    private sealed class ContentTypeFilterForAllowedTestMediaAtRoot : ContentTypeFilterForTestMediaAtRoot
    {
        public ContentTypeFilterForAllowedTestMediaAtRoot()
            : base(true)
        {
        }
    }

    private sealed class ContentTypeFilterForDisallowedTestMediaAtRoot : ContentTypeFilterForTestMediaAtRoot
    {
        public ContentTypeFilterForDisallowedTestMediaAtRoot()
            : base(false)
        {
        }
    }

    private abstract class ContentTypeFilterForTestMediaAtRoot : IContentTypeFilter
    {
        private readonly bool _allowed;

        protected ContentTypeFilterForTestMediaAtRoot(bool allowed) => _allowed = allowed;

        public Task<IEnumerable<TItem>> FilterAllowedAtRootAsync<TItem>(IEnumerable<TItem> contentTypes)
            where TItem : IContentTypeComposition
            => Task.FromResult(contentTypes.Where(x => (_allowed && x.Alias == "testMedia") || (!_allowed && x.Alias != "testMedia")));
    }
}

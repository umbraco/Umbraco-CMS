using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Filters;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests for the content blueprint editing service. Please notice that a lot of the functional tests are covered by the content
///     editing service tests, since these services share the same base implementation.
/// </summary>
public partial class ContentBlueprintEditingServiceTests : ContentEditingServiceTestsBase
{
    private IContentBlueprintContainerService ContentBlueprintContainerService => GetRequiredService<IContentBlueprintContainerService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private IAuditService AuditService => GetRequiredService<IAuditService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private async Task<IContent> CreateInvariantContentBlueprint()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Initial Blueprint Name" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The initial title" },
                new PropertyValueModel { Alias = "text", Value = "The initial text" }
            ],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private async Task<IContent> CreateVariantContentBlueprint()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial Danish title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Initial Blueprint English Name" },
                new VariantModel { Culture = "da-DK", Name = "Initial Blueprint Danish Name" }
            ],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private ContentBlueprintCreateModel SimpleContentBlueprintCreateModel(Guid blueprintKey, Guid? containerKey)
    {
        var createModel = new ContentBlueprintCreateModel
        {
            Key = blueprintKey,
            ContentTypeKey = ContentType.Key,
            ParentKey = containerKey,
            Variants = [new VariantModel { Name = "Blueprint #1" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "author", Value = "The author value" },
            ],
        };
        return createModel;
    }

    private ContentBlueprintUpdateModel SimpleContentBlueprintUpdateModel()
    {
        var createModel = new ContentBlueprintUpdateModel
        {
            Variants = [new VariantModel { Name = "Blueprint #1 updated" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value updated" },
                new PropertyValueModel { Alias = "author", Value = "The author value updated" }
            ],
        };
        return createModel;
    }

    private IEntitySlim[] GetBlueprintChildren(Guid? containerKey)
        => EntityService.GetPagedChildren(containerKey, [UmbracoObjectTypes.DocumentBlueprintContainer], UmbracoObjectTypes.DocumentBlueprint, 0, 100, out _).ToArray();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.ContentTypeFilters().Append<ExcludingContentTypeFilter>();
    }

    [SetUp]
    public void ResetContentTypeFilter()
    {
        ExcludingContentTypeFilter.ExcludedContentTypeKey = null;
        ExcludingContentTypeFilter.ExcludedForParentKey = null;
    }

    /// <summary>
    /// Stands in for an implementor's filter, so the tests can prove the blueprint create path honours
    /// <see cref="IContentTypeFilter.FilterAllowedForBlueprintsAsync{TItem}" />.
    /// </summary>
    private sealed class ExcludingContentTypeFilter : IContentTypeFilter
    {
        public static Guid? ExcludedContentTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the parent to exclude the content type for. When left unset, the content type is
        /// excluded everywhere, so the tests can also cover a filter that ignores the parent.
        /// </summary>
        public static Guid? ExcludedForParentKey { get; set; }

        public Task<IEnumerable<TItem>> FilterAllowedForBlueprintsAsync<TItem>(IEnumerable<TItem> contentTypes, Guid? parentKey)
            where TItem : IContentTypeComposition
            => Task.FromResult(contentTypes.Where(contentType
                => contentType.Key != ExcludedContentTypeKey
                   || (ExcludedForParentKey.HasValue && ExcludedForParentKey != parentKey)));
    }
}


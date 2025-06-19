using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests for the content blueprint editing service. Please notice that a lot of the functional tests are covered by the content
///     editing service tests, since these services share the same base implementation.
/// </summary>
public partial class ContentBlueprintEditingServiceTests : ContentEditingServiceTestsBase
{
    private IContentBlueprintContainerService ContentBlueprintContainerService => GetRequiredService<IContentBlueprintContainerService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private async Task<IContent> CreateInvariantContentBlueprint()
    {
        var contentType = CreateInvariantContentType();

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
                new PropertyValueModel { Alias = "author", Value = "The author value" }
            ]
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
            ]
        };
        return createModel;
    }

    private IEntitySlim[] GetBlueprintChildren(Guid? containerKey)
        => EntityService.GetPagedChildren(containerKey, new[] { UmbracoObjectTypes.DocumentBlueprintContainer }, UmbracoObjectTypes.DocumentBlueprint, 0, 100, out _).ToArray();
}

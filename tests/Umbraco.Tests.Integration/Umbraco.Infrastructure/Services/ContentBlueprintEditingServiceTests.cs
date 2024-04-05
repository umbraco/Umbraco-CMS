using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests for the content blueprint editing service. Please notice that a lot of the functional tests are covered by the content
///     editing service tests, since these services share the same base implementation.
/// </summary>
public partial class ContentBlueprintEditingServiceTests : ContentEditingServiceTestsBase
{
    private async Task<IContent> CreateInvariantContentBlueprint()
    {
        var contentType = CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Initial Blueprint Name",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The initial title" },
                new PropertyValueModel { Alias = "text", Value = "The initial text" },
            },
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
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" },
            },
            Variants = new[]
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Initial Blueprint English Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The initial English title" },
                    },
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Initial Blueprint Danish Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The initial Danish title" },
                    },
                },
            },
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }
}

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal abstract class BlockEditorWithReusableContentTestBase : BlockEditorElementVariationTestBase
{
    protected IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    protected IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    protected IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    protected async Task<Guid> CreateAndPublishInvariantReusableElement(Guid elementTypeKey)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementTypeKey,
                ParentKey = null,
                Properties =
                [
                    new PropertyValueModel { Alias = "invariantText", Value = "The reusable invariant text" },
                    new PropertyValueModel { Alias = "variantText", Value = "The reusable variant text" },
                ],
                Variants =
                [
                    new VariantModel { Name = "Reusable element" }
                ],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var elementKey = createResult.Result.Content!.Key;

        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        return elementKey;
    }

    protected async Task<Guid> CreateAndPublishVariantReusableElement(Guid elementTypeKey, string[] culturesToPublish)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementTypeKey,
                ParentKey = null,
                Properties =
                [
                    new PropertyValueModel { Alias = "invariantText", Value = "The reusable invariant text" },
                    new PropertyValueModel { Alias = "variantText", Value = "The reusable English text", Culture = "en-US" },
                    new PropertyValueModel { Alias = "variantText", Value = "The reusable Danish text", Culture = "da-DK" },
                ],
                Variants =
                [
                    new VariantModel { Name = "Reusable element (EN)", Culture = "en-US" },
                    new VariantModel { Name = "Reusable element (DA)", Culture = "da-DK" }
                ],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var elementKey = createResult.Result.Content!.Key;

        if (culturesToPublish.Any())
        {
            var publishResult = await ElementPublishingService.PublishAsync(
                elementKey,
                culturesToPublish.Select(culture => new CulturePublishScheduleModel { Culture = culture }).ToArray(),
                Constants.Security.SuperUserKey);
            Assert.IsTrue(publishResult.Success);
        }

        return elementKey;
    }
}

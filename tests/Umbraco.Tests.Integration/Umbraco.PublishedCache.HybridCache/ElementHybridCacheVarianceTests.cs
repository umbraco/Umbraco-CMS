// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCacheVarianceTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IPublishedElementCache PublishedElementCache => GetRequiredService<IPublishedElementCache>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    [Test]
    public async Task Published_Output_Reflects_Property_Variance_Change_Without_Republishing_Invariant_To_Variant()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);

        await CreateAdditionalLanguage("da-DK");
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementType.Key,
                Variants = [new() { Name = "Variance Render Test" }],
                Properties = [new PropertyValueModel { Alias = "title", Value = "invariant title" }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        var elementKey = createResult.Result.Content!.Key;

        await PublishCultures(elementKey, defaultCulture);

        // Routing sets this before rendering; simulate it so a culture-less read resolves like a real request.
        VariationContextAccessor.VariationContext = new VariationContext(defaultCulture);

        var beforeChange = await PublishedElementCache.GetByIdAsync(elementKey, preview: false);
        Assert.That(beforeChange, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(beforeChange!.Value<string>("title"), Is.EqualTo("invariant title"));
            Assert.That(beforeChange!.Value<string>("title", defaultCulture), Is.EqualTo("invariant title"));
            Assert.That(beforeChange!.Value<string>("title", "da-DK"), Is.EqualTo("invariant title"));
        });

        elementType.Variations = ContentVariation.Culture;
        var titleProperty = elementType.PropertyTypes.Single(p => p.Alias == "title");
        titleProperty.Variations = ContentVariation.Culture;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);

        var afterChange = await PublishedElementCache.GetByIdAsync(elementKey, preview: false);
        Assert.That(afterChange, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(afterChange!.Value<string>("title"), Is.EqualTo("invariant title"));
            Assert.That(afterChange!.Value<string>("title", defaultCulture), Is.EqualTo("invariant title"));
            Assert.That(afterChange!.Value<string>("title", "da-DK"), Is.Empty);
        });
    }

    [Test]
    public async Task Published_Output_Reflects_Property_Variance_Change_Without_Republishing_Variant_To_Invariant()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);

        await CreateAdditionalLanguage("da-DK");
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementType.Key,
                Variants = [new() { Culture = defaultCulture, Name = "Variance Render Test" }],
                Properties = [new PropertyValueModel { Alias = "title", Value = "variant title", Culture = defaultCulture }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        var elementKey = createResult.Result.Content!.Key;

        await PublishCultures(elementKey, defaultCulture);

        // Routing sets this before rendering; simulate it so a culture-less read resolves like a real request.
        VariationContextAccessor.VariationContext = new VariationContext(defaultCulture);

        var beforeChange = await PublishedElementCache.GetByIdAsync(elementKey, preview: false);
        Assert.That(beforeChange, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(beforeChange!.Value<string>("title"), Is.EqualTo("variant title"));
            Assert.That(beforeChange!.Value<string>("title", defaultCulture), Is.EqualTo("variant title"));
            Assert.That(beforeChange!.Value<string>("title", "da-DK"), Is.Empty);
        });

        elementType = await ContentTypeService.GetAsync(elementType.Key);
        elementType!.Variations = ContentVariation.Nothing;
        var titleProperty = elementType.PropertyTypes.Single(p => p.Alias == "title");
        titleProperty.Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);

        var afterChange = await PublishedElementCache.GetByIdAsync(elementKey, preview: false);
        Assert.That(afterChange, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(afterChange!.Value<string>("title"), Is.EqualTo("variant title"));
            Assert.That(afterChange!.Value<string>("title", defaultCulture), Is.EqualTo("variant title"));

            // Invariant properties ignore the requested culture and always resolve to the same value.
            Assert.That(afterChange!.Value<string>("title", "da-DK"), Is.EqualTo("variant title"));
        });
    }

    private async Task<IContentType> CreateElementType(ContentVariation variation)
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        elementType.AllowedAsRoot = true;
        elementType.Variations = variation;
        elementType.PropertyTypes.Single(p => p.Alias == "title").Variations = variation;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task CreateAdditionalLanguage(string culture)
    {
        var additionalLanguage = new LanguageBuilder().WithCultureInfo(culture).Build();
        var attempt = await LanguageService.CreateAsync(additionalLanguage, Constants.Security.SuperUserKey);
        Assert.That(attempt.Status, Is.EqualTo(LanguageOperationStatus.Success), $"Failed to create additional language '{culture}': {attempt.Exception?.Message}");
    }

    private async Task PublishCultures(Guid elementKey, params string[] cultures)
    {
        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [..cultures.Select(c => new CulturePublishScheduleModel { Culture = c, Schedule = null })],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);
    }
}

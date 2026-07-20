// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
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
internal sealed class DocumentHybridCacheVarianceTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    [Test]
    public async Task Published_Output_Reflects_Property_Variance_Change_Without_Republishing_Invariant_To_Variant()
    {
        var documentType = await CreateDocumentType(ContentVariation.Nothing);

        await CreateAdditionalLanguage("da-DK");
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var content = new ContentBuilder()
            .WithContentType(documentType)
            .WithName("Variance Render Test")
            .Build();
        content.SetValue("title", "invariant title");
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        VariationContextAccessor.VariationContext = new VariationContext(defaultCulture);

        var beforeChange = await PublishedContentCache.GetByIdAsync(content.Key, preview: false);
        Assert.That(beforeChange, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(beforeChange!.Value<string>("title"), Is.EqualTo("invariant title"));
            Assert.That(beforeChange!.Value<string>("title", defaultCulture), Is.EqualTo("invariant title"));
            Assert.That(beforeChange!.Value<string>("title", "da-DK"), Is.EqualTo("invariant title"));
        });

        documentType.Variations = ContentVariation.Culture;
        var titleProperty = documentType.PropertyTypes.Single(p => p.Alias == "title");
        titleProperty.Variations = ContentVariation.Culture;
        await ContentTypeService.UpdateAsync(documentType, Constants.Security.SuperUserKey);

        var afterChange = await PublishedContentCache.GetByIdAsync(content.Key, preview: false);
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
        var documentType = await CreateDocumentType(ContentVariation.Culture);

        await CreateAdditionalLanguage("da-DK");
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var content = new ContentBuilder()
            .WithContentType(documentType)
            .WithCultureName(defaultCulture, "Variance Render Test")
            .Build();
        content.SetValue("title", "variant title", defaultCulture);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        VariationContextAccessor.VariationContext = new VariationContext(defaultCulture);

        var beforeChange = await PublishedContentCache.GetByIdAsync(content.Key, preview: false);
        Assert.That(beforeChange, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(beforeChange!.Value<string>("title"), Is.EqualTo("variant title"));
            Assert.That(beforeChange!.Value<string>("title", defaultCulture), Is.EqualTo("variant title"));
            Assert.That(beforeChange!.Value<string>("title", "da-DK"), Is.Empty);
        });

        documentType = await ContentTypeService.GetAsync(documentType.Key);
        documentType!.Variations = ContentVariation.Nothing;
        var titleProperty = documentType.PropertyTypes.Single(p => p.Alias == "title");
        titleProperty.Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(documentType, Constants.Security.SuperUserKey);

        var afterChange = await PublishedContentCache.GetByIdAsync(content.Key, preview: false);
        Assert.That(afterChange, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(afterChange!.Value<string>("title"), Is.EqualTo("variant title"));
            Assert.That(afterChange!.Value<string>("title", defaultCulture), Is.EqualTo("variant title"));
            Assert.That(afterChange!.Value<string>("title", "da-DK"), Is.EqualTo("variant title"));
        });
    }

    private async Task<IContentType> CreateDocumentType(ContentVariation variation)
    {
        var documentType = new ContentTypeBuilder()
            .WithAlias("varianceRenderTest" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Render Test")
            .WithAllowAsRoot(true)
            .WithContentVariation(variation)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithVariations(variation)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);
        return documentType;
    }

    private async Task CreateAdditionalLanguage(string culture)
    {
        var additionalLanguage = new LanguageBuilder().WithCultureInfo(culture).Build();
        var attempt = await LanguageService.CreateAsync(additionalLanguage, Constants.Security.SuperUserKey);
        Assert.That(attempt.Status, Is.EqualTo(LanguageOperationStatus.Success), $"Failed to create additional language '{culture}': {attempt.Exception?.Message}");
    }
}

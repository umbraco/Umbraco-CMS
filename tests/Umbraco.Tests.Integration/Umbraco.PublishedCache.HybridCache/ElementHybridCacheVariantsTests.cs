using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCacheVariantsTests : UmbracoIntegrationTest
{
    private const string EnglishIsoCode = "en-US";
    private const string DanishIsoCode = "da-DK";
    private const string VariantPropertyAlias = "variantTitle";
    private const string VariantPropertyName = "Variant Title";
    private const string InvariantPropertyAlias = "invariantTitle";
    private const string InvariantPropertyName = "Invariant Title";

    private Guid _elementTypeKey;
    private Guid _variantElementKey;

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IPublishedElementCache PublishedElementCache => GetRequiredService<IPublishedElementCache>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [SetUp]
    public async Task Setup() => await CreateTestData();

    [Test]
    public async Task Can_Get_Variant_Property_Values()
    {
        // Arrange
        await PublishAllCultures(_variantElementKey);

        // Act
        IPublishedElement? element = await PublishedElementCache.GetByKeyAsync(_variantElementKey, preview: false);

        // Assert
        Assert.That(element, Is.Not.Null);
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();

        var englishValue = element!.GetProperty(VariantPropertyAlias)!.GetValue(EnglishIsoCode);
        var danishValue = element.GetProperty(VariantPropertyAlias)!.GetValue(DanishIsoCode);
        Assert.That(englishValue, Is.EqualTo("English Title"));
        Assert.That(danishValue, Is.EqualTo("Danish Title"));
    }

    [Test]
    public async Task Invariant_Property_Is_Same_Across_Cultures()
    {
        // Arrange
        await PublishAllCultures(_variantElementKey);

        // Act
        IPublishedElement? element = await PublishedElementCache.GetByKeyAsync(_variantElementKey, preview: false);

        // Assert
        Assert.That(element, Is.Not.Null);
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();

        var invariantForEnglish = element!.GetProperty(InvariantPropertyAlias)!.GetValue(EnglishIsoCode);
        var invariantForDanish = element.GetProperty(InvariantPropertyAlias)!.GetValue(DanishIsoCode);
        var invariantDefault = element.GetProperty(InvariantPropertyAlias)!.GetValue();
        Assert.That(invariantDefault, Is.EqualTo("Invariant Value"));
        Assert.That(invariantForEnglish, Is.EqualTo("Invariant Value"));
        Assert.That(invariantForDanish, Is.EqualTo("Invariant Value"));
    }

    [Test]
    public async Task Can_Update_Single_Culture_Property()
    {
        // Arrange
        await PublishAllCultures(_variantElementKey);

        // Update only the English variant property
        var updateResult = await ElementEditingService.UpdateAsync(
            _variantElementKey,
            new ElementUpdateModel
            {
                Properties =
                [
                    new PropertyValueModel { Alias = VariantPropertyAlias, Value = "Updated English", Culture = EnglishIsoCode }
                ],
                Variants =
                [
                    new VariantModel { Culture = EnglishIsoCode, Name = "Updated English Name" }
                ],
            },
            Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.True);

        // Republish English only
        await PublishCulture(_variantElementKey, EnglishIsoCode);

        // Act
        IPublishedElement? element = await PublishedElementCache.GetByKeyAsync(_variantElementKey, preview: false);

        // Assert
        Assert.That(element, Is.Not.Null);
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();

        var englishValue = element!.GetProperty(VariantPropertyAlias)!.GetValue(EnglishIsoCode);
        var danishValue = element.GetProperty(VariantPropertyAlias)!.GetValue(DanishIsoCode);
        Assert.That(englishValue, Is.EqualTo("Updated English"));
        Assert.That(danishValue, Is.EqualTo("Danish Title"));
        Assert.That(element.Cultures[EnglishIsoCode].Name, Is.EqualTo("Updated English Name"));
        Assert.That(element.Cultures[DanishIsoCode].Name, Is.EqualTo("Danish Element"));
    }

    [Test]
    public async Task Can_Publish_Single_Culture()
    {
        // Arrange — publish English only
        await PublishCulture(_variantElementKey, EnglishIsoCode);

        // Act
        IPublishedElement? element = await PublishedElementCache.GetByKeyAsync(_variantElementKey, preview: false);

        // Assert — element should be published with English culture only
        Assert.That(element, Is.Not.Null);
        Assert.That(element!.IsPublished(EnglishIsoCode), Is.True);
        Assert.That(element.IsPublished(DanishIsoCode), Is.False);
    }

    [Test]
    public async Task Draft_Has_Both_Culture_Values()
    {
        // Act — request draft (no publish needed)
        IPublishedElement? element = await PublishedElementCache.GetByKeyAsync(_variantElementKey, preview: true);

        // Assert
        Assert.That(element, Is.Not.Null);
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();

        var englishValue = element!.GetProperty(VariantPropertyAlias)!.GetValue(EnglishIsoCode);
        var danishValue = element.GetProperty(VariantPropertyAlias)!.GetValue(DanishIsoCode);
        Assert.That(englishValue, Is.EqualTo("English Title"));
        Assert.That(danishValue, Is.EqualTo("Danish Title"));
    }

    private async Task CreateTestData()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo(DanishIsoCode)
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var elementType = ContentTypeEditingBuilder.CreateContentTypeWithTwoPropertiesOneVariantAndOneInvariant(
            variantPropertyAlias: VariantPropertyAlias,
            variantPropertyName: VariantPropertyName,
            invariantAlias: InvariantPropertyAlias,
            invariantName: InvariantPropertyName,
            isElement: true);

        var contentTypeResult = await ContentTypeEditingService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        Assert.That(contentTypeResult.Success, Is.True);
        _elementTypeKey = contentTypeResult.Result.Key;

        _variantElementKey = await CreateVariantElement("English Title", "Danish Title", "Invariant Value");
    }

    private async Task<Guid> CreateVariantElement(string englishValue, string danishValue, string? invariantValue = null)
    {
        var properties = new List<PropertyValueModel>
        {
            new() { Alias = VariantPropertyAlias, Value = englishValue, Culture = EnglishIsoCode },
            new() { Alias = VariantPropertyAlias, Value = danishValue, Culture = DanishIsoCode },
        };

        if (invariantValue is not null)
        {
            properties.Add(new PropertyValueModel { Alias = InvariantPropertyAlias, Value = invariantValue });
        }

        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementTypeKey,
                Properties = properties,
                Variants =
                [
                    new VariantModel { Culture = EnglishIsoCode, Name = "English Element" },
                    new VariantModel { Culture = DanishIsoCode, Name = "Danish Element" },
                ],
            },
            Constants.Security.SuperUserKey);

        Assert.That(createResult.Success, Is.True);
        return createResult.Result.Content!.Key;
    }

    private async Task PublishAllCultures(Guid elementKey)
    {
        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [
                new CulturePublishScheduleModel { Culture = EnglishIsoCode, Schedule = null },
                new CulturePublishScheduleModel { Culture = DanishIsoCode, Schedule = null },
            ],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);
    }

    private async Task PublishCulture(Guid elementKey, string culture)
    {
        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = culture, Schedule = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);
    }
}

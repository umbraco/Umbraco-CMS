using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ElementPublishingServiceTests
{
    [Test]
    public async Task Can_Publish_Invariant()
    {
        var elementType = await SetupInvariantElementTypeAsync();
        var element = await CreateInvariantContentAsync(elementType);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = null }],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element!.PublishDate);

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.NotNull(publishedElement);
        Assert.IsTrue(publishedElement.IsPublished());
    }

    [Test]
    public async Task Can_Publish_Variant_Single_Culture()
    {
        var (langEn, langDa, langBe, elementType) = await SetupVariantElementTypeAsync();
        var element = await CreateVariantElementAsync(langEn, langDa, langBe, elementType);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.AreEqual(1, element!.PublishedCultures.Count());

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.NotNull(publishedElement);
        Assert.IsTrue(publishedElement.IsPublished(langEn.IsoCode));
        Assert.IsFalse(publishedElement.IsPublished(langDa.IsoCode));
        Assert.IsFalse(publishedElement.IsPublished(langBe.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Variant_Some_Cultures()
    {
        var (langEn, langDa, langBe, elementType) = await SetupVariantElementTypeAsync();
        var element = await CreateVariantElementAsync(langEn, langDa, langBe, elementType);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.AreEqual(2, element!.PublishedCultures.Count());

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.NotNull(publishedElement);
        Assert.IsTrue(publishedElement.IsPublished(langEn.IsoCode));
        Assert.IsTrue(publishedElement.IsPublished(langDa.IsoCode));
        Assert.IsFalse(publishedElement.IsPublished(langBe.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Variant_All_Cultures()
    {
        var (langEn, langDa, langBe, elementType) = await SetupVariantElementTypeAsync();
        var element = await CreateVariantElementAsync(langEn, langDa, langBe, elementType);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
                new() { Culture = langBe.IsoCode },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.AreEqual(3, element!.PublishedCultures.Count());

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.NotNull(publishedElement);
        Assert.IsTrue(publishedElement.IsPublished(langEn.IsoCode));
        Assert.IsTrue(publishedElement.IsPublished(langDa.IsoCode));
        Assert.IsTrue(publishedElement.IsPublished(langBe.IsoCode));
    }
}

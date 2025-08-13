using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ElementPublishingServiceTests
{
    [Test]
    public async Task Can_Unpublish_Invariant()
    {
        var elementType = await SetupInvariantElementTypeAsync();
        var element = await CreateInvariantContentAsync(elementType);

        await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);

        var unpublishAttempt = await ElementPublishingService.UnpublishAsync(
            element.Key,
            null,
            Constants.Security.SuperUserKey);

        Assert.IsTrue(unpublishAttempt.Success);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNull(element!.PublishDate);
    }

    [Test]
    public async Task Can_Unpublish_Variant_Single_Culture()
    {
        var (langEn, langDa, langBe, elementType) = await SetupVariantElementTypeAsync();
        var element = await CreateVariantElementAsync(langEn, langDa, langBe, elementType);

        await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);

        var unpublishAttempt = await ElementPublishingService.UnpublishAsync(
            element.Key,
            new HashSet<string>([langEn.IsoCode]),
            Constants.Security.SuperUserKey);

        Assert.IsTrue(unpublishAttempt.Success);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.AreEqual(0, element!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Unpublish_Variant_Some_Cultures()
    {
        var (langEn, langDa, langBe, elementType) = await SetupVariantElementTypeAsync();
        var element = await CreateVariantElementAsync(langEn, langDa, langBe, elementType);

        await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
                new() { Culture = langBe.IsoCode },
            ],
            Constants.Security.SuperUserKey);

        var unpublishAttempt = await ElementPublishingService.UnpublishAsync(
            element.Key,
            new HashSet<string>([langEn.IsoCode, langDa.IsoCode]),
            Constants.Security.SuperUserKey);

        Assert.IsTrue(unpublishAttempt.Success);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.AreEqual(1, element!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Unpublish_Variant_All_Cultures()
    {
        var (langEn, langDa, langBe, elementType) = await SetupVariantElementTypeAsync();
        var element = await CreateVariantElementAsync(langEn, langDa, langBe, elementType);

        await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
                new() { Culture = langBe.IsoCode },
            ],
            Constants.Security.SuperUserKey);

        var unpublishAttempt = await ElementPublishingService.UnpublishAsync(
            element.Key,
            new HashSet<string>([langEn.IsoCode, langDa.IsoCode, langBe.IsoCode]),
            Constants.Security.SuperUserKey);

        Assert.IsTrue(unpublishAttempt.Success);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.AreEqual(0, element!.PublishedCultures.Count());
    }
}

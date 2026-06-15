using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

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

        Assert.That(publishAttempt.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element!.PublishDate, Is.Not.Null);

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.That(publishedElement, Is.Not.Null);
        Assert.That(publishedElement.IsPublished(), Is.True);
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

        Assert.That(publishAttempt.Success, Is.True);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element!.PublishedCultures.Count(), Is.EqualTo(1));

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.That(publishedElement, Is.Not.Null);
        Assert.That(publishedElement.IsPublished(langEn.IsoCode), Is.True);
        Assert.That(publishedElement.IsPublished(langDa.IsoCode), Is.False);
        Assert.That(publishedElement.IsPublished(langBe.IsoCode), Is.False);
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

        Assert.That(publishAttempt.Success, Is.True);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element!.PublishedCultures.Count(), Is.EqualTo(2));

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.That(publishedElement, Is.Not.Null);
        Assert.That(publishedElement.IsPublished(langEn.IsoCode), Is.True);
        Assert.That(publishedElement.IsPublished(langDa.IsoCode), Is.True);
        Assert.That(publishedElement.IsPublished(langBe.IsoCode), Is.False);
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

        Assert.That(publishAttempt.Success, Is.True);
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element!.PublishedCultures.Count(), Is.EqualTo(3));

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.That(publishedElement, Is.Not.Null);
        Assert.That(publishedElement.IsPublished(langEn.IsoCode), Is.True);
        Assert.That(publishedElement.IsPublished(langDa.IsoCode), Is.True);
        Assert.That(publishedElement.IsPublished(langBe.IsoCode), Is.True);
    }

    [Test]
    public async Task Cannot_Publish_Trashed()
    {
        var elementType = await SetupInvariantElementTypeAsync();
        var element = await CreateInvariantContentAsync(elementType);

        await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = null }],
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(publishAttempt.Success, Is.False);
            Assert.That(publishAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.InTrash));
        });
    }

}

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

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

    [Test]
    public async Task Cannot_Publish_Content_With_Invalid_Value_In_Non_Default_Segment()
    {
        var elementType = await SetupSegmentVariantElementTypeAsync(ContentVariation.Segment);

        IElement element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Segment Test")
            .Build();
        element.SetValue("title", "Valid default value");
        element.SetValue("title", "Invalid seg-1 value", segment: "seg-1");
        ElementService.Save(element);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = null }],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, publishAttempt.Status);
        Assert.Contains("title", publishAttempt.Result.InvalidPropertyAliases.ToArray());

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNull(element!.PublishDate);
    }

    [Test]
    public async Task Cannot_Publish_Content_With_Invalid_Value_In_Non_Default_Segment_For_Culture()
    {
        var elementType = await SetupSegmentVariantElementTypeAsync(ContentVariation.CultureAndSegment);
        var langEn = (await LanguageService.GetAsync("en-US"))!;

        IElement element = new ElementBuilder()
            .WithContentType(elementType)
            .WithCultureName(langEn.IsoCode, "Segment Culture Test")
            .Build();
        element.SetValue("title", "Valid default value", culture: langEn.IsoCode);
        element.SetValue("title", "Invalid seg-1 value", culture: langEn.IsoCode, segment: "seg-1");
        ElementService.Save(element);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, publishAttempt.Status);
        Assert.Contains("title", publishAttempt.Result.InvalidPropertyAliases.ToArray());

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.AreEqual(0, element!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Publish_Content_With_Valid_Values_In_All_Segments()
    {
        var elementType = await SetupSegmentVariantElementTypeAsync(ContentVariation.Segment);

        IElement element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Segment Test")
            .Build();
        element.SetValue("title", "Valid default value");
        element.SetValue("title", "Valid seg-1 value", segment: "seg-1");
        element.SetValue("title", "Valid seg-2 value", segment: "seg-2");
        ElementService.Save(element);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = null }],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, publishAttempt.Status);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element!.PublishDate);
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
            Assert.IsFalse(publishAttempt.Success);
            Assert.AreEqual(ContentPublishingOperationStatus.InTrash, publishAttempt.Status);
        });
    }

}

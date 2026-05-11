using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ElementPublishingServiceTests
{
    private IRelationService RelationService => GetRequiredService<IRelationService>();

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

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.IsNull(publishedElement);
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

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.IsNull(publishedElement);
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

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.NotNull(publishedElement);
        Assert.IsFalse(publishedElement.IsPublished(langEn.IsoCode));
        Assert.IsFalse(publishedElement.IsPublished(langDa.IsoCode));
        Assert.IsTrue(publishedElement.IsPublished(langBe.IsoCode));
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

        var publishedElement = await ElementCacheService.GetByKeyAsync(element.Key, false);
        Assert.IsNull(publishedElement);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableUnpublishWhenReferencedTrue))]
    public async Task Cannot_Unpublish_Referenced_Element_When_Configured_To_Disable_When_Referenced()
    {
        var elementType = await SetupInvariantElementTypeAsync();
        var referencingElement = await CreateInvariantContentAsync(elementType);
        var referencedElement = await CreateInvariantContentAsync(elementType);

        await ElementPublishingService.PublishAsync(
            referencedElement.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);

        // Setup a relation where referencingElement references referencedElement.
        RelationService.Relate(referencingElement.Id, referencedElement.Id, Constants.Conventions.RelationTypes.RelatedDocumentAlias);

        var unpublishAttempt = await ElementPublishingService.UnpublishAsync(
            referencedElement.Key,
            null,
            Constants.Security.SuperUserKey);

        Assert.IsFalse(unpublishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CannotUnpublishWhenReferenced, unpublishAttempt.Result);

        // Verify the referencedElement is still published
        var publishedElement = await ElementCacheService.GetByKeyAsync(referencedElement.Key, false);
        Assert.IsNotNull(publishedElement);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableUnpublishWhenReferencedTrue))]
    public async Task Can_Unpublish_Referencing_Element_When_Configured_To_Disable_When_Referenced()
    {
        var elementType = await SetupInvariantElementTypeAsync();
        var referencingElement = await CreateInvariantContentAsync(elementType);
        var referencedElement = await CreateInvariantContentAsync(elementType);

        await ElementPublishingService.PublishAsync(
            referencingElement.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);

        // Setup a relation where referencingElement references referencedElement.
        RelationService.Relate(referencingElement.Id, referencedElement.Id, Constants.Conventions.RelationTypes.RelatedDocumentAlias);

        var unpublishAttempt = await ElementPublishingService.UnpublishAsync(
            referencingElement.Key,
            null,
            Constants.Security.SuperUserKey);

        Assert.IsTrue(unpublishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, unpublishAttempt.Result);

        // Verify the element is unpublished
        var publishedElement = await ElementCacheService.GetByKeyAsync(referencingElement.Key, false);
        Assert.IsNull(publishedElement);
    }

    public static void ConfigureDisableUnpublishWhenReferencedTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.DisableUnpublishWhenReferenced = true);
}

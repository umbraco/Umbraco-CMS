// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class SegmentTrackingVariationContextAccessorTests : UmbracoIntegrationTest
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private SegmentTrackingVariationContextAccessor _segmentTrackingVariationContextAccessor = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        _segmentTrackingVariationContextAccessor = new SegmentTrackingVariationContextAccessor();
        builder.AddUmbracoHybridCache();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.Services.AddUnique<IVariationContextAccessor>(_ => _segmentTrackingVariationContextAccessor);
    }

    [Test]
    public async Task Can_Track_Segments_Culture_Invariant()
    {
        var contentType = await SetupContentType(ContentVariation.Segment);
        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new() { Alias = "title", Segment = null, Value = "Default segment" },
                    new() { Alias = "title", Segment = "segment-1", Value = "Segment 1" },
                    new() { Alias = "title", Segment = "segment-2", Value = "Segment 2" },
                ],
                Variants = [
                    new() { Name = "Page" },
                ],
            },
            new HashSet<string>(),
            Constants.Security.SuperUserKey);

        Assert.IsTrue(createResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var titleValue = content.Value("title");
        Assert.AreEqual("Default segment", titleValue);

        CollectionAssert.AreEquivalent(new[] { "segment-1", "segment-2" }, _segmentTrackingVariationContextAccessor.TrackedSegments);
    }

    [Test]
    public async Task Can_Track_Segments_Culture_Variant()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var contentType = await SetupContentType(ContentVariation.CultureAndSegment);
        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new() { Alias = "title", Culture = "en-US", Segment = null, Value = "Default segment, EN" },
                    new() { Alias = "title", Culture = "en-US", Segment = "segment-1", Value = "Segment 1, EN" },
                    new() { Alias = "title", Culture = "en-US", Segment = "segment-2", Value = "Segment 2, EN" },
                    new() { Alias = "title", Culture = "da-DK", Segment = null, Value = "Default segment, DA" },
                    new() { Alias = "title", Culture = "da-DK", Segment = "segment-3", Value = "Segment 3, DA" },
                    new() { Alias = "title", Culture = "da-DK", Segment = "segment-4", Value = "Segment 4, DA" },
                ],
                Variants = [
                    new() { Name = "Page, EN", Culture = "en-US" },
                    new() { Name = "Page, DA", Culture = "da-DK" },
                ],
            },
            new HashSet<string> { "en-US", "da-DK" },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(createResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var titleValue = content.Value("title", "en-US");
        Assert.AreEqual("Default segment, EN", titleValue);

        CollectionAssert.AreEquivalent(new[] { "segment-1", "segment-2" }, _segmentTrackingVariationContextAccessor.TrackedSegments);

        titleValue = content.Value("title", "da-DK");
        Assert.AreEqual("Default segment, DA", titleValue);

        CollectionAssert.AreEquivalent(new[] { "segment-3", "segment-4" }, _segmentTrackingVariationContextAccessor.TrackedSegments);
    }

    [Test]
    public async Task Can_Track_Segments_For_Segment_Only_Property_On_Culture_Variant_Content_Type()
    {
        var contentType = await SetupContentType(ContentVariation.CultureAndSegment, ContentVariation.Segment);
        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new() { Alias = "title", Segment = null, Value = "Default segment" },
                    new() { Alias = "title", Segment = "segment-1", Value = "Segment 1" },
                    new() { Alias = "title", Segment = "segment-2", Value = "Segment 2" },
                ],
                Variants = [
                    new() { Name = "Page, EN", Culture = "en-US" },
                ],
            },
            new HashSet<string> { "en-US" },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(createResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var titleValue = content.Value("title", "en-US");
        Assert.AreEqual("Default segment", titleValue);

        CollectionAssert.AreEquivalent(new[] { "segment-1", "segment-2" }, _segmentTrackingVariationContextAccessor.TrackedSegments);
    }

    private async Task<IContentType> SetupContentType(ContentVariation variation)
        => await SetupContentType(variation, variation);

    private async Task<IContentType> SetupContentType(ContentVariation contentTypeVariation, ContentVariation propertyVariation)
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.Variations = contentTypeVariation;
        var titlePropertyType = contentType.PropertyTypes.First(pt => pt.Alias == "title");
        titlePropertyType.Variations = propertyVariation;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return contentType;
    }

    private class SegmentTrackingVariationContextAccessor : IVariationContextAccessor
    {
        public VariationContext VariationContext { get; set; } = new SegmentTrackingVariationContext();

        public string[] TrackedSegments => ((SegmentTrackingVariationContext)VariationContext).TrackedSegments;
    }

    private class SegmentTrackingVariationContext : VariationContext
    {
        public override string GetSegment(int contentId, string propertyAlias, Func<IEnumerable<string>?> availableSegments)
        {
            TrackedSegments = availableSegments()?.ToArray() ?? [];
            return base.GetSegment(contentId, propertyAlias, availableSegments);
        }

        public string[] TrackedSegments { get; private set; } = [];
    }
}

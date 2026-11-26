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

    [Test]
    public async Task Can_Render_Value_For_Segment_Selected_From_Available_Segments()
    {
        var content = await CreateSegmentVariantContent();
        TrackingContext.PreferredSegments = ["segment-not-stored", "segment-2"];

        var titleValue = content.Value("title");

        Assert.AreEqual("Segment 2", titleValue);
    }

    [Test]
    public async Task Can_Render_Value_For_Segment_Selected_From_Available_Segments_Per_Culture()
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
                    new() { Alias = "title", Culture = "da-DK", Segment = null, Value = "Default segment, DA" },
                    new() { Alias = "title", Culture = "da-DK", Segment = "segment-2", Value = "Segment 2, DA" },
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

        // Each culture stores only one of the preferred segments, so the selection must use the segments of the rendered culture.
        TrackingContext.PreferredSegments = ["segment-2", "segment-1"];

        Assert.Multiple(() =>
        {
            Assert.AreEqual("Segment 1, EN", content.Value("title", "en-US"));
            Assert.AreEqual("Segment 2, DA", content.Value("title", "da-DK"));
        });
    }

    [Test]
    public async Task Available_Segments_Are_Empty_When_Property_Has_No_Segment_Values()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.Variations = ContentVariation.CultureAndSegment;
        foreach (var propertyType in contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.CultureAndSegment;
        }

        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new() { Alias = "title", Culture = "en-US", Segment = null, Value = "Default segment, EN" },
                    new() { Alias = "title", Culture = "en-US", Segment = "segment-1", Value = "Segment 1, EN" },
                    new() { Alias = "title", Culture = "da-DK", Segment = null, Value = "Default segment, DA" },
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

        // A culture with only a default segment value.
        Assert.AreEqual("Default segment, DA", content.Value("title", "da-DK"));
        var titleCall = TrackingContext.Calls.First(call => call.PropertyAlias == "title");
        Assert.That(titleCall.AvailableSegments, Is.Not.Null.And.Empty);

        // A property with no stored values at all.
        TrackingContext.Calls.Clear();
        Assert.IsFalse(content.HasValue("bodyText", "en-US"));
        var bodyTextCall = TrackingContext.Calls.First(call => call.PropertyAlias == "bodyText");
        Assert.That(bodyTextCall.AvailableSegments, Is.Not.Null.And.Empty);
    }

    [Test]
    public async Task Renders_Default_Segment_Value_When_Selected_Segment_Has_No_Value()
    {
        var content = await CreateSegmentVariantContent();
        TrackingContext.ForcedSegment = "segment-not-stored";

        var titleValue = content.Value("title");

        Assert.AreEqual("Default segment", titleValue);
        CollectionAssert.AreEquivalent(new[] { "segment-1", "segment-2" }, TrackingContext.Calls.First().AvailableSegments);
    }

    [Test]
    public async Task Available_Segments_Are_Null_When_Segment_Is_Resolved_Through_Value_Fallback()
    {
        var content = await CreateSegmentVariantContent();
        TrackingContext.ForcedSegment = "segment-not-stored";

        var titleValue = content.Value("title");
        Assert.AreEqual("Default segment", titleValue);

        var titleCalls = TrackingContext.Calls.Where(call => call.PropertyAlias == "title").ToArray();
        Assert.AreEqual(2, titleCalls.Length);
        Assert.IsNotNull(titleCalls[0].AvailableSegments);
        Assert.IsNull(titleCalls[1].AvailableSegments);
    }

    [Test]
    public async Task Explicit_Segment_Does_Not_Resolve_Segment_From_Variation_Context()
    {
        var content = await CreateSegmentVariantContent();
        TrackingContext.ForcedSegment = "segment-2";

        var titleValue = content.Value("title", segment: "segment-1");

        Assert.AreEqual("Segment 1", titleValue);
        Assert.IsEmpty(TrackingContext.Calls);
    }

    [Test]
    public async Task Can_Get_Source_Value_For_Segment_Selected_From_Available_Segments()
    {
        var content = await CreateSegmentVariantContent();
        TrackingContext.PreferredSegments = ["segment-2"];

        var sourceValue = content.GetProperty("title")!.GetSourceValue();

        Assert.AreEqual("Segment 2", sourceValue);
        CollectionAssert.AreEquivalent(new[] { "segment-1", "segment-2" }, TrackingContext.TrackedSegments);
    }

    [Test]
    public async Task Can_Get_Delivery_Api_Value_For_Segment_Selected_From_Available_Segments()
    {
        var content = await CreateSegmentVariantContent();
        TrackingContext.PreferredSegments = ["segment-2"];

        var deliveryApiValue = content.GetProperty("title")!.GetDeliveryApiValue(false);

        Assert.AreEqual("Segment 2", deliveryApiValue);
        CollectionAssert.AreEquivalent(new[] { "segment-1", "segment-2" }, TrackingContext.TrackedSegments);
    }

    [Test]
    public async Task Available_Segments_Cannot_Be_Modified_By_Variation_Context()
    {
        var content = await CreateSegmentVariantContent();

        content.Value("title");

        var availableSegments = TrackingContext.LastAvailableSegments;
        Assert.IsInstanceOf<IList<string>>(availableSegments);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)availableSegments!)[0] = "modified");
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

    private async Task<IPublishedContent> CreateSegmentVariantContent()
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
        return content;
    }

    private SegmentTrackingVariationContext TrackingContext
        => (SegmentTrackingVariationContext)_segmentTrackingVariationContextAccessor.VariationContext;

    private class SegmentTrackingVariationContextAccessor : IVariationContextAccessor
    {
        public VariationContext VariationContext { get; set; } = new SegmentTrackingVariationContext();

        public string[] TrackedSegments => ((SegmentTrackingVariationContext)VariationContext).TrackedSegments;
    }

    private record TrackedCall(string PropertyAlias, string[]? AvailableSegments);

    private class SegmentTrackingVariationContext : VariationContext
    {
        public override string GetSegment(int contentId, string propertyAlias, Func<IEnumerable<string>?> availableSegments)
        {
            LastAvailableSegments = availableSegments();
            var segments = LastAvailableSegments?.ToArray();
            TrackedSegments = segments ?? [];
            Calls.Add(new TrackedCall(propertyAlias, segments));

            if (ForcedSegment is not null)
            {
                return ForcedSegment;
            }

            return PreferredSegments.FirstOrDefault(preferred => segments?.Contains(preferred) is true)
                ?? base.GetSegment(contentId, propertyAlias, availableSegments);
        }

        public string[] TrackedSegments { get; private set; } = [];

        public IEnumerable<string>? LastAvailableSegments { get; private set; }

        public List<TrackedCall> Calls { get; } = [];

        // Segments the request qualifies for, in priority order; the first one stored for the property is used.
        public string[] PreferredSegments { get; set; } = [];

        // Returned regardless of the available segments, to resolve a segment the property has no value for.
        public string? ForcedSegment { get; set; }
    }
}

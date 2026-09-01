using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

public class MediaPropertyCacheLevelTests : PropertyCacheLevelTestsBase
{
    private static readonly Guid _mediaKey = new("B4507763-591F-4E32-AD14-7EA67C6AE0D3");

    private IMediaCacheService MediaCacheService => GetRequiredService<IMediaCacheService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    [SetUp]
    public async Task SetUpTest()
    {
        PropertyValueLevelDetectionTestsConverter.Reset();

        var mediaTypeCreateModel = MediaTypeEditingBuilder.CreateMediaTypeWithOneProperty(propertyAlias: "title");
        var mediaTypeAttempt = await MediaTypeEditingService.CreateAsync(mediaTypeCreateModel, Constants.Security.SuperUserKey);
        Assert.That(mediaTypeAttempt.Success, Is.True);

        var mediaCreateModel = MediaEditingBuilder.CreateMediaWithAProperty(mediaTypeAttempt.Result.Key, "My Media", null, propertyAlias: "title", propertyValue: "The title");
        mediaCreateModel.Key = _mediaKey;
        var mediaAttempt = await MediaEditingService.CreateAsync(mediaCreateModel, Constants.Security.SuperUserKey);
        Assert.That(mediaAttempt.Success, Is.True);
    }

    [TestCase(PropertyCacheLevel.None, 1, 10)]
    [TestCase(PropertyCacheLevel.Element, 1, 1)]
    [TestCase(PropertyCacheLevel.Elements, 1, 1)]
    public async Task Property_Value_Conversion_Respects_Property_Cache_Level(PropertyCacheLevel cacheLevel, int expectedSourceConverts, int expectedInterConverts)
    {
        PropertyValueLevelDetectionTestsConverter.SetCacheLevel(cacheLevel);

        var publishedContent1 = await MediaCacheService.GetByKeyAsync(_mediaKey);
        Assert.That(publishedContent1, Is.Not.Null);

        var publishedContent2 = await MediaCacheService.GetByKeyAsync(_mediaKey);
        Assert.That(publishedContent2, Is.Not.Null);

        Assert.That(publishedContent2, Is.SameAs(publishedContent1));

        var titleValue1 = publishedContent1.Value<string>("title");
        Assert.That(titleValue1, Is.Not.Null);

        var titleValue2 = publishedContent2.Value<string>("title");
        Assert.That(titleValue2, Is.Not.Null);

        Assert.That(titleValue2, Is.EqualTo(titleValue1));

        // fetch title values 10 times in total, 5 times from each published content instance
        titleValue1 = publishedContent1.Value<string>("title");
        titleValue1 = publishedContent1.Value<string>("title");
        titleValue1 = publishedContent1.Value<string>("title");
        titleValue1 = publishedContent1.Value<string>("title");

        titleValue2 = publishedContent2.Value<string>("title");
        titleValue2 = publishedContent2.Value<string>("title");
        titleValue2 = publishedContent2.Value<string>("title");
        titleValue2 = publishedContent2.Value<string>("title");

        Assert.That(PropertyValueLevelDetectionTestsConverter.SourceConverts, Is.EqualTo(expectedSourceConverts));
        Assert.That(PropertyValueLevelDetectionTestsConverter.InterConverts, Is.EqualTo(expectedInterConverts));
    }

    [TestCase(PropertyCacheLevel.None)]
    [TestCase(PropertyCacheLevel.Element)]
    [TestCase(PropertyCacheLevel.Elements)]
    public async Task Property_Value_Conversion_Is_Triggered_After_Cache_Refresh(PropertyCacheLevel cacheLevel)
    {
        PropertyValueLevelDetectionTestsConverter.SetCacheLevel(cacheLevel);

        var publishedContent1 = await MediaCacheService.GetByKeyAsync(_mediaKey);
        Assert.That(publishedContent1, Is.Not.Null);

        var titleValue1 = publishedContent1.Value<string>("title");
        Assert.That(titleValue1, Is.EqualTo("The title"));

        // save the media to trigger a cache refresh for the media
        var mediaAttempt = await MediaEditingService.UpdateAsync(
            _mediaKey,
            new ()
            {
                Properties = [new () { Alias = "title", Value = "New title" }],
                Variants = [new() { Name = publishedContent1.Name }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(mediaAttempt.Success, Is.True);

        var publishedContent2 = await MediaCacheService.GetByKeyAsync(_mediaKey);
        Assert.That(publishedContent2, Is.Not.Null);

        Assert.That(publishedContent2, Is.Not.SameAs(publishedContent1));

        var titleValue2 = publishedContent2.Value<string>("title");
        Assert.That(titleValue2, Is.EqualTo("New title"));

        // expect conversions for each published content instance, due to the cache refresh
        Assert.That(PropertyValueLevelDetectionTestsConverter.SourceConverts, Is.EqualTo(2));
        Assert.That(PropertyValueLevelDetectionTestsConverter.InterConverts, Is.EqualTo(2));
    }
}

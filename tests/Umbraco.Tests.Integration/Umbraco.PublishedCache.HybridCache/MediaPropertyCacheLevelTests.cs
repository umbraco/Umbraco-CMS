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
        Assert.IsTrue(mediaTypeAttempt.Success);

        var mediaCreateModel = MediaEditingBuilder.CreateMediaWithAProperty(mediaTypeAttempt.Result.Key, "My Media", null, propertyAlias: "title", propertyValue: "The title");
        mediaCreateModel.Key = _mediaKey;
        var mediaAttempt = await MediaEditingService.CreateAsync(mediaCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(mediaAttempt.Success);
    }

    [TestCase(PropertyCacheLevel.None, 1, 10)]
    [TestCase(PropertyCacheLevel.Element, 1, 1)]
    [TestCase(PropertyCacheLevel.Elements, 1, 1)]
    public async Task Property_Value_Conversion_Respects_Property_Cache_Level(PropertyCacheLevel cacheLevel, int expectedSourceConverts, int expectedInterConverts)
    {
        PropertyValueLevelDetectionTestsConverter.SetCacheLevel(cacheLevel);

        var publishedContent1 = await MediaCacheService.GetByKeyAsync(_mediaKey);
        Assert.IsNotNull(publishedContent1);

        var publishedContent2 = await MediaCacheService.GetByKeyAsync(_mediaKey);
        Assert.IsNotNull(publishedContent2);

        Assert.AreSame(publishedContent1,  publishedContent2);

        var titleValue1 = publishedContent1.Value<string>("title");
        Assert.IsNotNull(titleValue1);

        var titleValue2 = publishedContent2.Value<string>("title");
        Assert.IsNotNull(titleValue2);

        Assert.AreEqual(titleValue1,  titleValue2);

        // fetch title values 10 times in total, 5 times from each published content instance
        titleValue1 = publishedContent1.Value<string>("title");
        titleValue1 = publishedContent1.Value<string>("title");
        titleValue1 = publishedContent1.Value<string>("title");
        titleValue1 = publishedContent1.Value<string>("title");

        titleValue2 = publishedContent2.Value<string>("title");
        titleValue2 = publishedContent2.Value<string>("title");
        titleValue2 = publishedContent2.Value<string>("title");
        titleValue2 = publishedContent2.Value<string>("title");

        Assert.AreEqual(expectedSourceConverts, PropertyValueLevelDetectionTestsConverter.SourceConverts);
        Assert.AreEqual(expectedInterConverts, PropertyValueLevelDetectionTestsConverter.InterConverts);
    }

    [TestCase(PropertyCacheLevel.None)]
    [TestCase(PropertyCacheLevel.Element)]
    [TestCase(PropertyCacheLevel.Elements)]
    public async Task Property_Value_Conversion_Is_Triggered_After_Cache_Refresh(PropertyCacheLevel cacheLevel)
    {
        PropertyValueLevelDetectionTestsConverter.SetCacheLevel(cacheLevel);

        var publishedContent1 = await MediaCacheService.GetByKeyAsync(_mediaKey);
        Assert.IsNotNull(publishedContent1);

        var titleValue1 = publishedContent1.Value<string>("title");
        Assert.AreEqual("The title", titleValue1);

        // save the media to trigger a cache refresh for the media
        var mediaAttempt = await MediaEditingService.UpdateAsync(
            _mediaKey,
            new ()
            {
                Properties = [new () { Alias = "title", Value = "New title" }],
                Variants = [new() { Name = publishedContent1.Name }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(mediaAttempt.Success);

        var publishedContent2 = await MediaCacheService.GetByKeyAsync(_mediaKey);
        Assert.IsNotNull(publishedContent2);

        Assert.AreNotSame(publishedContent1,  publishedContent2);

        var titleValue2 = publishedContent2.Value<string>("title");
        Assert.AreEqual("New title", titleValue2);

        // expect conversions for each published content instance, due to the cache refresh
        Assert.AreEqual(2, PropertyValueLevelDetectionTestsConverter.SourceConverts);
        Assert.AreEqual(2, PropertyValueLevelDetectionTestsConverter.InterConverts);
    }
}

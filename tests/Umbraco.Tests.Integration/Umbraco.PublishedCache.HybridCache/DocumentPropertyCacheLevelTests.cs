using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

public class DocumentPropertyCacheLevelTests : PropertyCacheLevelTestsBase
{
    private static readonly Guid _documentKey = new("9A526E75-DE41-4A81-8883-3E63F11A388D");

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    [SetUp]
    public async Task SetUpTest()
    {
        PropertyValueLevelDetectionTestsConverter.Reset();

        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType();
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(contentTypeAttempt.Success);

        var contentCreateModel = ContentEditingBuilder.CreateSimpleContent(contentTypeAttempt.Result.Key);
        contentCreateModel.Key = _documentKey;
        var contentAttempt = await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(contentAttempt.Success);

        await PublishPage();
    }

    [TestCase(PropertyCacheLevel.None, false, 1, 10)]
    [TestCase(PropertyCacheLevel.None, true, 2, 10)]
    [TestCase(PropertyCacheLevel.Element, false, 1, 1)]
    [TestCase(PropertyCacheLevel.Element, true, 2, 2)]
    [TestCase(PropertyCacheLevel.Elements, false, 1, 1)]
    [TestCase(PropertyCacheLevel.Elements, true, 1, 1)]
    public async Task Property_Value_Conversion_Respects_Property_Cache_Level(PropertyCacheLevel cacheLevel, bool preview, int expectedSourceConverts, int expectedInterConverts)
    {
        PropertyValueLevelDetectionTestsConverter.SetCacheLevel(cacheLevel);

        var publishedContent1 = await DocumentCacheService.GetByKeyAsync(_documentKey, preview);
        Assert.IsNotNull(publishedContent1);

        var publishedContent2 = await DocumentCacheService.GetByKeyAsync(_documentKey, preview);
        Assert.IsNotNull(publishedContent2);

        if (preview)
        {
            Assert.AreNotSame(publishedContent1,  publishedContent2);
        }
        else
        {
            Assert.AreSame(publishedContent1,  publishedContent2);
        }

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

    [TestCase(PropertyCacheLevel.None, false)]
    [TestCase(PropertyCacheLevel.None, true)]
    [TestCase(PropertyCacheLevel.Element, false)]
    [TestCase(PropertyCacheLevel.Element, true)]
    [TestCase(PropertyCacheLevel.Elements, false)]
    [TestCase(PropertyCacheLevel.Elements, true)]
    public async Task Property_Value_Conversion_Is_Triggered_After_Cache_Refresh(PropertyCacheLevel cacheLevel, bool preview)
    {
        PropertyValueLevelDetectionTestsConverter.SetCacheLevel(cacheLevel);

        var publishedContent1 = await DocumentCacheService.GetByKeyAsync(_documentKey, preview);
        Assert.IsNotNull(publishedContent1);

        var titleValue1 = publishedContent1.Value<string>("title");
        Assert.IsNotNull(titleValue1);

        // re-publish the page to trigger a cache refresh for the page
        await PublishPage();

        var publishedContent2 = await DocumentCacheService.GetByKeyAsync(_documentKey, preview);
        Assert.IsNotNull(publishedContent2);

        Assert.AreNotSame(publishedContent1,  publishedContent2);

        var titleValue2 = publishedContent2.Value<string>("title");
        Assert.IsNotNull(titleValue2);

        Assert.AreEqual(titleValue1,  titleValue2);

        // expect conversions for each published content instance, due to the cache refresh
        Assert.AreEqual(2, PropertyValueLevelDetectionTestsConverter.SourceConverts);
        Assert.AreEqual(2, PropertyValueLevelDetectionTestsConverter.InterConverts);
    }

    private async Task PublishPage()
    {
        var publishAttempt = await ContentPublishingService.PublishAsync(
            _documentKey,
            [new() { Culture = "*", }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);
    }
}

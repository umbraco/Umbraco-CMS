using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheAncestryVariantTests : UmbracoIntegrationTest
{
    private string _englishIsoCode = "en-US";
    private string _danishIsoCode = "da-DK";
    private string _variantTitleAlias = "variantTitle";
    private string _variantTitleName = "Variant Title";
    private string _invariantTitleAlias = "invariantTitle";
    private string _invariantTitleName = "Invariant Title";

    private IContent rootContent;
    private IContent childNode;
    private IContent grandChildNode;

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [SetUp]
    public async Task Setup() => await CreateTestData();

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task AllCulturesUnpublished(bool preview)
    {
        // Publish branch in all cultures
        var publishAttempt = await ContentPublishingService.PublishBranchAsync(rootContent.Key, [_englishIsoCode, _danishIsoCode], true, Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);
        Assert.That(publishAttempt.Result.SucceededItems.Count(), Is.EqualTo(3));

        // Unpublish all cultures in child
        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(childNode.Key, new HashSet<string>([_englishIsoCode, _danishIsoCode]), Constants.Security.SuperUserKey);
        Assert.IsTrue(unpublishAttempt.Success);

        var publishedGrandChild = await PublishedContentCache.GetByIdAsync(grandChildNode.Key, preview);

        if (preview)
        {
            CacheTestsHelper.AssertPage(grandChildNode, publishedGrandChild, false);
        }
        else
        {
            Assert.IsNull(publishedGrandChild);
        }
    }

    [Test]
    public async Task SingleCultureUnpublished()
    {
        var publishAttempt = await ContentPublishingService.PublishBranchAsync(rootContent.Key, [_englishIsoCode, _danishIsoCode], true, Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);
        Assert.That(publishAttempt.Result.SucceededItems.Count(), Is.EqualTo(3));

        // Unpublish only english culture
        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(childNode.Key, new HashSet<string> { _englishIsoCode }, Constants.Security.SuperUserKey);
        Assert.IsTrue(unpublishAttempt.Success);

        var publishedGrandChild = await PublishedContentCache.GetByIdAsync(grandChildNode.Key, false);
        CacheTestsHelper.AssertPage(grandChildNode, publishedGrandChild, false);
        Assert.IsTrue(publishedGrandChild!.IsPublished(_danishIsoCode));
    }

    [Test]
    public async Task SingleCulturePublished()
    {
        var publishAttempt = await ContentPublishingService.PublishAsync(
            rootContent.Key,
            new List<CulturePublishScheduleModel>
            {
                new() { Culture = _danishIsoCode },
                new() { Culture = _englishIsoCode },
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        // Publish only single culture.
        var publishChildAttempt = await ContentPublishingService.PublishAsync(
            childNode.Key,
            new List<CulturePublishScheduleModel>
            {
                new() { Culture = _danishIsoCode },
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishChildAttempt.Success);

        var publishGrandChildAttempt = await ContentPublishingService.PublishAsync(
            grandChildNode.Key,
            new List<CulturePublishScheduleModel>
            {
                new() { Culture = _danishIsoCode },
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishGrandChildAttempt.Success);

        var publishedGrandChild = await PublishedContentCache.GetByIdAsync(grandChildNode.Key, false);

        CacheTestsHelper.AssertPage(grandChildNode, publishedGrandChild, false);
        Assert.IsTrue(publishedGrandChild!.IsPublished(_danishIsoCode));
        Assert.IsFalse(publishedGrandChild.IsPublished(_englishIsoCode));
    }

    private async Task CreateTestData()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo(_danishIsoCode)
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateContentTypeWithTwoPropertiesOneVariantAndOneInvariant(
            "cultureVariationTest", "Culture Variation Test", _variantTitleAlias, _variantTitleName,
            _invariantTitleAlias, _invariantTitleName);
        contentTypeCreateModel.AllowedAsRoot = true;
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, Constants.Security.SuperUserKey);
        if (contentTypeAttempt.Success is false)
        {
            throw new Exception("Failed to create content type");
        }

        var contentType = contentTypeAttempt.Result!;
        var updateModel = ContentTypeUpdateHelper.CreateContentTypeUpdateModel(contentType);
        updateModel.AllowedContentTypes = [new ContentTypeSort { Alias = contentType.Alias, Key = contentType.Key, SortOrder = 0 }];
        var updateAttempt = await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);
        if (updateAttempt.Success is false)
        {
            throw new Exception("Failed to update content type");
        }

        var contentCreateModel = ContentEditingBuilder.CreateContentWithTwoVariantProperties(
            contentTypeAttempt.Result.Key,
            _danishIsoCode,
            _englishIsoCode,
            _variantTitleAlias,
            _variantTitleName);

        var rootResult = await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(rootResult.Success);
        rootContent = rootResult.Result.Content!;

        contentCreateModel.ParentKey = rootContent.Key;
        contentCreateModel.Key = Guid.NewGuid();
        var childResult = await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(childResult.Success);
        childNode = childResult.Result.Content!;

        contentCreateModel.ParentKey = childNode.Key;
        contentCreateModel.Key = Guid.NewGuid();
        var grandChildResult = await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(grandChildResult.Success);
        grandChildNode = grandChildResult.Result.Content!;
    }
}

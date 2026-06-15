using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheVariantsTests : UmbracoIntegrationTest
{
    private string _englishIsoCode = "en-US";
    private string _danishIsoCode = "da-DK";
    private string _variantTitleAlias = "variantTitle";
    private string _variantTitleName = "Variant Title";
    private string _invariantTitleAlias = "invariantTitle";
    private string _invariantTitleName = "Invariant Title";

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IContent VariantPage { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [SetUp]
    public async Task Setup() => await CreateTestData();

    [Test]
    public async Task Can_Set_Invariant_Title()
    {
        // Arrange
        await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);
        var updatedInvariantTitle = "Updated Invariant Title";
        var updatedVariantTitle = "Updated Variant Title";

        var updateModel = new ContentUpdateModel
        {
            Properties = [
                new PropertyValueModel { Alias = _invariantTitleAlias, Value = updatedInvariantTitle },
                new PropertyValueModel { Alias = _variantTitleAlias, Value = updatedVariantTitle, Culture = _englishIsoCode },
                new PropertyValueModel { Alias = _variantTitleAlias, Value = updatedVariantTitle, Culture = _danishIsoCode }
            ],
            Variants =
            [
                new VariantModel { Culture = _englishIsoCode, Name = "Updated English Name" },
                new VariantModel { Culture = _danishIsoCode, Name = "Updated Danish Name" }
            ],
        };

        var result =
            await ContentEditingService.UpdateAsync(VariantPage.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.That(textPage.Value(_invariantTitleAlias, string.Empty, string.Empty), Is.EqualTo(updatedInvariantTitle));
        Assert.That(textPage.Value(_variantTitleAlias, _englishIsoCode), Is.EqualTo(updatedVariantTitle));
        Assert.That(textPage.Value(_variantTitleAlias, _danishIsoCode), Is.EqualTo(updatedVariantTitle));
    }

    [Test]
    public async Task Can_Set_Invariant_Title_On_One_Culture()
    {
        // Arrange
        await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);
        var updatedInvariantTitle = "Updated Invariant Title";
        var updatedVariantTitle = "Updated Invariant Title";

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = _invariantTitleAlias, Value = updatedInvariantTitle },
                new PropertyValueModel { Alias = _variantTitleAlias, Value = updatedVariantTitle, Culture = _englishIsoCode }
            ],
            Variants =
            [
                new VariantModel { Culture = _englishIsoCode, Name = "Updated English Name" }
            ],
        };

        var result =
            await ContentEditingService.UpdateAsync(VariantPage.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.That(textPage.Value(_invariantTitleAlias, string.Empty, string.Empty), Is.EqualTo(updatedInvariantTitle));
        Assert.That(textPage.Value(_variantTitleAlias, _englishIsoCode), Is.EqualTo(updatedVariantTitle));
        Assert.That(textPage.Value(_variantTitleAlias, _danishIsoCode), Is.EqualTo(_variantTitleName));
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public async Task Can_Get_Draft_For_Unpublished_Culture(string cultureToUnpublish)
    {
        // Arrange
        var publishAttempt = await ContentPublishingService.PublishBranchAsync(VariantPage.Key, [_englishIsoCode, _danishIsoCode], PublishBranchFilter.All, Constants.Security.SuperUserKey, false);
        Assert.That(publishAttempt.Success, Is.True);
        Assert.That(publishAttempt.Result.SucceededItems.Count(), Is.EqualTo(1));

        var publishedPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, false);
        Assert.That(publishedPage, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(publishedPage.IsPublished(_englishIsoCode), Is.True);
            Assert.That(publishedPage.IsPublished(_danishIsoCode), Is.True);

            Assert.That(publishedPage.Cultures, Has.Count.EqualTo(2));
            Assert.That(publishedPage.Cultures.Keys, Is.EqualTo(new[] { _englishIsoCode, _danishIsoCode }).AsCollection);
            Assert.That(publishedPage.Cultures.Values.Select(v => v.Name), Is.EqualTo(new[] { _englishIsoCode, _danishIsoCode }).AsCollection);

            Assert.That(publishedPage.Value<string>(_variantTitleAlias, _englishIsoCode), Is.EqualTo(_variantTitleName));
            Assert.That(publishedPage.Value<string>(_variantTitleAlias, _danishIsoCode), Is.EqualTo(_variantTitleName));
        });

        var draftPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);
        Assert.That(draftPage, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(draftPage.IsPublished(_englishIsoCode), Is.True);
            Assert.That(draftPage.IsPublished(_danishIsoCode), Is.True);

            Assert.That(draftPage.Cultures, Has.Count.EqualTo(2));
            Assert.That(draftPage.Cultures.Keys, Is.EqualTo(new[] { _englishIsoCode, _danishIsoCode }).AsCollection);
            Assert.That(draftPage.Cultures.Values.Select(v => v.Name), Is.EqualTo(new[] { _englishIsoCode, _danishIsoCode }).AsCollection);

            Assert.That(draftPage.Value<string>(_variantTitleAlias, _englishIsoCode), Is.EqualTo(_variantTitleName));
            Assert.That(draftPage.Value<string>(_variantTitleAlias, _danishIsoCode), Is.EqualTo(_variantTitleName));
        });

        // Act
        var unpublishAttempt = await ContentPublishingService.UnpublishAsync(VariantPage.Key, new HashSet<string> { cultureToUnpublish }, Constants.Security.SuperUserKey);
        Assert.That(unpublishAttempt.Success, Is.True);

        // Assert
        var expectedPublishedCulture = cultureToUnpublish == _danishIsoCode ? _englishIsoCode : _danishIsoCode;

        publishedPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, false);
        Assert.That(publishedPage, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(publishedPage.IsPublished(expectedPublishedCulture), Is.True);
            Assert.That(publishedPage.IsPublished(cultureToUnpublish), Is.False);

            Assert.That(publishedPage.Cultures, Has.Count.EqualTo(1));
            Assert.That(publishedPage.Cultures.Single().Key, Is.EqualTo(expectedPublishedCulture));
            Assert.That(publishedPage.Cultures.Single().Value.Name, Is.EqualTo(expectedPublishedCulture));

            Assert.That(publishedPage.Value<string>(_variantTitleAlias, expectedPublishedCulture), Is.EqualTo(_variantTitleName));
            Assert.That(publishedPage.Value<string>(_variantTitleAlias, cultureToUnpublish), Is.Empty);
        });

        draftPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);
        Assert.That(draftPage, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(draftPage.IsPublished(expectedPublishedCulture), Is.True);
            Assert.That(draftPage.IsPublished(cultureToUnpublish), Is.False);

            Assert.That(draftPage.Cultures, Has.Count.EqualTo(2));
            Assert.That(draftPage.Cultures.Keys, Is.EqualTo(new[] { _englishIsoCode, _danishIsoCode }).AsCollection);
            Assert.That(draftPage.Cultures.Values.Select(v => v.Name), Is.EqualTo(new[] { _englishIsoCode, _danishIsoCode }).AsCollection);

            Assert.That(draftPage.Value<string>(_variantTitleAlias, _englishIsoCode), Is.EqualTo(_variantTitleName));
            Assert.That(draftPage.Value<string>(_variantTitleAlias, _danishIsoCode), Is.EqualTo(_variantTitleName));
        });
    }

    private async Task CreateTestData()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo(_danishIsoCode)
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var contentType = ContentTypeEditingBuilder.CreateContentTypeWithTwoPropertiesOneVariantAndOneInvariant(
            "cultureVariationTest",
            "Culture Variation Test",
            _variantTitleAlias,
            _variantTitleName,
            _invariantTitleAlias,
            _invariantTitleName);
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        if (!contentTypeAttempt.Success)
        {
            throw new Exception("Failed to create content type");
        }

        var rootContentCreateModel =
            ContentEditingBuilder.CreateContentWithTwoVariantProperties(
                contentTypeAttempt.Result.Key,
                "en-US",
                "da-DK",
                _variantTitleAlias,
                _variantTitleName);
        var result = await ContentEditingService.CreateAsync(rootContentCreateModel, Constants.Security.SuperUserKey);
        VariantPage = result.Result.Content;
    }
}

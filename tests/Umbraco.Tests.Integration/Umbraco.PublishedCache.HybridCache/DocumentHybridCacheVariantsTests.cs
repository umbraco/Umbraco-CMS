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
        Assert.IsTrue(result.Success);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(updatedInvariantTitle, textPage.Value(_invariantTitleAlias, string.Empty, string.Empty));
        Assert.AreEqual(updatedVariantTitle, textPage.Value(_variantTitleAlias, _englishIsoCode));
        Assert.AreEqual(updatedVariantTitle, textPage.Value(_variantTitleAlias, _danishIsoCode));
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
        Assert.IsTrue(result.Success);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(updatedInvariantTitle, textPage.Value(_invariantTitleAlias, string.Empty, string.Empty));
        Assert.AreEqual(updatedVariantTitle, textPage.Value(_variantTitleAlias, _englishIsoCode));
        Assert.AreEqual(_variantTitleName, textPage.Value(_variantTitleAlias, _danishIsoCode));
    }

    private async Task CreateTestData()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo(_danishIsoCode)
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var contentType = ContentTypeEditingBuilder.CreateContentTypeWithTwoPropertiesOneVariantAndOneInvariant(
            "cultureVariationTest", "Culture Variation Test", _variantTitleAlias, _variantTitleName,
            _invariantTitleAlias, _invariantTitleName);
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        if (!contentTypeAttempt.Success)
        {
            throw new Exception("Failed to create content type");
        }

        var rootContentCreateModel =
            ContentEditingBuilder.CreateContentWithTwoVariantProperties(contentTypeAttempt.Result.Key, "en-US", "da-DK",
                _variantTitleAlias, _variantTitleName);
        var result = await ContentEditingService.CreateAsync(rootContentCreateModel, Constants.Security.SuperUserKey);
        VariantPage = result.Result.Content;
    }
}

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class HybridCachingDocumentVariantsTests : UmbracoIntegrationTest
{
    private string _englishIsoCode = "en-US";
    private string _danishIsoCode = "da-DK";
    private string _variantTitleAlias = "variantTitle";
    private string _variantTitleName = "Variant Title";
    private string _invariantTitleAlias = "invariantTitle";
    private string _invariantTitleName = "Invariant Title";

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContent VariantPage { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

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
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = _invariantTitleAlias, Value = updatedInvariantTitle }
            },
            Variants = new []
            {
                new VariantModel
                {
                    Culture = _englishIsoCode,
                    Name = "Updated English Name",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = _variantTitleAlias, Value = updatedVariantTitle }
                    }
                },
                new VariantModel
                {
                    Culture = _danishIsoCode,
                    Name = "Updated Danish Name",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = _variantTitleAlias, Value = updatedVariantTitle }
                    },
                },
            },
        };

        var result = await ContentEditingService.UpdateAsync(VariantPage.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(updatedInvariantTitle, textPage.Value(_invariantTitleAlias, "", ""));
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
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = _invariantTitleAlias, Value = updatedInvariantTitle }
            },
            Variants = new []
            {
                new VariantModel
                {
                    Culture = _englishIsoCode,
                    Name = "Updated English Name",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = _variantTitleAlias, Value = updatedVariantTitle }
                    }
                },
            },
        };

        var result = await ContentEditingService.UpdateAsync(VariantPage.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(VariantPage.Id, true);

        // Assert
        using var contextReference = UmbracoContextFactory.EnsureUmbracoContext();
        Assert.AreEqual(updatedInvariantTitle, textPage.Value(_invariantTitleAlias, "", ""));
        Assert.AreEqual(updatedVariantTitle, textPage.Value(_variantTitleAlias, _englishIsoCode));
        Assert.AreEqual(_variantTitleName, textPage.Value(_variantTitleAlias, _danishIsoCode));
    }


    private async Task CreateTestData()
    {
        // NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.
        var language = new LanguageBuilder()
            .WithCultureInfo(_danishIsoCode)
            .Build();

        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("cultureVariationTest")
            .WithName("Culture Variation Test")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias(_variantTitleAlias)
            .WithName(_variantTitleName)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .AddPropertyType()
            .WithAlias(_invariantTitleAlias)
            .WithName(_invariantTitleName)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);
        var rootContentCreateModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            Variants = new[]
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "English Page",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = _variantTitleAlias, Value = _variantTitleName }
                    },
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Danish Page",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = _variantTitleAlias, Value = _variantTitleName }
                    },
                },
            },
        };

        var result = await ContentEditingService.CreateAsync(rootContentCreateModel, Constants.Security.SuperUserKey);
        VariantPage = result.Result.Content;
    }
}

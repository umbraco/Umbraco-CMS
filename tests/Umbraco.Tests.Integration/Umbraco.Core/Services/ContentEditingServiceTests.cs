using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
internal sealed class ContentEditingServiceTests : UmbracoIntegrationTestWithContent
{
    [SetUp]
    public void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    [Test]
    public async Task Only_Supplied_Cultures_Are_Updated()
    {
        var variantTestData = await SetupVariantTest();
        var documentKey = Guid.NewGuid();
        var propertyAlias = "title";
        var originalPropertyValue = "original";
        var updatedPropertyValue = "updated";

        var createModel = new ContentCreateModel
        {
            Key = documentKey,
            ContentTypeKey = variantTestData.contentType.Key,
            Variants = new[]
            {
                new VariantModel
                {
                    Name = variantTestData.LangEn.CultureName,
                    Culture = variantTestData.LangEn.IsoCode,
                    Properties = new[]
                    {
                        new PropertyValueModel
                        {
                            Alias = propertyAlias, Value = originalPropertyValue
                        }
                    }
                },
                new VariantModel
                {
                    Name = variantTestData.LangDa.CultureName,
                    Culture = variantTestData.LangDa.IsoCode,
                    Properties = new[]
                    {
                        new PropertyValueModel
                        {
                            Alias = propertyAlias, Value = originalPropertyValue
                        }
                    }
                }
            }
        };

        await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        var content = ContentService.GetById(documentKey)!;

        var updateModel = new ContentUpdateModel
        {
            Variants = new[]
            {
                new VariantModel
                {
                    Name = updatedPropertyValue,
                    Culture = variantTestData.LangEn.IsoCode,
                    Properties = new[] { new PropertyValueModel { Alias = propertyAlias, Value = updatedPropertyValue } }
                }
            }
        };

        await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);

        var updatedContent = ContentService.GetById(documentKey)!;

        Assert.AreEqual(originalPropertyValue, updatedContent.GetValue(propertyAlias,variantTestData.LangDa.IsoCode));
        Assert.AreEqual(updatedPropertyValue, updatedContent.GetValue(propertyAlias,variantTestData.LangEn.IsoCode));

        Assert.AreEqual(variantTestData.LangDa.CultureName, updatedContent.GetCultureName(variantTestData.LangDa.IsoCode));
        Assert.AreEqual(updatedPropertyValue, updatedContent.GetCultureName(variantTestData.LangEn.IsoCode));
    }

    private async Task<(ILanguage LangEn, ILanguage LangDa, IContentType contentType)> SetupVariantTest()
    {
        var langEn = (await LanguageService.GetAsync("en-US"))!;
        var langDa = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("variantContent")
            .WithName("Variant Content")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithVariations(ContentVariation.Culture)
            .WithMandatory(true)
            .Done()
            .Done()
            .Build();

        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return (langEn, langDa, contentType);
    }
}

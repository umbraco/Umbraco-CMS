using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class ContentTypeEditingServiceTests
{
    private const string VarianceTestPropertyAlias = "testProperty";

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Migrate_Invariant_Value_To_Default_Language_When_Property_Becomes_Variant(bool isElement)
    {
        var secondLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(secondLanguage, Constants.Security.SuperUserKey);

        var contentType = await CreateVarianceCapableType(isElement);
        IPublishableContentBase instance = isElement
            ? await CreateElementInstanceWithValue(contentType, (null, "invariant value"))
            : await CreateContentInstanceWithValue(contentType, (null, "invariant value"));

        var propertyType = contentType.PropertyTypes.Single(p => p.Alias == VarianceTestPropertyAlias);
        propertyType.Variations = ContentVariation.Culture;
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        IPublishableContentBase? reloaded = isElement
            ? ElementService.GetById(instance.Key)
            : ContentService.GetById(instance.Key);

        Assert.IsNotNull(reloaded);
        Assert.AreEqual("invariant value", reloaded!.GetValue<string>(VarianceTestPropertyAlias, defaultCulture));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, "da-DK"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Migrate_Default_Language_Value_To_Invariant_When_Property_Becomes_Invariant_And_Discards_Other_Cultures(bool isElement)
    {
        var secondLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(secondLanguage, Constants.Security.SuperUserKey);
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var contentType = await CreateVarianceCapableType(isElement, ContentVariation.Culture);

        IPublishableContentBase instance = isElement
            ? await CreateElementInstanceWithValue(contentType, (defaultCulture, "default lang value"), ("da-DK", "danish value"))
            : await CreateContentInstanceWithValue(contentType, (defaultCulture, "default lang value"), ("da-DK", "danish value"));

        var propertyType = contentType.PropertyTypes.Single(p => p.Alias == VarianceTestPropertyAlias);
        propertyType.Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        IPublishableContentBase? reloaded = isElement
            ? ElementService.GetById(instance.Key)
            : ContentService.GetById(instance.Key);

        Assert.IsNotNull(reloaded);
        Assert.AreEqual("default lang value", reloaded!.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, defaultCulture));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, "da-DK"));
    }

    private async Task<IContentType> CreateVarianceCapableType(bool isElement, ContentVariation propertyVariation = ContentVariation.Nothing)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("varianceTest" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Test")
            .WithIsElement(isElement)
            .WithAllowAsRoot(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias(VarianceTestPropertyAlias)
            .WithName("Test Property")
            .WithVariations(propertyVariation)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    // Deliberately not using ElementBuilder.CreateSimpleElement/ContentBuilder.CreateSimpleContent's
    // "simple" overloads: they hardcode property values for title/bodyText/author aliases, which
    // throw InvalidOperationException against a content type that only has `testProperty`.
    private async Task<IElement> CreateElementInstanceWithValue(IContentType contentType, params (string? Culture, string Value)[] values)
    {
        var elementBuilder = new ElementBuilder()
            .WithContentType(contentType)
            .WithName("Variance Test Instance");

        if (contentType.VariesByCulture())
        {
            var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();
            foreach (var culture in values.Select(v => v.Culture).Where(c => c != null).Append(defaultCulture).Distinct())
            {
                elementBuilder = elementBuilder.WithCultureName(culture!, $"Variance Test Instance ({culture})");
            }
        }

        IElement element = elementBuilder.Build();
        foreach (var (culture, value) in values)
        {
            element.SetValue(VarianceTestPropertyAlias, value, culture);
        }

        ElementService.Save(element);
        return element;
    }

    private async Task<IContent> CreateContentInstanceWithValue(IContentType contentType, params (string? Culture, string Value)[] values)
    {
        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Variance Test Instance");

        if (contentType.VariesByCulture())
        {
            var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();
            foreach (var culture in values.Select(v => v.Culture).Where(c => c != null).Append(defaultCulture).Distinct())
            {
                contentBuilder = contentBuilder.WithCultureName(culture!, $"Variance Test Instance ({culture})");
            }
        }

        IContent content = contentBuilder.Build();
        foreach (var (culture, value) in values)
        {
            content.SetValue(VarianceTestPropertyAlias, value, culture);
        }

        ContentService.Save(content);
        return content;
    }
}

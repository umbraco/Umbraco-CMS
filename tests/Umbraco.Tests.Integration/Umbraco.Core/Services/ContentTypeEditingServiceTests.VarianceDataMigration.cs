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

    private async Task<IContentType> CreateVarianceCapableType(bool isElement)
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
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    private async Task<IPublishableContentBase> CreateInstanceWithValue(bool isElement, IContentType contentType, string? culture, string value)
    {
        // Deliberately not using ElementBuilder.CreateSimpleElement/ContentBuilder.CreateSimpleContent's
        // "simple" overloads: they hardcode property values for title/bodyText/author aliases, which
        // throw InvalidOperationException against a content type that only has `testProperty`.
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        if (isElement)
        {
            var elementBuilder = new ElementBuilder()
                .WithContentType(contentType)
                .WithName("Variance Test Instance");

            if (contentType.VariesByCulture())
            {
                elementBuilder = elementBuilder.WithCultureName(defaultCulture, "Variance Test Instance");
            }

            IElement element = elementBuilder.Build();
            element.SetValue(VarianceTestPropertyAlias, value, culture);
            ElementService.Save(element);
            return element;
        }

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Variance Test Instance");

        if (contentType.VariesByCulture())
        {
            contentBuilder = contentBuilder.WithCultureName(defaultCulture, "Variance Test Instance");
        }

        IContent content = contentBuilder.Build();
        content.SetValue(VarianceTestPropertyAlias, value, culture);
        ContentService.Save(content);
        return content;
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Migrate_Invariant_Value_To_Default_Language_When_Property_Becomes_Variant(bool isElement)
    {
        var contentType = await CreateVarianceCapableType(isElement);
        IPublishableContentBase instance = await CreateInstanceWithValue(isElement, contentType, null, "invariant value");

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
    }
}

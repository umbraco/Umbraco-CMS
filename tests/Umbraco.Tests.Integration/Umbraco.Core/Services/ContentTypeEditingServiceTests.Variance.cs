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

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Migrate_Composed_Property_Value_When_Composing_Type_Variance_Changes(bool isElement)
    {
        var secondLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(secondLanguage, Constants.Security.SuperUserKey);

        var composition = new ContentTypeBuilder()
            .WithAlias("varianceComposition" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Composition")
            .WithIsElement(isElement)
            .WithAllowAsRoot(false)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias(VarianceTestPropertyAlias)
            .WithName("Test Property")
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(composition, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("varianceComposed" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Composed")
            .WithIsElement(isElement)
            .WithAllowAsRoot(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Nothing)
            .Build();
        contentType.AddContentType(composition);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IPublishableContentBase instance = isElement
            ? await CreateElementInstanceWithValue(contentType, (null, "composed invariant value"))
            : await CreateContentInstanceWithValue(contentType, (null, "composed invariant value"));

        composition.Variations = ContentVariation.Culture;
        var composedProperty = composition.PropertyTypes.Single(p => p.Alias == VarianceTestPropertyAlias);
        composedProperty.Variations = ContentVariation.Culture;
        await ContentTypeService.UpdateAsync(composition, Constants.Security.SuperUserKey);

        contentType = await ContentTypeService.GetAsync(contentType.Key);
        contentType!.Variations = ContentVariation.Culture;
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();
        IPublishableContentBase? reloaded = isElement
            ? ElementService.GetById(instance.Key)
            : ContentService.GetById(instance.Key);

        Assert.IsNotNull(reloaded);
        Assert.AreEqual("composed invariant value", reloaded!.GetValue<string>(VarianceTestPropertyAlias, defaultCulture));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, "da-DK"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Migrate_Composed_Property_Value_When_Composing_Type_Variance_Changes_To_Invariant(bool isElement)
    {
        var secondLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(secondLanguage, Constants.Security.SuperUserKey);
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var composition = new ContentTypeBuilder()
            .WithAlias("varianceComposition" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Composition")
            .WithIsElement(isElement)
            .WithAllowAsRoot(false)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias(VarianceTestPropertyAlias)
            .WithName("Test Property")
            .WithVariations(ContentVariation.Culture)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(composition, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("varianceComposed" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Composed")
            .WithIsElement(isElement)
            .WithAllowAsRoot(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        contentType.AddContentType(composition);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IPublishableContentBase instance = isElement
            ? await CreateElementInstanceWithValue(contentType, (defaultCulture, "composed variant value"), ("da-DK", "composed danish value"))
            : await CreateContentInstanceWithValue(contentType, (defaultCulture, "composed variant value"), ("da-DK", "composed danish value"));

        composition.Variations = ContentVariation.Nothing;
        var composedProperty = composition.PropertyTypes.Single(p => p.Alias == VarianceTestPropertyAlias);
        composedProperty.Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(composition, Constants.Security.SuperUserKey);

        contentType = await ContentTypeService.GetAsync(contentType.Key);
        contentType!.Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        IPublishableContentBase? reloaded = isElement
            ? ElementService.GetById(instance.Key)
            : ContentService.GetById(instance.Key);

        Assert.IsNotNull(reloaded);
        Assert.AreEqual("composed variant value", reloaded!.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, defaultCulture));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, "da-DK"));
    }

    // Composing type variance is fixed at Culture from creation and is never itself updated below -
    // only the composition's variance changes. This isolates whether variance-change propagation via
    // composition (GetImpactedContentTypes) correctly reaches the composing type's instances even when
    // only the composition is saved, not the composing type.
    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Migrate_Composed_Property_Value_When_Only_Composition_Variance_Changes_To_Variant(bool isElement)
    {
        var secondLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(secondLanguage, Constants.Security.SuperUserKey);
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var composition = new ContentTypeBuilder()
            .WithAlias("varianceComposition" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Composition")
            .WithIsElement(isElement)
            .WithAllowAsRoot(false)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias(VarianceTestPropertyAlias)
            .WithName("Test Property")
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(composition, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("varianceComposed" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Composed")
            .WithIsElement(isElement)
            .WithAllowAsRoot(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        contentType.AddContentType(composition);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IPublishableContentBase instance = isElement
            ? await CreateElementInstanceWithValue(contentType, (null, "composed invariant value"))
            : await CreateContentInstanceWithValue(contentType, (null, "composed invariant value"));

        composition.Variations = ContentVariation.Culture;
        var composedProperty = composition.PropertyTypes.Single(p => p.Alias == VarianceTestPropertyAlias);
        composedProperty.Variations = ContentVariation.Culture;
        await ContentTypeService.UpdateAsync(composition, Constants.Security.SuperUserKey);

        IPublishableContentBase? reloaded = isElement
            ? ElementService.GetById(instance.Key)
            : ContentService.GetById(instance.Key);

        Assert.IsNotNull(reloaded);
        Assert.AreEqual("composed invariant value", reloaded!.GetValue<string>(VarianceTestPropertyAlias, defaultCulture));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, "da-DK"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Migrate_Composed_Property_Value_When_Only_Composition_Variance_Changes_To_Invariant(bool isElement)
    {
        var secondLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(secondLanguage, Constants.Security.SuperUserKey);
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var composition = new ContentTypeBuilder()
            .WithAlias("varianceComposition" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Composition")
            .WithIsElement(isElement)
            .WithAllowAsRoot(false)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias(VarianceTestPropertyAlias)
            .WithName("Test Property")
            .WithVariations(ContentVariation.Culture)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(composition, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("varianceComposed" + Guid.NewGuid().ToString("N"))
            .WithName("Variance Composed")
            .WithIsElement(isElement)
            .WithAllowAsRoot(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        contentType.AddContentType(composition);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IPublishableContentBase instance = isElement
            ? await CreateElementInstanceWithValue(contentType, (defaultCulture, "composed variant value"), ("da-DK", "composed danish value"))
            : await CreateContentInstanceWithValue(contentType, (defaultCulture, "composed variant value"), ("da-DK", "composed danish value"));

        composition.Variations = ContentVariation.Nothing;
        var composedProperty = composition.PropertyTypes.Single(p => p.Alias == VarianceTestPropertyAlias);
        composedProperty.Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(composition, Constants.Security.SuperUserKey);

        IPublishableContentBase? reloaded = isElement
            ? ElementService.GetById(instance.Key)
            : ContentService.GetById(instance.Key);

        Assert.IsNotNull(reloaded);
        Assert.AreEqual("composed variant value", reloaded!.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, defaultCulture));
        Assert.IsNull(reloaded.GetValue<string>(VarianceTestPropertyAlias, "da-DK"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Migrate_Value_For_Both_Published_And_Draft_Versions(bool isElement)
    {
        var secondLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(secondLanguage, Constants.Security.SuperUserKey);

        var contentType = await CreateVarianceCapableType(isElement);
        IPublishableContentBase instance = isElement
            ? await CreateElementInstanceWithValue(contentType, (null, "published value"))
            : await CreateContentInstanceWithValue(contentType, (null, "published value"));

        if (isElement)
        {
            ElementService.Publish((IElement)instance, ["*"]);
            var elementToEdit = ElementService.GetById(instance.Key);
            elementToEdit!.SetValue(VarianceTestPropertyAlias, "draft edited value", null);
            ElementService.Save(elementToEdit);
        }
        else
        {
            ContentService.Publish((IContent)instance, ["*"]);
            var contentToEdit = ContentService.GetById(instance.Key);
            contentToEdit!.SetValue(VarianceTestPropertyAlias, "draft edited value", null);
            ContentService.Save(contentToEdit);
        }

        var propertyType = contentType.PropertyTypes.Single(p => p.Alias == VarianceTestPropertyAlias);
        propertyType.Variations = ContentVariation.Culture;
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        IPublishableContentBase? reloadedDraft = isElement
            ? ElementService.GetById(instance.Key)
            : ContentService.GetById(instance.Key);
        Assert.IsNotNull(reloadedDraft);
        Assert.AreEqual("draft edited value", reloadedDraft!.GetValue<string>(VarianceTestPropertyAlias, defaultCulture));
        Assert.IsNull(reloadedDraft.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsNull(reloadedDraft.GetValue<string>(VarianceTestPropertyAlias, "da-DK"));

        var publishedVersionId = reloadedDraft.PublishedVersionId;
        IPublishableContentBase? publishedVersion = isElement
            ? ElementService.GetVersions(instance.Id).FirstOrDefault(v => v.VersionId == publishedVersionId)
            : ContentService.GetVersions(instance.Id).FirstOrDefault(v => v.VersionId == publishedVersionId);
        Assert.IsNotNull(publishedVersion, "Expected the originally-published version to still exist after the variance change.");
        Assert.AreEqual("published value", publishedVersion!.GetValue<string>(VarianceTestPropertyAlias, defaultCulture));
        Assert.IsNull(publishedVersion.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsNull(publishedVersion.GetValue<string>(VarianceTestPropertyAlias, "da-DK"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Can_Renormalize_Edited_Flag_When_Property_Becomes_Invariant(bool isElement)
    {
        var secondLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(secondLanguage, Constants.Security.SuperUserKey);
        var defaultCulture = await LanguageService.GetDefaultIsoCodeAsync();

        var contentType = await CreateVarianceCapableType(isElement, ContentVariation.Culture);
        IPublishableContentBase instance = isElement
            ? await CreateElementInstanceWithValue(contentType, (defaultCulture, "default value"), ("da-DK", "danish value"))
            : await CreateContentInstanceWithValue(contentType, (defaultCulture, "default value"), ("da-DK", "danish value"));

        // Publish both cultures, then leave an unpublished edit in da-DK only - the default culture stays
        // clean (current == published). The da-DK edit is what will get discarded by the variance change below.
        if (isElement)
        {
            ElementService.Publish((IElement)instance, ["*"]);
            var elementToEdit = ElementService.GetById(instance.Key);
            elementToEdit!.SetValue(VarianceTestPropertyAlias, "unpublished danish edit", "da-DK");
            ElementService.Save(elementToEdit);
        }
        else
        {
            ContentService.Publish((IContent)instance, ["*"]);
            var contentToEdit = ContentService.GetById(instance.Key);
            contentToEdit!.SetValue(VarianceTestPropertyAlias, "unpublished danish edit", "da-DK");
            ContentService.Save(contentToEdit);
        }

        var propertyType = contentType.PropertyTypes.Single(p => p.Alias == VarianceTestPropertyAlias);
        propertyType.Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        IPublishableContentBase? reloaded = isElement
            ? ElementService.GetById(instance.Key)
            : ContentService.GetById(instance.Key);

        Assert.IsNotNull(reloaded);
        Assert.AreEqual("default value", reloaded!.GetValue<string>(VarianceTestPropertyAlias));
        Assert.IsFalse(reloaded.Edited, "The only pending edit was in a culture discarded by the variance change, so the content should no longer be considered edited.");
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

    private async Task<Element> CreateElementInstanceWithValue(IContentType contentType, params (string? Culture, string Value)[] values)
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

        Element element = elementBuilder.Build();
        foreach (var (culture, value) in values)
        {
            element.SetValue(VarianceTestPropertyAlias, value, culture);
        }

        ElementService.Save(element);
        return element;
    }

    private async Task<Content> CreateContentInstanceWithValue(IContentType contentType, params (string? Culture, string Value)[] values)
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

        Content content = contentBuilder.Build();
        foreach (var (culture, value) in values)
        {
            content.SetValue(VarianceTestPropertyAlias, value, culture);
        }

        ContentService.Save(content);
        return content;
    }
}

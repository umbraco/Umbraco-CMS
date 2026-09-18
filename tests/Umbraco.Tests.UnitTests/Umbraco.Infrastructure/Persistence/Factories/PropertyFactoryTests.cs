using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Factories;

[TestFixture]
internal sealed class PropertyFactoryTests
{
    private const string Culture = "en-US";

    [Test]
    public void BuildDtos_InvariantOnlyEdit_TracksInvariantSentinelNotCulture()
    {
        IPropertyType invariantType = CreatePropertyType("invariantProp", ContentVariation.Nothing);
        IPropertyType cultureType = CreatePropertyType("cultureProp", ContentVariation.Culture);

        IProperty[] properties =
        {
            CreateProperty(1, invariantType, culture: null, published: "old", edited: "new"),
            CreateProperty(2, cultureType, culture: Culture, published: "same", edited: "same"),
        };

        PropertyFactory.BuildDtos(
            ContentVariation.Culture,
            currentVersionId: 2,
            publishedVersionId: 1,
            properties,
            CreateLanguageRepository(),
            CreatePropertyEditorCollection(),
            out var edited,
            out HashSet<string>? editedCultures);

        Assert.IsTrue(edited);
        Assert.IsNotNull(editedCultures);
        Assert.IsTrue(editedCultures!.Contains(Constants.System.InvariantCulture));
        Assert.IsFalse(editedCultures.Contains(Culture));
    }

    [Test]
    public void BuildDtos_CultureOnlyEdit_TracksCultureNotInvariantSentinel()
    {
        IPropertyType invariantType = CreatePropertyType("invariantProp", ContentVariation.Nothing);
        IPropertyType cultureType = CreatePropertyType("cultureProp", ContentVariation.Culture);

        IProperty[] properties =
        {
            CreateProperty(1, invariantType, culture: null, published: "same", edited: "same"),
            CreateProperty(2, cultureType, culture: Culture, published: "old", edited: "new"),
        };

        PropertyFactory.BuildDtos(
            ContentVariation.Culture,
            currentVersionId: 2,
            publishedVersionId: 1,
            properties,
            CreateLanguageRepository(),
            CreatePropertyEditorCollection(),
            out var edited,
            out HashSet<string>? editedCultures);

        Assert.IsTrue(edited);
        Assert.IsNotNull(editedCultures);
        Assert.IsTrue(editedCultures!.Contains(Culture));
        Assert.IsFalse(editedCultures.Contains(Constants.System.InvariantCulture));
    }

    [Test]
    public void BuildDtos_BothInvariantAndCultureEdited_TracksBothDistinctly()
    {
        IPropertyType invariantType = CreatePropertyType("invariantProp", ContentVariation.Nothing);
        IPropertyType cultureType = CreatePropertyType("cultureProp", ContentVariation.Culture);

        IProperty[] properties =
        {
            CreateProperty(1, invariantType, culture: null, published: "old", edited: "new"),
            CreateProperty(2, cultureType, culture: Culture, published: "old", edited: "new"),
        };

        PropertyFactory.BuildDtos(
            ContentVariation.Culture,
            currentVersionId: 2,
            publishedVersionId: 1,
            properties,
            CreateLanguageRepository(),
            CreatePropertyEditorCollection(),
            out var edited,
            out HashSet<string>? editedCultures);

        Assert.IsTrue(edited);
        Assert.IsNotNull(editedCultures);
        Assert.IsTrue(editedCultures!.Contains(Culture));
        Assert.IsTrue(editedCultures.Contains(Constants.System.InvariantCulture));
    }

    [Test]
    public void BuildDtos_FullyInvariantContentType_DoesNotAllocateEditedCultures()
    {
        IPropertyType invariantType = CreatePropertyType("invariantProp", ContentVariation.Nothing);

        IProperty[] properties =
        {
            CreateProperty(1, invariantType, culture: null, published: "old", edited: "new"),
        };

        PropertyFactory.BuildDtos(
            ContentVariation.Nothing,
            currentVersionId: 2,
            publishedVersionId: 1,
            properties,
            CreateLanguageRepository(),
            CreatePropertyEditorCollection(),
            out var edited,
            out HashSet<string>? editedCultures);

        Assert.IsTrue(edited);
        Assert.IsNull(editedCultures);
    }

    private static IPropertyType CreatePropertyType(string alias, ContentVariation variations)
        => new PropertyTypeBuilder()
            .WithAlias(alias)
            .WithSupportsPublishing(true)
            .WithVariations(variations)
            .Build();

    private static IProperty CreateProperty(int id, IPropertyType propertyType, string? culture, object? published, object? edited)
        => Property.CreateWithValues(
            id,
            propertyType,
            new Property.InitialPropertyValue(culture, null, true, published),
            new Property.InitialPropertyValue(culture, null, false, edited));

    private static ILanguageRepository CreateLanguageRepository()
    {
        var mock = new Mock<ILanguageRepository>();
        mock.Setup(x => x.GetIdByIsoCode(It.IsAny<string?>(), It.IsAny<bool>())).Returns(1);
        return mock.Object;
    }

    private static PropertyEditorCollection CreatePropertyEditorCollection(params IDataEditor[] dataEditors)
        => new(new DataEditorCollection(() => dataEditors));
}

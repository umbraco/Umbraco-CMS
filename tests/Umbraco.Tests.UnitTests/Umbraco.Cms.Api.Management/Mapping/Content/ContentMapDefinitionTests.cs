using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Mapping.Content;

[TestFixture]
public class ContentMapDefinitionTests
{
    private const string EditorAlias = "Test.Editor";

    [Test]
    public void MapValueViewModels_Orders_Values_By_Culture_Then_Segment_Then_Alias()
    {
        // alias-first ordering would yield a different (wrong) result for these values
        IContent content = CreateContent(
            ["en-US", "da-DK"],
            CreateProperty("title", ("en-US", null), ("da-DK", "segment-b")),
            CreateProperty("body", ("en-US", "segment-a"), ("da-DK", null), (null, null)));

        var values = CreateMapDefinition().MapValues(content.Properties)
            .Select(value => (value.Culture, value.Segment, value.Alias))
            .ToArray();

        CollectionAssert.AreEqual(
            new (string?, string?, string)[]
            {
                (null, null, "body"),
                ("da-DK", null, "body"),
                ("da-DK", "segment-b", "title"),
                ("en-US", null, "title"),
                ("en-US", "segment-a", "body"),
            },
            values);
    }

    [Test]
    public void MapVariantViewModels_Orders_Variants_By_Culture_Then_Segment()
    {
        IContent content = CreateContent(
            ["en-US", "da-DK"],
            CreateProperty("title", ("en-US", "segment-b"), ("en-US", "segment-a")));

        var variants = CreateMapDefinition().MapVariants(content)
            .Select(variant => (variant.Culture, variant.Segment))
            .ToArray();

        CollectionAssert.AreEqual(
            new (string?, string?)[]
            {
                ("da-DK", null),
                ("da-DK", "segment-a"),
                ("da-DK", "segment-b"),
                ("en-US", null),
                ("en-US", "segment-a"),
                ("en-US", "segment-b"),
            },
            variants);
    }

    internal static IContent CreateContent(string[] cultures, params IProperty[] properties)
    {
        var propertyCollection = new Mock<IPropertyCollection>();
        propertyCollection.Setup(x => x.GetEnumerator()).Returns(() => properties.AsEnumerable().GetEnumerator());

        var content = new Mock<IContent>();
        content.SetupGet(x => x.Properties).Returns(propertyCollection.Object);
        content.SetupGet(x => x.AvailableCultures).Returns(cultures);
        content.SetupGet(x => x.CreateDate).Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        content.SetupGet(x => x.UpdateDate).Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        content.Setup(x => x.GetCultureName(It.IsAny<string?>())).Returns<string?>(culture => culture ?? "Invariant");
        return content.Object;
    }

    internal static IProperty CreateProperty(string alias, params (string? Culture, string? Segment)[] variations)
    {
        var propertyType = new Mock<IPropertyType>();
        propertyType.SetupGet(x => x.PropertyEditorAlias).Returns(EditorAlias);

        IPropertyValue[] values = variations
            .Select(variation => Mock.Of<IPropertyValue>(v => v.Culture == variation.Culture && v.Segment == variation.Segment))
            .ToArray();

        var property = new Mock<IProperty>();
        property.SetupGet(x => x.Alias).Returns(alias);
        property.SetupGet(x => x.PropertyType).Returns(propertyType.Object);
        property.SetupGet(x => x.Values).Returns(values);
        return property.Object;
    }

    internal static PropertyEditorCollection CreatePropertyEditorCollection()
    {
        var editor = new Mock<IDataEditor>();
        editor.SetupGet(x => x.Alias).Returns(EditorAlias);
        editor.Setup(x => x.GetValueEditor()).Returns(Mock.Of<IDataValueEditor>());
        return new PropertyEditorCollection(new DataEditorCollection(() => [editor.Object]));
    }

    private static TestContentMapDefinition CreateMapDefinition()
        => new(CreatePropertyEditorCollection(), Mock.Of<IDataValueEditorFactory>());

    private sealed class TestContentMapDefinition : ContentMapDefinition<IContent, DocumentValueResponseModel, DocumentVariantResponseModel>
    {
        public TestContentMapDefinition(PropertyEditorCollection propertyEditorCollection, IDataValueEditorFactory dataValueEditorFactory)
            : base(propertyEditorCollection, dataValueEditorFactory)
        {
        }

        public IEnumerable<DocumentValueResponseModel> MapValues(IEnumerable<IProperty> properties)
            => MapValueViewModels(properties);

        public IEnumerable<DocumentVariantResponseModel> MapVariants(IContent content)
            => MapVariantViewModels(content);
    }
}

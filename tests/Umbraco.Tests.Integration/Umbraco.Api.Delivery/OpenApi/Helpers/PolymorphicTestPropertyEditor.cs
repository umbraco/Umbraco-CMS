using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Delivery.OpenApi.Helpers;

/// <summary>
/// Test-only property editor whose Delivery API value type is a polymorphic interface declared with
/// <see cref="JsonDerivedTypeAttribute"/>. Used to verify that the OpenAPI schema generator correctly
/// resolves auto-built discriminator mappings for polymorphic types reached via property walks.
/// </summary>
[HideFromTypeFinder]
[DataEditor(EditorAlias)]
internal sealed class PolymorphicTestPropertyEditor : DataEditor
{
    public const string EditorAlias = "Test.Polymorphic";

    public PolymorphicTestPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
    }
}

[JsonDerivedType(typeof(PolymorphicTestModelChildA), nameof(PolymorphicTestModelChildA))]
[JsonDerivedType(typeof(PolymorphicTestModelChildB), nameof(PolymorphicTestModelChildB))]
internal interface IPolymorphicTestModel
{
}

internal sealed class PolymorphicTestModelChildA : IPolymorphicTestModel
{
    public string FieldA { get; set; } = string.Empty;
}

internal sealed class PolymorphicTestModelChildB : IPolymorphicTestModel
{
    public int FieldB { get; set; }
}

[HideFromTypeFinder]
internal sealed class PolymorphicTestPropertyValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == PolymorphicTestPropertyEditor.EditorAlias;

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(IPolymorphicTestModel);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Element;

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Element;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IPolymorphicTestModel);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding) => null;
}

using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "propertyTypeHasValueDisplay")]
public class PropertyTypeHasValuesDisplay
{
    public PropertyTypeHasValuesDisplay(string propertyTypeAlias, bool hasValue)
    {
        PropertyTypeAlias = propertyTypeAlias;
        HasValue = hasValue;
    }

    [DataMember(Name = "propertyTypeAlias")]
    public string PropertyTypeAlias { get; }

    [DataMember(Name = "hasValue")]
    public bool HasValue { get; }
}

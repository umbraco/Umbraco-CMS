using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "propertyTypeHasValuesDisplay")]
public class PropertyTypeHasValuesDisplay
{
    public PropertyTypeHasValuesDisplay(string propertyTypeAlias, bool hasValues)
    {
        PropertyTypeAlias = propertyTypeAlias;
        HasValues = hasValues;
    }

    [DataMember(Name = "propertyTypeAlias")]
    public string PropertyTypeAlias { get; }

    [DataMember(Name = "hasValues")]
    public bool HasValues { get; }
}

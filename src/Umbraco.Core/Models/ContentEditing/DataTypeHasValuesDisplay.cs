using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "dataTypeHasValuesDisplay")]
public class DataTypeHasValuesDisplay
{
    public DataTypeHasValuesDisplay(int id, bool hasValues)
    {
        Id = id;
        HasValues = hasValues;
    }

    [DataMember(Name = "id")]
    public int Id { get; }

    [DataMember(Name = "hasValues")]
    public bool HasValues { get; }
}

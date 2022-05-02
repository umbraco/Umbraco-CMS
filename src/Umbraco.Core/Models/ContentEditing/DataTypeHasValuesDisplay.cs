using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "dataTypeHasValuesDisplay")]
public class DataTypeHasValuesDisplay
{
    public DataTypeHasValuesDisplay(int id, bool hasValue)
    {
        Id = id;
        HasValue = hasValue;
    }

    [DataMember(Name = "id")]
    public int Id { get; }

    [DataMember(Name = "hasValue")]
    public bool HasValue { get; }
}

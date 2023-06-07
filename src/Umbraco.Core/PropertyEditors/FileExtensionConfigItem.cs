using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataContract]
public class FileExtensionConfigItem : IFileExtensionConfigItem
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "value")]
    public string? Value { get; set; }
}

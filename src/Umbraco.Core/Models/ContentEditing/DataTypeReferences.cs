using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "dataTypeReferences", Namespace = "")]
public class DataTypeReferences
{
    [DataMember(Name = "documentTypes")]
    public IEnumerable<ContentTypeReferences> DocumentTypes { get; set; } = Enumerable.Empty<ContentTypeReferences>();

    [DataMember(Name = "mediaTypes")]
    public IEnumerable<ContentTypeReferences> MediaTypes { get; set; } = Enumerable.Empty<ContentTypeReferences>();

    [DataMember(Name = "memberTypes")]
    public IEnumerable<ContentTypeReferences> MemberTypes { get; set; } = Enumerable.Empty<ContentTypeReferences>();

    [DataContract(Name = "contentType", Namespace = "")]
    public class ContentTypeReferences : EntityBasic
    {
        [DataMember(Name = "properties")]
        public object? Properties { get; set; }

        [DataContract(Name = "property", Namespace = "")]
        public class PropertyTypeReferences
        {
            [DataMember(Name = "name")]
            public string? Name { get; set; }

            [DataMember(Name = "alias")]
            public string? Alias { get; set; }
        }
    }
}

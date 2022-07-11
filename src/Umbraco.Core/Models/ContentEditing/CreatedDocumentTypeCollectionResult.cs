using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The result of creating a content type collection in the UI
/// </summary>
[DataContract(Name = "contentTypeCollection", Namespace = "")]
public class CreatedContentTypeCollectionResult
{
    [DataMember(Name = "collectionId")]
    public int CollectionId { get; set; }

    [DataMember(Name = "containerId")]
    public int ContainerId { get; set; }
}

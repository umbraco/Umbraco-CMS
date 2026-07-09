namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents the model for creating a new document content type.
/// </summary>
public class ContentTypeCreateModel : ContentTypeModelBase
{
    /// <summary>
    ///     Gets or sets the unique key for the content type being created.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the container (folder) to place the content type in.
    /// </summary>
    public Guid? ContainerKey { get; set; }
}

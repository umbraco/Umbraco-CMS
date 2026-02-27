namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents the model for creating a new media type.
/// </summary>
public class MediaTypeCreateModel : MediaTypeModelBase
{
    /// <summary>
    ///     Gets or sets the unique key for the media type being created.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the container (folder) to place the media type in.
    /// </summary>
    public Guid? ContainerKey { get; set; }
}

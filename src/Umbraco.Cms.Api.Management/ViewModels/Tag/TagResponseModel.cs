namespace Umbraco.Cms.Api.Management.ViewModels.Tag;

/// <summary>
/// Represents a model used to return tag information in API responses.
/// </summary>
public class TagResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the tag.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the text of the tag.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the group to which the tag belongs.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>Gets or sets the number of nodes associated with the tag.</summary>
    public int NodeCount { get; set; }
}

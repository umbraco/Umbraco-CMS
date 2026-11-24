namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the API response model for a named draft branch of an element.
/// </summary>
public class ElementDraftBranchResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the draft branch.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a reference to the element this draft branch belongs to.
    /// </summary>
    public ReferenceByIdModel Element { get; set; } = null!;

    /// <summary>
    /// Gets or sets the editor-facing name of the draft branch (e.g. "Black Friday").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time the draft branch was created.
    /// </summary>
    public DateTimeOffset CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time the draft branch was last updated.
    /// </summary>
    public DateTimeOffset UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) the draft branch is scheduled to be automatically published, or
    /// <c>null</c> if it has no scheduled publish date.
    /// </summary>
    public DateTimeOffset? ScheduledPublishDate { get; set; }
}

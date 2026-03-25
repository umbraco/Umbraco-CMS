namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the API response model for a specific version of an element.
/// </summary>
public class ElementVersionItemResponseModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementVersionItemResponseModel"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this element version.</param>
    /// <param name="element">A reference to the associated element.</param>
    /// <param name="documentType">A reference to the document type of the element.</param>
    /// <param name="user">A reference to the user who created this version.</param>
    /// <param name="versionDate">The date and time this version was created.</param>
    /// <param name="isCurrentPublishedVersion">True if this is the currently published version; otherwise, false.</param>
    /// <param name="isCurrentDraftVersion">True if this is the current draft version; otherwise, false.</param>
    /// <param name="preventCleanup">True to prevent automatic cleanup of this version; otherwise, false.</param>
    public ElementVersionItemResponseModel(
        Guid id,
        ReferenceByIdModel element,
        ReferenceByIdModel documentType,
        ReferenceByIdModel user,
        DateTimeOffset versionDate,
        bool isCurrentPublishedVersion,
        bool isCurrentDraftVersion,
        bool preventCleanup)
    {
        Id = id;
        Element = element;
        DocumentType = documentType;

        User = user;
        VersionDate = versionDate;
        IsCurrentPublishedVersion = isCurrentPublishedVersion;
        IsCurrentDraftVersion = isCurrentDraftVersion;
        PreventCleanup = preventCleanup;
    }

    /// <summary>
    /// Gets the unique identifier of the element version.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets a reference to the element associated with this version, identified by its ID.
    /// </summary>
    public ReferenceByIdModel Element { get; }

    /// <summary>
    /// Gets a reference to the document type associated with this element version.
    /// </summary>
    public ReferenceByIdModel DocumentType { get; }

    /// <summary>
    /// Gets the reference to the user who created or modified this element version.
    /// </summary>
    public ReferenceByIdModel User { get; }

    /// <summary>
    /// Gets the date and time when the element version was created.
    /// </summary>
    public DateTimeOffset VersionDate { get; }

    /// <summary>
    /// Gets a value indicating whether this version is the currently published version.
    /// </summary>
    public bool IsCurrentPublishedVersion { get; }

    /// <summary>
    /// Gets a value indicating whether this version is the current draft.
    /// </summary>
    public bool IsCurrentDraftVersion { get; }

    /// <summary>
    /// Gets a value indicating whether cleanup should be prevented for this element version.
    /// </summary>
    public bool PreventCleanup { get; }
}

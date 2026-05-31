namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the API response model for a specific version of a document.
/// </summary>
public class DocumentVersionItemResponseModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.ViewModels.Document.DocumentVersionItemResponseModel"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this document version.</param>
    /// <param name="document">A reference to the associated document.</param>
    /// <param name="documentType">A reference to the type of the document.</param>
    /// <param name="user">A reference to the user who created this version.</param>
    /// <param name="versionDate">The date and time this version was created.</param>
    /// <param name="isCurrentPublishedVersion">True if this is the currently published version; otherwise, false.</param>
    /// <param name="isCurrentDraftVersion">True if this is the current draft version; otherwise, false.</param>
    /// <param name="preventCleanup">True to prevent automatic cleanup of this version; otherwise, false.</param>
    public DocumentVersionItemResponseModel(
        Guid id,
        ReferenceByIdModel document,
        ReferenceByIdModel documentType,
        ReferenceByIdModel user,
        DateTimeOffset versionDate,
        bool isCurrentPublishedVersion,
        bool isCurrentDraftVersion,
        bool preventCleanup)
    {
        Id = id;
        Document = document;
        DocumentType = documentType;

        User = user;
        VersionDate = versionDate;
        IsCurrentPublishedVersion = isCurrentPublishedVersion;
        IsCurrentDraftVersion = isCurrentDraftVersion;
        PreventCleanup = preventCleanup;
    }

    /// <summary>
    /// Gets the unique identifier of the document version.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets a reference to the document associated with this version, identified by its ID.
    /// </summary>
    public ReferenceByIdModel Document { get; }

    /// <summary>
    /// Gets a reference to the document type associated with this document version.
    /// </summary>
    public ReferenceByIdModel DocumentType { get; }

    /// <summary>
    /// Gets the reference to the user who created or modified this document version.
    /// </summary>
    public ReferenceByIdModel User { get; }

    /// <summary>
    /// Gets the date and time when the document version was created.
    /// </summary>
    public DateTimeOffset VersionDate { get; }

    /// <summary>
    /// Gets a value indicating whether this version is the currently published version.
    /// </summary>
    public bool IsCurrentPublishedVersion { get; }

    /// <summary>
    /// Indicates whether this version is the current draft.
    /// </summary>
    public bool IsCurrentDraftVersion { get; }

    /// <summary>
    /// Gets a value indicating whether cleanup should be prevented for this document version.
    /// </summary>
    public bool PreventCleanup { get; }
}

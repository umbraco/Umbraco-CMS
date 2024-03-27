namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVersionItemResponseModel
{
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

    public Guid Id { get; }

    public ReferenceByIdModel Document { get; }

    public ReferenceByIdModel DocumentType { get; }

    public ReferenceByIdModel User { get; }

    public DateTimeOffset VersionDate { get; }

    public bool IsCurrentPublishedVersion { get; }

    public bool IsCurrentDraftVersion { get; }

    public bool PreventCleanup { get; }
}

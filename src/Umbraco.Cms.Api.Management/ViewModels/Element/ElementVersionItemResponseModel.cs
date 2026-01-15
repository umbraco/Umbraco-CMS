namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class ElementVersionItemResponseModel
{
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

    public Guid Id { get; }

    public ReferenceByIdModel Element { get; }

    public ReferenceByIdModel DocumentType { get; }

    public ReferenceByIdModel User { get; }

    public DateTimeOffset VersionDate { get; }

    public bool IsCurrentPublishedVersion { get; }

    public bool IsCurrentDraftVersion { get; }

    public bool PreventCleanup { get; }
}

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVersionItemResponseModel
{
    public DocumentVersionItemResponseModel(
        ReferenceByIdModel version,
        ReferenceByIdModel content,
        ReferenceByIdModel contentType,
        ReferenceByIdModel user,
        DateTimeOffset versionDate,
        bool currentPublishedVersion,
        bool currentDraftVersion,
        bool preventCleanup)
    {
        Version = version;
        Content = content;
        ContentType = contentType;

        User = user;
        VersionDate = versionDate;
        CurrentPublishedVersion = currentPublishedVersion;
        CurrentDraftVersion = currentDraftVersion;
        PreventCleanup = preventCleanup;
    }

    public ReferenceByIdModel Content { get; }

    public ReferenceByIdModel ContentType { get; }

    public ReferenceByIdModel Version { get; }

    public ReferenceByIdModel User { get; }

    public DateTimeOffset VersionDate { get; }

    public bool CurrentPublishedVersion { get; }

    public bool CurrentDraftVersion { get; }

    public bool PreventCleanup { get; }
}

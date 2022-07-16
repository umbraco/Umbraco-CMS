namespace Umbraco.Cms.Core.Models;

public class ContentVersionMeta
{
    public ContentVersionMeta()
    {
    }

    public ContentVersionMeta(
        int versionId,
        int contentId,
        int contentTypeId,
        int userId,
        DateTime versionDate,
        bool currentPublishedVersion,
        bool currentDraftVersion,
        bool preventCleanup,
        string username)
    {
        VersionId = versionId;
        ContentId = contentId;
        ContentTypeId = contentTypeId;

        UserId = userId;
        VersionDate = versionDate;
        CurrentPublishedVersion = currentPublishedVersion;
        CurrentDraftVersion = currentDraftVersion;
        PreventCleanup = preventCleanup;
        Username = username;
    }

    public int ContentId { get; }

    public int ContentTypeId { get; }

    public int VersionId { get; }

    public int UserId { get; }

    public DateTime VersionDate { get; }

    public bool CurrentPublishedVersion { get; }

    public bool CurrentDraftVersion { get; }

    public bool PreventCleanup { get; }

    public string? Username { get; }

    public override string ToString() => $"ContentVersionMeta(versionId: {VersionId}, versionDate: {VersionDate:s}";
}

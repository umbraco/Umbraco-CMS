using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents metadata about a content version.
/// </summary>
/// <remarks>
///     This class contains information about a specific version of content, including
///     the user who created it, the date, and whether it's the current published or draft version.
/// </remarks>
public class ContentVersionMeta
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentVersionMeta" /> class.
    /// </summary>
    public ContentVersionMeta()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentVersionMeta" /> class with the specified values.
    /// </summary>
    /// <param name="versionId">The version identifier.</param>
    /// <param name="contentId">The content identifier.</param>
    /// <param name="contentTypeId">The content type identifier.</param>
    /// <param name="userId">The identifier of the user who created this version.</param>
    /// <param name="versionDate">The date and time when this version was created.</param>
    /// <param name="currentPublishedVersion">A value indicating whether this is the current published version.</param>
    /// <param name="currentDraftVersion">A value indicating whether this is the current draft version.</param>
    /// <param name="preventCleanup">A value indicating whether this version should be prevented from cleanup.</param>
    /// <param name="username">The username of the user who created this version.</param>
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

    /// <summary>
    ///     Gets the content identifier.
    /// </summary>
    public int ContentId { get; }

    /// <summary>
    ///     Gets the content type identifier.
    /// </summary>
    public int ContentTypeId { get; }

    /// <summary>
    ///     Gets the version identifier.
    /// </summary>
    public int VersionId { get; }

    /// <summary>
    ///     Gets the identifier of the user who created this version.
    /// </summary>
    public int UserId { get; }

    /// <summary>
    ///     Gets the date and time when this version was created.
    /// </summary>
    public DateTime VersionDate { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether this is the current published version.
    /// </summary>
    public bool CurrentPublishedVersion { get; }

    /// <summary>
    ///     Gets a value indicating whether this is the current draft version.
    /// </summary>
    public bool CurrentDraftVersion { get; }

    /// <summary>
    ///     Gets a value indicating whether this version should be prevented from automatic cleanup.
    /// </summary>
    public bool PreventCleanup { get; }

    /// <summary>
    ///     Gets the username of the user who created this version.
    /// </summary>
    public string? Username { get; }

    /// <inheritdoc />
    public override string ToString() => $"ContentVersionMeta(versionId: {VersionId}, versionDate: {VersionDate:s}";

    /// <summary>
    ///     Ensures the <see cref="VersionDate" /> is in UTC format.
    /// </summary>
    public void EnsureUtc() => VersionDate = VersionDate.EnsureUtc();
}

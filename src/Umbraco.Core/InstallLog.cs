namespace Umbraco.Cms.Core;

/// <summary>
///     Represents an installation or upgrade log entry.
/// </summary>
public class InstallLog
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallLog" /> class.
    /// </summary>
    /// <param name="installId">The unique identifier for this installation.</param>
    /// <param name="isUpgrade">Whether this is an upgrade from a previous version.</param>
    /// <param name="installCompleted">Whether the installation completed successfully.</param>
    /// <param name="timestamp">The timestamp of the installation.</param>
    /// <param name="versionMajor">The major version number.</param>
    /// <param name="versionMinor">The minor version number.</param>
    /// <param name="versionPatch">The patch version number.</param>
    /// <param name="versionComment">A comment about the version (e.g., pre-release tag).</param>
    /// <param name="error">Any error message from the installation.</param>
    /// <param name="userAgent">The user agent string from the browser performing the installation.</param>
    /// <param name="dbProvider">The database provider being used.</param>
    public InstallLog(
        Guid installId,
        bool isUpgrade,
        bool installCompleted,
        DateTime timestamp,
        int versionMajor,
        int versionMinor,
        int versionPatch,
        string versionComment,
        string error,
        string? userAgent,
        string dbProvider)
    {
        InstallId = installId;
        IsUpgrade = isUpgrade;
        InstallCompleted = installCompleted;
        Timestamp = timestamp;
        VersionMajor = versionMajor;
        VersionMinor = versionMinor;
        VersionPatch = versionPatch;
        VersionComment = versionComment;
        Error = error;
        UserAgent = userAgent;
        DbProvider = dbProvider;
    }

    /// <summary>
    ///     Gets the unique identifier for this installation.
    /// </summary>
    public Guid InstallId { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether this is an upgrade from a previous version.
    /// </summary>
    public bool IsUpgrade { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the installation completed successfully.
    /// </summary>
    public bool InstallCompleted { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp of the installation.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Gets the major version number of the installed version.
    /// </summary>
    public int VersionMajor { get; }

    /// <summary>
    ///     Gets the minor version number of the installed version.
    /// </summary>
    public int VersionMinor { get; }

    /// <summary>
    ///     Gets the patch version number of the installed version.
    /// </summary>
    public int VersionPatch { get; }

    /// <summary>
    ///     Gets a comment about the version (e.g., pre-release tag like "beta").
    /// </summary>
    public string VersionComment { get; }

    /// <summary>
    ///     Gets any error message from the installation, or an empty string if no error occurred.
    /// </summary>
    public string Error { get; }

    /// <summary>
    ///     Gets the user agent string from the browser performing the installation.
    /// </summary>
    public string? UserAgent { get; }

    /// <summary>
    ///     Gets or sets the database provider being used.
    /// </summary>
    public string DbProvider { get; set; }
}

namespace Umbraco.Cms.Core;

public class InstallLog
{
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

    public Guid InstallId { get; }

    public bool IsUpgrade { get; set; }

    public bool InstallCompleted { get; set; }

    public DateTime Timestamp { get; set; }

    public int VersionMajor { get; }

    public int VersionMinor { get; }

    public int VersionPatch { get; }

    public string VersionComment { get; }

    public string Error { get; }

    public string? UserAgent { get; }

    public string DbProvider { get; set; }
}

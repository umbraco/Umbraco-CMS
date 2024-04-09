namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeCleanupBase
{
    public bool PreventCleanup { get; init; }

    public int? KeepAllVersionsNewerThanDays { get; init; }

    public int? KeepLatestVersionPerDayForDays { get; init; }
}

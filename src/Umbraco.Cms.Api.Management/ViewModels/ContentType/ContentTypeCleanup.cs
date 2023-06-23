namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public class ContentTypeCleanup
{
    public bool PreventCleanup { get; init; }

    public int? KeepAllVersionsNewerThanDays { get; init; }

    public int? KeepLatestVersionPerDayForDays { get; init; }
}

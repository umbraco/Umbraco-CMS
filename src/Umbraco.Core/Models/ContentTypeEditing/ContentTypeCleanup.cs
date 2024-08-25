namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class ContentTypeCleanup
{
    public bool PreventCleanup { get; init; }

    public int? KeepAllVersionsNewerThanDays { get; init; }

    public int? KeepLatestVersionPerDayForDays { get; init; }
}

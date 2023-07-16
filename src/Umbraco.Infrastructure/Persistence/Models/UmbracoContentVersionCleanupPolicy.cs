namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoContentVersionCleanupPolicy
{
    public int ContentTypeId { get; set; }

    public bool PreventCleanup { get; set; }

    public int? KeepAllVersionsNewerThanDays { get; set; }

    public int? KeepLatestVersionPerDayForDays { get; set; }

    public DateTime Updated { get; set; }

    public virtual CmsContentType ContentType { get; set; } = null!;
}

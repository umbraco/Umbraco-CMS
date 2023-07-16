namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoConsent
{
    public int Id { get; set; }

    public bool Current { get; set; }

    public string Source { get; set; } = null!;

    public string Context { get; set; } = null!;

    public string Action { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public int State { get; set; }

    public string? Comment { get; set; }
}

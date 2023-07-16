namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoKeyValue
{
    public string Key { get; set; } = null!;

    public string? Value { get; set; }

    public DateTime Updated { get; set; }
}

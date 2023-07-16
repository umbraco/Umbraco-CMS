namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoMediaVersion
{
    public int Id { get; set; }

    public string? Path { get; set; }

    public virtual UmbracoContentVersion IdNavigation { get; set; } = null!;
}

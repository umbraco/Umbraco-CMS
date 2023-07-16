namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoUser2NodeNotify
{
    public int UserId { get; set; }

    public int NodeId { get; set; }

    public string Action { get; set; } = null!;

    public virtual UmbracoNode Node { get; set; } = null!;

    public virtual UmbracoUser User { get; set; } = null!;
}

namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoUserStartNode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int StartNode { get; set; }

    public int StartNodeType { get; set; }

    public virtual UmbracoNode StartNodeNavigation { get; set; } = null!;

    public virtual UmbracoUser User { get; set; } = null!;
}

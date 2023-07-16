namespace Umbraco.Cms.Infrastructure.Persistence.Models;

public class UmbracoAccess
{
    public Guid Id { get; set; }

    public int NodeId { get; set; }

    public int LoginNodeId { get; set; }

    public int NoAccessNodeId { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual UmbracoNode LoginNode { get; set; } = null!;

    public virtual UmbracoNode NoAccessNode { get; set; } = null!;

    public virtual UmbracoNode Node { get; set; } = null!;

    public virtual ICollection<UmbracoAccessRule> UmbracoAccessRules { get; set; } = new List<UmbracoAccessRule>();
}

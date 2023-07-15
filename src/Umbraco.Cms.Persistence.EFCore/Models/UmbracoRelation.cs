namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoRelation
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public int ChildId { get; set; }

    public int RelType { get; set; }

    public DateTime Datetime { get; set; }

    public string Comment { get; set; } = null!;

    public virtual UmbracoNode Child { get; set; } = null!;

    public virtual UmbracoNode Parent { get; set; } = null!;

    public virtual UmbracoRelationType RelTypeNavigation { get; set; } = null!;
}

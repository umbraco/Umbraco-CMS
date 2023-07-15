namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoRelationType
{
    public int Id { get; set; }

    public Guid TypeUniqueId { get; set; }

    public bool Dual { get; set; }

    public Guid? ParentObjectType { get; set; }

    public Guid? ChildObjectType { get; set; }

    public string Name { get; set; } = null!;

    public string Alias { get; set; } = null!;

    public bool? IsDependency { get; set; }

    public virtual ICollection<UmbracoRelation> UmbracoRelations { get; set; } = new List<UmbracoRelation>();
}

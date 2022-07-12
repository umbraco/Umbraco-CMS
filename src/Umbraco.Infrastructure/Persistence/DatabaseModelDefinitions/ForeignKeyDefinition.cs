using System.Data;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

public class ForeignKeyDefinition
{
    public ForeignKeyDefinition()
    {
        ForeignColumns = new List<string>();
        PrimaryColumns = new List<string>();

        // Set to None by Default
        OnDelete = Rule.None;
        OnUpdate = Rule.None;
    }

    public virtual string? Name { get; set; }

    public virtual string? ForeignTable { get; set; }

    public virtual string? ForeignTableSchema { get; set; }

    public virtual string? PrimaryTable { get; set; }

    public virtual string? PrimaryTableSchema { get; set; }

    public virtual Rule OnDelete { get; set; }

    public virtual Rule OnUpdate { get; set; }

    public virtual ICollection<string> ForeignColumns { get; set; }

    public virtual ICollection<string> PrimaryColumns { get; set; }
}

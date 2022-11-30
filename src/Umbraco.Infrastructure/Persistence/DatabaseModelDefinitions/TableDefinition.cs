namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

public class TableDefinition
{
    public TableDefinition()
    {
        Columns = new List<ColumnDefinition>();
        ForeignKeys = new List<ForeignKeyDefinition>();
        Indexes = new List<IndexDefinition>();
    }

    public virtual string Name { get; set; } = null!;

    public virtual string SchemaName { get; set; } = null!;

    public virtual ICollection<ColumnDefinition> Columns { get; set; }

    public virtual ICollection<ForeignKeyDefinition> ForeignKeys { get; set; }

    public virtual ICollection<IndexDefinition> Indexes { get; set; }
}

using System.Collections.Generic;

namespace Umbraco.Core.Persistence.DatabaseModelDefinitions
{
    public class TableDefinition
    {
        public TableDefinition()
        {
            Columns = new List<ColumnDefinition>();
            ForeignKeys = new List<ForeignKeyDefinition>();
            Indexes = new List<IndexDefinition>();
        }

        public virtual string Name { get; set; }
        public virtual string SchemaName { get; set; }
        public virtual ICollection<ColumnDefinition> Columns { get; set; }
        public virtual ICollection<ForeignKeyDefinition> ForeignKeys { get; set; }
        public virtual ICollection<IndexDefinition> Indexes { get; set; }
    }
}
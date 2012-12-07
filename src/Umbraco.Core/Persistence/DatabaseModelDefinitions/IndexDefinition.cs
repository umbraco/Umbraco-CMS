using System.Collections.Generic;

namespace Umbraco.Core.Persistence.DatabaseModelDefinitions
{
    public class IndexDefinition
    {
        public IndexDefinition()
        {
            Columns = new List<IndexColumnDefinition>();
        }

        public virtual string Name { get; set; }
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
        public virtual ICollection<IndexColumnDefinition> Columns { get; set; }
    }
}
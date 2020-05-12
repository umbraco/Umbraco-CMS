using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseAnnotations;

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
        public virtual string ColumnName { get; set; }

        [Obsolete("Use the IndexType property instead and set it to IndexTypes.UniqueNonClustered")]
        public virtual bool IsUnique { get; set; }

        [Obsolete("Use the IndexType property instead and set it to IndexTypes.Clustered")]
        public bool IsClustered { get; set; }
        public virtual ICollection<IndexColumnDefinition> Columns { get; set; }
        public IndexTypes IndexType { get; set; }
    }
}

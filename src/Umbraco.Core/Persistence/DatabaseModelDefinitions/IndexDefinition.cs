using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.DatabaseModelDefinitions
{
    public class IndexDefinition
    {
        public virtual string Name { get; set; }
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }

        public virtual ICollection<IndexColumnDefinition> Columns { get; set; } = new List<IndexColumnDefinition>();
        public virtual ICollection<IndexColumnDefinition> IncludeColumns { get; set; } = new List<IndexColumnDefinition>();
        public IndexTypes IndexType { get; set; }
    }
}

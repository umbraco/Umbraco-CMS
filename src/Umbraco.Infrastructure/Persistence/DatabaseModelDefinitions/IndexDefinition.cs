﻿using System.Collections.Generic;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions
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

        public virtual ICollection<IndexColumnDefinition> Columns { get; set; }
        public IndexTypes IndexType { get; set; }
    }
}

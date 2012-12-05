using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions
{
    public class TableDefinition
    {
        public TableDefinition()
        {
            ColumnDefinitions = new List<ColumnDefinition>();
            ForeignKeyDefinitions = new List<ForeignKeyDefinition>();
            IndexDefinitions = new List<IndexDefinition>();
        }

        public string TableName { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKeyClustered
        {
            get { return ColumnDefinitions.Any(x => x.IsPrimaryKeyClustered); }
        }
        public string PrimaryKeyName
        {
            get { return ColumnDefinitions.First(x => x.IsPrimaryKey).PrimaryKeyName ?? string.Empty; }
        }
        public string PrimaryKeyColumns
        {
            get { return ColumnDefinitions.First(x => x.IsPrimaryKey).PrimaryKeyColumns ?? string.Empty; }
        }
        
        public List<ColumnDefinition> ColumnDefinitions { get; set; }
        public List<ForeignKeyDefinition> ForeignKeyDefinitions { get; set; }
        public List<IndexDefinition> IndexDefinitions { get; set; }
    }
}
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions
{
    public class IndexDefinition
    {
        public string IndexName { get; set; }
        public IndexTypes IndexType { get; set; }
        public string ColumnNames { get; set; }
        public string IndexForColumn { get; set; }
    }
}
namespace Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions
{
    public class ForeignKeyDefinition
    {
        public string ConstraintName { get; set; }
        public string ColumnName { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
    }
}
namespace Umbraco.Core.Persistence.DatabaseModelDefinitions
{
    /// <summary>
    /// Represents a database index definition retreived by querying the database
    /// </summary>
    internal class DbIndexDefinition
    {
        public virtual string IndexName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }
        public virtual bool IsUnique { get; set; }
    }
}
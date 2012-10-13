using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// Represents an abstract class for descriping changes to a database column
    /// Used to Remove a column
    /// </summary>
    public abstract class DropColumnChange : BaseChange
    {
        public abstract string ColumnName { get; }

        public virtual ChangeTypes ChangeType
        {
            get
            {
                return ChangeTypes.DROP;
            }
        }

        public override Sql ToSql()
        {
            var sql = new Sql();
            sql.Append(string.Format("ALTER TABLE [{0}] DROP COLUMN [{1}];",
                        TableName, ColumnName));
            return sql;
        }
    }
}
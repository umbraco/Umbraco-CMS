using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// Represents an abstract class for descriping changes to a database column
    /// Used to Add a column with an optional constraint
    /// </summary>
    public abstract class AddColumnChange : DropColumnChange
    {
        public abstract string Constraint { get; }

        public abstract string DefaultForConstraint { get; }

        public abstract DatabaseTypes DatabaseType { get; }

        public virtual int DatabaseTypeLength
        {
            get { return 0; }
        }

        public abstract NullSettings NullSetting { get; }

        public override Sql ToSql()
        {
            var sql = new Sql();

            string constraint = !string.IsNullOrEmpty(Constraint)
                                    ? string.Format("CONSTRAINT [{0}] DEFAULT ({1})", Constraint, DefaultForConstraint)
                                    : string.Format("CONSTRAINT [DF_{0}_{1}] DEFAULT ({2})", TableName, ColumnName,
                                                    DefaultForConstraint);

            sql.Append(string.Format("ALTER TABLE [{0}] ADD [{1}] {2} {3} {4};",
                                     TableName, ColumnName,
                                     DatabaseType.ToSqlSyntax(DatabaseTypeLength),
                                     NullSetting.ToSqlSyntax(),
                                     constraint));
            return sql;
        }
    }
}
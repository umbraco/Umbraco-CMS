using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Delete.Expressions
{
    public class DeleteDefaultConstraintExpression : MigrationExpressionBase
    {
        public DeleteDefaultConstraintExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }

        public override string ToString() // fixme kill
            => GetSql();

        protected override string GetSql()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return string.Format(SqlSyntax.DeleteDefaultConstraint,
                SqlSyntax.GetQuotedTableName(TableName),
                SqlSyntax.GetQuotedColumnName(ColumnName));
        }
    }
}

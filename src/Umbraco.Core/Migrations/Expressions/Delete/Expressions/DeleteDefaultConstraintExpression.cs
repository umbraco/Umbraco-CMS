using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Delete.Expressions
{
    public class DeleteDefaultConstraintExpression : MigrationExpressionBase
    {
        public DeleteDefaultConstraintExpression(IMigrationContext context)
            : base(context)
        { }

        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }
        public virtual string ConstraintName { get; set; }

        protected override string GetSql()
        {
            return ConstraintName.IsNullOrWhiteSpace()
                ? string.Empty
                : string.Format(SqlSyntax.DeleteDefaultConstraint,
                    SqlSyntax.GetQuotedTableName(TableName),
                    SqlSyntax.GetQuotedColumnName(ColumnName),
                    SqlSyntax.GetQuotedName(ConstraintName));
        }
    }
}

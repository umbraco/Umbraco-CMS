using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteDefaultConstraintExpression : IMigrationExpression
    {
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }

        public override string ToString()
        {
            return string.Format(SyntaxConfig.SqlSyntaxProvider.DeleteDefaultConstraint,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(ColumnName));
        }
    }
}
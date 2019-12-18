using System.Collections.Generic;
using System.Text;
using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Delete.Expressions
{
    public class DeleteColumnExpression : MigrationExpressionBase
    {
        public DeleteColumnExpression(IMigrationContext context)
            : base(context)
        {
            ColumnNames = new List<string>();
        }

        public virtual string TableName { get; set; }
        public ICollection<string> ColumnNames { get; set; }

        protected override string GetSql()
        {
            var stmts = new StringBuilder();
            foreach (var columnName in ColumnNames)
            {
                stmts.AppendFormat(SqlSyntax.DropColumn, SqlSyntax.GetQuotedTableName(TableName), SqlSyntax.GetQuotedColumnName(columnName));
                AppendStatementSeparator(stmts);
            }
            return stmts.ToString();
        }
    }
}

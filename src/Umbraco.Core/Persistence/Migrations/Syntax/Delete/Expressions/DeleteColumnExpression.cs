using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteColumnExpression : IMigrationExpression
    {
        public DeleteColumnExpression()
        {
            ColumnNames = new List<string>();
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public ICollection<string> ColumnNames { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (string columnName in ColumnNames)
            {
                if (ColumnNames.First() != columnName) sb.AppendLine(";");
                sb.AppendFormat(SyntaxConfig.SqlSyntaxProvider.DropColumn,
                                SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(columnName));
            }

            return sb.ToString();
        }
    }
}
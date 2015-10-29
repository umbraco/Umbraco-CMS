using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteColumnExpression : MigrationExpressionBase
    {
        //public DeleteColumnExpression()
        //{
        //    ColumnNames = new List<string>();
        //}

        public DeleteColumnExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
        {
            ColumnNames = new List<string>();
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public ICollection<string> ColumnNames { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (string columnName in ColumnNames)
            {
                if (ColumnNames.First() != columnName) sb.AppendLine(";");
                sb.AppendFormat(SqlSyntax.DropColumn,
                                SqlSyntax.GetQuotedTableName(TableName),
                                SqlSyntax.GetQuotedColumnName(columnName));
            }

            return sb.ToString();
        }
    }
}
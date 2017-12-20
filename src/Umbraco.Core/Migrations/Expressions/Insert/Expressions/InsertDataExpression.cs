using System.Collections.Generic;
using System.Text;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Insert.Expressions
{
    public class InsertDataExpression : MigrationExpressionBase
    {
        public InsertDataExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public string TableName { get; set; }
        public bool EnabledIdentityInsert { get; set; }

        public List<InsertionDataDefinition> Rows { get; } = new List<InsertionDataDefinition>();

        protected override string GetSql()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            var stmts = new StringBuilder();

            if (EnabledIdentityInsert && SqlSyntax.SupportsIdentityInsert())
            {
                stmts.AppendLine($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(TableName)} ON");
                AppendStatementSeparator(stmts);
            }

            try
            {
                foreach (var item in Rows)
                {
                    var cols = string.Join(",", item.Select(x => x.Key));
                    var vals = string.Join(",", item.Select(x => x.Value));

                    var sql = string.Format(SqlSyntax.InsertData,
                        SqlSyntax.GetQuotedTableName(TableName),
                        cols, vals);

                    stmts.Append(sql);
                    AppendStatementSeparator(stmts);
                }
            }
            finally
            {
                if (EnabledIdentityInsert && SqlSyntax.SupportsIdentityInsert())
                {
                    stmts.AppendLine($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(TableName)} OFF");
                    AppendStatementSeparator(stmts);
                }
            }

            return stmts.ToString();
        }
    }
}

using System.Collections.Generic;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Update.Expressions
{
    public class UpdateDataExpression : MigrationExpressionBase
    {
        public UpdateDataExpression()
        {
        }

        public UpdateDataExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
        }

        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public List<KeyValuePair<string, object>> Set { get; set; }
        public List<KeyValuePair<string, object>> Where { get; set; }
        public bool IsAllRows { get; set; }

        public override string ToString()
        {
            var updateItems = new List<string>();
            var whereClauses = new List<string>();

            foreach (var item in Set)
            {
                updateItems.Add(string.Format("{0} = {1}",
                                              SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(item.Key),
                                              SyntaxConfig.SqlSyntaxProvider.GetQuotedValue(item.Value.ToString())));
            }

            if (IsAllRows)
            {
                whereClauses.Add("1 = 1");
            }
            else
            {
                foreach (var item in Where)
                {
                    whereClauses.Add(string.Format("{0} {1} {2}",
                                                   SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(item.Key),
                                                   item.Value == null ? "IS" : "=",
                                                   SyntaxConfig.SqlSyntaxProvider.GetQuotedValue(item.Value.ToString())));
                }
            }
            return string.Format(SyntaxConfig.SqlSyntaxProvider.UpdateData,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                 string.Join(", ", updateItems.ToArray()), 
                                 string.Join(" AND ", whereClauses.ToArray()));
        }
    }
}
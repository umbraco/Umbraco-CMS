using System.Collections.Generic;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Update.Expressions
{
    public class UpdateDataExpression : MigrationExpressionBase
    {
        public UpdateDataExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) 
            : base(current, databaseProviders, sqlSyntax)
        {
        }

        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public List<KeyValuePair<string, object>> Set { get; set; }
        public List<KeyValuePair<string, object>> Where { get; set; }
        public bool IsAllRows { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            var updateItems = new List<string>();
            var whereClauses = new List<string>();

            foreach (var item in Set)
            {
                updateItems.Add(string.Format("{0} = {1}",
                                              SqlSyntax.GetQuotedColumnName(item.Key),
                                              GetQuotedValue(item.Value)));
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
                                                   SqlSyntax.GetQuotedColumnName(item.Key),
                                                   item.Value == null ? "IS" : "=",
                                                   GetQuotedValue(item.Value)));
                }
            }
            return string.Format(SqlSyntax.UpdateData,
                                 SqlSyntax.GetQuotedTableName(TableName),
                                 string.Join(", ", updateItems.ToArray()), 
                                 string.Join(" AND ", whereClauses.ToArray()));
        }
    }
}
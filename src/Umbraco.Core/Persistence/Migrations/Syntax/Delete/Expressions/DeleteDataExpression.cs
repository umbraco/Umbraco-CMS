using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteDataExpression : MigrationExpressionBase
    {
        private readonly List<DeletionDataDefinition> _rows = new List<DeletionDataDefinition>();

        public DeleteDataExpression()
        {
        }

        public DeleteDataExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
        }

        public virtual string SchemaName { get; set; }
        public string TableName { get; set; }
        public virtual bool IsAllRows { get; set; }

        public List<DeletionDataDefinition> Rows
        {
            get { return _rows; }
        }

        public override string ToString()
        {
            var deleteItems = new List<string>();

            if (IsAllRows)
            {
                deleteItems.Add(string.Format(SqlSyntaxContext.SqlSyntaxProvider.DeleteData, SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(TableName), "1 = 1"));
            }
            else
            {
                foreach (var row in Rows)
                {
                    var whereClauses = new List<string>();
                    foreach (KeyValuePair<string, object> item in row)
                    {
                        whereClauses.Add(string.Format("{0} {1} {2}",
                                                       SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(item.Key),
                                                       item.Value == null ? "IS" : "=",
                                                       GetQuotedValue(item.Value)));
                    }

                    deleteItems.Add(string.Format(SqlSyntaxContext.SqlSyntaxProvider.DeleteData,
                                                  SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                                  String.Join(" AND ", whereClauses.ToArray())));
                }
            }

            return String.Join("; ", deleteItems.ToArray());
        }
    }
}
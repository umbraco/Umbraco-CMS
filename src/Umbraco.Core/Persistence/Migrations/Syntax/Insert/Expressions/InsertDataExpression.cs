using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert.Expressions
{
    public class InsertDataExpression : MigrationExpressionBase
    {
        private readonly List<InsertionDataDefinition> _rows = new List<InsertionDataDefinition>();
        
        public InsertDataExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) 
            : base(current, databaseProviders, sqlSyntax)
        {
        }

        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public bool EnabledIdentityInsert { get; set; }

        public List<InsertionDataDefinition> Rows
        {
            get { return _rows; }
        }
        
        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;
            
            var sb = new StringBuilder();

            if (EnabledIdentityInsert && SqlSyntax.SupportsIdentityInsert())
            {
                sb.AppendLine(string.Format("SET IDENTITY_INSERT {0} ON;", SqlSyntax.GetQuotedTableName(TableName)));
                if (CurrentDatabaseProvider == DatabaseProviders.SqlServer || CurrentDatabaseProvider == DatabaseProviders.SqlServerCE)
                {
                    sb.AppendLine("GO");
                }
            }

            try
            {
                foreach (var item in Rows)
                {
                    var cols = "";
                    var vals = "";
                    foreach (var keyVal in item)
                    {
                        cols += SqlSyntax.GetQuotedColumnName(keyVal.Key) + ",";
                        vals += GetQuotedValue(keyVal.Value) + ",";
                    }
                    cols = cols.TrimEnd(',');
                    vals = vals.TrimEnd(',');


                    var sql = string.Format(SqlSyntax.InsertData,
                                  SqlSyntax.GetQuotedTableName(TableName),
                                  cols, vals);

                    sb.AppendLine(string.Format("{0};", sql));
                    if (CurrentDatabaseProvider == DatabaseProviders.SqlServer || CurrentDatabaseProvider == DatabaseProviders.SqlServerCE)
                    {
                        sb.AppendLine("GO");
                    }
                }
            }
            finally
            {
                if (EnabledIdentityInsert && SqlSyntax.SupportsIdentityInsert())
                {
                    sb.AppendLine(string.Format("SET IDENTITY_INSERT {0} OFF;", SqlSyntax.GetQuotedTableName(TableName)));
                    if (CurrentDatabaseProvider == DatabaseProviders.SqlServer || CurrentDatabaseProvider == DatabaseProviders.SqlServerCE)
                    {
                        sb.AppendLine("GO");
                    }
                }
            }

            return sb.ToString();
        }

        
    }
}
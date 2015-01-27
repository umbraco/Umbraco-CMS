using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert.Expressions
{
    public class InsertDataExpression : MigrationExpressionBase
    {
        private readonly List<InsertionDataDefinition> _rows = new List<InsertionDataDefinition>();

        public InsertDataExpression()
        {
        }

        public InsertDataExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
        }

        public string SchemaName { get; set; }
        public string TableName { get; set; }

       
        public List<InsertionDataDefinition> Rows
        {
            get { return _rows; }
        }

        

        public override string ToString()
        {
            //TODO: This works for single insertion entries, not sure if it is valid for bulk insert operations!!!

            if (IsExpressionSupported() == false)
                return string.Empty;

            var insertItems = new List<string>();
            
            foreach (var item in Rows)
            {
                var cols = "";
                var vals = "";
                foreach (var keyVal in item)
                {
                    cols += keyVal.Key + ",";
                    vals += GetQuotedValue(keyVal.Value) + ",";
                }
                cols = cols.TrimEnd(',');
                vals = vals.TrimEnd(',');


                var sql = string.Format(SqlSyntaxContext.SqlSyntaxProvider.InsertData,
                              SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(TableName),
                              cols, vals);
                
                insertItems.Add(sql);
            }

            return string.Join(",", insertItems);
        }

        private string GetQuotedValue(object val)
        {
            var type = val.GetType();

            switch (Type.GetTypeCode(type))
            {                
                case TypeCode.Boolean:
                    return ((bool) val) ? "1" : "0";
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:             
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return val.ToString();
                default:
                    return SqlSyntaxContext.SqlSyntaxProvider.GetQuotedValue(val.ToString());
            }
        }
    }
}
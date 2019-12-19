using System;
using System.Collections.Generic;
using System.Data;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    public class DbTypes
    {
        public DbType DbType;
        public string TextDefinition;
        public bool ShouldQuoteValue;
        public Dictionary<Type, string> ColumnTypeMap = new Dictionary<Type, string>();
        public Dictionary<Type, DbType> ColumnDbTypeMap = new Dictionary<Type, DbType>();

        public void Set<T>(DbType dbType, string fieldDefinition)
        {
            DbType = dbType;
            TextDefinition = fieldDefinition;
            ShouldQuoteValue = fieldDefinition != "INTEGER"
                && fieldDefinition != "BIGINT"
                && fieldDefinition != "DOUBLE"
                && fieldDefinition != "DECIMAL"
                && fieldDefinition != "BOOL";

            ColumnTypeMap[typeof(T)] = fieldDefinition;
            ColumnDbTypeMap[typeof(T)] = dbType;
        }
    }
}

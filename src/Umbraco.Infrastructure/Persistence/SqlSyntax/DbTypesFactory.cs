using System;
using System.Collections.Generic;
using System.Data;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax
{
    internal class DbTypesFactory
    {
        private readonly Dictionary<Type, string> _columnTypeMap = new Dictionary<Type, string>();
        private readonly Dictionary<Type, DbType> _columnDbTypeMap = new Dictionary<Type, DbType>();

        public void Set<T>(DbType dbType, string fieldDefinition)
        {
            _columnTypeMap[typeof(T)] = fieldDefinition;
            _columnDbTypeMap[typeof(T)] = dbType;
        }

        public DbTypes Create() => new DbTypes(_columnTypeMap, _columnDbTypeMap);
    }
}

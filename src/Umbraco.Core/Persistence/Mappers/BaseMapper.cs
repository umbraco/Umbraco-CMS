using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Persistence.Mappers
{
    public abstract class BaseMapper : IDiscoverable
    {
        protected BaseMapper()
        {
            Build();
        }

        internal abstract ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache { get; }

        private void Build()
        {
            BuildMap();
        }

        protected abstract void BuildMap();

        internal string Map(ISqlSyntaxProvider sqlSyntax, string propertyName, bool throws = false)
        {
            if (PropertyInfoCache.TryGetValue(propertyName, out var dtoTypeProperty))
                return GetColumnName(sqlSyntax, dtoTypeProperty.Type, dtoTypeProperty.PropertyInfo);

            if (throws)
                throw new InvalidOperationException("Could not get the value with the key " + propertyName + " from the property info cache, keys available: " + string.Join(", ", PropertyInfoCache.Keys));

            return string.Empty;
        }

        internal void CacheMap<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var property = ResolveMapping(sourceMember, destinationMember);
            PropertyInfoCache.AddOrUpdate(property.SourcePropertyName, property, (x, y) => property);
        }

        internal DtoMapModel ResolveMapping<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var source = ExpressionHelper.FindProperty(sourceMember);
            var destination = (PropertyInfo) ExpressionHelper.FindProperty(destinationMember).Item1;

            if (destination == null)
            {
                throw new InvalidOperationException("The 'destination' returned was null, cannot resolve the mapping");
            }

            return new DtoMapModel(typeof(TDestination), destination, source.Item1.Name);
        }

        internal virtual string GetColumnName(ISqlSyntaxProvider sqlSyntax, Type dtoType, PropertyInfo dtoProperty)
        {
            var tableNameAttribute = dtoType.FirstAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute.Value;

            var columnAttribute = dtoProperty.FirstAttribute<ColumnAttribute>();
            var columnName = columnAttribute.Name;

            var columnMap = sqlSyntax.GetQuotedTableName(tableName) + "." + sqlSyntax.GetQuotedColumnName(columnName);
            return columnMap;
        }
    }
}

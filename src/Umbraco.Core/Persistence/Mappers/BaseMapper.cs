using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using umbraco.interfaces;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    public abstract class BaseMapper : IDiscoverable
    {
        private readonly ISqlSyntaxProvider _sqlSyntax;

        protected BaseMapper() : this(SqlSyntaxContext.SqlSyntaxProvider)
        {            
        }

        protected BaseMapper(ISqlSyntaxProvider sqlSyntax)
        {
            _sqlSyntax = sqlSyntax;
        }

        internal abstract ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache { get; }

        internal abstract void BuildMap();

        internal string Map(string propertyName, bool throws = false)
        {
            DtoMapModel dtoTypeProperty;
            if (PropertyInfoCache.TryGetValue(propertyName, out dtoTypeProperty))
            {
                return GetColumnName(dtoTypeProperty.Type, dtoTypeProperty.PropertyInfo);
            }
            else
            {
                if (throws)
                {
                    throw new InvalidOperationException("Could not get the value with the key " + propertyName + " from the property info cache, keys available: " + string.Join(", ", PropertyInfoCache.Keys));
                }
                return string.Empty;
            }
        }

        internal void CacheMap<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var property = ResolveMapping(sourceMember, destinationMember);
            PropertyInfoCache.AddOrUpdate(property.SourcePropertyName, property, (x, y) => property);
        }

        internal DtoMapModel ResolveMapping<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var source = ExpressionHelper.FindProperty(sourceMember);
            var destination = (PropertyInfo)ExpressionHelper.FindProperty(destinationMember);

            if (destination == null)
            {
                throw new InvalidOperationException("The 'destination' returned was null, cannot resolve the mapping");
            }

            return new DtoMapModel(typeof(TDestination), destination, source.Name);
        }

        internal virtual string GetColumnName(Type dtoType, PropertyInfo dtoProperty)
        {
            var tableNameAttribute = dtoType.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute.Value;

            var columnAttribute = dtoProperty.FirstAttribute<ColumnAttribute>();
            string columnName = columnAttribute.Name;

            string columnMap = string.Format("{0}.{1}",
                                             _sqlSyntax.GetQuotedTableName(tableName),
                                             _sqlSyntax.GetQuotedColumnName(columnName));
            return columnMap;
        }
    }
}
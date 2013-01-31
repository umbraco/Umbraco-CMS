using System;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    internal abstract class BaseMapper
    {
        internal abstract void BuildMap();

        internal abstract string Map(string propertyName);

        internal abstract void CacheMap<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember);

        internal DtoMapModel ResolveMapping<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var source = ExpressionHelper.FindProperty(sourceMember);
            var destination = ExpressionHelper.FindProperty(destinationMember) as PropertyInfo;

            return new DtoMapModel(typeof(TDestination), destination, source.Name);
        }

        internal virtual string GetColumnName(Type dtoType, PropertyInfo dtoProperty)
        {
            var tableNameAttribute = dtoType.FirstAttribute<TableNameAttribute>();
            string tableName = tableNameAttribute.Value;

            var columnAttribute = dtoProperty.FirstAttribute<ColumnAttribute>();
            string columnName = columnAttribute.Name;

            string columnMap = string.Format("{0}.{1}",
                                             SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName),
                                             SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(columnName));
            return columnMap;
        }
    }
}
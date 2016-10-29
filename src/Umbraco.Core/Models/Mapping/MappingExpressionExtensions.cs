using AutoMapper;

namespace Umbraco.Core.Models.Mapping
{
    internal static class MappingExpressionExtensions
    {
        /// <summary>
        /// Ignores all unmapped members by default - Use with caution!
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDest> IgnoreAllUnmapped<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
        {
            expression.ForAllMembers(opt => opt.Ignore());
            return expression;
        }
    }
}
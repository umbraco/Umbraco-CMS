using AutoMapper;

namespace Umbraco.Core
{
    /// <summary>
    /// Helper class for static model mapping with automapper
    /// </summary>
    internal static class ModelMapperHelper
    {
        internal static IMappingExpression<TSource, TSource> SelfMap<TSource>(this IConfiguration config)
        {
            return config.CreateMap<TSource, TSource>();
        }
    }
}
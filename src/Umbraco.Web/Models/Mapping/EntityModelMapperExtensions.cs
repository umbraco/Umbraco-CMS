using AutoMapper;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Mapping extension methods for re-use with other mappers (saves code duplication)
    /// </summary>
    internal static class EntityModelMapperExtensions
    {
        /// <summary>
        /// Ignores readonly properties and the date values
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDest> IgnoreDeletableEntityCommonProperties<TSource, TDest>(
            this IMappingExpression<TSource, TDest> mapping)            
            where TDest: IDeletableEntity           
        {
            return mapping
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.DeletedDate, map => map.Ignore());
        }

        /// <summary>
        /// Ignores readonly properties and the date values
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDest> IgnoreEntityCommonProperties<TSource, TDest>(
            this IMappingExpression<TSource, TDest> mapping)
            where TDest : IEntity
        {
            return mapping
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .ForMember(dest => dest.CreateDate, map => map.Ignore())
                .ForMember(dest => dest.UpdateDate, map => map.Ignore());
        }

    }
}

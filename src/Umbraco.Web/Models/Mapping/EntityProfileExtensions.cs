using AutoMapper;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Mapping extension methods for re-use with other mappers (saves code duplication)
    /// </summary>
    internal static class EntityProfileExtensions
    {
        /// <summary>
        /// Ignores readonly properties and the date values
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDest> IgnoreDeletableEntityCommonProperties<TSource, TDest>(this IMappingExpression<TSource, TDest> mapping)
            where TDest: IDeletableEntity
        {
            return mapping
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore());
        }

        /// <summary>
        /// Ignores readonly properties and the date values
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDest> IgnoreEntityCommonProperties<TSource, TDest>(this IMappingExpression<TSource, TDest> mapping)
            where TDest : IEntity
        {
            return mapping
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.Ignore());
        }
    }
}

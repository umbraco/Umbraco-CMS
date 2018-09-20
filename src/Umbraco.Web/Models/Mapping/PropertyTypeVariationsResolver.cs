using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using ContentVariation = Umbraco.Core.Models.ContentVariation;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Returns the <see cref="ContentVariation"/> for a <see cref="PropertyType"/>
    /// </summary>
    internal class PropertyTypeVariationsResolver: IValueResolver<PropertyTypeBasic, PropertyType, ContentVariation>
    {
        public ContentVariation Resolve(PropertyTypeBasic source, PropertyType destination, ContentVariation destMember, ResolutionContext context)
        {
            return source.AllowCultureVariant ? ContentVariation.Culture : ContentVariation.Nothing;
        }
    }
}

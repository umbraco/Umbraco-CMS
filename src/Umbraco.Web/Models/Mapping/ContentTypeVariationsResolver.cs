using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using ContentVariation = Umbraco.Core.Models.ContentVariation;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentTypeVariationsResolver<TSource, TSourcePropertyType, TDestination> : IValueResolver<TSource, TDestination, ContentVariation>
        where TSource : ContentTypeSave<TSourcePropertyType>
        where TDestination : IContentTypeComposition
        where TSourcePropertyType : PropertyTypeBasic
    {
        public ContentVariation Resolve(TSource source, TDestination destination, ContentVariation destMember, ResolutionContext context)
        {
            //this will always be the case, a content type will always be allowed to be invariant
            var result = ContentVariation.Nothing;

            if (source.AllowCultureVariant)
                result |= ContentVariation.Culture;

            return result;
        }
    }
}

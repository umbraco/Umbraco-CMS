using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using ContentVariation = Umbraco.Core.Models.ContentVariation;

namespace Umbraco.Web.Models.Mapping
{
    internal class PropertyTypeVariationsResolver: IValueResolver<PropertyTypeBasic, PropertyType, ContentVariation>
    {
        public ContentVariation Resolve(PropertyTypeBasic source, PropertyType destination, ContentVariation destMember, ResolutionContext context)
        {
            //this will always be the case, a content type will always be allowed to be invariant
            var result = ContentVariation.InvariantNeutral;

            if (source.AllowCultureVariant)
            {
                result |= ContentVariation.CultureNeutral;
            }

            return result;
        }
    }
}

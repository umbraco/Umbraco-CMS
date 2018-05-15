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
            //A property type should only be one type of culture variation.
            //If a property type allows both variant and invariant then it generally won't be able to save because validation
            //occurs when performing something like IContent.TryPublishAllValues and it will result in validation problems because
            //the invariant value will not be set since in the UI only the variant values are edited if it supports it.
            var result = source.AllowCultureVariant ? ContentVariation.CultureNeutral : ContentVariation.InvariantNeutral;
            return result;
        }
    }
}

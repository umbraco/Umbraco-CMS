using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class PropertyGroupDisplayResolver<TSource, TPropertyTypeDisplay> : ValueResolver<IEnumerable<TSource>, IEnumerable<PropertyGroupDisplay<TPropertyTypeDisplay>>>
        where TSource : ContentTypeSave 
        where TPropertyTypeDisplay : PropertyTypeDisplay
    {
        protected override IEnumerable<PropertyGroupDisplay<TPropertyTypeDisplay>> ResolveCore(IEnumerable<TSource> source)
        {
            return source.Select(Mapper.Map<PropertyGroupDisplay<TPropertyTypeDisplay>>);
        }
    }
}
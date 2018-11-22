using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class PropertyGroupDisplayResolver<TSource, TPropertyTypeSource, TPropertyTypeDestination>
        where TSource : ContentTypeSave<TPropertyTypeSource>
        where TPropertyTypeDestination : PropertyTypeDisplay
        where TPropertyTypeSource : PropertyTypeBasic
    {
        public IEnumerable<PropertyGroupDisplay<TPropertyTypeDestination>> Resolve(TSource source)
        {
            return source.Groups.Select(Mapper.Map<PropertyGroupDisplay<TPropertyTypeDestination>>);
        }
    }
}

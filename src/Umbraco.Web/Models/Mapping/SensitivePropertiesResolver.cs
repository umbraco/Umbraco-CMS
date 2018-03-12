using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class SensitivePropertiesResolver<TSource> : IValueResolver where TSource : IContentBase
    {
        public ResolutionResult Resolve(ResolutionResult source)
        {
            if (source.Value != null && (source.Value is TSource) == false)
                throw new AutoMapperMappingException(string.Format("Value supplied is of type {0} but expected {1}.\nChange the value resolver source type, or redirect the source value supplied to the value resolver using FromMember.", new object[]
                {
                    source.Value.GetType(),
                    typeof (TSource)
                }));
            return source.New(
                //perform the mapping with the current umbraco context
                ResolveCore(source.Context.GetUmbracoContext(), (TSource)source.Value), typeof(IEnumerable<ContentPropertyDisplay>));
        }

        protected virtual IEnumerable<ContentPropertyDisplay> ResolveCore(UmbracoContext umbracoContext, TSource content)
        {
            var properties = new List<Property>();

            properties.AddRange(content.Properties);

            //map the properties
            var mappedProperties = MapProperties(umbracoContext, content, properties);

            return mappedProperties;
        }

        protected virtual List<ContentPropertyDisplay> MapProperties(UmbracoContext umbracoContext, IContentBase content, List<Property> properties)
        {
            var result = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(
                    // Sort properties so items from different compositions appear in correct order (see U4-9298). Map sorted properties.
                    properties.OrderBy(prop => prop.PropertyType.SortOrder))
                .ToList();

            return result;
        }
    }
}

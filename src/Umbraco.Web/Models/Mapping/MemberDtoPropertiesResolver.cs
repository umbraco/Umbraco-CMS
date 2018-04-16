using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// This ensures that the custom membership provider properties are not mapped - these property values are controller by the membership provider
    /// </summary>
    /// <remarks>
    /// Because these properties don't exist on the form, if we don't remove them for this map we'll get validation errors when posting data
    /// </remarks>
    internal class MemberDtoPropertiesResolver
    {
        public IEnumerable<ContentPropertyDto> Resolve(IMember source)
        {
            var defaultProps = Constants.Conventions.Member.GetStandardPropertyTypeStubs();

            //remove all membership properties, these values are set with the membership provider.
            var exclude = defaultProps.Select(x => x.Value.Alias).ToArray();

            return source.Properties
                .Where(x => exclude.Contains(x.Alias) == false)
                .Select(Mapper.Map<Property, ContentPropertyDto>);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Used to map property values when saving content/media/members
    /// </summary>
    /// <remarks>
    /// This is only used during mapping operations, it is not used for angular purposes
    /// </remarks>
    [UmbracoVolatile]
    public class ContentPropertyCollectionDto : IContentProperties<ContentPropertyDto>
    {
        public ContentPropertyCollectionDto()
        {
            Properties = Enumerable.Empty<ContentPropertyDto>();
        }

        public IEnumerable<ContentPropertyDto> Properties { get; set; }
    }
}

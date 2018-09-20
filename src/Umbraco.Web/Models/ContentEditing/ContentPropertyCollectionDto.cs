﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Used to map property values when saving content/media/members
    /// </summary>
    /// <remarks>
    /// This is only used during mapping operations, it is not used for angular purposes
    /// </remarks>
    internal class ContentPropertyCollectionDto : IContentProperties<ContentPropertyDto>
    {
        public ContentPropertyCollectionDto()
        {
            Properties = Enumerable.Empty<ContentPropertyDto>();
        }

        public IEnumerable<ContentPropertyDto> Properties { get; set; }
    }
}

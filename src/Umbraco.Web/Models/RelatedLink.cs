﻿﻿using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{
    public class RelatedLink : RelatedLinkBase
    {
        public int? Id { get; internal set; }
        internal bool IsDeleted { get; set; }
        public IPublishedContent Content { get; set; }
    }
}

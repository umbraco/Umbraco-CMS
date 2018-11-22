using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.media;

namespace umbraco.MacroEngines
{
    //for backwards compatibility only
    public class DynamicMedia : DynamicNode
    {
        public DynamicMedia(DynamicBackingItem item) : base(item) { }

        public DynamicMedia(int mediaId) : base(mediaId, DynamicBackingItemType.Media) { }
        public DynamicMedia(Media media) : base(media.Id, DynamicBackingItemType.Media) { }
        public DynamicMedia(string mediaId) : base(mediaId) { }
        public DynamicMedia(object mediaId) : base(mediaId) { }
        public DynamicMedia() : base() { }

    }
}

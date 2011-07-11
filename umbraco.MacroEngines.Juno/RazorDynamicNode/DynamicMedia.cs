using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
    //for backwards compatibility only
    public class DynamicMedia : DynamicNode
    {
        public DynamicMedia(DynamicBackingItem item) : base(item) { }
    }
}

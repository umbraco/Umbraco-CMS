using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic
{
    public abstract class ProviderBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

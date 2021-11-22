using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.PublishedCache.NuCache
{

    [ComposeAfter(typeof(NuCacheComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class NuCacheSerializerComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<NuCacheSerializerComponent>();
        }
    }
}

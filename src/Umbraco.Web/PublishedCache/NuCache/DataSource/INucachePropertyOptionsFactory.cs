using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    public interface INuCachePropertyOptionsFactory
    {
        NuCachePropertyCompressionOptions GetNuCachePropertyOptions();
    }
}

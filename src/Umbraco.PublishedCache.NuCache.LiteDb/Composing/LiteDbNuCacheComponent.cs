using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.PublishedCache.NuCache.LiteDb.Composing
{
    public class LiteDbNuCacheComponent : IComponent
    {
        public void Initialize()
        {
          
            //.Ignore(x => x.Node) // ignore this property (do not store)

            ;
        }

       

        public void Terminate()
        {
        }
    }
}

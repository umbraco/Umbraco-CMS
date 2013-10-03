using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{

    //TODO: Why do we have this ? If we want a URL why isn't it just on the content controller ?

    [PluginController("UmbracoApi")]
    public class PublishedContentController : UmbracoAuthorizedJsonController
    {
        
        public string GetNiceUrl(int id)
        {
            return Umbraco.NiceUrl(id);
        }
    }
}

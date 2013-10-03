using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class PublishedContentController : UmbracoAuthorizedJsonController
    {
        UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);
        public string GetNiceUrl(int id)
        {
            return helper.NiceUrl(id);
        }
    }
}

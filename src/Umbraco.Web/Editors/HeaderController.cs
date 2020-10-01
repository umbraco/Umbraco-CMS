using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class HeaderController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<HeaderApp> GetApps()
        {
            //TODO: Get this from somewhere else than here
            return new List<HeaderApp>
            {
                new HeaderApp{Alias = "help", Weight = -100, View = "views/header/apps/search/search.html"},
                new HeaderApp{Alias = "search", Weight = -200, View = "views/header/apps/help/help.html"}
            }.OrderByDescending(it => it.Weight);
        }
    }
}

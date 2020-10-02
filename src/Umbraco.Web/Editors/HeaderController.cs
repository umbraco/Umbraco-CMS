using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Header;
using Umbraco.Web.HeaderApps;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class HeaderController : UmbracoAuthorizedJsonController
    {
        private readonly HeaderAppFactoryCollection _headerAppDefinitions;

        public HeaderController(HeaderAppFactoryCollection headerAppDefinitions)
        {
            _headerAppDefinitions = headerAppDefinitions;
        }

        public IEnumerable<HeaderApp> GetApps()
        {
            return _headerAppDefinitions.GetHeaderAppsFor();
        }
    }
}

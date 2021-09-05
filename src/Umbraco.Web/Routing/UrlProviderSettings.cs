using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    public class UrlProviderSettings
    {
        public UrlMode Mode { get; set; }
        public UrlProviderSettings(IWebRoutingSection routingSettings)
        {
            var provider = UrlMode.Auto;
            Mode = provider;

            if (Enum<UrlMode>.TryParse(routingSettings.UrlProviderMode, out provider))
            {
                Mode = provider;
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ServiceWorkerConfigElement : RawXmlConfigurationElement, IServiceWorkerSection
    {
        public IEnumerable<string> Domains
        {
            get
            {
                if (RawXml != null)
                {
                    var domains = RawXml.Elements("domain");
                    if (domains != null)
                    {
                        var result = domains.Select(a => a.Value).Where(d => string.IsNullOrWhiteSpace(d) == false);
                        return result;
                    }
                }
                return Enumerable.Empty<string>();

            }
        }
    }
}

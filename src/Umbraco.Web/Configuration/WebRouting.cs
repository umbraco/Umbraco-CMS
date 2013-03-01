using System.ComponentModel;
using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Configuration
{
    /// <summary>
    /// The Web.Routing settings section.
    /// </summary>
    [ConfigurationKey("web.routing", ConfigurationKeyType.Umbraco)]
    internal class WebRouting : UmbracoConfigurationSection
    {
    }
}

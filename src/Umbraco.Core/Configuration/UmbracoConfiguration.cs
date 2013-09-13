using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// The gateway to all umbraco configuration
    /// </summary>
    public class UmbracoConfiguration
    {
        //TODO: Add other configurations here !

        public IUmbracoSettings UmbracoSettings { get; private set; }

        public UmbracoConfiguration(IUmbracoSettings umbracoSettings)
        {
            UmbracoSettings = umbracoSettings;
        }
    }
}
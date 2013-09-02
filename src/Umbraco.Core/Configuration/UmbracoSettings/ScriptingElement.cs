using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScriptingElement : ConfigurationElement
    {
        [ConfigurationProperty("razor")]
        public RazorElement Razor
        {
            get { return (RazorElement) base["razor"]; }
        }

    }
}
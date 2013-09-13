using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScriptingElement : ConfigurationElement, IScripting
    {
        [ConfigurationProperty("razor")]
        internal RazorElement Razor
        {
            get { return (RazorElement) base["razor"]; }
        }


        IRazor IScripting.Razor
        {
            get { return Razor; }
        }
    }
}
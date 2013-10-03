using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class AreasElement : ConfigurationElement, IArea
    {
        [ConfigurationProperty("area", IsRequired = true)]
        public InnerTextConfigurationElement<string> AreaName
        {
            get { return (InnerTextConfigurationElement<string>)this["area"]; }
        }
        
        string IArea.AreaName
        {
            get { return AreaName; }
        }
    }
}
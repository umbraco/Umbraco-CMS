using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class ControlElement : InnerTextConfigurationElement<string>, IControl
    {
        [ConfigurationProperty("showOnce", DefaultValue = false)]
        public bool ShowOnce
        {
            get { return (bool)this["showOnce"]; }
        }

        [ConfigurationProperty("addPanel", DefaultValue = true)]
        public bool AddPanel
        {
            get { return (bool)this["addPanel"]; }
        }

        [ConfigurationProperty("panelCaption", DefaultValue = "")]
        public string PanelCaption
        {
            get { return (string)this["panelCaption"]; }
        }

        public string ControlPath 
        { 
            get { return Value; }
        }
    }
}
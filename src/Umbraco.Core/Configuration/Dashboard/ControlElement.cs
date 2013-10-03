using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class ControlElement : RawXmlConfigurationElement, IControl
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

        [ConfigurationProperty("access")]
        public AccessElement Access
        {
            get { return (AccessElement)this["access"]; }
        }

        public string ControlPath 
        { 
            get
            {
                //we need to return the first (and only) text element of the children (wtf... who designed this configuration ! :P )
                var txt = RawXml.Nodes().OfType<XText>().FirstOrDefault();
                if (txt == null)
                {
                    throw new ConfigurationErrorsException("The control element must contain a text node indicating the control path");
                }
                return txt.Value;
            }
        }
    }
}
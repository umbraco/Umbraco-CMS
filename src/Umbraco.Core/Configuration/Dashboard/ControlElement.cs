using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace Umbraco.Core.Configuration.Dashboard
{

    internal class ControlElement : RawXmlConfigurationElement, IDashboardControl
    {
        public string PanelCaption
        {
            get
            {
                var panelCaption = RawXml.Attribute("panelCaption");
                return panelCaption == null ? "" : panelCaption.Value;
            }
        }

        public AccessElement Access
        {
            get
            {
                var access = RawXml.Element("access");
                return access == null ? new AccessElement() : new AccessElement(access);
            }
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
                return txt.Value.Trim();
            }
        }

        IAccess IDashboardControl.AccessRights => Access;
    }
}

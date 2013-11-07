using System;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace Umbraco.Core.Configuration.Dashboard
{
    
    internal class ControlElement : RawXmlConfigurationElement, IDashboardControl
    {
        public bool ShowOnce
        {
            get
            {
                return RawXml.Attribute("showOnce") == null
                           ? false
                           : bool.Parse(RawXml.Attribute("showOnce").Value);
            }
        }

        public bool AddPanel
        {
            get
            {
                return RawXml.Attribute("addPanel") == null
                           ? true
                           : bool.Parse(RawXml.Attribute("addPanel").Value);
            }
        }

        public string PanelCaption
        {
            get
            {
                return RawXml.Attribute("panelCaption") == null
                           ? ""
                           : RawXml.Attribute("panelCaption").Value;
            }
        }

        public AccessElement Access
        {
            get
            {
                var access = RawXml.Element("access");
                if (access == null)
                {
                    return new AccessElement();
                }
                return new AccessElement(access);
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


        IAccess IDashboardControl.AccessRights
        {
            get { return Access; }
        }
    }
}
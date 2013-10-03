using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class TabElement : ConfigurationElement, ITab
    {
        [ConfigurationProperty("caption", IsRequired = true)]
        public string Caption
        {
            get { return (string)this["caption"]; }
        }
        
        [ConfigurationProperty("access")]
        public AccessElement Access
        {
            get { return (AccessElement)this["access"]; }
        }

        [ConfigurationCollection(typeof(ControlCollection), AddItemName = "control")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ControlCollection ControlCollection
        {
            get { return (ControlCollection)base[""]; }
            set { base[""] = value; }
        }
        
        IEnumerable<IControl> ITab.Controls
        {
            get { return ControlCollection; }            
        }

        IAccess ITab.AccessRights
        {
            get { return Access; }
        }
    }
}
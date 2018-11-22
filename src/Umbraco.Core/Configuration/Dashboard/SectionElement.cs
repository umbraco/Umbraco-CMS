using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class SectionElement : ConfigurationElement, ISection
    {
        [ConfigurationProperty("alias", IsRequired = true)]
        public string Alias
        {
            get { return (string) this["alias"]; }
        }

        [ConfigurationProperty("areas", IsRequired = true)]
        public AreasElement Areas
        {
            get { return (AreasElement)this["areas"]; }
        }
        
        [ConfigurationProperty("access")]
        public AccessElement Access
        {
            get { return (AccessElement)this["access"]; }
        }

        [ConfigurationCollection(typeof(SectionCollection), AddItemName = "tab")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public TabCollection TabCollection
        {
            get { return (TabCollection)base[""]; }
            set { base[""] = value; }
        }

        IEnumerable<IDashboardTab> ISection.Tabs
        {
            get { return TabCollection; }            
        }

        IEnumerable<string> ISection.Areas
        {
            get { return Areas.AreaCollection.Cast<AreaElement>().Select(x => x.Value); }
        }

        IAccess ISection.AccessRights
        {
            get { return Access; }
        }
    }
}
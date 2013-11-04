using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class AreasElement : ConfigurationElement
    {
        [ConfigurationCollection(typeof(SectionCollection), AddItemName = "area")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public AreaCollection AreaCollection
        {
            get { return (AreaCollection)base[""]; }
            set { base[""] = value; }
        }
    }
}
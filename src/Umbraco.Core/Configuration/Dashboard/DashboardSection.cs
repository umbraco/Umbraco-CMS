using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class DashboardSection : ConfigurationSection, IDashboardSection
    {
        [ConfigurationCollection(typeof(SectionCollection), AddItemName = "section")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public SectionCollection SectionCollection
        {
            get { return (SectionCollection)base[""]; }
            set { base[""] = value; }
        }

        IEnumerable<ISection> IDashboardSection.Sections
        {
            get { return SectionCollection; }
        }
    }
}

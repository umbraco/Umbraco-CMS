using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Web.Sections
{
    public class SectionCollection : BuilderCollectionBase<ISection>
    {
        public SectionCollection(IEnumerable<ISection> items)
            : base(items)
        { }
    }
}

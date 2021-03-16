using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Sections
{
    public class SectionCollection : BuilderCollectionBase<ISection>
    {
        public SectionCollection(IEnumerable<ISection> items)
            : base(items)
        { }
    }
}

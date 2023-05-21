using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Sections;

public class SectionCollection : BuilderCollectionBase<ISection>
{
    public SectionCollection(Func<IEnumerable<ISection>> items)
        : base(items)
    {
    }
}

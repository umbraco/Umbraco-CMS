using Umbraco.Cms.Core.Models;
using Umbraco.Search.ValueSet.ValueSetBuilders;

namespace Umbraco.Search.Lifti;

public class DeliveryApiContentInMemoryIndex : UmbracoMemoryIndex<IContent>
{
    public DeliveryApiContentInMemoryIndex(ILiftiIndex? index, IValueSetBuilder<IContent> valueSetBuilder) : base(index, valueSetBuilder)
    {
    }
}

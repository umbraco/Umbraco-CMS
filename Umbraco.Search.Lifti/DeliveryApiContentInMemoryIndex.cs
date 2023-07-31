using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models;
using Umbraco.Search.ValueSet.ValueSetBuilders;

namespace Umbraco.Search.InMemory;

public class DeliveryApiContentInMemoryIndex : UmbracoMemoryIndex<IContent>
{
    public DeliveryApiContentInMemoryIndex(ILiftiIndex? index, IValueSetBuilder<IContent> valueSetBuilder) : base(index, valueSetBuilder)
    {
    }
}

using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Search.InMemory;

public class DeliveryApiContentInMemoryIndex : UmbracoMemoryIndex<IContent>
{
    public DeliveryApiContentInMemoryIndex(IMemoryCache memoryCache, string name) : base(memoryCache, name)
    {
    }
}

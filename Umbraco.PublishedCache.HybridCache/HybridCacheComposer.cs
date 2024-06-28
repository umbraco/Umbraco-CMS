using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public class HybridCacheComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
    }
}

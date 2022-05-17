using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Core.Composing;

public class LazyResolve<T> : Lazy<T>
    where T : class
{
    public LazyResolve(IServiceProvider serviceProvider)
        : base(serviceProvider.GetRequiredService<T>)
    {
    }
}

using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScope : IDisposable
{
    public Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method);
    public void Complete();
}

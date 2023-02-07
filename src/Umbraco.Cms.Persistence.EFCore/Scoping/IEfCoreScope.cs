using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScope : IDisposable
{
    public Guid InstanceId { get; }
    public Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method);
    public Task ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task> method);
    public void Complete();
}

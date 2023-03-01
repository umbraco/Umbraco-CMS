using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Services;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScope : IDisposable
{
    /// <summary>
    /// Instance ID of the current scope.
    /// </summary>
    Guid InstanceId { get; }

    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task<T> ExecuteWithContextAsync<T>(Func<IUmbracoEfCoreDatabase, Task<T>> method);

    public IScopeContext? ScopeContext { get; set; }

    public ILockingMechanism Locks { get; }

    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task ExecuteWithContextAsync<T>(Func<IUmbracoEfCoreDatabase, Task> method);

    /// <summary>
    /// Completes the scope, if this is not call, the transaction will be rolled back.
    /// </summary>
    void Complete();
}

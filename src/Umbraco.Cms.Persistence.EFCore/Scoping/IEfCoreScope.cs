using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;

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
    Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method);

    public IScopeContext? ScopeContext { get; set; }

    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task> method);

    /// <summary>
    /// Completes the scope, if this is not call, the transaction will be rolled back.
    /// </summary>
    void Complete();

    /// <summary>
    ///     Read-locks some lock objects.
    /// </summary>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    void ReadLock(params int[] lockIds);

    /// <summary>
    ///     Write-locks some lock objects.
    /// </summary>
    /// <param name="lockIds">Array of object identifiers.</param>
    void WriteLock(params int[] lockIds);

    void EagerReadLock(params int[] lockIds);

    void EagerWriteLock(params int[] lockIds);
}

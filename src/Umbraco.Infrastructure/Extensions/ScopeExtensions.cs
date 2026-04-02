using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for managing Umbraco scope lifecycles and operations.
/// </summary>
public static class ScopeExtensions
{
    /// <summary>
    /// Acquires read locks on the specified lock IDs within the given scope.
    /// </summary>
    /// <param name="scope">The scope on which to acquire the read locks.</param>
    /// <param name="lockIds">The collection of lock IDs to acquire read locks for.</param>
    public static void ReadLock(this IScope scope, ICollection<int> lockIds)
    {
        foreach (var lockId in lockIds)
        {
            scope.ReadLock(lockId);
        }
    }

    /// <summary>
    /// Acquires write locks on the specified lock IDs within the given scope.
    /// </summary>
    /// <param name="scope">The scope on which to acquire the write locks.</param>
    /// <param name="lockIds">The collection of lock IDs to acquire write locks for.</param>
    public static void WriteLock(this IScope scope, ICollection<int> lockIds)
    {
        foreach (var lockId in lockIds)
        {
            scope.WriteLock(lockId);
        }
    }
}

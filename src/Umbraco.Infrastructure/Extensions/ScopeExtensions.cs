using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Extensions;

public static class ScopeExtensions
{
    public static void ReadLock(this IScope scope, ICollection<int> lockIds)
    {
        foreach (var lockId in lockIds)
        {
            scope.ReadLock(lockId);
        }
    }

    public static void WriteLock(this IScope scope, ICollection<int> lockIds)
    {
        foreach (var lockId in lockIds)
        {
            scope.WriteLock(lockId);
        }
    }
}

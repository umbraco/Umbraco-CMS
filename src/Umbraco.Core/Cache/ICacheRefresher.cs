using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     The IcacheRefresher Interface is used for load balancing.
/// </summary>
public interface ICacheRefresher : IDiscoverable
{
    Guid RefresherUniqueId { get; }

    string Name { get; }

    void RefreshAll();

    void Refresh(int id);

    void Remove(int id);

    void Refresh(Guid id);
}

/// <summary>
///     Strongly type cache refresher that is able to refresh cache of real instances of objects as well as IDs
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
///     This is much better for performance when we're not running in a load balanced environment so we can refresh the
///     cache
///     against a already resolved object instead of looking the object back up by id.
/// </remarks>
public interface ICacheRefresher<T> : ICacheRefresher
{
    void Refresh(T instance);

    void Remove(T instance);
}

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

public sealed class MacroCacheRefresher : PayloadCacheRefresherBase<MacroCacheRefresherNotification, MacroCacheRefresher.JsonPayload>
{
    public MacroCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer jsonSerializer,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, jsonSerializer, eventAggregator, factory)
    {
    }

    #region Json

    public class JsonPayload
    {
        public JsonPayload(int id, string alias)
        {
            Id = id;
            Alias = alias;
        }

        public int Id { get; }

        public string Alias { get; }
    }

    #endregion

    #region Define

    public static readonly Guid UniqueId = Guid.Parse("7B1E683C-5F34-43dd-803D-9699EA1E98CA");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Macro Cache Refresher";

    #endregion

    #region Refresher

    public override void RefreshAll()
    {
        foreach (var prefix in GetAllMacroCacheKeys())
        {
            AppCaches.RuntimeCache.ClearByKey(prefix);
        }

        ClearAllIsolatedCacheByEntityType<IMacro>();

        base.RefreshAll();
    }

    public override void Refresh(string json)
    {
        JsonPayload[]? payloads = Deserialize(json);

        if (payloads is not null)
        {
            Refresh(payloads);
        }
    }

    public override void Refresh(JsonPayload[] payloads)
    {
        foreach (JsonPayload payload in payloads)
        {
            foreach (var alias in GetCacheKeysForAlias(payload.Alias))
            {
                AppCaches.RuntimeCache.ClearByKey(alias);
            }

            Attempt<IAppPolicyCache?> macroRepoCache = AppCaches.IsolatedCaches.Get<IMacro>();
            if (macroRepoCache)
            {
                macroRepoCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMacro, int>(payload.Id));
                macroRepoCache.Result?.Clear(
                    RepositoryCacheKeys
                        .GetKey<IMacro, string>(payload.Alias)); // Repository caching of macro definition by alias
            }
        }

        base.Refresh(payloads);
    }

    #endregion

    #region Helpers

    internal static string[] GetAllMacroCacheKeys() =>
        new[]
        {
            CacheKeys.MacroContentCacheKey, // macro render cache
            CacheKeys.MacroFromAliasCacheKey, // lookup macro by alias
        };

    internal static string[] GetCacheKeysForAlias(string alias) =>
        GetAllMacroCacheKeys().Select(x => x + alias).ToArray();

    #endregion
}

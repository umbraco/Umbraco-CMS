using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <summary>
///     Provides a base class for all <see cref="IServerMessenger" /> implementations.
/// </summary>
public abstract class ServerMessengerBase : IServerMessenger
{
    protected ServerMessengerBase(bool distributedEnabled) => DistributedEnabled = distributedEnabled;

    protected bool DistributedEnabled { get; set; }

    public abstract void Sync();

    public abstract void SendMessages();

    /// <summary>
    ///     Determines whether to make distributed calls when messaging a cache refresher.
    /// </summary>
    /// <param name="refresher">The cache refresher.</param>
    /// <param name="messageType">The message type.</param>
    /// <returns>true if distributed calls are required; otherwise, false, all we have is the local server.</returns>
    protected virtual bool RequiresDistributed(ICacheRefresher refresher, MessageType messageType) =>
        DistributedEnabled;

    // ensures that all items in the enumerable are of the same type, either int or Guid.
    protected static bool GetArrayType(IEnumerable<object>? ids, out Type? arrayType)
    {
        arrayType = null;
        if (ids == null)
        {
            return true;
        }

        foreach (var id in ids)
        {
            // only int and Guid are supported
            if (id is int == false && id is Guid == false)
            {
                return false;
            }

            // initialize with first item
            if (arrayType == null)
            {
                arrayType = id.GetType();
            }

            // check remaining items
            if (arrayType != id.GetType())
            {
                return false;
            }
        }

        return true;
    }

    #region IServerMessenger

    public void QueueRefresh<TPayload>(ICacheRefresher refresher, TPayload[] payload)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        Deliver(refresher, payload);
    }

    public void PerformRefresh(ICacheRefresher refresher, string jsonPayload)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        if (jsonPayload == null)
        {
            throw new ArgumentNullException(nameof(jsonPayload));
        }

        Deliver(refresher, MessageType.RefreshByJson, json: jsonPayload);
    }

    public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        if (getNumericId == null)
        {
            throw new ArgumentNullException(nameof(getNumericId));
        }

        if (instances == null || instances.Length == 0)
        {
            return;
        }

        Func<T, object> getId = x => getNumericId(x);
        Deliver(refresher, MessageType.RefreshByInstance, getId, instances);
    }

    public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        if (getGuidId == null)
        {
            throw new ArgumentNullException(nameof(getGuidId));
        }

        if (instances == null || instances.Length == 0)
        {
            return;
        }

        Func<T, object> getId = x => getGuidId(x);
        Deliver(refresher, MessageType.RefreshByInstance, getId, instances);
    }

    public void QueueRemove<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        if (getNumericId == null)
        {
            throw new ArgumentNullException(nameof(getNumericId));
        }

        if (instances == null || instances.Length == 0)
        {
            return;
        }

        Func<T, object> getId = x => getNumericId(x);
        Deliver(refresher, MessageType.RemoveByInstance, getId, instances);
    }

    public void QueueRemove(ICacheRefresher refresher, params int[] numericIds)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        if (numericIds == null || numericIds.Length == 0)
        {
            return;
        }

        Deliver(refresher, MessageType.RemoveById, numericIds.Cast<object>());
    }

    public void QueueRefresh(ICacheRefresher refresher, params int[] numericIds)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        if (numericIds == null || numericIds.Length == 0)
        {
            return;
        }

        Deliver(refresher, MessageType.RefreshById, numericIds.Cast<object>());
    }

    public void QueueRefresh(ICacheRefresher refresher, params Guid[] guidIds)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        if (guidIds == null || guidIds.Length == 0)
        {
            return;
        }

        Deliver(refresher, MessageType.RefreshById, guidIds.Cast<object>());
    }

    public void QueueRefreshAll(ICacheRefresher refresher)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        Deliver(refresher, MessageType.RefreshAll);
    }

    // public void PerformNotify(ICacheRefresher refresher, object payload)
    // {
    //    if (servers == null) throw new ArgumentNullException("servers");
    //    if (refresher == null) throw new ArgumentNullException("refresher");

    // Deliver(refresher, payload);
    // }
    #endregion

    #region Deliver

    protected void DeliverLocal<TPayload>(ICacheRefresher refresher, TPayload[] payload)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        StaticApplicationLogging.Logger.LogDebug(
            "Invoking refresher {RefresherType} on local server for message type RefreshByPayload",
            refresher.GetType());

        var payloadRefresher = refresher as IPayloadCacheRefresher<TPayload>;
        if (payloadRefresher == null)
        {
            throw new InvalidOperationException("The cache refresher " + refresher.GetType() + " is not of type " +
                                                typeof(IPayloadCacheRefresher<TPayload>));
        }

        payloadRefresher.Refresh(payload);
    }

    /// <summary>
    ///     Executes the non strongly typed <see cref="ICacheRefresher" /> on the local/current server
    /// </summary>
    /// <param name="refresher"></param>
    /// <param name="messageType"></param>
    /// <param name="ids"></param>
    /// <param name="json"></param>
    /// <remarks>
    ///     Since this is only for non strongly typed <see cref="ICacheRefresher" /> it will throw for message types that by
    ///     instance
    /// </remarks>
    protected void DeliverLocal(ICacheRefresher refresher, MessageType messageType, IEnumerable<object>? ids = null, string? json = null)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        StaticApplicationLogging.Logger.LogDebug(
            "Invoking refresher {RefresherType} on local server for message type {MessageType}", refresher.GetType(), messageType);

        switch (messageType)
        {
            case MessageType.RefreshAll:
                refresher.RefreshAll();
                break;

            case MessageType.RefreshById:
                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        if (id is int)
                        {
                            refresher.Refresh((int)id);
                        }
                        else if (id is Guid)
                        {
                            refresher.Refresh((Guid)id);
                        }
                        else
                        {
                            throw new InvalidOperationException("The id must be either an int or a Guid.");
                        }
                    }
                }

                break;

            case MessageType.RefreshByJson:
                var jsonRefresher = refresher as IJsonCacheRefresher;
                if (jsonRefresher == null)
                {
                    throw new InvalidOperationException("The cache refresher " + refresher.GetType() +
                                                        " is not of type " + typeof(IJsonCacheRefresher));
                }

                if (json is not null)
                {
                    jsonRefresher.Refresh(json);
                }

                break;

            case MessageType.RemoveById:
                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        if (id is int)
                        {
                            refresher.Remove((int)id);
                        }
                        else
                        {
                            throw new InvalidOperationException("The id must be an int.");
                        }
                    }
                }

                break;

            default:
                // Case MessageType.RefreshByInstance:
                // Case MessageType.RemoveByInstance:
                throw new NotSupportedException("Invalid message type " + messageType);
        }
    }

    /// <summary>
    ///     Executes the strongly typed <see cref="ICacheRefresher{T}" /> on the local/current server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="refresher"></param>
    /// <param name="messageType"></param>
    /// <param name="getId"></param>
    /// <param name="instances"></param>
    /// <remarks>
    ///     Since this is only for strongly typed <see cref="ICacheRefresher{T}" /> it will throw for message types that are
    ///     not by instance
    /// </remarks>
    protected void DeliverLocal<T>(ICacheRefresher refresher, MessageType messageType, Func<T, object> getId, IEnumerable<T> instances)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        StaticApplicationLogging.Logger.LogDebug(
            "Invoking refresher {RefresherType} on local server for message type {MessageType}", refresher.GetType(), messageType);

        var typedRefresher = refresher as ICacheRefresher<T>;

        switch (messageType)
        {
            case MessageType.RefreshAll:
                refresher.RefreshAll();
                break;

            case MessageType.RefreshByInstance:
                if (typedRefresher == null)
                {
                    throw new InvalidOperationException("The refresher must be a typed refresher.");
                }

                foreach (T instance in instances)
                {
                    typedRefresher.Refresh(instance);
                }

                break;

            case MessageType.RemoveByInstance:
                if (typedRefresher == null)
                {
                    throw new InvalidOperationException("The cache refresher " + refresher.GetType() +
                                                        " is not a typed refresher.");
                }

                foreach (T instance in instances)
                {
                    typedRefresher.Remove(instance);
                }

                break;

            default:
                // Case MessageType.RefreshById:
                // Case MessageType.RemoveById:
                // Case MessageType.RefreshByJson:
                throw new NotSupportedException("Invalid message type " + messageType);
        }
    }

    //protected void DeliverLocal(ICacheRefresher refresher, object payload)
    //{
    //    if (refresher == null) throw new ArgumentNullException("refresher");

    //    Current.Logger.LogDebug("Invoking refresher {0} on local server for message type Notify",
    //        () => refresher.GetType());

    //    refresher.Notify(payload);
    //}

    protected abstract void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object>? ids = null, string? json = null);

    // Protected abstract void DeliverRemote(ICacheRefresher refresher, object payload);
    protected virtual void Deliver<TPayload>(ICacheRefresher refresher, TPayload[] payload)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        // deliver local
        DeliverLocal(refresher, payload);

        // distribute?
        if (RequiresDistributed(refresher, MessageType.RefreshByJson) == false)
        {
            return;
        }

        // deliver remote
        var json = JsonConvert.SerializeObject(payload);
        DeliverRemote(refresher, MessageType.RefreshByJson, null, json);
    }

    protected virtual void Deliver(ICacheRefresher refresher, MessageType messageType, IEnumerable<object>? ids = null, string? json = null)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        var idsA = ids?.ToArray();

        // deliver local
        DeliverLocal(refresher, messageType, idsA, json);

        // distribute?
        if (RequiresDistributed(refresher, messageType) == false)
        {
            return;
        }

        // deliver remote
        DeliverRemote(refresher, messageType, idsA, json);
    }

    protected virtual void Deliver<T>(ICacheRefresher refresher, MessageType messageType, Func<T, object> getId, IEnumerable<T> instances)
    {
        if (refresher == null)
        {
            throw new ArgumentNullException(nameof(refresher));
        }

        T[] instancesA = instances.ToArray();

        // deliver local
        DeliverLocal(refresher, messageType, getId, instancesA);

        // distribute?
        if (RequiresDistributed(refresher, messageType) == false)
        {
            return;
        }

        // deliver remote

        // map ByInstance to ById as there's no remote instances
        if (messageType == MessageType.RefreshByInstance)
        {
            messageType = MessageType.RefreshById;
        }

        if (messageType == MessageType.RemoveByInstance)
        {
            messageType = MessageType.RemoveById;
        }

        // convert instances to identifiers
        var idsA = instancesA.Select(getId).ToArray();

        DeliverRemote(refresher, messageType, idsA);
    }

    //protected virtual void Deliver(ICacheRefresher refresher, object payload)
    //{
    //    if (servers == null) throw new ArgumentNullException("servers");
    //    if (refresher == null) throw new ArgumentNullException("refresher");

    //    var serversA = servers.ToArray();

    //    // deliver local
    //    DeliverLocal(refresher, payload);

    //    // distribute?
    //    if (RequiresDistributed(serversA, refresher, messageType) == false)
    //        return;

    //    // deliver remote
    //    DeliverRemote(serversA, refresher, payload);
    //}

    #endregion
}

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <summary>
/// Provides a base class for all <see cref="IServerMessenger" /> implementations.
/// </summary>
public abstract class ServerMessengerBase : IServerMessenger
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerMessengerBase"/> class.
    /// </summary>
    /// <param name="distributedEnabled">If set to <c>true</c> makes distributed calls when messaging a cache refresher.</param>
    /// <param name="jsonSerializer"></param>
    public ServerMessengerBase(bool distributedEnabled, IJsonSerializer jsonSerializer)
    {
        DistributedEnabled = distributedEnabled;
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    /// Gets or sets a value indicating whether distributed calls are made when messaging a cache refresher.
    /// </summary>
    /// <value>
    ///   <c>true</c> if distributed calls are required; otherwise, <c>false</c> if all we have is the local server.
    /// </value>
    protected bool DistributedEnabled { get; set; }

    /// <inheritdoc />
    public abstract void Sync();

    /// <inheritdoc />
    public abstract void SendMessages();

    /// <summary>
    /// Determines whether to make distributed calls when messaging a cache refresher.
    /// </summary>
    /// <param name="refresher">The cache refresher.</param>
    /// <param name="messageType">The message type.</param>
    /// <returns>
    ///   <c>true</c> if distributed calls are required; otherwise, <c>false</c> if all we have is the local server.
    /// </returns>
    protected virtual bool RequiresDistributed(ICacheRefresher refresher, MessageType messageType)
        => DistributedEnabled;

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
            if (id is not int && id is not Guid)
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

    /// <inheritdoc />
    public void QueueRefresh<TPayload>(ICacheRefresher refresher, TPayload[] payload)
    {
        ArgumentNullException.ThrowIfNull(refresher);

        if (payload == null || payload.Length == 0)
        {
            return;
        }

        Deliver(refresher, payload);
    }

    /// <inheritdoc />
    public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
    {
        ArgumentNullException.ThrowIfNull(refresher);
        ArgumentNullException.ThrowIfNull(getNumericId);

        if (instances == null || instances.Length == 0)
        {
            return;
        }

        Func<T, object> getId = x => getNumericId(x);
        Deliver(refresher, MessageType.RefreshByInstance, getId, instances);
    }

    /// <inheritdoc />
    public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
    {
        ArgumentNullException.ThrowIfNull(refresher);
        ArgumentNullException.ThrowIfNull(getGuidId);

        if (instances == null || instances.Length == 0)
        {
            return;
        }

        Func<T, object> getId = x => getGuidId(x);
        Deliver(refresher, MessageType.RefreshByInstance, getId, instances);
    }

    /// <inheritdoc />
    public void QueueRemove<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
    {
        ArgumentNullException.ThrowIfNull(refresher);
        ArgumentNullException.ThrowIfNull(getNumericId);

        if (instances == null || instances.Length == 0)
        {
            return;
        }

        Func<T, object> getId = x => getNumericId(x);
        Deliver(refresher, MessageType.RemoveByInstance, getId, instances);
    }

    /// <inheritdoc />
    public void QueueRemove(ICacheRefresher refresher, params int[] numericIds)
    {
        ArgumentNullException.ThrowIfNull(refresher);

        if (numericIds == null || numericIds.Length == 0)
        {
            return;
        }

        Deliver(refresher, MessageType.RemoveById, numericIds.Cast<object>());
    }

    /// <inheritdoc />
    public void QueueRefresh(ICacheRefresher refresher, params int[] numericIds)
    {
        ArgumentNullException.ThrowIfNull(refresher);

        if (numericIds == null || numericIds.Length == 0)
        {
            return;
        }

        Deliver(refresher, MessageType.RefreshById, numericIds.Cast<object>());
    }

    /// <inheritdoc />
    public void QueueRefresh(ICacheRefresher refresher, params Guid[] guidIds)
    {
        ArgumentNullException.ThrowIfNull(refresher);

        if (guidIds == null || guidIds.Length == 0)
        {
            return;
        }

        Deliver(refresher, MessageType.RefreshById, guidIds.Cast<object>());
    }

    /// <inheritdoc />
    public void QueueRefreshAll(ICacheRefresher refresher)
    {
        ArgumentNullException.ThrowIfNull(refresher);

        Deliver(refresher, MessageType.RefreshAll);
    }

    #endregion

    #region Deliver

    protected void DeliverLocal<TPayload>(ICacheRefresher refresher, TPayload[] payload)
    {
        ArgumentNullException.ThrowIfNull(refresher);
        if (StaticApplicationLogging.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
        {
            StaticApplicationLogging.Logger.LogDebug("Invoking refresher {RefresherType} on local server for message type RefreshByPayload", refresher.GetType());
        }

        if (refresher is not IPayloadCacheRefresher<TPayload> payloadRefresher)
        {
            throw new InvalidOperationException("The cache refresher " + refresher.GetType() + " is not of type " + typeof(IPayloadCacheRefresher<TPayload>));
        }

        payloadRefresher.Refresh(payload);
    }

    /// <summary>
    /// Executes the non-strongly typed <see cref="ICacheRefresher" /> on the local/current server.
    /// </summary>
    /// <param name="refresher">The cache refresher.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="ids">The IDs.</param>
    /// <param name="json">The JSON.</param>
    /// <remarks>
    /// Since this is only for non strongly typed <see cref="ICacheRefresher" />, it will throw for message types that are by instance.
    /// </remarks>
    protected void DeliverLocal(ICacheRefresher refresher, MessageType messageType, IEnumerable<object>? ids = null, string? json = null)
    {
        ArgumentNullException.ThrowIfNull(refresher);

        if (StaticApplicationLogging.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
        {
            StaticApplicationLogging.Logger.LogDebug(
            "Invoking refresher {RefresherType} on local server for message type {MessageType}", refresher.GetType(), messageType);
        }

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
                        if (id is int intId)
                        {
                            refresher.Refresh(intId);
                        }
                        else if (id is Guid guidId)
                        {
                            refresher.Refresh(guidId);
                        }
                        else
                        {
                            throw new InvalidOperationException("The id must be either an int or a Guid.");
                        }
                    }
                }

                break;

            case MessageType.RefreshByJson:
                if (refresher is not IJsonCacheRefresher jsonRefresher)
                {
                    throw new InvalidOperationException("The cache refresher " + refresher.GetType() + " is not of type " + typeof(IJsonCacheRefresher));
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
                        if (id is int intId)
                        {
                            refresher.Remove(intId);
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
    /// Executes the strongly typed <see cref="ICacheRefresher{T}" /> on the local/current server.
    /// </summary>
    /// <typeparam name="T">The cache refresher instance type.</typeparam>
    /// <param name="refresher">The cache refresher.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="getId">The function that gets the IDs from the instance.</param>
    /// <param name="instances">The instances.</param>
    /// <remarks>
    /// Since this is only for strongly typed <see cref="ICacheRefresher{T}" />, it will throw for message types that are not by instance.
    /// </remarks>
    protected void DeliverLocal<T>(ICacheRefresher refresher, MessageType messageType, Func<T, object> getId, IEnumerable<T> instances)
    {
        ArgumentNullException.ThrowIfNull(refresher);

        if (StaticApplicationLogging.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
        {
            StaticApplicationLogging.Logger.LogDebug(
            "Invoking refresher {RefresherType} on local server for message type {MessageType}", refresher.GetType(), messageType);
        }

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
                    throw new InvalidOperationException("The cache refresher " + refresher.GetType() + " is not a typed refresher.");
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

    protected abstract void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object>? ids = null, string? json = null);

    protected virtual void Deliver<TPayload>(ICacheRefresher refresher, TPayload[] payload)
    {
        ArgumentNullException.ThrowIfNull(refresher);

        // deliver local
        DeliverLocal(refresher, payload);

        // distribute?
        if (RequiresDistributed(refresher, MessageType.RefreshByJson) == false)
        {
            return;
        }

        // deliver remote
        var json = _jsonSerializer.Serialize(payload);
        DeliverRemote(refresher, MessageType.RefreshByJson, null, json);
    }

    protected virtual void Deliver(ICacheRefresher refresher, MessageType messageType, IEnumerable<object>? ids = null, string? json = null)
    {
        ArgumentNullException.ThrowIfNull(refresher);

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
        ArgumentNullException.ThrowIfNull(refresher);

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

    #endregion
}

using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCoreApplicationShutdownRegistry : IApplicationShutdownRegistry
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    private readonly ConcurrentDictionary<IRegisteredObject, RegisteredObjectWrapper> _registeredObjects = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="AspNetCoreApplicationShutdownRegistry" /> class.
    /// </summary>
    public AspNetCoreApplicationShutdownRegistry(IHostApplicationLifetime hostApplicationLifetime)
        => _hostApplicationLifetime = hostApplicationLifetime;

    public void RegisterObject(IRegisteredObject registeredObject)
    {
        var wrapped = new RegisteredObjectWrapper(registeredObject);
        if (!_registeredObjects.TryAdd(registeredObject, wrapped))
        {
            throw new InvalidOperationException("Could not register object");
        }

        CancellationTokenRegistration cancellationTokenRegistration =
            _hostApplicationLifetime.ApplicationStopping.Register(() => wrapped.Stop(true));
        wrapped.CancellationTokenRegistration = cancellationTokenRegistration;
    }

    public void UnregisterObject(IRegisteredObject registeredObject)
    {
        if (_registeredObjects.TryGetValue(registeredObject, out RegisteredObjectWrapper? wrapped))
        {
            wrapped.CancellationTokenRegistration.Unregister();
        }
    }

    private class RegisteredObjectWrapper
    {
        private readonly IRegisteredObject _inner;

        public RegisteredObjectWrapper(IRegisteredObject inner) => _inner = inner;

        public CancellationTokenRegistration CancellationTokenRegistration { get; set; }

        public void Stop(bool immediate) => _inner.Stop(immediate);
    }
}

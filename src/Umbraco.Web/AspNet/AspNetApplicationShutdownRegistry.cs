using System;
using System.Collections.Concurrent;
using System.Web.Hosting;
using Umbraco.Cms.Core.Hosting;
using IRegisteredObject = Umbraco.Cms.Core.IRegisteredObject;

namespace Umbraco.Web.Hosting
{
    public class AspNetApplicationShutdownRegistry : IApplicationShutdownRegistry
    {
        private readonly ConcurrentDictionary<IRegisteredObject, RegisteredObjectWrapper> _registeredObjects =
            new ConcurrentDictionary<IRegisteredObject, RegisteredObjectWrapper>();

        public void RegisterObject(IRegisteredObject registeredObject)
        {
            var wrapped = new RegisteredObjectWrapper(registeredObject);
            if (!_registeredObjects.TryAdd(registeredObject, wrapped))
            {
                throw new InvalidOperationException("Could not register object");
            }
            HostingEnvironment.RegisterObject(wrapped);
        }

        public void UnregisterObject(IRegisteredObject registeredObject)
        {
            if (_registeredObjects.TryGetValue(registeredObject, out var wrapped))
            {
                HostingEnvironment.UnregisterObject(wrapped);
            }
        }

        private class RegisteredObjectWrapper : System.Web.Hosting.IRegisteredObject
        {
            private readonly IRegisteredObject _inner;

            public RegisteredObjectWrapper(IRegisteredObject inner)
            {
                _inner = inner;
            }

            public void Stop(bool immediate)
            {
                _inner.Stop(immediate);
            }
        }
    }
}

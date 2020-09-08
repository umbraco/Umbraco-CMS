using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Core.Composing
{
    public class TempMsdiServiceProviderFactoryAdapter : IFactory
    {
        private readonly IServiceProvider _container;
        public TempMsdiServiceProviderFactoryAdapter(IServiceCollection services)
        {
            _container = services.BuildServiceProvider();
        }

        public object Concrete => _container;

        public object GetInstance(Type type)
        {
            return _container.GetInstance(type);
        }

        public TService GetInstanceFor<TService, TTarget>()
        {
            return _container.GetRequiredService<TService>();
        }

        public object TryGetInstance(Type type)
        {
            return _container.TryGetInstance(type);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetServices(serviceType);
        }

        public IEnumerable<TService> GetAllInstances<TService>() where TService : class
        {
            return _container.GetServices<TService>();
        }

        public void Release(object instance)
        {
            // NOOP
        }

        public IDisposable BeginScope()
        {
            return _container.CreateScope();
        }

        public void EnablePerWebRequestScope()
        {
            // NOOP
        }
    }
}

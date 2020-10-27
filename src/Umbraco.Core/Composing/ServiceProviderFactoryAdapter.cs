using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Infrastructure.Composing
{
    public class ServiceProviderFactoryAdapter : IFactory
    {
        public IServiceProvider ServiceProvider { get; }

        private ServiceProviderFactoryAdapter(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
        
        public object Concrete => ServiceProvider;

        public object GetRequiredService(Type type)
        {
            return ServiceProvider.GetRequiredService(type);
        }

        public object GetService(Type type)
        {
            return ServiceProvider.GetService(type);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }

        public IEnumerable<TService> GetServices<TService>() where TService : class
        {
            return ServiceProvider.GetServices<TService>();
        }

        public IDisposable BeginScope()
        {
            return ServiceProvider.CreateScope();
        }

        public static IFactory Wrap(IServiceProvider serviceProvider)
        {
            return new ServiceProviderFactoryAdapter(serviceProvider);
        }
    }
}

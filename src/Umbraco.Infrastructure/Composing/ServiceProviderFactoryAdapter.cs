﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Infrastructure.Composing
{
    internal class ServiceProviderFactoryAdapter : IFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private ServiceProviderFactoryAdapter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public object Concrete => _serviceProvider;

        public object GetInstance(Type type)
        {
            return _serviceProvider.GetRequiredService(type);
        }

        public TService GetInstanceFor<TService, TTarget>()
        {
            throw new NotImplementedException("MSDI");
        }

        public object TryGetInstance(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _serviceProvider.GetServices(serviceType);
        }

        public IEnumerable<TService> GetAllInstances<TService>() where TService : class
        {
            return _serviceProvider.GetServices<TService>();
        }

        public void Release(object instance) { }

        public IDisposable BeginScope()
        {
            return _serviceProvider.CreateScope();
        }

        public void EnablePerWebRequestScope() { }

        public static IFactory Wrap(IServiceProvider serviceProvider)
        {
            return new ServiceProviderFactoryAdapter(serviceProvider);
        }
    }
}

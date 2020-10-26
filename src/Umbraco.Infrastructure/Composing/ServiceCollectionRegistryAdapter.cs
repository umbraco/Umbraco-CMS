using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Infrastructure.Composing
{
    public class ServiceCollectionRegistryAdapter : IRegister
    {
        private readonly IServiceCollection _services;

        public ServiceCollectionRegistryAdapter(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _services.AddTransient(typeof(Lazy<>), typeof(LazyResolve<>));
        }

        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            switch (lifetime)
            {
                case Lifetime.Request:
                case Lifetime.Scope:
                    _services.AddScoped(serviceType);
                    break;
                case Lifetime.Transient:
                    _services.AddTransient(serviceType);
                    break;
                case Lifetime.Singleton:
                    _services.AddSingleton(serviceType);
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Lifetime: {lifetime}");
            }
        }

        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            switch (lifetime)
            {
                case Lifetime.Request:
                case Lifetime.Scope:
                    _services.AddScoped(serviceType, implementingType);
                    break;
                case Lifetime.Transient:
                    _services.AddTransient(serviceType, implementingType);
                    break;
                case Lifetime.Singleton:
                    _services.AddSingleton(serviceType, implementingType);
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Lifetime: {lifetime}");
            }
        }

        public void Register<TService>(Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient) where TService : class
        {
            switch (lifetime)
            {
                case Lifetime.Request:
                case Lifetime.Scope:
                    _services.AddScoped(sp => factory(ServiceProviderFactoryAdapter.Wrap(sp)));
                    break;
                case Lifetime.Transient:
                    _services.AddTransient(sp => factory(ServiceProviderFactoryAdapter.Wrap(sp)));
                    break;
                case Lifetime.Singleton:
                    _services.AddSingleton(sp => factory(ServiceProviderFactoryAdapter.Wrap(sp)));
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Lifetime: {lifetime}");
            }
        }
        public void Register(Type serviceType, object instance)
        {
            _services.AddSingleton(serviceType, instance);
        }

        public IFactory CreateFactory()
        {
            return ServiceProviderFactoryAdapter.Wrap(_services.BuildServiceProvider());
        }
    }

    public class LazyResolve<T> : Lazy<T>
        where T : class
    {
        public LazyResolve(IServiceProvider serviceProvider) : base(serviceProvider.GetRequiredService<T>)
        {
            
        }
    }
}

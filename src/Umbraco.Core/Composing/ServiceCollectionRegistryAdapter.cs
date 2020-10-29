using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Infrastructure.Composing
{
    public class ServiceCollectionRegistryAdapter : IRegister
    {
        public IServiceCollection Services { get; }

        public ServiceCollectionRegistryAdapter(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Services.AddTransient(typeof(Lazy<>), typeof(LazyResolve<>));
        }

        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            switch (lifetime)
            {
                case Lifetime.Request:
                case Lifetime.Scope:
                    Services.AddScoped(serviceType);
                    break;
                case Lifetime.Transient:
                    Services.AddTransient(serviceType);
                    break;
                case Lifetime.Singleton:
                    Services.AddSingleton(serviceType);
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
                    Services.AddScoped(serviceType, implementingType);
                    break;
                case Lifetime.Transient:
                    Services.AddTransient(serviceType, implementingType);
                    break;
                case Lifetime.Singleton:
                    Services.AddSingleton(serviceType, implementingType);
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Lifetime: {lifetime}");
            }
        }

        public void Register<TService>(Func<IServiceProvider, TService> factory, Lifetime lifetime = Lifetime.Transient) where TService : class
        {
            switch (lifetime)
            {
                case Lifetime.Request:
                case Lifetime.Scope:
                    Services.AddScoped(factory);
                    break;
                case Lifetime.Transient:
                    Services.AddTransient(factory);
                    break;
                case Lifetime.Singleton:
                    Services.AddSingleton(factory);
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Lifetime: {lifetime}");
            }
        }
        public void Register(Type serviceType, object instance)
        {
            Services.AddSingleton(serviceType, instance);
        }

        public static IRegister Wrap(IServiceCollection services)
        {
            return new ServiceCollectionRegistryAdapter(services);
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

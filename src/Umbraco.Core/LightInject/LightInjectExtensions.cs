using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.LightInject
{
    internal static class LightInjectExtensions
    {
        /// <summary>
        /// In order for LightInject to deal with enumerables of the same type, each one needs to be named individually
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TLifetime"></typeparam>
        /// <param name="container"></param>
        /// <param name="implementationTypes"></param>
        public static void RegisterCollection<TService, TLifetime>(this IServiceContainer container, IEnumerable<Type> implementationTypes)
            where TLifetime : ILifetime
        {
            var i = 0;
            foreach (var type in implementationTypes)
            {
                //This works as of 3.0.2.2: https://github.com/seesharper/LightInject/issues/68#issuecomment-70611055
                // but means that the explicit type is registered, not the implementing type
                container.Register(type, Activator.CreateInstance<TLifetime>());

                //NOTE: This doesn't work, but it would be nice if it did (autofac supports thsi)
                //container.Register(typeof(TService), type,
                //    Activator.CreateInstance<TLifetime>());

                //This does work, but requires a unique name per service
                //container.Register(typeof(TService), type,
                //    //need to name it, we'll keep the name tiny
                //    i.ToString(CultureInfo.InvariantCulture),
                //    Activator.CreateInstance<TLifetime>());
                //i++;
            }
        }

        /// <summary>
        /// In order for LightInject to deal with enumerables of the same type, each one needs to be named individually
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="container"></param>
        /// <param name="implementationTypes"></param>
        public static void RegisterCollection<TService>(this IServiceContainer container, IEnumerable<Type> implementationTypes)
        {
            var i = 0;
            foreach (var type in implementationTypes)
            {
                //This works as of 3.0.2.2: https://github.com/seesharper/LightInject/issues/68#issuecomment-70611055
                // but means that the explicit type is registered, not the implementing type
                container.Register(type);

                //NOTE: This doesn't work, but it would be nice if it did (autofac supports thsi)
                //container.Register(typeof(TService), type);

                //This does work, but requires a unique name per service
                //container.Register(typeof(TService), type,
                //    //need to name it, we'll keep the name tiny
                //    i.ToString(CultureInfo.InvariantCulture));
                //i++;
            }
        }

        /// <summary>
        /// Creates a child container from the parent container
        /// </summary>
        /// <param name="parentContainer"></param>
        /// <returns></returns>
        public static ServiceContainer CreateChildContainer(this IServiceContainer parentContainer)
        {
            var child = new ChildContainer(parentContainer);
            return child;
        }

        private class ChildContainer : ServiceContainer
        {
            public ChildContainer(IServiceRegistry parentContainer)
            {
                foreach (var svc in parentContainer.AvailableServices)
                {
                    Register(svc);
                }
            }
        }

        ///// <summary>
        ///// A container wrapper for 2 containers: Child and Parent
        ///// </summary>
        //private class ChildContainer : IServiceContainer
        //{
        //    private readonly IServiceContainer _parent;
        //    private readonly IServiceContainer _current = new ServiceContainer();

        //    public ChildContainer(IServiceContainer parent)
        //    {
        //        _parent = parent;
        //    }

        //    /// <summary>
        //    /// Gets a list of <see cref="ServiceRegistration"/> instances that represents the 
        //    /// registered services.          
        //    /// </summary>
        //    public IEnumerable<ServiceRegistration> AvailableServices
        //    {
        //        get { return _current.AvailableServices.Union(_parent.AvailableServices); }
        //    }

        //    /// <summary>
        //    /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The service type to register.</param>
        //    /// <param name="implementingType">The implementing type.</param>
        //    public void Register(Type serviceType, Type implementingType)
        //    {
        //        _current.Register(serviceType, implementingType);
        //    }

        //    /// <summary>
        //    /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The service type to register.</param>
        //    /// <param name="implementingType">The implementing type.</param>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void Register(Type serviceType, Type implementingType, ILifetime lifetime)
        //    {
        //        _current.Register(serviceType, implementingType, lifetime);
        //    }

        //    /// <summary>
        //    /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The service type to register.</param>
        //    /// <param name="implementingType">The implementing type.</param>
        //    /// <param name="serviceName">The name of the service.</param>
        //    public void Register(Type serviceType, Type implementingType, string serviceName)
        //    {
        //        _current.Register(serviceType, implementingType, serviceName);
        //    }

        //    /// <summary>
        //    /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The service type to register.</param>
        //    /// <param name="implementingType">The implementing type.</param>
        //    /// <param name="serviceName">The name of the service.</param>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void Register(Type serviceType, Type implementingType, string serviceName, ILifetime lifetime)
        //    {
        //        _current.Register(serviceType, implementingType, serviceName, lifetime);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <typeparam name="TImplementation">The implementing type.</typeparam>
        //    public void Register<TService, TImplementation>() where TImplementation : TService
        //    {
        //        _current.Register<TService, TImplementation>();
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <typeparam name="TImplementation">The implementing type.</typeparam>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void Register<TService, TImplementation>(ILifetime lifetime) where TImplementation : TService
        //    {
        //        _current.Register<TService, TImplementation>(lifetime);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <typeparam name="TImplementation">The implementing type.</typeparam>
        //    /// <param name="serviceName">The name of the service.</param>
        //    public void Register<TService, TImplementation>(string serviceName) where TImplementation : TService
        //    {
        //        _current.Register<TService, TImplementation>(serviceName);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <typeparam name="TImplementation">The implementing type.</typeparam>
        //    /// <param name="serviceName">The name of the service.</param>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void Register<TService, TImplementation>(string serviceName, ILifetime lifetime) where TImplementation : TService
        //    {
        //        _current.Register<TService, TImplementation>(serviceName, lifetime);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>. 
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <param name="instance">The instance returned when this service is requested.</param>
        //    public void RegisterInstance<TService>(TService instance)
        //    {
        //        _current.RegisterInstance<TService>(instance);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>. 
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <param name="instance">The instance returned when this service is requested.</param>
        //    /// <param name="serviceName">The name of the service.</param>
        //    public void RegisterInstance<TService>(TService instance, string serviceName)
        //    {
        //        _current.RegisterInstance<TService>(instance, serviceName);
        //    }

        //    /// <summary>
        //    /// Registers the <paramref name="serviceType"/> with the given <paramref name="instance"/>. 
        //    /// </summary>
        //    /// <param name="serviceType">The service type to register.</param>
        //    /// <param name="instance">The instance returned when this service is requested.</param>
        //    public void RegisterInstance(Type serviceType, object instance)
        //    {
        //        _current.RegisterInstance(serviceType, instance);
        //    }

        //    /// <summary>
        //    /// Registers the <paramref name="serviceType"/> with the given <paramref name="instance"/>. 
        //    /// </summary>
        //    /// <param name="serviceType">The service type to register.</param>
        //    /// <param name="instance">The instance returned when this service is requested.</param>
        //    /// <param name="serviceName">The name of the service.</param>
        //    public void RegisterInstance(Type serviceType, object instance, string serviceName)
        //    {
        //        _current.RegisterInstance(serviceType, instance, serviceName);
        //    }

        //    /// <summary>
        //    /// Registers a concrete type as a service.
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    public void Register<TService>()
        //    {
        //        _current.Register<TService>();
        //    }

        //    /// <summary>
        //    /// Registers a concrete type as a service.
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void Register<TService>(ILifetime lifetime)
        //    {
        //        _current.Register<TService>(lifetime);
        //    }

        //    /// <summary>
        //    /// Registers a concrete type as a service.
        //    /// </summary>
        //    /// <param name="serviceType">The concrete type to register.</param>
        //    public void Register(Type serviceType)
        //    {
        //        _current.Register(serviceType);
        //    }

        //    /// <summary>
        //    /// Registers a concrete type as a service.
        //    /// </summary>
        //    /// <param name="serviceType">The concrete type to register.</param>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void Register(Type serviceType, ILifetime lifetime)
        //    {
        //        _current.Register(serviceType, lifetime);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>    
        //    public void Register<TService>(Expression<Func<IServiceFactory, TService>> factory)
        //    {
        //        _current.Register<TService>(factory);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="T">The parameter type.</typeparam>
        //    /// <typeparam name="TService">The service type to register.</typeparam>        
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>    
        //    public void Register<T, TService>(Expression<Func<IServiceFactory, T, TService>> factory)
        //    {
        //        _current.Register<T, TService>(factory);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="T">The parameter type.</typeparam>
        //    /// <typeparam name="TService">The service type to register.</typeparam>        
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param> 
        //    /// <param name="serviceName">The name of the service.</param>        
        //    public void Register<T, TService>(Expression<Func<IServiceFactory, T, TService>> factory, string serviceName)
        //    {
        //        _current.Register<T, TService>(factory, serviceName);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="TService">The service type to register.</typeparam>        
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>    
        //    public void Register<T1, T2, TService>(Expression<Func<IServiceFactory, T1, T2, TService>> factory)
        //    {
        //        _current.Register<T1, T2, TService>(factory);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="TService">The service type to register.</typeparam>        
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>    
        //    /// <param name="serviceName">The name of the service.</param>
        //    public void Register<T1, T2, TService>(Expression<Func<IServiceFactory, T1, T2, TService>> factory, string serviceName)
        //    {
        //        _current.Register<T1, T2, TService>(factory, serviceName);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="T3">The type of the third parameter.</typeparam>
        //    /// <typeparam name="TService">The service type to register.</typeparam>        
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>    
        //    public void Register<T1, T2, T3, TService>(Expression<Func<IServiceFactory, T1, T2, T3, TService>> factory)
        //    {
        //        _current.Register<T1, T2, T3, TService>(factory);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="T3">The type of the third parameter.</typeparam>
        //    /// <typeparam name="TService">The service type to register.</typeparam>        
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>    
        //    /// <param name="serviceName">The name of the service.</param>
        //    public void Register<T1, T2, T3, TService>(Expression<Func<IServiceFactory, T1, T2, T3, TService>> factory, string serviceName)
        //    {
        //        _current.Register<T1, T2, T3, TService>(factory, serviceName);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="T3">The type of the third parameter.</typeparam>
        //    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
        //    /// <typeparam name="TService">The service type to register.</typeparam>        
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>    
        //    public void Register<T1, T2, T3, T4, TService>(Expression<Func<IServiceFactory, T1, T2, T3, T4, TService>> factory)
        //    {
        //        _current.Register<T1, T2, T3, T4, TService>(factory);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="T3">The type of the third parameter.</typeparam>
        //    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
        //    /// <typeparam name="TService">The service type to register.</typeparam>        
        //    /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>    
        //    /// <param name="serviceName">The name of the service.</param>
        //    public void Register<T1, T2, T3, T4, TService>(Expression<Func<IServiceFactory, T1, T2, T3, T4, TService>> factory, string serviceName)
        //    {
        //        _current.Register<T1, T2, T3, T4, TService>(factory, serviceName);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void Register<TService>(Expression<Func<IServiceFactory, TService>> factory, ILifetime lifetime)
        //    {
        //        _current.Register<TService>(factory, lifetime);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        //    /// <param name="serviceName">The name of the service.</param>        
        //    public void Register<TService>(Expression<Func<IServiceFactory, TService>> factory, string serviceName)
        //    {
        //        _current.Register<TService>(factory, serviceName);
        //    }

        //    /// <summary>
        //    /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        //    /// describes the dependencies of the service. 
        //    /// </summary>
        //    /// <typeparam name="TService">The service type to register.</typeparam>
        //    /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        //    /// <param name="serviceName">The name of the service.</param>        
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void Register<TService>(Expression<Func<IServiceFactory, TService>> factory, string serviceName, ILifetime lifetime)
        //    {
        //        _current.Register<TService>(factory, serviceName, lifetime);
        //    }

        //    /// <summary>
        //    /// Registers a custom factory delegate used to create services that is otherwise unknown to the service container.
        //    /// </summary>
        //    /// <param name="predicate">Determines if the service can be created by the <paramref name="factory"/> delegate.</param>
        //    /// <param name="factory">Creates a service instance according to the <paramref name="predicate"/> predicate.</param>
        //    public void RegisterFallback(Func<Type, string, bool> predicate, Func<ServiceRequest, object> factory)
        //    {
        //        _current.RegisterFallback(predicate, factory);
        //    }

        //    /// <summary>
        //    /// Registers a custom factory delegate used to create services that is otherwise unknown to the service container.
        //    /// </summary>
        //    /// <param name="predicate">Determines if the service can be created by the <paramref name="factory"/> delegate.</param>
        //    /// <param name="factory">Creates a service instance according to the <paramref name="predicate"/> predicate.</param>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    public void RegisterFallback(Func<Type, string, bool> predicate, Func<ServiceRequest, object> factory, ILifetime lifetime)
        //    {
        //        _current.RegisterFallback(predicate, factory, lifetime);
        //    }

        //    /// <summary>
        //    /// Registers a service based on a <see cref="ServiceRegistration"/> instance.
        //    /// </summary>
        //    /// <param name="serviceRegistration">The <see cref="ServiceRegistration"/> instance that contains service metadata.</param>
        //    public void Register(ServiceRegistration serviceRegistration)
        //    {
        //        _current.Register(serviceRegistration);
        //    }

        //    /// <summary>
        //    /// Registers composition roots from the given <paramref name="assembly"/>.
        //    /// </summary>
        //    /// <param name="assembly">The assembly to be scanned for services.</param>        
        //    /// <remarks>
        //    /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        //    /// will be used to configure the container.
        //    /// </remarks>     
        //    public void RegisterAssembly(Assembly assembly)
        //    {
        //        _current.RegisterAssembly(assembly);
        //    }

        //    /// <summary>
        //    /// Registers services from the given <paramref name="assembly"/>.
        //    /// </summary>
        //    /// <param name="assembly">The assembly to be scanned for services.</param>
        //    /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        //    /// <remarks>
        //    /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        //    /// will be used to configure the container.
        //    /// </remarks>     
        //    public void RegisterAssembly(Assembly assembly, Func<Type, Type, bool> shouldRegister)
        //    {
        //        _current.RegisterAssembly(assembly, shouldRegister);
        //    }

        //    /// <summary>
        //    /// Registers services from the given <paramref name="assembly"/>.
        //    /// </summary>
        //    /// <param name="assembly">The assembly to be scanned for services.</param>
        //    /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        //    /// <remarks>
        //    /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        //    /// will be used to configure the container.
        //    /// </remarks>     
        //    public void RegisterAssembly(Assembly assembly, Func<ILifetime> lifetime)
        //    {
        //        _current.RegisterAssembly(assembly, lifetime);
        //    }

        //    /// <summary>
        //    /// Registers services from the given <paramref name="assembly"/>.
        //    /// </summary>
        //    /// <param name="assembly">The assembly to be scanned for services.</param>
        //    /// <param name="lifetimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of the registered service.</param>
        //    /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        //    /// <remarks>
        //    /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        //    /// will be used to configure the container.
        //    /// </remarks>     
        //    public void RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory, Func<Type, Type, bool> shouldRegister)
        //    {
        //        _current.RegisterAssembly(assembly, lifetimeFactory, shouldRegister);
        //    }

        //    /// <summary>
        //    /// Registers services from the given <typeparamref name="TCompositionRoot"/> type.
        //    /// </summary>
        //    /// <typeparam name="TCompositionRoot">The type of <see cref="ICompositionRoot"/> to register from.</typeparam>
        //    public void RegisterFrom<TCompositionRoot>() where TCompositionRoot : ICompositionRoot, new()
        //    {
        //        _current.RegisterFrom<TCompositionRoot>();
        //    }

        //    /// <summary>
        //    /// Registers composition roots from assemblies in the base directory that matches the <paramref name="searchPattern"/>.
        //    /// </summary>
        //    /// <param name="searchPattern">The search pattern used to filter the assembly files.</param>
        //    public void RegisterAssembly(string searchPattern)
        //    {
        //        _current.RegisterAssembly(searchPattern);
        //    }

        //    /// <summary>
        //    /// Decorates the <paramref name="serviceType"/> with the given <paramref name="decoratorType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The target service type.</param>
        //    /// <param name="decoratorType">The decorator type used to decorate the <paramref name="serviceType"/>.</param>
        //    /// <param name="predicate">A function delegate that determines if the <paramref name="decoratorType"/>
        //    /// should be applied to the target <paramref name="serviceType"/>.</param>
        //    public void Decorate(Type serviceType, Type decoratorType, Func<ServiceRegistration, bool> predicate)
        //    {
        //        _current.Decorate(serviceType, decoratorType, predicate);
        //    }

        //    /// <summary>
        //    /// Decorates the <paramref name="serviceType"/> with the given <paramref name="decoratorType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The target service type.</param>
        //    /// <param name="decoratorType">The decorator type used to decorate the <paramref name="serviceType"/>.</param>        
        //    public void Decorate(Type serviceType, Type decoratorType)
        //    {
        //        _current.Decorate(serviceType, decoratorType);
        //    }

        //    /// <summary>
        //    /// Decorates the <typeparamref name="TService"/> with the given <typeparamref name="TDecorator"/>.
        //    /// </summary>
        //    /// <typeparam name="TService">The target service type.</typeparam>
        //    /// <typeparam name="TDecorator">The decorator type used to decorate the <typeparamref name="TService"/>.</typeparam>
        //    public void Decorate<TService, TDecorator>() where TDecorator : TService
        //    {
        //        _current.Decorate<TService, TDecorator>();
        //    }

        //    /// <summary>
        //    /// Decorates the <typeparamref name="TService"/> using the given decorator <paramref name="factory"/>.
        //    /// </summary>
        //    /// <typeparam name="TService">The target service type.</typeparam>
        //    /// <param name="factory">A factory delegate used to create a decorator instance.</param>
        //    public void Decorate<TService>(Expression<Func<IServiceFactory, TService, TService>> factory)
        //    {
        //        _current.Decorate<TService>(factory);
        //    }

        //    /// <summary>
        //    /// Registers a decorator based on a <see cref="DecoratorRegistration"/> instance.
        //    /// </summary>
        //    /// <param name="decoratorRegistration">The <see cref="DecoratorRegistration"/> instance that contains the decorator metadata.</param>
        //    public void Decorate(DecoratorRegistration decoratorRegistration)
        //    {
        //        _current.Decorate(decoratorRegistration);
        //    }

        //    /// <summary>
        //    /// Allows a registered service to be overridden by another <see cref="ServiceRegistration"/>.
        //    /// </summary>
        //    /// <param name="serviceSelector">A function delegate that is used to determine the service that should be
        //    /// overridden using the <see cref="ServiceRegistration"/> returned from the <paramref name="serviceRegistrationFactory"/>.</param>
        //    /// <param name="serviceRegistrationFactory">The factory delegate used to create a <see cref="ServiceRegistration"/> that overrides
        //    /// the incoming <see cref="ServiceRegistration"/>.</param>
        //    public void Override(Func<ServiceRegistration, bool> serviceSelector, Func<IServiceFactory, ServiceRegistration, ServiceRegistration> serviceRegistrationFactory)
        //    {
        //        _current.Override(serviceSelector, serviceRegistrationFactory);
        //    }

        //    /// <summary>
        //    /// Starts a new <see cref="Scope"/>.
        //    /// </summary>
        //    /// <returns><see cref="Scope"/></returns>
        //    public Scope BeginScope()
        //    {
        //        return _current.BeginScope();
        //    }

        //    /// <summary>
        //    /// Ends the current <see cref="Scope"/>.
        //    /// </summary>
        //    public void EndCurrentScope()
        //    {
        //        _current.EndCurrentScope();
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <paramref name="serviceType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The type of the requested service.</param>
        //    /// <returns>The requested service instance.</returns>
        //    public object GetInstance(Type serviceType)
        //    {
        //        var instance = _current.TryGetInstance(serviceType);
        //        return instance ?? _parent.GetInstance(serviceType);
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <paramref name="serviceType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The type of the requested service.</param>
        //    /// <param name="arguments">The arguments to be passed to the target instance.</param>
        //    /// <returns>The requested service instance.</returns>
        //    public object GetInstance(Type serviceType, object[] arguments)
        //    {
        //        try
        //        {
        //            return _current.GetInstance(serviceType, arguments);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance(serviceType, arguments);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <paramref name="serviceType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The type of the requested service.</param>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <param name="arguments">The arguments to be passed to the target instance.</param>        
        //    /// <returns>The requested service instance.</returns>
        //    public object GetInstance(Type serviceType, string serviceName, object[] arguments)
        //    {
        //        try
        //        {
        //            return _current.GetInstance(serviceType, serviceName, arguments);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance(serviceType, serviceName, arguments);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets a named instance of the given <paramref name="serviceType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The type of the requested service.</param>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <returns>The requested service instance.</returns>
        //    public object GetInstance(Type serviceType, string serviceName)
        //    {
        //        var instance = _current.TryGetInstance(serviceType, serviceName);
        //        return instance ?? _parent.GetInstance(serviceType, serviceName);
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/> type.
        //    /// </summary>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>
        //    /// <returns>The requested service instance.</returns>
        //    public TService GetInstance<TService>()
        //    {
        //        var instance = _current.TryGetInstance<TService>() as object;
        //        return (TService) (instance ?? _parent.GetInstance<TService>());
        //    }

        //    /// <summary>
        //    /// Gets a named instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<TService>(string serviceName)
        //    {
        //        var instance = _current.TryGetInstance<TService>(serviceName) as object;
        //        return (TService)(instance ?? _parent.GetInstance<TService>(serviceName));
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="T">The type of the argument.</typeparam>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>        
        //    /// <param name="value">The argument value.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<T, TService>(T value)
        //    {
        //        try
        //        {
        //            return _current.GetInstance<T, TService>(value);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance<T, TService>(value);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="T">The type of the parameter.</typeparam>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>        
        //    /// <param name="value">The argument value.</param>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<T, TService>(T value, string serviceName)
        //    {
        //        try
        //        {
        //            return _current.GetInstance<T, TService>(value, serviceName);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance<T, TService>(value, serviceName);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>        
        //    /// <param name="arg1">The first argument value.</param>
        //    /// <param name="arg2">The second argument value.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<T1, T2, TService>(T1 arg1, T2 arg2)
        //    {
        //        try
        //        {
        //            return _current.GetInstance<T1, T2, TService>(arg1, arg2);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance<T1, T2, TService>(arg1, arg2);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>        
        //    /// <param name="arg1">The first argument value.</param>
        //    /// <param name="arg2">The second argument value.</param>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<T1, T2, TService>(T1 arg1, T2 arg2, string serviceName)
        //    {
        //        try
        //        {
        //            return _current.GetInstance<T1, T2, TService>(arg1, arg2, serviceName);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance<T1, T2, TService>(arg1, arg2, serviceName);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="T3">The type of the third parameter.</typeparam>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>        
        //    /// <param name="arg1">The first argument value.</param>
        //    /// <param name="arg2">The second argument value.</param>
        //    /// <param name="arg3">The third argument value.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<T1, T2, T3, TService>(T1 arg1, T2 arg2, T3 arg3)
        //    {
        //        try
        //        {
        //            return _current.GetInstance<T1, T2, T3, TService>(arg1, arg2, arg3);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance<T1, T2, T3, TService>(arg1, arg2, arg3);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="T3">The type of the third parameter.</typeparam>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>        
        //    /// <param name="arg1">The first argument value.</param>
        //    /// <param name="arg2">The second argument value.</param>
        //    /// <param name="arg3">The third argument value.</param>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<T1, T2, T3, TService>(T1 arg1, T2 arg2, T3 arg3, string serviceName)
        //    {
        //        try
        //        {
        //            return _current.GetInstance<T1, T2, T3, TService>(arg1, arg2, arg3, serviceName);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance<T1, T2, T3, TService>(arg1, arg2, arg3, serviceName);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="T3">The type of the third parameter.</typeparam>
        //    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>        
        //    /// <param name="arg1">The first argument value.</param>
        //    /// <param name="arg2">The second argument value.</param>
        //    /// <param name="arg3">The third argument value.</param>
        //    /// <param name="arg4">The fourth argument value.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<T1, T2, T3, T4, TService>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        //    {
        //        try
        //        {
        //            return _current.GetInstance<T1, T2, T3, T4, TService>(arg1, arg2, arg3, arg4);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance<T1, T2, T3, T4, TService>(arg1, arg2, arg3, arg4);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="T1">The type of the first parameter.</typeparam>
        //    /// <typeparam name="T2">The type of the second parameter.</typeparam>
        //    /// <typeparam name="T3">The type of the third parameter.</typeparam>
        //    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>        
        //    /// <param name="arg1">The first argument value.</param>
        //    /// <param name="arg2">The second argument value.</param>
        //    /// <param name="arg3">The third argument value.</param>
        //    /// <param name="arg4">The fourth argument value.</param>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <returns>The requested service instance.</returns>    
        //    public TService GetInstance<T1, T2, T3, T4, TService>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, string serviceName)
        //    {
        //        try
        //        {
        //            return _current.GetInstance<T1, T2, T3, T4, TService>(arg1, arg2, arg3, arg4, serviceName);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            return _parent.GetInstance<T1, T2, T3, T4, TService>(arg1, arg2, arg3, arg4, serviceName);
        //        }
        //    }

        //    /// <summary>
        //    /// Gets an instance of the given <paramref name="serviceType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The type of the requested service.</param>
        //    /// <returns>The requested service instance if available, otherwise null.</returns>
        //    public object TryGetInstance(Type serviceType)
        //    {
        //        return _current.TryGetInstance(serviceType) ?? _parent.TryGetInstance(serviceType);
        //    }

        //    /// <summary>
        //    /// Gets a named instance of the given <paramref name="serviceType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The type of the requested service.</param>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <returns>The requested service instance if available, otherwise null.</returns>
        //    public object TryGetInstance(Type serviceType, string serviceName)
        //    {
        //        return _current.TryGetInstance(serviceType, serviceName) ?? _parent.TryGetInstance(serviceType, serviceName);
        //    }

        //    /// <summary>
        //    /// Tries to get an instance of the given <typeparamref name="TService"/> type.
        //    /// </summary>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>
        //    /// <returns>The requested service instance if available, otherwise default(T).</returns>
        //    public TService TryGetInstance<TService>()
        //    {
        //        return (TService) ((_current.TryGetInstance<TService>() as object) ?? _parent.TryGetInstance<TService>());
        //    }

        //    /// <summary>
        //    /// Tries to get an instance of the given <typeparamref name="TService"/> type.
        //    /// </summary>
        //    /// <typeparam name="TService">The type of the requested service.</typeparam>
        //    /// <param name="serviceName">The name of the requested service.</param>
        //    /// <returns>The requested service instance if available, otherwise default(T).</returns>
        //    public TService TryGetInstance<TService>(string serviceName)
        //    {
        //        return (TService)((_current.TryGetInstance<TService>(serviceName) as object) ?? _parent.TryGetInstance<TService>(serviceName));
        //    }

        //    /// <summary>
        //    /// Gets all instances of the given <paramref name="serviceType"/>.
        //    /// </summary>
        //    /// <param name="serviceType">The type of services to resolve.</param>
        //    /// <returns>A list that contains all implementations of the <paramref name="serviceType"/>.</returns>
        //    public IEnumerable<object> GetAllInstances(Type serviceType)
        //    {
        //        return _current.GetAllInstances(serviceType).Union(_parent.GetAllInstances(serviceType));
        //    }

        //    /// <summary>
        //    /// Gets all instances of type <typeparamref name="TService"/>.
        //    /// </summary>
        //    /// <typeparam name="TService">The type of services to resolve.</typeparam>
        //    /// <returns>A list that contains all implementations of the <typeparamref name="TService"/> type.</returns>
        //    public IEnumerable<TService> GetAllInstances<TService>()
        //    {
        //        return _current.GetAllInstances<TService>().Union(_parent.GetAllInstances<TService>());
        //    }

        //    /// <summary>
        //    /// Creates an instance of a concrete class.
        //    /// </summary>
        //    /// <typeparam name="TService">The type of class for which to create an instance.</typeparam>
        //    /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        //    /// <remarks>The concrete type will be registered if not already registered with the container.</remarks>
        //    public TService Create<TService>() where TService : class
        //    {
        //        return _current.Create<TService>();
        //    }

        //    /// <summary>
        //    /// Creates an instance of a concrete class.
        //    /// </summary>
        //    /// <param name="serviceType">The type of class for which to create an instance.</param>
        //    /// <returns>An instance of the <paramref name="serviceType"/>.</returns>
        //    public object Create(Type serviceType)
        //    {
        //        return _current.Create(serviceType);
        //    }

        //    /// <summary>
        //    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        //    /// </summary>
        //    public void Dispose()
        //    {
        //        //only dispose the current container
        //        _current.Dispose();
        //    }

        //    /// <summary>
        //    /// Gets or sets the <see cref="IScopeManagerProvider"/> that is responsible 
        //    /// for providing the <see cref="ScopeManager"/> used to manage scopes.
        //    /// </summary>
        //    public IScopeManagerProvider ScopeManagerProvider
        //    {
        //        get { return _current.ScopeManagerProvider; }
        //        set { _current.ScopeManagerProvider = value; }
        //    }

        //    /// <summary>
        //    /// Returns <b>true</b> if the container can create the requested service, otherwise <b>false</b>.
        //    /// </summary>
        //    /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        //    /// <param name="serviceName">The name of the service.</param>
        //    /// <returns><b>true</b> if the container can create the requested service, otherwise <b>false</b>.</returns>
        //    public bool CanGetInstance(Type serviceType, string serviceName)
        //    {
        //        return _current.CanGetInstance(serviceType, serviceName) || _parent.CanGetInstance(serviceType, serviceName);
        //    }

        //    /// <summary>
        //    /// Injects the property dependencies for a given <paramref name="instance"/>.
        //    /// </summary>
        //    /// <param name="instance">The target instance for which to inject its property dependencies.</param>
        //    /// <returns>The <paramref name="instance"/> with its property dependencies injected.</returns>
        //    public object InjectProperties(object instance)
        //    {
        //        var result = _current.InjectProperties(instance);
        //        return _parent.InjectProperties(result);
        //    }
        //}

    }
}

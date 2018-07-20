using System;
using System.Collections.Generic;
using System.Reflection;
using LightInject;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Implements <see cref="IContainer"/> with LightInject.
    /// </summary>
    public class LightInjectContainer : IContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectContainer"/> with a LightInject container.
        /// </summary>
        public LightInjectContainer(ServiceContainer container)
        {
            Container = container;
        }

        /// <summary>
        /// Gets the LightInject container.
        /// </summary>
        protected ServiceContainer Container { get; }

        /// <inheritdoc />
        public object ConcreteContainer => Container;

        /// <inheritdoc />
        public void Dispose()
            => Container.Dispose();

        #region Factory

        /// <inheritdoc />
        public object GetInstance(Type type)
            => Container.GetInstance(type);

        /// <inheritdoc />
        public object GetInstance(Type type, string name)
            => Container.GetInstance(type, name);

        /// <inheritdoc />
        public object GetInstance(Type type, object[] args)
            => Container.GetInstance(type, args);

        /// <inheritdoc />
        public object TryGetInstance(Type type)
            => Container.TryGetInstance(type);

        /// <inheritdoc />
        public IEnumerable<T> GetAllInstances<T>()
            => Container.GetAllInstances<T>();

        /// <inheritdoc />
        public IEnumerable<object> GetAllInstances(Type type)
            => Container.GetAllInstances(type);

        #endregion

        #region Registry

        /// <inheritdoc />
        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            switch (lifetime)
            {
                case Lifetime.Transient:
                case Lifetime.Request:
                case Lifetime.Scope:
                    Container.Register(serviceType, GetLifetime(lifetime));
                    break;
                case Lifetime.Singleton:
                    Container.RegisterSingleton(serviceType);
                    break;
                default:
                    throw new NotSupportedException($"Lifetime {lifetime} is not supported.");
            }
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            switch (lifetime)
            {
                case Lifetime.Transient:
                case Lifetime.Request:
                case Lifetime.Scope:
                    Container.Register(serviceType, implementingType, GetLifetime(lifetime));
                    break;
                case Lifetime.Singleton:
                    Container.RegisterSingleton(serviceType, implementingType);
                    break;
                default:
                    throw new NotSupportedException($"Lifetime {lifetime} is not supported.");
            }
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementingType, string name, Lifetime lifetime = Lifetime.Transient)
        {
            switch (lifetime)
            {
                case Lifetime.Transient:
                case Lifetime.Request:
                case Lifetime.Scope:
                    Container.Register(serviceType, implementingType, name, GetLifetime(lifetime));
                    break;
                case Lifetime.Singleton:
                    Container.RegisterSingleton(serviceType, implementingType, name);
                    break;
                default:
                    throw new NotSupportedException($"Lifetime {lifetime} is not supported.");
            }
        }

        /// <inheritdoc />
        public void Register<TService>(Func<IContainer, TService> factory, Lifetime lifetime = Lifetime.Transient)
        {
            switch (lifetime)
            {
                case Lifetime.Transient:
                case Lifetime.Request:
                case Lifetime.Scope:
                    Container.Register(f => factory(this), GetLifetime(lifetime));
                    break;
                case Lifetime.Singleton:
                    Container.RegisterSingleton(f => factory(this));
                    break;
                default:
                    throw new NotSupportedException($"Lifetime {lifetime} is not supported.");
            }
        }

        /// <inheritdoc />
        public void Register<T, TService>(Func<IContainer, T, TService> factory)
            => Container.Register<T, TService>((f, x) => factory(this, x));

        private ILifetime GetLifetime(Lifetime lifetime)
        {
            switch (lifetime)
            {
                case Lifetime.Transient:
                    return null;
                case Lifetime.Request:
                    return new PerRequestLifeTime();
                case Lifetime.Scope:
                    return new PerScopeLifetime();
                case Lifetime.Singleton:
                    return new PerContainerLifetime();
                default:
                    throw new NotSupportedException($"Lifetime {lifetime} is not supported.");
            }
        }

        /// <inheritdoc />
        public void RegisterInstance(Type serviceType, object instance)
            => Container.RegisterInstance(serviceType, instance);

        /// <inheritdoc />
        public void RegisterAuto(Type serviceBaseType)
        {
            Container.RegisterFallback((serviceType, serviceName) =>
            {
                // https://github.com/seesharper/LightInject/issues/173
                if (serviceBaseType.IsAssignableFromGtd(serviceType))
                    Container.Register(serviceType);
                return false;
            }, null);
        }

        /// <inheritdoc />
        public void RegisterOrdered(Type serviceType, Type[] implementingTypes, Lifetime lifetime = Lifetime.Transient)
            => Container.RegisterOrdered(serviceType, implementingTypes, _ => GetLifetime(lifetime));

        /// <inheritdoc />
        public T RegisterCollectionBuilder<T>()
            => Container.RegisterCollectionBuilder<T>();

        #endregion

        #region Control

        /// <inheritdoc />
        public IDisposable BeginScope()
            => Container.BeginScope();

        /// <inheritdoc />
        public void ConfigureForUmbraco()
        {
            // supports annotated constructor injections
            // eg to specify the service name on some services
            Container.EnableAnnotatedConstructorInjection();

            // from the docs: "LightInject considers all read/write properties a dependency, but implements
            // a loose strategy around property dependencies, meaning that it will NOT throw an exception
            // in the case of an unresolved property dependency."
            //
            // in Umbraco we do NOT want to do property injection by default, so we have to disable it.
            // from the docs, the following line will cause the container to "now only try to inject
            // dependencies for properties that is annotated with the InjectAttribute."
            //
            // could not find it documented, but tests & code review shows that LightInject considers a
            // property to be "injectable" when its setter exists and is not static, nor private, nor
            // it is an index property. which means that eg protected or internal setters are OK.
            Container.EnableAnnotatedPropertyInjection();

            // ensure that we do *not* scan assemblies
            // we explicitely RegisterFrom our own composition roots and don't want them scanned
            Container.AssemblyScanner = new AssemblyScanner(/*container.AssemblyScanner*/);

            // see notes in MixedLightInjectScopeManagerProvider
            Container.ScopeManagerProvider = new MixedLightInjectScopeManagerProvider();
        }

        private class AssemblyScanner : IAssemblyScanner
        {
            //private readonly IAssemblyScanner _scanner;

            //public AssemblyScanner(IAssemblyScanner scanner)
            //{
            //    _scanner = scanner;
            //}

            public void Scan(Assembly assembly, IServiceRegistry serviceRegistry, Func<ILifetime> lifetime, Func<Type, Type, bool> shouldRegister, Func<Type, Type, string> serviceNameProvider)
            {
                // nothing - we *could* scan non-Umbraco assemblies, though
            }

            public void Scan(Assembly assembly, IServiceRegistry serviceRegistry)
            {
                // nothing - we *could* scan non-Umbraco assemblies, though
            }
        }

        /// <inheritdoc />
        public virtual void ConfigureForWeb()
        { }

        /// <inheritdoc />
        public void EnablePerWebRequestScope()
        {
            if (!(Container.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            smp.EnablePerWebRequestScope();
        }

        #endregion
    }
}

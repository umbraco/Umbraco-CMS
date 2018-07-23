using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;
using LightInject;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Implements <see cref="IContainer"/> with LightInject.
    /// </summary>
    public class LightInjectContainer : IContainer
    {
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectContainer"/> with a LightInject container.
        /// </summary>
        protected LightInjectContainer(ServiceContainer container)
        {
            Container = container;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LightInjectContainer"/> class.
        /// </summary>
        public static LightInjectContainer Create()
            => new LightInjectContainer(CreateServiceContainer());

        /// <summary>
        /// Creates a new instance of the LightInject service container.
        /// </summary>
        protected static ServiceContainer CreateServiceContainer()
            => new ServiceContainer(new ContainerOptions { EnablePropertyInjection = false });

        /// <summary>
        /// Gets the LightInject container.
        /// </summary>
        protected ServiceContainer Container { get; }

        /// <inheritdoc />
        public object ConcreteContainer => Container;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            Container.Dispose();
        }

        #region Factory

        /// <inheritdoc />
        public object GetInstance(Type type)
            => Container.GetInstance(type);

        /// <inheritdoc />
        public object GetInstance(Type type, string name)
            => Container.GetInstance(type, name);

        /// <inheritdoc />
        public object GetInstance(Type type, params object[] args)
        {
            // LightInject has this, but then it requires RegisterConstructorDependency etc and has various oddities
            //return Container.GetInstance(type, args);

            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
            if (ctor == null) throw new InvalidOperationException($"Could not find a public constructor for type {type.FullName}.");

            var ctorParameters = ctor.GetParameters();
            var ctorArgs = new object[ctorParameters.Length];
            var i = 0;
            foreach (var parameter in ctorParameters)
            {
                // no! IsInstanceOfType is not ok here
                // ReSharper disable once UseMethodIsInstanceOfType
                var arg = args?.FirstOrDefault(a => parameter.ParameterType.IsAssignableFrom(a.GetType()));
                ctorArgs[i++] = arg ?? GetInstance(parameter.ParameterType);
            }
            return ctor.Invoke(ctorArgs);
        }

        /// <inheritdoc />
        public object TryGetInstance(Type type)
            => Container.TryGetInstance(type);

        /// <inheritdoc />
        public IEnumerable<T> GetAllInstances<T>()
            => Container.GetAllInstances<T>();

        /// <inheritdoc />
        public IEnumerable<object> GetAllInstances(Type type)
            => Container.GetAllInstances(type);

        /// <inheritdoc />
        public IEnumerable<Registration> GetRegistered(Type type)
            => Container.GetAvailableServices(type).Select(x => new Registration(x.ServiceType, x.ServiceName));

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

        // was the Light-Inject specific way of dealing with args, but we've replaced it with our own
        // beware! does NOT work on singletons, see https://github.com/seesharper/LightInject/issues/294
        //
        ///// <inheritdoc />
        //public void RegisterConstructorDependency<TDependency>(Func<IContainer, ParameterInfo, TDependency> factory)
        //    => Container.RegisterConstructorDependency((f, x) => factory(this, x));
        //
        ///// <inheritdoc />
        //public void RegisterConstructorDependency<TDependency>(Func<IContainer, ParameterInfo, object[], TDependency> factory)
        //    => Container.RegisterConstructorDependency((f, x, a) => factory(this, x, a));

        #endregion

        #region Control

        /// <inheritdoc />
        public IDisposable BeginScope()
            => Container.BeginScope();

        /// <inheritdoc />
        public IContainer ConfigureForUmbraco()
        {
            // supports annotated constructor injections
            // eg to specify the service name on some services
            Container.EnableAnnotatedConstructorInjection();

            // note: the block below is disabled, we do not allow property injection at all anymore
            //       (see options in CreateServiceContainer)
            //
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
            //Container.EnableAnnotatedPropertyInjection();

            // ensure that we do *not* scan assemblies
            // we explicitely RegisterFrom our own composition roots and don't want them scanned
            Container.AssemblyScanner = new AssemblyScanner(/*container.AssemblyScanner*/);

            // see notes in MixedLightInjectScopeManagerProvider
            Container.ScopeManagerProvider = new MixedLightInjectScopeManagerProvider();

            // note: the block below is disabled, because it does not work, because collection builders
            //       are singletons, and constructor dependencies don't work on singletons, see
            //       https://github.com/seesharper/LightInject/issues/294
            //
            // if looking for a IContainer, and one was passed in args, use it
            // this is for collection builders which require the IContainer
            //Container.RegisterConstructorDependency((c, i, a) => a.OfType<IContainer>().FirstOrDefault());
            //
            // and, the block below is also disabled, because it is ugly
            //
            //// which means that the only way to inject the container into builders is to register it
            //Container.RegisterInstance<IContainer>(this);
            //
            // instead, we use an explicit GetInstance with arguments implementation

            return this;
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
        public virtual IContainer ConfigureForWeb()
        {
            return this;
        }

        /// <inheritdoc />
        public IContainer EnablePerWebRequestScope()
        {
            if (!(Container.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            smp.EnablePerWebRequestScope();
            return this;
        }

        #endregion
    }
}

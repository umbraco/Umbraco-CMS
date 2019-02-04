using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing.MSDI;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Implements DI with LightInject.
    /// </summary>
    public class LightInjectContainer : IFactory, IDisposable // IRegister, 
    {
        private int _disposed;

        private static IServiceCollection services = null;
        private readonly IServiceProvider serviceProvider;
        private readonly ServiceContainer lightinjectContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectContainer"/> with a LightInject container.
        /// </summary>
        protected LightInjectContainer(ServiceContainer container, IServiceCollection services)
        {
            lightinjectContainer = container;
            serviceProvider = container.CreateServiceProvider(services);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LightInjectContainer"/> class.
        /// </summary>
        public static IServiceCollection Create()
            => services ?? (services = new DefaultServiceCollection());

        public static IFactory CreateFactory(IServiceCollection serviceCollection)
            => new LightInjectContainer(CreateServiceContainer(), serviceCollection);

        /// <summary>
        /// Creates a new instance of the LightInject service container.
        /// </summary>
        protected static ServiceContainer CreateServiceContainer()
        {
            var container = new ServiceContainer(new ContainerOptions { EnablePropertyInjection = false });

            // note: the block below is disabled, as it is too LightInject-specific
            //
            // supports annotated constructor injections
            // eg to specify the service name on some services
            //container.EnableAnnotatedConstructorInjection();

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
            // we explicitly RegisterFrom our own composition roots and don't want them scanned
            container.AssemblyScanner = new AssemblyScanner(/*container.AssemblyScanner*/);

            // see notes in MixedLightInjectScopeManagerProvider
            container.ScopeManagerProvider = new MixedLightInjectScopeManagerProvider();

            // note: the block below is disabled, because it does not work, because collection builders
            //       are singletons, and constructor dependencies don't work on singletons, see
            //       https://github.com/seesharper/LightInject/issues/294
            //
            // if looking for a IContainer, and one was passed in args, use it
            // this is for collection builders which require the IContainer
            //container.RegisterConstructorDependency((c, i, a) => a.OfType<IContainer>().FirstOrDefault());
            //
            // and, the block below is also disabled, because it is ugly
            //
            //// which means that the only way to inject the container into builders is to register it
            //container.RegisterInstance<IContainer>(this);
            //
            // instead, we use an explicit GetInstance with arguments implementation

            return container;
        }

        /// <summary>
        /// Gets the LightInject container.
        /// </summary>
        //protected ServiceContainer Container => serviceContainer;

        /// <inheritdoc cref="IRegister"/>
        /// <inheritdoc cref="IFactory"/>
        public object Concrete => lightinjectContainer;

        protected ServiceContainer LightinjectContainer => lightinjectContainer;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            lightinjectContainer.Dispose();
        }

        /// <inheritdoc />
        public IFactory CreateFactory() => this;

        #region IServiceProvider

        public object GetService(Type type)
        {
            return serviceProvider.GetService(type);
        }

        #endregion

        #region Factory

        /// <inheritdoc />
        public object GetInstance(Type type)
            => this.GetRequiredService(type);

        /// <inheritdoc />
        public TService GetInstanceFor<TService, TTarget>()
            => lightinjectContainer.GetInstance<TService>(DefaultServiceCollection.GetTargetedServiceName<TService, TTarget>());

        /// <inheritdoc />
        public object TryGetInstance(Type type)
            => this.GetService(type);

        /// <inheritdoc />
        public IEnumerable<T> GetAllInstances<T>()
            where T : class
            => this.GetAllInstances(typeof(T)).Cast<T>();

        /// <inheritdoc />
        public IEnumerable<object> GetAllInstances(Type type)
            => this.GetServices(type);

        /// <inheritdoc />
        public void Release(object instance)
        {
            // nothing to release with LightInject
        }

        // notes:
        // we may want to look into MS code, eg:
        // TypeActivatorCache in MVC at https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.Core/Internal/TypeActivatorCache.cs
        // which relies onto
        // ActivatorUtilities at https://github.com/aspnet/DependencyInjection/blob/master/shared/Microsoft.Extensions.ActivatorUtilities.Sources/ActivatorUtilities.cs

        #endregion


        #region Control

        /// <inheritdoc />
        public IDisposable BeginScope()
            => lightinjectContainer.BeginScope();

        /// <inheritdoc />
        public virtual void ConfigureForWeb()
        { }

        /// <inheritdoc />
        public void EnablePerWebRequestScope()
        {
            if (!(lightinjectContainer.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            smp.EnablePerWebRequestScope();
        }

        private class AssemblyScanner : IAssemblyScanner
        {
            public void Scan(Assembly assembly, IServiceRegistry serviceRegistry, Func<ILifetime> lifetime, Func<Type, Type, bool> shouldRegister, Func<Type, Type, string> serviceNameProvider)
            {
                // nothing - we don't want LightInject to scan
            }

            public void Scan(Assembly assembly, IServiceRegistry serviceRegistry)
            {
                // nothing - we don't want LightInject to scan
            }
        }

        #endregion
    }
}

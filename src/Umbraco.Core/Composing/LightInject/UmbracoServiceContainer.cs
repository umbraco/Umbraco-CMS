using System;
using System.Reflection;
using LightInject;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Light Inject service container with modifications for Umbraco
    /// </summary>
    internal class UmbracoServiceContainer : ServiceContainer, IServiceRegistry
    {
        private readonly IAssemblyScanner _origAssemblyScanner;

        public UmbracoServiceContainer(ContainerOptions options) : base(options)
        {
            // store ref to original (real) assembly scanner
            _origAssemblyScanner = AssemblyScanner;

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
            AssemblyScanner = new NullAssemblyScanner(/*container.AssemblyScanner*/);

            // see notes in MixedLightInjectScopeManagerProvider
            ScopeManagerProvider = new MixedLightInjectScopeManagerProvider();
        }

        #region RegisterAssembly - allows assembly scanning
        IServiceRegistry IServiceRegistry.RegisterAssembly(Assembly assembly)
        {
            using (AllowAssemblyScan())
            {
                return RegisterAssembly(assembly);
            }
        }

        IServiceRegistry IServiceRegistry.RegisterAssembly(Assembly assembly, Func<Type, Type, bool> shouldRegister)
        {
            using (AllowAssemblyScan())
            {
                return RegisterAssembly(assembly, shouldRegister);
            }
        }

        IServiceRegistry IServiceRegistry.RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory)
        {
            using (AllowAssemblyScan())
            {
                return RegisterAssembly(assembly, lifetimeFactory);
            }
        }

        IServiceRegistry IServiceRegistry.RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory, Func<Type, Type, bool> shouldRegister)
        {
            using (AllowAssemblyScan())
            {
                return RegisterAssembly(assembly, lifetimeFactory, shouldRegister);
            }
        }

        IServiceRegistry IServiceRegistry.RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory, Func<Type, Type, bool> shouldRegister, Func<Type, Type, string> serviceNameProvider)
        {
            using (AllowAssemblyScan())
            {
                return RegisterAssembly(assembly, lifetimeFactory, shouldRegister, serviceNameProvider);
            }
        } 
        #endregion

        private IDisposable AllowAssemblyScan() => new ReplaceAssemblyScanner(this, _origAssemblyScanner);

        /// <summary>
        /// A Noop assembly scanner
        /// </summary>
        private class NullAssemblyScanner : IAssemblyScanner
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

        /// <summary>
        /// Used to replace the current IAssemblyScanner with another one in a using block
        /// </summary>
        private class ReplaceAssemblyScanner : IDisposable
        {
            private readonly ServiceContainer _serviceContainer;
            private readonly IAssemblyScanner _origAssemblyScanner;

            public ReplaceAssemblyScanner(ServiceContainer serviceContainer, IAssemblyScanner assemblyScanner)
            {
                _serviceContainer = serviceContainer;
                // store orig
                _origAssemblyScanner = _serviceContainer.AssemblyScanner;
                // set to new one
                _serviceContainer.AssemblyScanner = assemblyScanner;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls            

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // restore the original scanner
                        _serviceContainer.AssemblyScanner = _origAssemblyScanner;
                    }
                    disposedValue = true;
                }
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                Dispose(true);            
            }
            #endregion

        }
    }
}

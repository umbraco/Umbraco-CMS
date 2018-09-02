using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Composing
{
    /// <summary>
    /// Provides a base class for module injectors.
    /// </summary>
    /// <typeparam name="TModule">The type of the injected module.</typeparam>
    public abstract class ModuleInjector<TModule> : IHttpModule
        where TModule : IHttpModule
    {
        protected TModule Module { get; private set; }

        /// <inheritdoc />
        public void Init(HttpApplication context)
        {
            try
            {
                // using the service locator here - no other way, really
                Module = Current.Container.GetInstance<TModule>();
                Module.Init(context);
            }
            catch
            {
                var runtimeState = Current.Container.GetInstance<IRuntimeState>();
                if (runtimeState.BootFailedException != null)
                {
                    throw new Exception("Failed to boot", runtimeState.BootFailedException);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Module?.Dispose();
        }
    }
}

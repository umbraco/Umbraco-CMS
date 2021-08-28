using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;

namespace Umbraco.Web.Composing
{
    /// <summary>
    /// Provides a base class for module injectors.
    /// </summary>
    /// <typeparam name="TModule">The type of the injected module.</typeparam>
    public abstract class ModuleInjector<TModule> : IHttpModule
        where TModule : class, IHttpModule
    {
        protected TModule Module { get; private set; }

        /// <inheritdoc />
        public void Init(HttpApplication context)
        {
            try
            {
                // using the service locator here - no other way, really
                Module = Current.Factory.GetInstance<TModule>();
            }
            catch
            {
                // if GetInstance fails, it may be because of a boot error, in
                // which case that is the error we actually want to report
                IRuntimeState runtimeState = null;

                try
                {
                    runtimeState = Current.RuntimeState;
                }
                catch { /* don't make it worse */ }

                if (runtimeState?.BootFailedException != null)
                {
                    // if we throw the exception here the HttpApplication.Application_Error method will never be hit
                    // and our custom error page will not be shown, so we need to wait for the request to begin
                    // before we throw the exception.
                    context.BeginRequest += (sender, args) =>
                    {
                        BootFailedException.Rethrow(runtimeState.BootFailedException);
                    };
                    return;
                }

                // else... throw what we have
                throw;
            }

            // initialize
            Module.Init(context);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Module?.Dispose();
        }
    }
}

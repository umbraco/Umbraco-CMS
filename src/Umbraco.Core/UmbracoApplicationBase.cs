using System;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using log4net;
using LightInject;
using Umbraco.Core.DI;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides an abstract base class for the Umbraco HttpApplication.
    /// </summary>
    public abstract class UmbracoApplicationBase : HttpApplication
    {
        private IRuntime _runtime;

        /// <summary>
        /// Gets a runtime.
        /// </summary>
        protected abstract IRuntime GetRuntime();

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected virtual ILogger GetLogger()
        {
            return Logger.CreateWithDefaultLog4NetConfiguration();
        }

        // events - in the order they trigger

        // were part of the BootManager architecture, would trigger only for the initial
        // application, so they need not be static, and they would let ppl hook into the
        // boot process... but I believe this can be achieved with components as well and
        // we don't need these events.
        //public event EventHandler ApplicationStarting;
        //public event EventHandler ApplicationStarted;

        // this event can only be static since there will be several instances of this class
        // triggers for each application instance, ie many times per lifetime of the application
        public static event EventHandler ApplicationInit;

        // this event can only be static since there will be several instances of this class
        // triggers once per error
        public static event EventHandler ApplicationError;

        // this event can only be static since there will be several instances of this class
        // triggers once per lifetime of the application, before it is unloaded
        public static event EventHandler ApplicationEnd;

        #region Start

        // internal for tests
        internal void HandleApplicationStart(object sender, EventArgs evargs)
        {
            // ******** THIS IS WHERE EVERYTHING BEGINS ********

            // create the container for the application, and configure.
            // the boot manager is responsible for registrations
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore(); // also sets Current.Container

            // register the essential stuff,
            // ie the global application logger
            // (profiler etc depend on boot manager)
            var logger = GetLogger();
            container.RegisterInstance(logger);
            // now it is ok to use Current.Logger

            ConfigureUnhandledException(logger);

            // get runtime & boot
            _runtime = GetRuntime();
            _runtime.Boot(container);
        }

        protected virtual void ConfigureUnhandledException(ILogger logger)
        {
            // take care of unhandled exceptions - there is nothing we can do to
            // prevent the entire w3wp process to go down but at least we can try
            // and log the exception
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?

                var msg = "Unhandled exception in AppDomain";
                if (isTerminating) msg += " (terminating)";
                msg += ".";
                logger.Error<UmbracoApplicationBase>(msg, exception);
            };
        }

        // called by ASP.NET (auto event wireup) once per app domain
        // do NOT set instance data here - only static (see docs)
        // sender is System.Web.HttpApplicationFactory, evargs is EventArgs.Empty
        protected void Application_Start(object sender, EventArgs evargs)
        {
            Thread.CurrentThread.SanitizeThreadCulture();
            HandleApplicationStart(sender, evargs);
        }

        #endregion

        #region Init

        private void OnApplicationInit(object sender, EventArgs evargs)
        {
            TryInvoke(ApplicationInit, "ApplicationInit", sender, evargs);
        }

        // called by ASP.NET for every HttpApplication instance after all modules have been created
        // which means that this will be called *many* times for different apps when Umbraco runs
        public override void Init()
        {
            // note: base.Init() is what initializes all of the httpmodules, ties up a bunch of stuff with IIS, etc...
            // therefore, since OWIN is an HttpModule when running in IIS/ASP.Net the OWIN startup is not executed
            // until this method fires and by that time - Umbraco has booted already

            base.Init();
            OnApplicationInit(this, new EventArgs());
        }

        #endregion

        #region End

        protected virtual void OnApplicationEnd(object sender, EventArgs evargs)
        {
            ApplicationEnd?.Invoke(this, EventArgs.Empty);
        }

        // internal for tests
        internal void HandleApplicationEnd()
        {
            if (_runtime != null)
            {
                _runtime.Terminate();
                _runtime.DisposeIfDisposable();
                _runtime = null;
            }

            Current.Reset(); // dispose the container and everything

            if (SystemUtilities.GetCurrentTrustLevel() != AspNetHostingPermissionLevel.Unrestricted) return;

            // try to log the detailed shutdown message (typical asp.net hack: http://weblogs.asp.net/scottgu/433194)
            try
            {
                var runtime = (HttpRuntime) typeof(HttpRuntime).InvokeMember("_theRuntime",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                    null, null, null);
                if (runtime == null)
                    return;

                var shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                    null, runtime, null);

                var shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                    null, runtime, null);

                var shutdownMsg = $"Application shutdown. Details: {HostingEnvironment.ShutdownReason}\r\n\r\n_shutDownMessage={shutDownMessage}\r\n\r\n_shutDownStack={shutDownStack}";

                Current.Logger.Info<UmbracoApplicationBase>(shutdownMsg);
            }
            catch (Exception)
            {
                //if for some reason that fails, then log the normal output
                Current.Logger.Info<UmbracoApplicationBase>("Application shutdown. Reason: " + HostingEnvironment.ShutdownReason);
            }
        }

        // called by ASP.NET (auto event wireup) once per app domain
        // sender is System.Web.HttpApplicationFactory, evargs is EventArgs.Empty
        protected void Application_End(object sender, EventArgs evargs)
        {
            HandleApplicationEnd();
            OnApplicationEnd(sender, evargs);
            LogManager.Shutdown();
        }

        #endregion

        #region Error

        protected virtual void OnApplicationError(object sender, EventArgs evargs)
        {
            ApplicationError?.Invoke(this, EventArgs.Empty);
        }

        private void HandleApplicationError()
        {
            var exception = Server.GetLastError();

            // ignore HTTP errors
            if (exception.GetType() == typeof(HttpException)) return;

            Current.Logger.Error<UmbracoApplicationBase>("An unhandled exception occurred.", exception);
        }

        // called by ASP.NET (auto event wireup) at any phase in the application life cycle
        protected void Application_Error(object sender, EventArgs e)
        {
            // when unhandled errors occur
            HandleApplicationError();
            OnApplicationError(sender, e);
        }

        #endregion
        
        #region Utilities

        private static void TryInvoke(EventHandler handler, string name, object sender, EventArgs evargs)
        {
            try
            {
                handler?.Invoke(sender, evargs);
            }
            catch (Exception ex)
            {
                Current.Logger.Error<UmbracoApplicationBase>($"Error in {name} handler.", ex);
                throw;
            }
        }

        #endregion
    }
}

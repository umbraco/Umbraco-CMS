using System;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using log4net;
using LightInject;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides an abstract base class for the Umbraco HttpApplication.
    /// </summary>
    /// <remarks>
    /// This is exposed in Core so that we can have the IApplicationEventHandler in the core project so that
    /// IApplicationEventHandler's can fire/execute outside of the web contenxt (i.e. in console applications). fixme wtf?
    /// </remarks>
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

        #region Start

        // fixme? dont make much sense!
        public event EventHandler ApplicationStarting;
        public event EventHandler ApplicationStarted;

        // internal for tests
        internal void HandleApplicationStart(object sender, EventArgs evargs)
        {
            // NOTE: THIS IS WHERE EVERYTHING BEGINS!

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

            // get runtime & boot
            _runtime = GetRuntime();
            _runtime.Boot(container);

            // this is extra that should get removed
            _runtime
                .Initialize()
                .Startup(appContext => OnApplicationStarting(sender, evargs))
                .Complete(appContext => OnApplicationStarted(sender, evargs));
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

        // this event can only be static since there will be several instances of this class
        public static event EventHandler ApplicationInit;

        private void OnApplicationInit(object sender, EventArgs evargs)
        {
            try
            {
                ApplicationInit?.Invoke(sender, evargs);
            }
            catch (Exception ex)
            {
                Current.Logger.Error<UmbracoApplicationBase>("Exception in an ApplicationInit event handler.", ex);
                throw;
            }
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

        // this event can only be static since there will be several instances of this class
        public static event EventHandler ApplicationEnd;

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

        // this event can only be static since there will be several instances of this class
        public static event EventHandler ApplicationError;

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


        /// <summary>
        /// Developers can override this method to modify objects on startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="evargs"></param>
        protected virtual void OnApplicationStarting(object sender, EventArgs evargs)
        {
            try
            {
                ApplicationStarting?.Invoke(sender, evargs);
            }
            catch (Exception ex)
            {
                Current.Logger.Error<UmbracoApplicationBase>("An error occurred in an ApplicationStarting event handler", ex);
                throw;
            }
        }

        /// <summary>
        /// Developers can override this method to do anything they need to do once the application startup routine is completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="evargs"></param>
        protected virtual void OnApplicationStarted(object sender, EventArgs evargs)
        {
            try
            {
                ApplicationStarted?.Invoke(sender, evargs);
            }
            catch (Exception ex)
            {
                Current.Logger.Error<UmbracoApplicationBase>("An error occurred in an ApplicationStarted event handler", ex);
                throw;
            }
        }
    }
}

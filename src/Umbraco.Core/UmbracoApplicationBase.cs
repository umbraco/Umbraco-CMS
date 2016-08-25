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
    /// The abstract class for the Umbraco HttpApplication
    /// </summary>
    /// <remarks>
    /// This is exposed in the core so that we can have the IApplicationEventHandler in the core project so that
    /// IApplicationEventHandler's can fire/execute outside of the web contenxt (i.e. in console applications)
    /// </remarks>
    public abstract class UmbracoApplicationBase : HttpApplication
    {
        /// <summary>
        /// Gets a boot manager.
        /// </summary>
        protected abstract IBootManager GetBootManager();

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected virtual ILogger GetLogger()
        {
            return Logger.CreateWithDefaultLog4NetConfiguration();
        }

        /// <summary>
        /// Boots up the Umbraco application.
        /// </summary>
        internal void StartApplication(object sender, EventArgs e)
        {
            // create the container for the application, and configure.
            // the boot manager is responsible for registrations
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore(); // also sets Current.Container

            // register the essential stuff,
            // ie the global application logger
            // (profiler etc depend on boot manager)
            var logger = GetLogger();
            container.RegisterInstance(logger);

            // take care of unhandled exceptions - there is nothing we can do to
            // prevent the entire w3wp process to go down but at least we can try
            // and log the exception
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?

                var msg = "Unhandled exception in AppDomain";
                if (isTerminating) msg += " (terminating)";
                logger.Error<UmbracoApplicationBase>(msg, exception);
            };

            // boot
            GetBootManager()
                .Initialize()
                .Startup(appContext => OnApplicationStarting(sender, e))
                .Complete(appContext => OnApplicationStarted(sender, e));
        }

        #region Events

        public event EventHandler ApplicationStarting;
        public event EventHandler ApplicationStarted;

        /// <summary>
        /// Called when the HttpApplication.Init() is fired, allows developers to subscribe to the HttpApplication events
        /// </summary>
        /// <remarks>
        /// Needs to be static otherwise null refs occur - though I don't know why FIXME wtf?
        /// </remarks>
        public static event EventHandler ApplicationInit;
        public static event EventHandler ApplicationError;
        public static event EventHandler ApplicationEnd;


        /// <summary>
        /// Initializes the Umbraco application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Start(object sender, EventArgs e)
        {
            Thread.CurrentThread.SanitizeThreadCulture();
            StartApplication(sender, e);
        }

        /// <summary>
        /// Override init and raise the event
        /// </summary>
        /// <remarks>
        /// DID YOU KNOW? The Global.asax Init call is the thing that initializes all of the httpmodules, ties up a bunch of stuff with IIS, etc...
        /// Therefore, since OWIN is an HttpModule when running in IIS/ASP.Net the OWIN startup is not executed until this method fires and by that
        /// time, Umbraco has performed it's bootup sequence.
        /// </remarks>
        public override void Init()
        {
            base.Init();
            OnApplicationInit(this, new EventArgs());
        }

        /// <summary>
        /// Developers can override this method to modify objects on startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnApplicationStarting(object sender, EventArgs e)
        {
            try
            {
                ApplicationStarting?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoApplicationBase>("An error occurred in an ApplicationStarting event handler", ex);
                throw;
            }
        }

        /// <summary>
        /// Developers can override this method to do anything they need to do once the application startup routine is completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnApplicationStarted(object sender, EventArgs e)
        {
            try
            {
                ApplicationStarted?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoApplicationBase>("An error occurred in an ApplicationStarted event handler", ex);
                throw;
            }
        }

        /// <summary>
        /// Called to raise the ApplicationInit event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationInit(object sender, EventArgs e)
        {
            try
            {
                ApplicationInit?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoApplicationBase>("An error occurred in an ApplicationInit event handler", ex);
                throw;
            }
        }

        /// <summary>
        /// A method that can be overridden to invoke code when the application has an error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnApplicationError(object sender, EventArgs e)
        {
            ApplicationError?.Invoke(this, EventArgs.Empty);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            var exc = Server.GetLastError();

            // Ignore HTTP errors
            if (exc.GetType() == typeof(HttpException))
            {
                return;
            }

            Current.Logger.Error<UmbracoApplicationBase>("An unhandled exception occurred", exc);

            OnApplicationError(sender, e);
        }

        /// <summary>
        /// A method that can be overridden to invoke code when the application shuts down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnApplicationEnd(object sender, EventArgs e)
        {
            ApplicationEnd?.Invoke(this, EventArgs.Empty);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            if (SystemUtilities.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted)
            {
                //Try to log the detailed shutdown message (typical asp.net hack: http://weblogs.asp.net/scottgu/433194)
                try
                {
                    var runtime = (HttpRuntime)typeof(HttpRuntime).InvokeMember("_theRuntime",
                                BindingFlags.NonPublic
                                | BindingFlags.Static
                                | BindingFlags.GetField,
                                null,
                                null,
                                null);
                    if (runtime == null)
                        return;

                    var shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                        BindingFlags.NonPublic
                        | BindingFlags.Instance
                        | BindingFlags.GetField,
                        null,
                        runtime,
                        null);

                    var shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                        BindingFlags.NonPublic
                        | BindingFlags.Instance
                        | BindingFlags.GetField,
                        null,
                        runtime,
                        null);

                    var shutdownMsg = $"{HostingEnvironment.ShutdownReason}\r\n\r\n_shutDownMessage={shutDownMessage}\r\n\r\n_shutDownStack={shutDownStack}";

                    Current.Logger.Info<UmbracoApplicationBase>("Application shutdown. Details: " + shutdownMsg);
                }
                catch (Exception)
                {
                    //if for some reason that fails, then log the normal output
                    Current.Logger.Info<UmbracoApplicationBase>("Application shutdown. Reason: " + HostingEnvironment.ShutdownReason);
                }
            }
            OnApplicationEnd(sender, e);

            // last thing to do is shutdown log4net
            LogManager.Shutdown();
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using StackExchange.Profiling;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core
{

    /// <summary>
    /// The abstract class for the Umbraco HttpApplication
    /// </summary>
    /// <remarks>
    /// This is exposed in the core so that we can have the IApplicationEventHandler in the core project so that 
    /// IApplicationEventHandler's can fire/execute outside of the web contenxt (i.e. in console applications)
    /// </remarks>
    public abstract class UmbracoApplicationBase : System.Web.HttpApplication
    {

        public static event EventHandler ApplicationStarting;
        public static event EventHandler ApplicationStarted;

        /// <summary>
        /// Called when the HttpApplication.Init() is fired, allows developers to subscribe to the HttpApplication events
        /// </summary>
        public static event EventHandler ApplicationInit;

        /// <summary>
        /// Boots up the Umbraco application
        /// </summary>
        internal void StartApplication(object sender, EventArgs e)
        {
            //don't output the MVC version header (security)
            MvcHandler.DisableMvcResponseHeader = true;

            //boot up the application
            GetBootManager()
                .Initialize()
                .Startup(appContext => OnApplicationStarting(sender, e))
                .Complete(appContext => OnApplicationStarted(sender, e));

            //And now we can dispose of our startup handlers - save some memory
            ApplicationEventsResolver.Current.Dispose();
        }

        /// <summary>
        /// Initializes the Umbraco application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Start(object sender, EventArgs e)
        {
            StartApplication(sender, e);
        }

        /// <summary>
        /// Override init and raise the event
        /// </summary>
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
            if (ApplicationStarting != null)
                ApplicationStarting(sender, e);
        }

        /// <summary>
        /// Developers can override this method to do anything they need to do once the application startup routine is completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnApplicationStarted(object sender, EventArgs e)
        {
            if (ApplicationStarted != null)
                ApplicationStarted(sender, e);
        }

        /// <summary>
        /// Called to raise the ApplicationInit event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationInit(object sender, EventArgs e)
        {
            if (ApplicationInit != null)
                ApplicationInit(sender, e);
        }

        /// <summary>
        /// A method that can be overridden to invoke code when the application has an error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnApplicationError(object sender, EventArgs e)
        {

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
            
            Logger.Error<UmbracoApplicationBase>("An unhandled exception occurred", exc);

            OnApplicationError(sender, e);
        }

        /// <summary>
        /// A method that can be overridden to invoke code when the application shuts down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnApplicationEnd(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            if (SystemUtilities.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted)
            {
                Logger.Info<UmbracoApplicationBase>("Application shutdown. Reason: " + HostingEnvironment.ShutdownReason);
            }
            OnApplicationEnd(sender, e);
        }

        protected abstract IBootManager GetBootManager();

        protected ILogger Logger
        {
            get
            {
                if (LoggerResolver.HasCurrent && LoggerResolver.Current.HasValue)
                {
                    return LoggerResolver.Current.Logger;
                }
                return new HttpTraceLogger();
            }
        }

        private class HttpTraceLogger : ILogger
        {
            public void Error(Type callingType, string message, Exception exception)
            {
                if (HttpContext.Current == null) return;
                HttpContext.Current.Trace.Warn(callingType.ToString(), message + Environment.NewLine + exception);
            }

            public void Warn(Type callingType, string message, params Func<object>[] formatItems)
            {
                if (HttpContext.Current == null) return;
                HttpContext.Current.Trace.Warn(callingType.ToString(), string.Format(message, formatItems.Select(x => x())));
            }

            public void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
            {
                if (HttpContext.Current == null) return;
                HttpContext.Current.Trace.Warn(callingType.ToString(), string.Format(message + Environment.NewLine + e, formatItems.Select(x => x())));
            }

            public void Info(Type callingType, Func<string> generateMessage)
            {
                if (HttpContext.Current == null) return;
                HttpContext.Current.Trace.Write(callingType.ToString(), generateMessage());
            }

            public void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems)
            {
                if (HttpContext.Current == null) return;
                HttpContext.Current.Trace.Write(type.ToString(), string.Format(generateMessageFormat, formatItems.Select(x => x())));
            }

            public void Debug(Type callingType, Func<string> generateMessage)
            {
                if (HttpContext.Current == null) return;
                HttpContext.Current.Trace.Write(callingType.ToString(), generateMessage());
            }

            public void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems)
            {
                if (HttpContext.Current == null) return;
                HttpContext.Current.Trace.Write(type.ToString(), string.Format(generateMessageFormat, formatItems.Select(x => x())));
            }
        }
    }
}

using System;
using System.Web;
using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Profiling
{
    /// <summary>
    /// A profiler used for web based activity based on the MiniProfiler framework
    /// </summary>
    internal class WebProfiler : IProfiler
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Binds to application events to enable the MiniProfiler
        /// </remarks>
        internal WebProfiler()
        {
            UmbracoApplicationBase.ApplicationInit += UmbracoApplicationApplicationInit;
        }

        /// <summary>
        /// Handle the Init event o fthe UmbracoApplication which allows us to subscribe to the HttpApplication events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UmbracoApplicationApplicationInit(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app == null) return;

            if (SystemUtilities.GetCurrentTrustLevel() < AspNetHostingPermissionLevel.High)
            {
                //If we don't have a high enough trust level we cannot bind to the events
                LogHelper.Info<WebProfiler>("Cannot start the WebProfiler since the application is running in Medium trust");
            }
            else
            {
                app.BeginRequest += UmbracoApplicationBeginRequest;
                app.EndRequest += UmbracoApplicationEndRequest;
            }
        }

        /// <summary>
        /// Handle the begin request event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UmbracoApplicationEndRequest(object sender, EventArgs e)
        {
            if (CanPerformProfilingAction(sender))
            {
                Stop();
            }
        }

        /// <summary>
        /// Handle the end request event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UmbracoApplicationBeginRequest(object sender, EventArgs e)
        {
            if (CanPerformProfilingAction(sender))
            {
                Start();
            }
        }

        private bool CanPerformProfilingAction(object sender)
        {
            if (GlobalSettings.DebugMode == false) 
                return false;
            
            //will not run in medium trust
            if (SystemUtilities.GetCurrentTrustLevel() < AspNetHostingPermissionLevel.High) 
                return false;

            var request = TryGetRequest(sender);

            if (request.Success == false || request.Result.Url.IsClientSideRequest())
                return false;

            if (string.IsNullOrEmpty(request.Result.QueryString["umbDebug"]))
                return true;

            if (request.Result.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath))
                return true;

            return true;
        }

        /// <summary>
        /// Render the UI to display the profiler 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Generally used for HTML displays
        /// </remarks>
        public string Render()
        {
            return MiniProfiler.RenderIncludes(RenderPosition.Right).ToString();
        }

        /// <summary>
        /// Profile a step
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use the 'using(' syntax
        /// </remarks>
        public IDisposable Step(string name)
        {
            return GlobalSettings.DebugMode == false ? null : MiniProfiler.Current.Step(name);
        }

        /// <summary>
        /// Start the profiler
        /// </summary>
        public void Start()
        {
            MiniProfiler.Settings.SqlFormatter = new SqlServerFormatter();
            MiniProfiler.Settings.StackMaxLength = 5000;
            MiniProfiler.Start();
        }

        /// <summary>
        /// Start the profiler
        /// </summary>
        /// <remarks>
        /// set discardResults to false when you want to abandon all profiling, this is useful for 
        /// when someone is not authenticated or you want to clear the results based on some other mechanism.
        /// </remarks>
        public void Stop(bool discardResults = false)
        {
            MiniProfiler.Stop(discardResults);
        }

        /// <summary>
        /// Gets the request object from the app instance if it is available
        /// </summary>
        /// <param name="sender">The application object</param>
        /// <returns></returns>
        private Attempt<HttpRequestBase> TryGetRequest(object sender)
        {
            var app = sender as HttpApplication;
            if (app == null) return Attempt<HttpRequestBase>.Fail();

            try
            {
                var req = app.Request;
                return Attempt<HttpRequestBase>.Succeed(new HttpRequestWrapper(req));
            }
            catch (HttpException ex)
            {
                return Attempt<HttpRequestBase>.Fail(ex);
            }
        }
    }
}
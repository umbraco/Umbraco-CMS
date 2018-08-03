using System;
using System.Threading;
using System.Web;
using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;

namespace Umbraco.Web.Profiling
{
    /// <summary>
    /// A profiler used for web based activity based on the MiniProfiler framework
    /// </summary>
    internal class WebProfiler : IProfiler
    {
        private const string BootRequestItemKey = "Umbraco.Web.Profiling.WebProfiler__isBootRequest";
        private readonly WebProfilerProvider _provider;
        private int _first;

        /// <summary>
        /// Constructor
        /// </summary>
        internal WebProfiler()
        {
            // create our own provider, which can provide a profiler even during boot
            // MiniProfiler's default cannot because there's no HttpRequest in HttpContext
            _provider = new WebProfilerProvider();

            // settings
            MiniProfiler.Settings.SqlFormatter = new SqlServerFormatter();
            MiniProfiler.Settings.StackMaxLength = 5000;
            MiniProfiler.Settings.ProfilerProvider = _provider;

            //Binds to application events to enable the MiniProfiler with a real HttpRequest
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

        void UmbracoApplicationBeginRequest(object sender, EventArgs e)
        {
            // if this is the first request, notify our own provider that this request is the boot request
            var first = Interlocked.Exchange(ref _first, 1) == 0;
            if (first)
            {
                _provider.BeginBootRequest();
                ((HttpApplication)sender).Context.Items[BootRequestItemKey] = true;
                // and no need to start anything, profiler is already there
            }
            // else start a profiler, the normal way
            else if (ShouldProfile(sender))
                Start();
        }

        void UmbracoApplicationEndRequest(object sender, EventArgs e)
        {
            // if this is the boot request, or if we should profile this request, stop
            // (the boot request is always profiled, no matter what)
            var isBootRequest = ((HttpApplication)sender).Context.Items[BootRequestItemKey] != null;
            if (isBootRequest)
                _provider.EndBootRequest();
            if (isBootRequest || ShouldProfile(sender))
                Stop();
        }

        private bool ShouldProfile(object sender)
        {
            if (GlobalSettings.DebugMode == false)
                return false;

            //will not run in medium trust
            if (SystemUtilities.GetCurrentTrustLevel() < AspNetHostingPermissionLevel.High)
                return false;

            var request = TryGetRequest(sender);

            if (request.Success == false || request.Result.Url.IsClientSideRequest())
                return false;

            //if there is an umbDebug query string than profile it
            bool umbDebug;
            if (string.IsNullOrEmpty(request.Result.QueryString["umbDebug"]) == false && bool.TryParse(request.Result.QueryString["umbDebug"], out umbDebug))
                return true;

            //if there is an umbDebug header than profile it
            if (string.IsNullOrEmpty(request.Result.Headers["X-UMB-DEBUG"]) == false && bool.TryParse(request.Result.Headers["X-UMB-DEBUG"], out umbDebug))
                return true;

            //everything else is ok to profile
            return false;
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
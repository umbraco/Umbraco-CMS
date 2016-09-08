using System;
using System.Web;
using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// A profiler used for web based activity based on the MiniProfiler framework.
    /// </summary>
    internal class WebProfiler : IProfiler
    {
        private readonly IRuntimeState _runtime;

        public WebProfiler(IRuntimeState runtime)
        {
            _runtime = runtime;
        }

        public void UmbracoApplicationEndRequest(object sender, EventArgs e)
        {
            if (CanPerformProfilingAction(sender))
                Stop();
        }

        public void UmbracoApplicationBeginRequest(object sender, EventArgs e)
        {
            if (CanPerformProfilingAction(sender))
                Start();
        }

        private static bool CanPerformProfilingAction(object sender)
        {
            var request = TryGetRequest(sender);
            return request.Success && request.Result.Url.IsClientSideRequest() == false;
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
            return _runtime.Debug ? MiniProfiler.Current.Step(name) : null;
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
        private static Attempt<HttpRequestBase> TryGetRequest(object sender)
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
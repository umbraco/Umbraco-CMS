using System;
using System.Web;
using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Implements <see cref="IProfiler"/> by using the MiniProfiler framework.
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

        /// <inheritdoc/>
        public string Render()
        {
            return MiniProfiler.RenderIncludes(RenderPosition.Right).ToString();
        }

        /// <inheritdoc/>
        public IDisposable Step(string name)
        {
            return _runtime.Debug ? MiniProfiler.Current.Step(name) : null;
        }

        /// <inheritdoc/>
        public void Start()
        {
            MiniProfiler.Settings.SqlFormatter = new SqlServerFormatter();
            MiniProfiler.Settings.StackMaxLength = 5000;
            MiniProfiler.Start();
        }

        /// <inheritdoc/>
        public void Stop(bool discardResults = false)
        {
            MiniProfiler.Stop(discardResults);
        }

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
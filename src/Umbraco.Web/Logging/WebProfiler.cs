using System;
using System.Threading;
using System.Web;
using StackExchange.Profiling;
using StackExchange.Profiling.SqlFormatters;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Logging
{
    /// <summary>
    /// Implements <see cref="IProfiler"/> by using the MiniProfiler framework.
    /// </summary>
    /// <remarks>
    /// Profiling only runs when the app is in debug mode, see WebRuntime for how this gets created
    /// </remarks>
    internal class WebProfiler : IProfiler
    {
        private const string BootRequestItemKey = "Umbraco.Core.Logging.WebProfiler__isBootRequest";
        private readonly WebProfilerProvider _provider;
        private int _first;

        public WebProfiler()
        {
            // create our own provider, which can provide a profiler even during boot
            _provider = new WebProfilerProvider();

            //see https://miniprofiler.com/dotnet/AspDotNet
            var options = new MiniProfilerOptions
            {
                SqlFormatter = new SqlServerFormatter(),
                StackMaxLength = 5000,
                ProfilerProvider = _provider
            };
            // this is a default path and by default it performs a 'contains' check which will match our content controller
            // (and probably other requests) and ignore them.
            options.IgnoredPaths.Remove("/content/"); 
            MiniProfiler.Configure(options);
        }

        public void UmbracoApplicationBeginRequest(object sender, EventArgs e)
        {
            // if this is the first request, notify our own provider that this request is the boot request
            var first = Interlocked.Exchange(ref _first, 1) == 0;
            if (first)
            {
                _provider.BeginBootRequest();
                ((HttpApplication) sender).Context.Items[BootRequestItemKey] = true;
                // and no need to start anything, profiler is already there
            }
            // else start a profiler, the normal way
            else if (ShouldProfile(sender))
                Start();
        }

        public void UmbracoApplicationEndRequest(object sender, EventArgs e)
        {
            // if this is the boot request, or if we should profile this request, stop
            // (the boot request is always profiled, no matter what)
            var isBootRequest = ((HttpApplication) sender).Context.Items[BootRequestItemKey] != null;
            if (isBootRequest)
                _provider.EndBootRequest();
            if (isBootRequest || ShouldProfile(sender))
                Stop();
        }

        private static bool ShouldProfile(object sender)
        {
            var request = TryGetRequest(sender);
            if (request.Success == false) return false;

            if (request.Result.Url.IsClientSideRequest()) return false;
            if (bool.TryParse(request.Result.QueryString["umbDebug"], out var umbDebug)) return umbDebug;
            if (bool.TryParse(request.Result.Headers["X-UMB-DEBUG"], out var xUmbDebug)) return xUmbDebug;
            if (bool.TryParse(request.Result.Cookies["UMB-DEBUG"]?.Value, out var cUmbDebug)) return cUmbDebug;
            return false;
        }

        /// <inheritdoc/>
        public string Render()
        {
            return MiniProfiler.Current.RenderIncludes(RenderPosition.Right).ToString();
        }

        /// <inheritdoc/>
        public IDisposable Step(string name)
        {
            return MiniProfiler.Current?.Step(name);
        }

        /// <inheritdoc/>
        public void Start()
        {
            MiniProfiler.StartNew();
        }

        /// <inheritdoc/>
        public void Stop(bool discardResults = false)
        {
            MiniProfiler.Current?.Stop(discardResults);
        }

        private static Attempt<HttpRequestBase> TryGetRequest(object sender)
        {
            if (sender is HttpRequest httpRequest)
                return Attempt<HttpRequestBase>.Succeed(new HttpRequestWrapper(httpRequest));

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

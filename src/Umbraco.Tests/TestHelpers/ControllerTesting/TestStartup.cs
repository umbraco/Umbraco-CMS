using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Tracing;
using Owin;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.WebApi;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
    /// <summary>
    /// Startup class for the self-hosted web server works for OWIN and WebAPI
    /// </summary>
    public class TestStartup
    {
        private readonly Func<HttpRequestMessage, UmbracoHelper, ApiController> _controllerFactory;
        private readonly Action<HttpConfiguration> _initialize;

        public TestStartup(Action<HttpConfiguration> initialize, Func<HttpRequestMessage, UmbracoHelper, ApiController> controllerFactory)
        {
            _controllerFactory = controllerFactory;
            _initialize = initialize;
        }

        public void Configuration(IAppBuilder app)
        {            
            var httpConfig = new HttpConfiguration();

            //TODO: Enable this if you can't see the errors produced
            //var traceWriter = httpConfig.EnableSystemDiagnosticsTracing();
            //traceWriter.IsVerbose = true;
            //traceWriter.MinimumLevel = TraceLevel.Debug;

            httpConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            // Add in a simple exception tracer so we can see what is causing the 500 Internal Server Error
            httpConfig.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());

            httpConfig.Services.Replace(typeof(IAssembliesResolver), new SpecificAssemblyResolver(new[] { typeof(UsersController).Assembly }));
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new TestControllerActivator(_controllerFactory));
            httpConfig.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(httpConfig));
            
            //auth everything
            app.AuthenticateEverything();            

            _initialize(httpConfig);

            app.UseWebApi(httpConfig);
        }
    }
}
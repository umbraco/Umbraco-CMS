using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.ModelsBuilder.Embedded;

// will install only if configuration says it needs to be installed
[assembly: PreApplicationStartMethod(typeof(LiveModelsProviderModule), "Install")]

namespace Umbraco.ModelsBuilder.Embedded
{
    // have to do this because it's the only way to subscribe to EndRequest,
    // module is installed by assembly attribute at the top of this file
    public class LiveModelsProviderModule : IHttpModule
    {
        private static LiveModelsProvider _liveModelsProvider;

        public void Init(HttpApplication app)
        {
            app.EndRequest += App_EndRequest;
        }

        private void App_EndRequest(object sender, EventArgs e)
        {
            if (((HttpApplication)sender).Request.Url.IsClientSideRequest())
                return;

            // here we're using "Current." since we're in a module, it is possible in a round about way to inject into a module but for now we'll just use Current
            if (_liveModelsProvider == null)
                _liveModelsProvider = Current.Factory.TryGetInstance<LiveModelsProvider>(); // will be null in upgrade mode or if embedded MB is disabled

            if (_liveModelsProvider?.IsEnabled ?? false)
                _liveModelsProvider.GenerateModelsIfRequested(sender, e);
        }

        public void Dispose()
        {
            // nothing
        }

        public static void Install()
        {
            // always - don't read config in PreApplicationStartMethod
            HttpApplication.RegisterModule(typeof(LiveModelsProviderModule));
        }
    }
}

using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.ModelsBuilder.Umbraco;

// will install only if configuration says it needs to be installed
[assembly: PreApplicationStartMethod(typeof(LiveModelsProviderModule), "Install")]

namespace Umbraco.ModelsBuilder.Umbraco
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
            // here we're using "Current." since we're in a module, it is possible in a round about way to inject into a module but for now we'll just use Current
            if (_liveModelsProvider == null)
                _liveModelsProvider = Current.Factory.GetInstance<LiveModelsProvider>();

            if (_liveModelsProvider.IsEnabled)
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

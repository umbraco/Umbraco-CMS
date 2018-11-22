using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Web
{
    /// <summary>
    /// Used to boot up the server messenger once the application succesfully starts
    /// </summary>
    internal class BatchedDatabaseServerMessengerStartup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var messenger = ServerMessengerResolver.HasCurrent 
                ? ServerMessengerResolver.Current.Messenger as BatchedDatabaseServerMessenger 
                : null;

            if (messenger != null)
                messenger.Startup();
        }
    }
}
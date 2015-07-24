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
            var msgr = ServerMessengerResolver.HasCurrent ? ServerMessengerResolver.Current.Messenger as BatchedDatabaseServerMessenger : null;

            if (msgr == null) return;

            UmbracoModule.EndRequest += msgr.UmbracoModule_EndRequest;
            UmbracoModule.RouteAttempt += msgr.UmbracoModule_RouteAttempt;

            if (applicationContext.DatabaseContext.IsDatabaseConfigured == false || applicationContext.DatabaseContext.CanConnect == false)
            {
                applicationContext.ProfilingLogger.Logger.Warn<BatchedDatabaseServerMessenger>(
                    "The app cannot connect to the database, this server cannot be initialized with "
                    + typeof(BatchedDatabaseServerMessenger) + ", distributed calls will not be enabled for this server");
            }
            else
            {
                msgr.Boot();
            }
        }
    }
}
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Publishing;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web.Scheduling
{
    internal class ScheduledPublishing : RecurringTaskBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IUmbracoSettingsSection _settings;

        public ScheduledPublishing(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            ApplicationContext appContext, IUmbracoSettingsSection settings)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
            _settings = settings;
        }

        private ILogger Logger { get { return _appContext.ProfilingLogger.Logger; } }

        public override bool PerformRun()
        {
            if (_appContext == null) return true; // repeat...

            if (Suspendable.ScheduledPublishing.CanRun == false)
                return true; // repeat, later

            switch (_appContext.GetCurrentServerRole())
            {
                case ServerRole.Slave:
                    Logger.Debug<ScheduledPublishing>("Does not run on slave servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    Logger.Debug<ScheduledPublishing>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_appContext.MainDom.IsMainDom == false)
            {
                LogHelper.Debug<ScheduledPublishing>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            UmbracoContext tempContext = null;
            try
            {
                // DO not run publishing if content is re-loading
                if (content.Instance.isInitializing == false)
                {
                    //TODO: We should remove this in v8, this is a backwards compat hack
                    // see notes in CacheRefresherEventHandler
                    // because notifications will not be sent if there is no UmbracoContext
                    // see NotificationServiceExtensions
                    var httpContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("temp.aspx", "", new StringWriter())));
                    tempContext = UmbracoContext.EnsureContext(
                        httpContext,
                        _appContext,
                        new WebSecurity(httpContext, _appContext),
                        _settings,
                        UrlProviderResolver.Current.Providers,
                        true);

                    var publisher = new ScheduledPublisher(_appContext.Services.ContentService);
                    var count = publisher.CheckPendingAndProcess();
                    Logger.Debug<ScheduledPublishing>(() => string.Format("Processed {0} items", count));
                }
            }
            catch (Exception e)
            {
                Logger.Error<ScheduledPublishing>("Failed (see exception).", e);
            }
            finally
            {
                if (tempContext != null)
                {
                    // because we created an http context and assigned it to UmbracoContext,
                    // the batched messenger does batch instructions, but since there is no
                    // request, we need to explicitely tell it to flush the batch of instrs.
                    var batchedMessenger = ServerMessengerResolver.Current.Messenger as BatchedDatabaseServerMessenger;
                    if (batchedMessenger != null)
                        batchedMessenger.FlushBatch();

                    tempContext.Dispose(); // nulls the ThreadStatic context
                }
            }

            return true; // repeat
        }

        public override bool IsAsync
        {
            get { return false; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Umbraco.Web.Strategies
{

    /// <summary>
    /// This is used to trigger the legacy ActionHandlers based on events
    /// </summary>
    public sealed class LegacyActionHandlerEventHandler : ApplicationEventHandler
    {
        //NOTE: this is to fix this currently: http://issues.umbraco.org/issue/U4-1550

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentService_Published;
            ContentService.UnPublished += ContentService_UnPublished;
        }

        static void ContentService_UnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            e.PublishedEntities.ForEach(x =>
                global::umbraco.BusinessLogic.Actions.Action.RunActionHandlers(
                    new Document(x), ActionUnPublish.Instance));
        }

        static void ContentService_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            e.PublishedEntities.ForEach(x =>
                global::umbraco.BusinessLogic.Actions.Action.RunActionHandlers(
                    new Document(x), ActionPublish.Instance));
        }
    }
}

using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.Hubs
{
    public class NodeEditsEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //// don't let the event handlers kick in if Redirect Tracking is turned off in the config
            //if (UmbracoConfig.For.UmbracoSettings().WebRouting.DisableRedirectUrlTracking) return;

            ContentService.Published += ContentService_Published;
        }

        private void ContentService_Published(Core.Publishing.IPublishingStrategy sender, Core.Events.PublishEventArgs<Core.Models.IContent> e)
        {
            var currentUser = UmbracoContext.Current.Security.CurrentUser;
            if (currentUser == null)
            {
                return;
            }

            var email = currentUser.Email;
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NodeEditsHub>();
            foreach (var node in e.PublishedEntities)
            {
                hubContext.Clients.Group(NodeEditsHub.EditGroup).broadcastPublished(node.Id, currentUser.Id, currentUser.Name, DateTime.Now.ToString("HH:mm:ss"));
            }
        }
    }
}

using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace UmbracoPackage._1.Controllers
{
    public class DashboardComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingNotificationHandler>();
        }
    }

    internal class ServerVariablesParsingNotificationHandler : INotificationHandler<ServerVariablesParsingNotification>
    {
        private readonly LinkGenerator linkGenerator;

        public ServerVariablesParsingNotificationHandler(LinkGenerator linkGenerator)
        {
            this.linkGenerator = linkGenerator;
        }

        public void Handle(ServerVariablesParsingNotification notification)
        {
            notification.ServerVariables.Add("umbracopackage__1", new Dictionary<string, object>()
            {
                { "dashboardController", linkGenerator.GetUmbracoApiServiceBaseUrl<DashboardApiController>(controller => controller.GetApi()) }
            });
        }
    }
}

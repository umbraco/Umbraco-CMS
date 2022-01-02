using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.UI.App_Plugins.Test
{
    public class TestNotificationHandler : INotificationHandler<ContentUnpublishingNotification>
    {
        private readonly ILogger<TestNotificationHandler> _logger;

        public TestNotificationHandler(ILogger<TestNotificationHandler> logger)
        {
            _logger = logger;
        }

        public void Handle(RootNodeRenderingNotification notification)
        {
            if (notification.TreeAlias == "templates")
            {
                //notification.Node = null;
            }
        }

        public void Handle(ContentUnpublishingNotification notification)
        {
            _logger.LogInformation("Test!");
            notification.CancelOperation(new EventMessage("Test", "Test", EventMessageType.Error));
        }
    }
}

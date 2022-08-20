using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace UmbracoPackage._1.Controllers;

public class DashboardComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder
            .AddNotificationHandler<ServerVariablesParsingNotification,
                DashboardServerVariablesParserNotificationHandler>();
    }
}

public class DashboardServerVariablesParserNotificationHandler : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;

    public DashboardServerVariablesParserNotificationHandler(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }
    public void Handle(ServerVariablesParsingNotification notification)
    {
        notification.ServerVariables.Add("umbracopackage__1", new Dictionary<string, object>
        {
            { "dashboardController", _linkGenerator.GetUmbracoApiServiceBaseUrl<DashboardApiController>(controller => controller.GetApi()) }
        });
    }
}
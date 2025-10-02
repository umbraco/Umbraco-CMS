using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Web.UI.SignalRLoadBalancing;

public class SignalRComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.Services.AddSignalR().AddAzureSignalR();
}

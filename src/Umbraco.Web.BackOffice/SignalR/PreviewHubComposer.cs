using Umbraco.Core;
using Umbraco.Core.Builder;
using Umbraco.Core.Composing;

namespace Umbraco.Web.BackOffice.SignalR
{
    public class PreviewHubComposer : ComponentComposer<PreviewHubComponent>, ICoreComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);
        }
    }
}

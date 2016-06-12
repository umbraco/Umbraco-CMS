using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Umbraco.Core.Services;

[assembly: OwinStartup("SignalRStartup", typeof(Umbraco.Web.UI.SignalRStartup))]
namespace Umbraco.Web.UI
{
    public class SignalRStartup : UmbracoDefaultOwinStartup
    {
        private IHubContext _hub;

        public void Configuration(IAppBuilder app)
        {
            base.Configuration(app);

            app.MapSignalR();

            _hub = GlobalHost.ConnectionManager.GetHubContext<StephansHub>();

            ContentService.Published += (sender, args) =>
            {
                var entity = args.PublishedEntities.FirstOrDefault();
                if (entity != null)
                {
                    _hub.Clients.All.someEventName(sender.GetType().Name.ToString(), entity.Name);
                }
            };
        }
    }
}
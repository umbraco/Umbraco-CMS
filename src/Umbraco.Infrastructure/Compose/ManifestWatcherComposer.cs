using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Events;

namespace Umbraco.Core.Compose
{
    public class ManifestWatcherComposer : ICoreComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<ManifestWatcher>();
            builder.AddNotificationHandler<UmbracoApplicationStarting, ManifestWatcher>(factory => factory.GetRequiredService<ManifestWatcher>());
            builder.AddNotificationHandler<UmbracoApplicationStopping, ManifestWatcher>(factory => factory.GetRequiredService<ManifestWatcher>());
        }
    }
}

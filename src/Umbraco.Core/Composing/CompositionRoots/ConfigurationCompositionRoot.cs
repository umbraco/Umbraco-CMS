using LightInject;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Composing.CompositionRoots
{
    /// <summary>
    /// Sets up IoC container for Umbraco configuration classes
    /// </summary>
    public sealed class ConfigurationCompositionRoot : IComposition
    {
        public void Compose(IServiceCollection services)
        {
            services.AddTransient(factory => UmbracoConfig.For.UmbracoSettings());
            services.AddTransient(factory => factory.GetService<IUmbracoSettingsSection>().Content);
            services.AddTransient(factory => factory.GetService<IUmbracoSettingsSection>().Templates);
            services.AddTransient(factory => factory.GetService<IUmbracoSettingsSection>().RequestHandler);
            services.AddTransient(factory => UmbracoConfig.For.GlobalSettings());

            // fixme - other sections we need to add?
        }
    }
}

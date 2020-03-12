using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IUmbracoSettingsSection : IUmbracoConfigurationSection
    {
        IContentSection Content { get; }

        ISecuritySection Security { get; }

        IRequestHandlerSection RequestHandler { get; }

        IWebRoutingSection WebRouting { get; }

        IKeepAliveSection KeepAlive { get; }
    }
}

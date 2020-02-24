﻿using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IUmbracoSettingsSection : IUmbracoConfigurationSection
    {
        IBackOfficeSection BackOffice { get; }

        IContentSection Content { get; }

        ISecuritySection Security { get; }

        IRequestHandlerSection RequestHandler { get; }
        
        ILoggingSection Logging { get; }

        IWebRoutingSection WebRouting { get; }

        IKeepAliveSection KeepAlive { get; }
    }
}

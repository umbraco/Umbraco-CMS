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

        IScheduledTasksSection ScheduledTasks { get; }

        IProvidersSection Providers { get; }

        IWebRoutingSection WebRouting { get; }
    }
}

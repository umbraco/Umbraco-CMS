using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ILoggingSettings : IUmbracoConfigurationSection
    {
        int MaxLogAge { get; }
    }
}

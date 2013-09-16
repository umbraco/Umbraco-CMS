using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IDistributedCallSection : IUmbracoConfigurationSection
    {
        bool Enabled { get; }

        int UserId { get; }

        IEnumerable<IServer> Servers { get; }
    }
}
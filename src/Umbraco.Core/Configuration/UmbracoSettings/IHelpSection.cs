using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IHelpSection : IUmbracoConfigurationSection
    {
        string DefaultUrl { get; }

        IEnumerable<ILink> Links { get; }
    }
}
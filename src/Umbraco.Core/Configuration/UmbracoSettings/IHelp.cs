using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IHelp
    {
        string DefaultUrl { get; }

        IEnumerable<ILink> Links { get; }
    }
}
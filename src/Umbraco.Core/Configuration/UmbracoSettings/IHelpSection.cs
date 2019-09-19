using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer used and will be removed in future versions")]
    public interface IHelpSection : IUmbracoConfigurationSection
    {
        string DefaultUrl { get; }

        IEnumerable<ILink> Links { get; }
    }
}
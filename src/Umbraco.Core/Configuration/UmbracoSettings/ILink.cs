using System;
using System.ComponentModel;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer used and will be removed in future versions")]
    public interface ILink
    {
        string Application { get; }

        string ApplicationUrl { get; }

        string Language { get; }

        string UserType { get; }

        string HelpUrl { get; }
    }
}
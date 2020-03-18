using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ITypeFinderConfig
    {
        IEnumerable<string> AssembliesAcceptingLoadExceptions { get; }
    }
}

using System.Collections.Generic;

namespace Umbraco.Core.Configuration
{
    public interface IFileSystemProvidersSection
    {
        IDictionary<string, IFileSystemProviderElement> Providers { get; }
    }
}
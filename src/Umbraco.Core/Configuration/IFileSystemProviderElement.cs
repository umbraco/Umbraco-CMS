using System.Collections.Generic;

namespace Umbraco.Core.Configuration
{
    public interface IFileSystemProviderElement
    {
        string Alias { get; }
        string Type { get; }
        IDictionary<string, string> Parameters { get; }
    }
}
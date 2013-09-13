using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IDeveloper
    {
        IEnumerable<IFileExtension> AppCodeFileExtensions { get; }
    }
}
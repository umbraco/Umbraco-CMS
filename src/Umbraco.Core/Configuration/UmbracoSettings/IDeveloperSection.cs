using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IDeveloperSection : IUmbracoConfigurationSection
    {
        IEnumerable<IFileExtension> AppCodeFileExtensions { get; }
    }
}
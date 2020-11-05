using System.Collections.Generic;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Marker interface for any editor configuration that supports defining file extensions
    /// </summary>
    public interface IFileExtensionsConfig
    {
        List<IFileExtensionConfigItem> FileExtensions { get; set; }
    }
}

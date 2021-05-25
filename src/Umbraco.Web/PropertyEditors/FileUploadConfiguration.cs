using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the file upload address value editor.
    /// </summary>
    public class FileUploadConfiguration : IFileExtensionsConfig
    {
        [ConfigurationField("fileExtensions", "Accepted file extensions", "multivalues")]
        public List<FileExtensionConfigItem> FileExtensions { get; set; } = new List<FileExtensionConfigItem>();
    }
}

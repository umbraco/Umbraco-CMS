using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the file upload value editor.
/// </summary>
public class FileUploadConfigurationEditor : ConfigurationEditor<FileUploadConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public FileUploadConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}

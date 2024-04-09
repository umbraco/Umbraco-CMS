using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the file upload value editor.
/// </summary>
public class FileUploadConfigurationEditor : ConfigurationEditor<FileUploadConfiguration>
{
    public FileUploadConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}

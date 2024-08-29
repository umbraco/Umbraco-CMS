using Microsoft.Extensions.FileProviders;

namespace Umbraco.Cms.Core.Services;

public class LocalizedTextServiceSupplementaryFileSource
{

    public LocalizedTextServiceSupplementaryFileSource(IFileInfo file, bool overwriteCoreKeys)
    {
        FileInfo = file ?? throw new ArgumentNullException(nameof(file));
        OverwriteCoreKeys = overwriteCoreKeys;
    }

    public IFileInfo FileInfo { get; }

    public bool OverwriteCoreKeys { get; }
}

namespace Umbraco.Cms.Core.Services;

public class LocalizedTextServiceSupplementaryFileSource
{
    public LocalizedTextServiceSupplementaryFileSource(FileInfo file, bool overwriteCoreKeys)
    {
        File = file ?? throw new ArgumentNullException("file");
        OverwriteCoreKeys = overwriteCoreKeys;
    }

    public FileInfo File { get; }

    public bool OverwriteCoreKeys { get; }
}

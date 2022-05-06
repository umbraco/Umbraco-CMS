namespace Umbraco.Cms.Core.Services;

public class LocalizedTextServiceSupplementaryFileSource
{
    public LocalizedTextServiceSupplementaryFileSource(FileInfo file, bool overwriteCoreKeys)
    {
        if (file == null)
        {
            throw new ArgumentNullException("file");
        }

        File = file;
        OverwriteCoreKeys = overwriteCoreKeys;
    }

    public FileInfo File { get; }
    public bool OverwriteCoreKeys { get; }
}

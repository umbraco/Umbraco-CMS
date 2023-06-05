using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Umbraco.Cms.Core.Services;

public class LocalizedTextServiceSupplementaryFileSource
{
    [Obsolete("Use other ctor. Will be removed in Umbraco 12")]
    public LocalizedTextServiceSupplementaryFileSource(FileInfo file, bool overwriteCoreKeys)
        : this(new PhysicalFileInfo(file), overwriteCoreKeys)
    {
    }

    public LocalizedTextServiceSupplementaryFileSource(IFileInfo file, bool overwriteCoreKeys)
    {
        FileInfo = file ?? throw new ArgumentNullException(nameof(file));
        File = file is PhysicalFileInfo && file.PhysicalPath is not null ? new FileInfo(file.PhysicalPath) : null!;
        OverwriteCoreKeys = overwriteCoreKeys;
    }

    [Obsolete("Use FileInfo instead. Will be removed in Umbraco 12")]
    public FileInfo File { get; }

    public IFileInfo FileInfo { get; }

    public bool OverwriteCoreKeys { get; }
}

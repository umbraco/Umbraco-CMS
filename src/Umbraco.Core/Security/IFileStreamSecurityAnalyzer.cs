namespace Umbraco.Cms.Core.Security;

public interface IFileStreamSecurityAnalyzer
{

    bool FileContentMatchesFileType(Stream fileStream);

    /// <summary>
    /// Analyzes whether the file content is considered safe
    /// </summary>
    /// <param name="fileStream">Needs to be a Read/Write seekable stream</param>
    /// <returns>Whether the file is considered safe</returns>
    bool IsConsideredSafe(Stream fileStream);
}

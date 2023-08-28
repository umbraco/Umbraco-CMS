namespace Umbraco.Cms.Core.Security;

public interface IFileStreamSecurityAnalyzer
{

    /// <summary>
    /// Indicates whether the analyzer should process the file
    /// The implementation should be considerably faster than IsConsideredSafe
    /// </summary>
    /// <param name="fileStream"></param>
    /// <returns></returns>
    bool ShouldHandle(Stream fileStream);

    /// <summary>
    /// Analyzes whether the file content is considered safe
    /// </summary>
    /// <param name="fileStream">Needs to be a Read/Write seekable stream</param>
    /// <returns>Whether the file is considered safe</returns>
    bool IsConsideredSafe(Stream fileStream);
}

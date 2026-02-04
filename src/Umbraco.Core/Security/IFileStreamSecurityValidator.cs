namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Validates file streams for security using registered <see cref="IFileStreamSecurityAnalyzer" /> implementations.
/// </summary>
public interface IFileStreamSecurityValidator
{
    /// <summary>
    /// Analyzes wether the file content is considered safe with registered IFileStreamSecurityAnalyzers
    /// </summary>
    /// <param name="fileStream">Needs to be a Read seekable stream</param>
    /// <returns>Whether the file is considered safe after running the necessary analyzers</returns>
    bool IsConsideredSafe(Stream fileStream);
}

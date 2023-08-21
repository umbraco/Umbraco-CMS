namespace Umbraco.Cms.Core.Security;

public class FileStreamSecurityValidator : IFileStreamSecurityValidator
{
    private readonly IEnumerable<IFileStreamSecurityAnalyzer> _fileAnalyzers;

    public FileStreamSecurityValidator(IEnumerable<IFileStreamSecurityAnalyzer> fileAnalyzers)
    {
        _fileAnalyzers = fileAnalyzers;
    }

    /// <summary>
    /// Analyzes whether the file content is considered safe with registered IFileStreamSecurityAnalyzers
    /// </summary>
    /// <param name="fileStream">Needs to be a Read seekable stream</param>
    /// <returns>Whether the file is considered safe after running the necessary analyzers</returns>
    public bool IsConsideredSafe(Stream fileStream)
    {
        foreach (var fileAnalyzer in _fileAnalyzers)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            if (!fileAnalyzer.ShouldHandle(fileStream))
            {
                continue;
            }

            fileStream.Seek(0, SeekOrigin.Begin);
            if (fileAnalyzer.IsConsideredSafe(fileStream) == false)
            {
                return false;
            }
        }
        fileStream.Seek(0, SeekOrigin.Begin);
        // If no analyzer we consider the file to be safe as the implementer has the possibility to add additional analyzers
        // Or all analyzers deem te file to be safe
        return true;
    }
}

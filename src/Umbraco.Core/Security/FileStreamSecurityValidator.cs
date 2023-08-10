namespace Umbraco.Cms.Core.Security;

public class FileStreamSecurityValidator : IFileStreamSecurityValidator
{
    private readonly IEnumerable<IFileStreamSecurityAnalyzer> _fileAnalyzers;

    public FileStreamSecurityValidator(IEnumerable<IFileStreamSecurityAnalyzer> fileAnalyzers)
    {
        _fileAnalyzers = fileAnalyzers;
    }

    /// <summary>
    /// Analyzes wether the file content is considered safe with registered IFileStreamSecurityAnalyzers
    /// </summary>
    /// <param name="fileStream">Needs to be a Read/Write seekable stream</param>
    /// <returns>Whether the file is considered safe after running the necessary analyzers</returns>
    public bool IsConsideredSafe(Stream fileStream)
    {
        foreach (var fileSanitizer in _fileAnalyzers)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            if (!fileSanitizer.FileContentMatchesFileType(fileStream))
            {
                continue;
            }

            fileStream.Seek(0, SeekOrigin.Begin);
            if (fileSanitizer.IsConsideredSafe(fileStream) == false)
                return false;
        }
        fileStream.Seek(0, SeekOrigin.Begin);
        // No analyzer found, we consider the file to be safe as the implementer has the possibility to add additional sanitizers
        return true;
    }
}

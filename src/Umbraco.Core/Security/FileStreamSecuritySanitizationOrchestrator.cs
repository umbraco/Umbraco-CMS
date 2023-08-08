namespace Umbraco.Cms.Core.Security;

public class FileStreamSecuritySanitizationOrchestrator : IFileStreamSecuritySanitizationOrchestrator
{
    private readonly IEnumerable<IFileStreamSecuritySanitizer> _fileSanitizers;

    public FileStreamSecuritySanitizationOrchestrator(IEnumerable<IFileStreamSecuritySanitizer> fileSanitizers)
    {
        _fileSanitizers = fileSanitizers;
    }

    /// <summary>
    /// Reads the file content and executes sanitization routines based on the determined file type
    /// </summary>
    /// <param name="fileStream">Needs to be a Read/Write seekable stream</param>
    /// <param name="clearContentIfSanitizationFails">Clear fileContent if sanitization fails?
    /// If true, this will make the method return true if sanitization fails and file was cleared</param>
    /// <returns>Whether the file is considered clean after running the necessary sanitizers</returns>
    public bool Sanitize(FileStream fileStream, bool clearContentIfSanitizationFails)
    {
        var startBuffer = new byte[_fileSanitizers.Max(fs => fs.MinimumStartBytesRequiredForContentTypeMatching)];
        var endBuffer = new byte[_fileSanitizers.Max(fs => fs.MinimumEndBytesRequiredForContentTypeMatching)];
        fileStream.Read(startBuffer);
        fileStream.Seek(endBuffer.Length * -1, SeekOrigin.End);
        fileStream.Read(endBuffer);
        fileStream.Seek(0, SeekOrigin.Begin);

        foreach (var fileSanitizer in _fileSanitizers)
        {
            if (!fileSanitizer.FileContentMatchesFileType(startBuffer, endBuffer))
            {
                continue;
            }

            var sanitationResult = fileSanitizer.RemoveSensitiveContent(fileStream);
            if (clearContentIfSanitizationFails == false)
                return sanitationResult;

            fileStream.SetLength(0);
            return true;
        }

        // No sanitizer found, we consider the file to be safe as the implementer has the possibility to add additional sanitizers
        return true;
    }
}

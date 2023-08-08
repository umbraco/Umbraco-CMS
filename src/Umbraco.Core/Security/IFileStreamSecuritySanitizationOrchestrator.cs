namespace Umbraco.Cms.Core.Security;

public interface IFileStreamSecuritySanitizationOrchestrator
{
    /// <summary>
    /// Reads the file content and executes sanitization routines based on the determined file type
    /// </summary>
    /// <param name="fileStream">Needs to be a Read/Write seekable stream</param>
    /// <param name="clearContentIfSanitizationFails">Clear fileContent if sanitization fails?
    /// If true, this will make the method return true if sanitization fails and file was cleared</param>
    /// <returns>Whether the file is considered clean after running the necessary sanitizers</returns>
    bool Sanitize(FileStream fileStream, bool clearContentIfSanitizationFails);
}

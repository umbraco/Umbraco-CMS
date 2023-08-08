namespace Umbraco.Cms.Core.Security;

public interface IFileStreamSecuritySanitizer
{
    public int MinimumStartBytesRequiredForContentTypeMatching { get; }
    public int MinimumEndBytesRequiredForContentTypeMatching{ get; }

    bool FileContentMatchesFileType(byte[] startBytes, byte[] endBytes);

    /// <summary>
    /// Sanitizes the filestream so it doesn't contain security sensitive content
    /// </summary>
    /// <param name="fileStream">Needs to be a read/Write seekable stream</param>
    /// <returns>Whether the fileStream is considered clean after the method was run</returns>
    bool RemoveSensitiveContent(FileStream fileStream);
}

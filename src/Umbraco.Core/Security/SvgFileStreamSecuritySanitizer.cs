namespace Umbraco.Cms.Core.Security;

public class SvgFileStreamSecuritySanitizer : IFileStreamSecuritySanitizer
{
    private readonly IHtmlSanitizer _htmlSanitizer;
    public int MinimumStartBytesRequiredForContentTypeMatching => 256;
    public int MinimumEndBytesRequiredForContentTypeMatching => 256;

    public SvgFileStreamSecuritySanitizer(IHtmlSanitizer htmlSanitizer)
    {
        _htmlSanitizer = htmlSanitizer;
    }

    public bool FileContentMatchesFileType(byte[] startBytes, byte[] endBytes)
    {
        var startString = System.Text.Encoding.UTF8.GetString(startBytes, 0, startBytes.Length);
        return startString.Contains("<svg") && startString.Contains(" xmlns=\"http://www.w3.org/2000/svg\"");
    }

    /// <summary>
    /// Sanitizes the svg filestream (Remove html and nested Javascript)
    /// </summary>
    /// <param name="fileStream">Needs to be a read/Write seekable stream</param>
    /// <returns>Whether the fileStream is considered clean after the method was run</returns>
    public bool RemoveSensitiveContent(FileStream fileStream)
    {
        //todo optimize streams?, make async?
        using var streamReader = new StreamReader(fileStream);
        var fileContent = streamReader.ReadToEnd();
        _htmlSanitizer.Sanitize(fileContent);
        var outBytes = streamReader.CurrentEncoding.GetBytes(fileContent);
        fileStream.Seek(0, SeekOrigin.Begin);
        fileStream.SetLength(outBytes.Length);
        fileStream.Write(outBytes);
        return true;
    }
}

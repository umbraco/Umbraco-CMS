namespace Umbraco.Cms.Infrastructure.PropertyEditors;

/// <summary>
/// Determines whether a file source posted by an editor can be stored as the value of a file based property.
/// </summary>
/// <remarks>
/// The source is rendered as a URL wherever the file is referenced, so a value that is not a plain web location
/// (a different scheme, a protocol-relative or authority-changing form, or characters that are significant in markup)
/// is refused rather than stored.
/// </remarks>
internal static class FileUploadSource
{
    /// <summary>
    /// Determines whether the posted source can be stored, given the source currently stored for the property.
    /// </summary>
    /// <param name="source">The posted source.</param>
    /// <param name="currentSource">The currently stored source, if any.</param>
    /// <returns><c>true</c> if the source is unchanged or is well-formed; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// An unchanged source is always accepted, so values stored before this check existed keep saving.
    /// </remarks>
    public static bool IsAcceptable(string source, string? currentSource)
        => string.Equals(source, currentSource, StringComparison.Ordinal) || IsWellFormed(source);

    /// <summary>
    /// Determines whether the source is a root-relative path or an absolute http or https URL.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns><c>true</c> if the source is well-formed; otherwise <c>false</c>.</returns>
    public static bool IsWellFormed(string source)
    {
        if (string.IsNullOrWhiteSpace(source) || source.Any(IsDisallowedCharacter))
        {
            return false;
        }

        // Checked before parsing as an absolute URI, as a rooted path parses as a file URI on Unix.
        if (source.StartsWith('/'))
        {
            return source.StartsWith("//", StringComparison.Ordinal) is false && HasParentSegment(source) is false;
        }

        return Uri.TryCreate(source, UriKind.Absolute, out Uri? uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
               && HasParentSegment(source) is false;
    }

    private static bool IsDisallowedCharacter(char c)
        => char.IsControl(c) || c is '"' or '<' or '>' or '\\';

    private static bool HasParentSegment(string source)
    {
        var path = source.Split('?', '#')[0];
        return path.Split('/').Any(segment => Uri.UnescapeDataString(segment) == "..");
    }
}

namespace Umbraco.Cms.Api.Management.Patching;

/// <summary>
/// Parses Umbraco's patch path syntax into typed segments.
/// <para>
/// The syntax is based on <see href="https://datatracker.ietf.org/doc/html/rfc6901">JSON Pointer (RFC 6901)</see>
/// with a custom extension for array element filtering:
/// <list type="bullet">
///   <item><c>/property</c> — access object property (RFC 6901 reference token)</item>
///   <item><c>/0</c> — access array element by index (RFC 6901 numeric token)</item>
///   <item><c>/-</c> — append to end of array (RFC 6901 past-the-end token, Add operations only)</item>
///   <item><c>[key=value,key2=null]</c> — filter array element by matching properties (Umbraco extension, not part of RFC 6901)</item>
/// </list>
/// </para>
/// <para>
/// RFC 6901 escape sequences are supported in property name tokens:
/// <c>~1</c> decodes to <c>/</c> and <c>~0</c> decodes to <c>~</c>.
/// </para>
/// <example>
/// <c>/variants[culture=en-US,segment=null]/name</c>
/// <c>/values[alias=title,culture=en-US,segment=null]/value</c>
/// </example>
/// </summary>
public static class PatchPathParser
{
    /// <summary>
    /// Parses a patch path string into an array of typed segments.
    /// </summary>
    /// <param name="path">The patch path expression.</param>
    /// <returns>An array of <see cref="PatchPathSegment"/> representing the parsed path.</returns>
    /// <exception cref="FormatException">Thrown when the path syntax is invalid.</exception>
    public static PatchPathSegment[] Parse(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new FormatException("Path cannot be null or empty.");
        }

        if (!path.StartsWith('/'))
        {
            throw new FormatException("Path must start with '/'.");
        }

        var segments = new List<PatchPathSegment>();
        var span = path.AsSpan(1); // Skip leading '/'

        while (span.Length > 0)
        {
            // Find the next '/' or '[' to determine segment boundary
            var slashIndex = span.IndexOf('/');
            var bracketIndex = span.IndexOf('[');

            ReadOnlySpan<char> token;
            if (slashIndex < 0 && bracketIndex < 0)
            {
                // Last segment, no more '/' or '['
                token = span;
                span = ReadOnlySpan<char>.Empty;
            }
            else if (bracketIndex >= 0 && (slashIndex < 0 || bracketIndex < slashIndex))
            {
                // '[' comes before '/' — property + filter
                token = span[..bracketIndex];
                span = span[bracketIndex..];
            }
            else
            {
                // '/' comes first — regular segment
                token = span[..slashIndex];
                span = span[(slashIndex + 1)..];
            }

            // Parse the property/index token
            if (token.Length > 0)
            {
                segments.Add(ParseToken(token));
            }

            // Parse filter if present
            if (span.Length > 0 && span[0] == '[')
            {
                var closeBracket = span.IndexOf(']');
                if (closeBracket < 0)
                {
                    throw new FormatException("Unclosed filter bracket '[' in path.");
                }

                ReadOnlySpan<char> filterContent = span[1..closeBracket];
                segments.Add(ParseFilter(filterContent));

                span = span[(closeBracket + 1)..];

                // Skip the '/' after the filter if present
                if (span.Length > 0 && span[0] == '/')
                {
                    span = span[1..];
                }
            }
        }

        if (segments.Count == 0)
        {
            throw new FormatException("Path must contain at least one segment.");
        }

        return segments.ToArray();
    }

    /// <summary>
    /// Validates that a path string is syntactically correct.
    /// </summary>
    /// <param name="path">The path expression to validate.</param>
    /// <returns>True if the path is valid, false otherwise.</returns>
    public static bool IsValid(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            Parse(path);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts all culture values from filter segments in a path.
    /// </summary>
    public static ISet<string> ExtractCultures(string path)
    {
        var cultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!IsValid(path))
        {
            return cultures;
        }

        PatchPathSegment[] segments = Parse(path);
        foreach (PatchPathSegment segment in segments)
        {
            if (segment is FilterSegment filter)
            {
                foreach (FilterCondition condition in filter.Conditions)
                {
                    if (string.Equals(condition.Key, "culture", StringComparison.OrdinalIgnoreCase)
                        && condition.Value is not null)
                    {
                        cultures.Add(condition.Value);
                    }
                }
            }
        }

        return cultures;
    }

    /// <summary>
    /// Extracts all segment values from filter segments in a path.
    /// </summary>
    public static ISet<string> ExtractSegments(string path)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!IsValid(path))
        {
            return result;
        }

        PatchPathSegment[] segments = Parse(path);
        foreach (PatchPathSegment segment in segments)
        {
            if (segment is FilterSegment filter)
            {
                foreach (FilterCondition condition in filter.Conditions)
                {
                    if (string.Equals(condition.Key, "segment", StringComparison.OrdinalIgnoreCase)
                        && condition.Value is not null)
                    {
                        result.Add(condition.Value);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a path targets invariant content (culture=null filter).
    /// </summary>
    public static bool TargetsInvariantCulture(string path)
    {
        if (!IsValid(path))
        {
            return false;
        }

        PatchPathSegment[] segments = Parse(path);
        foreach (PatchPathSegment segment in segments)
        {
            if (segment is FilterSegment filter)
            {
                foreach (FilterCondition condition in filter.Conditions)
                {
                    if (string.Equals(condition.Key, "culture", StringComparison.OrdinalIgnoreCase)
                        && condition.Value is null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static PatchPathSegment ParseToken(ReadOnlySpan<char> token)
    {
        if (token.Length == 1 && token[0] == '-')
        {
            return new AppendSegment();
        }

        if (int.TryParse(token, out var index))
        {
            return new IndexSegment(index);
        }

        var name = UnescapeRfc6901(token.ToString());
        return new PropertySegment(name);
    }

    /// <summary>
    /// Decodes RFC 6901 escape sequences in a reference token.
    /// <c>~1</c> is decoded to <c>/</c> and <c>~0</c> is decoded to <c>~</c>.
    /// Per the RFC, <c>~1</c> must be decoded before <c>~0</c> to avoid double-decoding.
    /// </summary>
    private static string UnescapeRfc6901(string token)
    {
        if (!token.Contains('~'))
        {
            return token;
        }

        return token.Replace("~1", "/").Replace("~0", "~");
    }

    private static FilterSegment ParseFilter(ReadOnlySpan<char> filterContent)
    {
        if (filterContent.IsEmpty)
        {
            throw new FormatException("Empty filter expression.");
        }

        var conditions = new List<FilterCondition>();

        while (filterContent.Length > 0)
        {
            // Find comma separator (but not inside nested brackets)
            var commaIndex = filterContent.IndexOf(',');
            ReadOnlySpan<char> pair;

            if (commaIndex < 0)
            {
                pair = filterContent;
                filterContent = ReadOnlySpan<char>.Empty;
            }
            else
            {
                pair = filterContent[..commaIndex];
                filterContent = filterContent[(commaIndex + 1)..];
            }

            var equalsIndex = pair.IndexOf('=');
            if (equalsIndex < 0)
            {
                throw new FormatException($"Filter condition must contain '=': '{pair.ToString()}'");
            }

            var key = pair[..equalsIndex].Trim().ToString();
            var rawValue = pair[(equalsIndex + 1)..].Trim();

            if (key.Length == 0)
            {
                throw new FormatException("Filter condition key cannot be empty.");
            }

            string? value = rawValue.Equals("null", StringComparison.OrdinalIgnoreCase)
                ? null
                : rawValue.ToString();

            conditions.Add(new FilterCondition(key, value));
        }

        return new FilterSegment(conditions.ToArray());
    }
}

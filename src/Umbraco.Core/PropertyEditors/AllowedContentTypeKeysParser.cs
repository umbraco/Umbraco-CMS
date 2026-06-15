using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Parses the comma-separated content type keys stored in a picker's "allowed content types" configuration value
/// (e.g. <see cref="ContentPickerConfiguration.AllowedContentTypeIds"/> or <see cref="ElementPickerConfiguration.AllowedContentTypeIds"/>).
/// </summary>
internal static class AllowedContentTypeKeysParser
{
    /// <summary>
    /// Parses the configured value into the set of allowed content type keys.
    /// </summary>
    /// <param name="configValue">The comma-separated configuration value. Non-GUID entries are ignored.</param>
    /// <returns>The set of allowed content type keys, or an empty set when nothing is configured.</returns>
    public static HashSet<Guid> Parse(string? configValue)
    {
        if (configValue.IsNullOrWhiteSpace())
        {
            return [];
        }

        var result = new HashSet<Guid>();
        foreach (var entry in configValue.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries))
        {
            if (Guid.TryParse(entry, out Guid guid))
            {
                result.Add(guid);
            }
        }

        return result;
    }
}

using System.Text.Json;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ExternalMemberIndexing;

/// <summary>
/// Builds index fields from the profile data of an external member.
/// </summary>
/// <remarks>
/// Profile data is a JSON object. Each top-level property becomes a field named after the property: strings are
/// indexed as text, numbers as integers or decimals, and booleans as integers (1 or 0). Nulls, arrays and nested
/// objects are not indexed, and neither is profile data that is not a JSON object.
/// </remarks>
internal static class ExternalMemberProfileDataIndexFields
{
    /// <summary>
    /// Creates the index fields for the given profile data.
    /// </summary>
    /// <param name="profileData">The profile data JSON, if any.</param>
    /// <param name="reservedFieldNames">The names of fields already written for the member, which profile data must not overwrite.</param>
    /// <returns>The profile data fields.</returns>
    public static IEnumerable<IndexField> Create(string? profileData, IEnumerable<string> reservedFieldNames)
    {
        if (string.IsNullOrWhiteSpace(profileData))
        {
            return [];
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(profileData);
        }
        catch (JsonException)
        {
            return [];
        }

        using (document)
        {
            if (document.RootElement.ValueKind is not JsonValueKind.Object)
            {
                return [];
            }

            var reserved = new HashSet<string>(reservedFieldNames, StringComparer.OrdinalIgnoreCase);
            var fields = new List<IndexField>();
            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                if (string.IsNullOrWhiteSpace(property.Name) || reserved.Add(property.Name) is false)
                {
                    continue;
                }

                IndexValue? value = CreateIndexValue(property.Value);
                if (value is not null)
                {
                    fields.Add(new IndexField(property.Name, value, null, null));
                }
            }

            return fields;
        }
    }

    private static IndexValue? CreateIndexValue(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.String when element.GetString() is { Length: > 0 } text => new IndexValue { Texts = [text] },
            JsonValueKind.Number when element.TryGetInt32(out var integer) => new IndexValue { Integers = [integer] },
            JsonValueKind.Number when element.TryGetDecimal(out var number) => new IndexValue { Decimals = [number] },
            JsonValueKind.True => new IndexValue { Integers = [1] },
            JsonValueKind.False => new IndexValue { Integers = [0] },
            _ => null,
        };
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using Examine;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Builds <see cref="ValueSet"/> instances for external-only members so they can be indexed by Examine.
/// </summary>
/// <remarks>
///     <para>
///         External-only members do not have content properties or content types,
///         so this builder produces a fixed set of fields from the <see cref="ExternalMemberIdentity"/> model.
///     </para>
///     <para>
///         If the member has <see cref="ExternalMemberIdentity.ProfileData"/>, each top-level key in
///         the JSON object is indexed as an additional field. This allows profile data to be searchable
///         via Examine alongside the standard identity fields.
///     </para>
/// </remarks>
public class ExternalMemberValueSetBuilder : IValueSetBuilder<ExternalMemberIdentity>
{
    /// <inheritdoc />
    public IEnumerable<ValueSet> GetValueSets(params ExternalMemberIdentity[] members)
    {
        foreach (ExternalMemberIdentity member in members)
        {
            var values = new Dictionary<string, IEnumerable<object?>>
            {
                { UmbracoExamineFieldNames.NodeKeyFieldName, new object[] { member.Key } },
                { UmbracoExamineFieldNames.NodeNameFieldName, member.Name?.Yield() ?? Enumerable.Empty<string>() },
                { "loginName", member.UserName.Yield() },
                { "email", member.Email.Yield() },
                { "createDate", new object[] { member.CreateDate } },
                { "id", new object[] { member.Id } },
                { "isExternalOnly", "1".Yield() },
            };

            AddProfileDataFields(values, member.ProfileData);

            var vs = new ValueSet(member.Id.ToInvariantString(), IndexTypes.Member, "ExternalMember", values);

            yield return vs;
        }
    }

    private static void AddProfileDataFields(Dictionary<string, IEnumerable<object?>> values, string? profileData)
    {
        if (string.IsNullOrWhiteSpace(profileData))
        {
            return;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(profileData);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (JsonProperty property in doc.RootElement.EnumerateObject())
            {
                // Skip if this would collide with a built-in field.
                if (values.ContainsKey(property.Name))
                {
                    continue;
                }

                var fieldValue = ConvertJsonElement(property.Value);
                if (fieldValue is not null)
                {
                    values[property.Name] = new object[] { fieldValue };
                }
            }
        }
        catch (JsonException)
        {
            // Invalid JSON — skip profile data indexing silently.
        }
    }

    private static object? ConvertJsonElement(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt64(out var l) => l,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null, // Null, arrays, and nested objects are not indexed.
        };
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A lightweight <see cref="IPublishedMember"/> representation for external-only members
///     that are not backed by the content system.
/// </summary>
/// <remarks>
///     <para>
///         External members have no content type, no content properties, no tree position,
///         and no template. This implementation provides sensible defaults for those members
///         so that member picker property values resolve correctly in templates.
///     </para>
///     <para>
///         If the external member has <see cref="ExternalMemberIdentity.ProfileData"/> (a JSON string),
///         each top-level key in the JSON object is exposed as an <see cref="IPublishedProperty"/>.
///         This allows <c>@Model.Member.Value("department")</c> to work identically for both
///         content members (where "department" is a content property) and external members
///         (where "department" is a key in the profile data JSON).
///     </para>
/// </remarks>
public sealed class PublishedExternalMember : IPublishedMember
{
    private static readonly IPublishedContentType _externalMemberContentType =
        new PublishedContentType(
            Guid.Empty,
            0,
            "ExternalMember",
            PublishedItemType.Member,
            [],
            [],
            ContentVariation.Nothing);

    private static readonly PublishedDataType _stringDataType =
        new(0, Constants.PropertyEditors.Aliases.Label, null, new Lazy<object?>(() => null));

    private readonly ExternalMemberIdentity _identity;
    private readonly Lazy<IReadOnlyDictionary<string, IPublishedProperty>> _properties;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedExternalMember"/> class.
    /// </summary>
    /// <param name="identity">The external member identity to wrap.</param>
    public PublishedExternalMember(ExternalMemberIdentity identity)
    {
        _identity = identity;
        _properties = new Lazy<IReadOnlyDictionary<string, IPublishedProperty>>(ParseProfileData);
    }

    /// <inheritdoc />
    public string Email => _identity.Email;

    /// <inheritdoc />
    public string UserName => _identity.UserName;

    /// <inheritdoc />
    public string? Comments => null;

    /// <inheritdoc />
    public bool IsApproved => _identity.IsApproved;

    /// <inheritdoc />
    public bool IsLockedOut => _identity.IsLockedOut;

    /// <inheritdoc />
    public DateTime? LastLockoutDate => _identity.LastLockoutDate;

    /// <inheritdoc />
    public DateTime CreationDate => _identity.CreateDate;

    /// <inheritdoc />
    public DateTime? LastLoginDate => _identity.LastLoginDate;

    /// <inheritdoc />
    public DateTime? LastPasswordChangedDate => null;

    /// <inheritdoc />
    public int Id => _identity.Id;

    /// <inheritdoc />
    public Guid Key => _identity.Key;

    /// <inheritdoc />
    public string Name => _identity.Name ?? _identity.UserName;

    /// <inheritdoc />
    public string? UrlSegment => null;

    /// <inheritdoc />
    public int SortOrder => 0;

    /// <inheritdoc />
    public int Level => 0;

    /// <inheritdoc />
    public string Path => $"-1,{_identity.Id}";

    /// <inheritdoc />
    public int? TemplateId => null;

    /// <inheritdoc />
    public int CreatorId => -1;

    /// <inheritdoc />
    public DateTime CreateDate => _identity.CreateDate;

    /// <inheritdoc />
    public int WriterId => -1;

    /// <inheritdoc />
    public DateTime UpdateDate => _identity.CreateDate;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => new Dictionary<string, PublishedCultureInfo>();

    /// <inheritdoc />
    public PublishedItemType ItemType => PublishedItemType.Member;

#pragma warning disable CS0618 // Type or member is obsolete
    /// <inheritdoc />
    public IPublishedContent? Parent => null;

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> Children => [];
#pragma warning restore CS0618

    /// <inheritdoc />
    public bool IsDraft(string? culture = null) => false;

    /// <inheritdoc />
    public bool IsPublished(string? culture = null) => true;

    /// <inheritdoc />
    public IPublishedContentType ContentType => _externalMemberContentType;

    /// <inheritdoc />
    public IEnumerable<IPublishedProperty> Properties => _properties.Value.Values;

    /// <inheritdoc />
    public IPublishedProperty? GetProperty(string alias)
        => _properties.Value.TryGetValue(alias, out IPublishedProperty? property) ? property : null;

    private IReadOnlyDictionary<string, IPublishedProperty> ParseProfileData()
    {
        if (string.IsNullOrWhiteSpace(_identity.ProfileData))
        {
            return new Dictionary<string, IPublishedProperty>();
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(_identity.ProfileData);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, IPublishedProperty>();
            }

            var properties = new Dictionary<string, IPublishedProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty jsonProperty in doc.RootElement.EnumerateObject())
            {
                object? value = ConvertJsonElement(jsonProperty.Value);
                properties[jsonProperty.Name] = new ProfileDataProperty(jsonProperty.Name, value);
            }

            return properties;
        }
        catch (JsonException)
        {
            return new Dictionary<string, IPublishedProperty>();
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
            JsonValueKind.Null => null,
            // For arrays and nested objects, return the raw JSON string.
            _ => element.GetRawText(),
        };

    /// <summary>
    ///     A lightweight <see cref="IPublishedProperty"/> backed by a single value from profile data JSON.
    /// </summary>
    private sealed class ProfileDataProperty : IPublishedProperty
    {
        private readonly object? _value;

        public ProfileDataProperty(string alias, object? value)
        {
            Alias = alias;
            _value = value;
            PropertyType = new ProfileDataPropertyType(alias);
        }

        /// <inheritdoc />
        public IPublishedPropertyType PropertyType { get; }

        /// <inheritdoc />
        public string Alias { get; }

        /// <inheritdoc />
        public bool HasValue(string? culture = null, string? segment = null) => _value is not null;

        /// <inheritdoc />
        public object? GetSourceValue(string? culture = null, string? segment = null) => _value;

        /// <inheritdoc />
        public object? GetValue(string? culture = null, string? segment = null) => _value;

        /// <inheritdoc />
        public object? GetDeliveryApiValue(bool expanding, string? culture = null, string? segment = null) => _value;
    }

    /// <summary>
    ///     A minimal <see cref="IPublishedPropertyType"/> for profile data properties.
    /// </summary>
    private sealed class ProfileDataPropertyType : IPublishedPropertyType
    {
        public ProfileDataPropertyType(string alias) => Alias = alias;

        /// <inheritdoc />
        public IPublishedContentType? ContentType => _externalMemberContentType;

        /// <inheritdoc />
        public PublishedDataType DataType => _stringDataType;

        /// <inheritdoc />
        public string Alias { get; }

        /// <inheritdoc />
        public string EditorAlias => Constants.PropertyEditors.Aliases.Label;

        /// <inheritdoc />
        public string EditorUiAlias => Constants.PropertyEditors.Aliases.Label;

        /// <inheritdoc />
        public bool IsUserProperty => true;

        /// <inheritdoc />
        public ContentVariation Variations => ContentVariation.Nothing;

        /// <inheritdoc />
        public PropertyCacheLevel CacheLevel => PropertyCacheLevel.None;

        /// <inheritdoc />
        public PropertyCacheLevel DeliveryApiCacheLevel => PropertyCacheLevel.None;

        /// <inheritdoc />
        public Type ModelClrType => typeof(object);

        /// <inheritdoc />
        public Type? ClrType => typeof(object);

        /// <inheritdoc />
        public bool? IsValue(object? value, PropertyValueLevel level) => value is not null;

        /// <inheritdoc />
        public object? ConvertSourceToInter(IPublishedElement owner, object? source, bool preview) => source;

        /// <inheritdoc />
        public object? ConvertInterToObject(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview) => inter;

        /// <inheritdoc />
        public object? ConvertInterToDeliveryApiObject(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding) => inter;
    }
}

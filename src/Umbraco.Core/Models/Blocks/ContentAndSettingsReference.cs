// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

public struct ContentAndSettingsReference : IEquatable<ContentAndSettingsReference>
{
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public ContentAndSettingsReference(Udi? contentUdi, Udi? settingsUdi)
        : this(
            (contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)),
            (settingsUdi as GuidUdi)?.Guid)
    {
    }

    public ContentAndSettingsReference(Guid contentKey, Guid? settingsKey)
    {
        ContentKey = contentKey;
        SettingsKey = settingsKey;
        ContentUdi = new GuidUdi(Constants.UdiEntityType.Element, contentKey);
        SettingsUdi = settingsKey.HasValue
            ? new GuidUdi(Constants.UdiEntityType.Element, settingsKey.Value)
            : null;
    }

    [Obsolete("Use ContentKey instead. Will be removed in V18.")]
    public Udi ContentUdi { get; }

    [Obsolete("Use SettingsKey instead. Will be removed in V18.")]
    public Udi? SettingsUdi { get; }

    public Guid ContentKey { get; set; }

    public Guid? SettingsKey { get; set; }

    public static bool operator ==(ContentAndSettingsReference left, ContentAndSettingsReference right) =>
        left.Equals(right);

    public override bool Equals(object? obj) => obj is ContentAndSettingsReference reference && Equals(reference);

    public bool Equals(ContentAndSettingsReference other) => other != null
                                                             && EqualityComparer<Udi>.Default.Equals(
                                                                 ContentUdi,
                                                                 other.ContentUdi)
                                                             && EqualityComparer<Udi>.Default.Equals(
                                                                 SettingsUdi,
                                                                 other.SettingsUdi);

    public override int GetHashCode() => (ContentUdi, SettingsUdi).GetHashCode();

    public static bool operator !=(ContentAndSettingsReference left, ContentAndSettingsReference right) =>
        !(left == right);
}

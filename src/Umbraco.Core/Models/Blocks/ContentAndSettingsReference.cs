// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a reference to block content and settings data.
/// </summary>
public struct ContentAndSettingsReference : IEquatable<ContentAndSettingsReference>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentAndSettingsReference" /> struct.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    /// <param name="settingsUdi">The settings UDI.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    public ContentAndSettingsReference(Udi? contentUdi, Udi? settingsUdi)
        : this(
            (contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)),
            (settingsUdi as GuidUdi)?.Guid)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentAndSettingsReference" /> struct.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    /// <param name="settingsKey">The settings key.</param>
    public ContentAndSettingsReference(Guid contentKey, Guid? settingsKey)
    {
        ContentKey = contentKey;
        SettingsKey = settingsKey;
        ContentUdi = new GuidUdi(Constants.UdiEntityType.Element, contentKey);
        SettingsUdi = settingsKey.HasValue
            ? new GuidUdi(Constants.UdiEntityType.Element, settingsKey.Value)
            : null;
    }

    /// <summary>
    ///     Gets the content UDI.
    /// </summary>
    /// <value>
    ///     The content UDI.
    /// </value>
    [Obsolete("Use ContentKey instead. Will be removed in V18.")]
    public Udi ContentUdi { get; }

    /// <summary>
    ///     Gets the settings UDI.
    /// </summary>
    /// <value>
    ///     The settings UDI.
    /// </value>
    [Obsolete("Use SettingsKey instead. Will be removed in V18.")]
    public Udi? SettingsUdi { get; }

    /// <summary>
    ///     Gets or sets the content key.
    /// </summary>
    /// <value>
    ///     The content key.
    /// </value>
    public Guid ContentKey { get; set; }

    /// <summary>
    ///     Gets or sets the settings key.
    /// </summary>
    /// <value>
    ///     The settings key.
    /// </value>
    public Guid? SettingsKey { get; set; }

    /// <summary>
    ///     Determines whether two <see cref="ContentAndSettingsReference" /> instances are equal.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns>
    ///     <c>true</c> if the instances are equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(ContentAndSettingsReference left, ContentAndSettingsReference right) =>
        left.Equals(right);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ContentAndSettingsReference reference && Equals(reference);

    /// <inheritdoc />
    public bool Equals(ContentAndSettingsReference other) => other != null
                                                             && EqualityComparer<Udi>.Default.Equals(
                                                                 ContentUdi,
                                                                 other.ContentUdi)
                                                             && EqualityComparer<Udi>.Default.Equals(
                                                                 SettingsUdi,
                                                                 other.SettingsUdi);

    /// <inheritdoc />
    public override int GetHashCode() => (ContentUdi, SettingsUdi).GetHashCode();

    /// <summary>
    ///     Determines whether two <see cref="ContentAndSettingsReference" /> instances are not equal.
    /// </summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns>
    ///     <c>true</c> if the instances are not equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator !=(ContentAndSettingsReference left, ContentAndSettingsReference right) =>
        !(left == right);
}

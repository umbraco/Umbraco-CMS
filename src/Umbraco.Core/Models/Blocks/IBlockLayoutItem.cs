// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a layout item for a block editor.
/// </summary>
public interface IBlockLayoutItem
{
    /// <summary>
    ///     Gets or sets the content UDI.
    /// </summary>
    /// <value>
    ///     The content UDI.
    /// </value>
    [Obsolete("Use ContentKey instead. Will be removed in V18.")]
    public Udi? ContentUdi { get; set; }

    /// <summary>
    ///     Gets or sets the settings UDI.
    /// </summary>
    /// <value>
    ///     The settings UDI.
    /// </value>
    [Obsolete("Use SettingsKey instead. Will be removed in V18.")]
    public Udi? SettingsUdi { get; set; }

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
    ///     Determines whether this layout item references the specified content key.
    /// </summary>
    /// <param name="key">The content key to check.</param>
    /// <returns>
    ///     <c>true</c> if this layout item references the specified content key; otherwise, <c>false</c>.
    /// </returns>
    public bool ReferencesContent(Guid key) => ContentKey == key;

    /// <summary>
    ///     Determines whether this layout item references the specified settings key.
    /// </summary>
    /// <param name="key">The settings key to check.</param>
    /// <returns>
    ///     <c>true</c> if this layout item references the specified settings key; otherwise, <c>false</c>.
    /// </returns>
    public bool ReferencesSetting(Guid key) => SettingsKey == key;
}

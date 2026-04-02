// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for a block in block-based editors.
/// </summary>
public interface IBlockConfiguration
{
    /// <summary>
    ///     Gets or sets the unique key of the content element type for this block.
    /// </summary>
    public Guid ContentElementTypeKey { get; set; }

    /// <summary>
    ///     Gets or sets the unique key of the settings element type for this block.
    /// </summary>
    /// <remarks>
    ///     Can be <c>null</c> if the block does not have settings.
    /// </remarks>
    public Guid? SettingsElementTypeKey { get; set; }
}

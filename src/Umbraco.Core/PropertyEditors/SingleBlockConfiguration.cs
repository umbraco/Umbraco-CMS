// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The configuration object for the Single Block editor.
/// </summary>
public class SingleBlockConfiguration
{
    /// <summary>
    ///     Gets or sets the available block type configurations.
    /// </summary>
    /// <remarks>
    ///     For the Single Block editor, this typically contains only one block configuration.
    /// </remarks>
    [ConfigurationField("blocks")]
    public BlockListConfiguration.BlockConfiguration[] Blocks { get; set; } = [];
}

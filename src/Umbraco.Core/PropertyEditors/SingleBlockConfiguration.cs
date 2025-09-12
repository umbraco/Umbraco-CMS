// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The configuration object for the Single Block editor
/// </summary>
public class SingleBlockConfiguration
{
    [ConfigurationField("blocks")]
    public BlockListConfiguration.BlockConfiguration[] Blocks { get; set; } = [];
}

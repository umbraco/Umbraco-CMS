// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Used for deserializing the block list layout
/// </summary>
public class BlockListLayoutItem : IBlockLayoutItem
{
    public Udi? ContentUdi { get; set; }

    public Udi? SettingsUdi { get; set; }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

public abstract class BlockValue<TLayout> where TLayout : IBlockLayoutItem
{
    public IDictionary<string, IEnumerable<TLayout>> Layout { get; set; } = null!;

    public List<BlockItemData> ContentData { get; set; } = new();

    public List<BlockItemData> SettingsData { get; set; } = new();
}

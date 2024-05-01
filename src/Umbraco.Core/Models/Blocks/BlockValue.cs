// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

public abstract class BlockValue<TLayout> : BlockValue
    where TLayout : IBlockLayoutItem
{
    public IEnumerable<TLayout>? GetLayouts(string propertyEditorAlias)
        => Layout.TryGetValue(propertyEditorAlias, out IEnumerable<IBlockLayoutItem>? layouts) is true
            ? layouts.OfType<TLayout>()
            : null;
}

public abstract class BlockValue
{
    public IDictionary<string, IEnumerable<IBlockLayoutItem>> Layout { get; set; } = new Dictionary<string, IEnumerable<IBlockLayoutItem>>();

    public List<BlockItemData> ContentData { get; set; } = new();

    public List<BlockItemData> SettingsData { get; set; } = new();

    public abstract string PropertyEditorAlias { get; }
}

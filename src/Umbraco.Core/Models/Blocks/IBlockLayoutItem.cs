// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

public interface IBlockLayoutItem
{
    [Obsolete("Use ContentKey instead. Will be removed in V18.")]
    public Udi? ContentUdi { get; set; }

    [Obsolete("Use SettingsKey instead. Will be removed in V18.")]
    public Udi? SettingsUdi { get; set; }

    public Guid ContentKey { get; set; }

    public Guid? SettingsKey { get; set; }
}

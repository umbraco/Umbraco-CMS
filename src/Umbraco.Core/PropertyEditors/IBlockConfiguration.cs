// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

public interface IBlockConfiguration
{
    public Guid ContentElementTypeKey { get; set; }

    public Guid? SettingsElementTypeKey { get; set; }

    public string? Label { get; set; }

    public string? EditorSize { get; set; }

    public string? IconColor { get; set; }

    public string? BackgroundColor { get; set; }

    public string? Thumbnail { get; set; }

    public bool? ForceHideContentEditorInOverlay { get; set; }
}

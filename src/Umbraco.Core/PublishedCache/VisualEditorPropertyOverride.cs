namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// A single unsaved property value to overlay onto draft content when rendering the visual editor preview.
/// </summary>
/// <param name="Alias">The property alias to override.</param>
/// <param name="EditorValue">
/// The editor-format value as held by the backoffice workspace. Complex editors (rich text, block list)
/// expect their serialized JSON; plain editors (e.g. text box) expect the raw value.
/// </param>
/// <param name="Culture">The culture the override applies to, or <c>null</c> for invariant.</param>
/// <param name="Segment">The segment the override applies to, or <c>null</c> for none.</param>
public readonly record struct VisualEditorPropertyOverride(string Alias, object? EditorValue, string? Culture, string? Segment);

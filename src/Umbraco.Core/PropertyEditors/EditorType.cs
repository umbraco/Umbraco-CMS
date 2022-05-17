namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the type of an editor.
/// </summary>
[Flags]
public enum EditorType
{
    /// <summary>
    ///     Nothing.
    /// </summary>
    Nothing = 0,

    /// <summary>
    ///     Property value editor.
    /// </summary>
    PropertyValue = 1,

    /// <summary>
    ///     Macro parameter editor.
    /// </summary>
    MacroParameter = 2,
}

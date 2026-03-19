namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
/// Tracks property accesses during Razor rendering so that the visual editor
/// can automatically wrap property output with annotation attributes.
///
/// <para>
/// When <c>@Model.Title</c> or <c>@Model.Value("title")</c> is evaluated in a Razor view,
/// the <c>Value()</c> extension method records the property alias and content key here.
/// When Razor subsequently calls <c>Write()</c>, the recorded access is consumed and the output
/// is wrapped with <c>data-umb-property</c> attributes.
/// </para>
/// </summary>
public static class VisualEditorPropertyTracker
{
    private static readonly AsyncLocal<PropertyAccess?> _lastAccess = new();
    private static readonly AsyncLocal<bool> _enabled = new();

    /// <summary>
    /// Enables tracking for the current async context.
    /// Should be called when the request is in visual edit / preview mode.
    /// </summary>
    public static void Enable() => _enabled.Value = true;

    /// <summary>
    /// Whether tracking is currently enabled for this async context.
    /// </summary>
    public static bool IsEnabled => _enabled.Value;

    /// <summary>
    /// Records a property access. Called from <c>Value()</c> / <c>Value&lt;T&gt;()</c> extension methods.
    /// </summary>
    public static void RecordAccess(string alias, Guid contentKey)
    {
        if (_enabled.Value)
        {
            _lastAccess.Value = new PropertyAccess(alias, contentKey);
        }
    }

    /// <summary>
    /// Consumes the last recorded access, returning it and clearing the state.
    /// </summary>
    public static PropertyAccess? ConsumeAccess()
    {
        PropertyAccess? access = _lastAccess.Value;
        _lastAccess.Value = null;
        return access;
    }

    /// <summary>
    /// Clears any pending recorded access without consuming it.
    /// </summary>
    public static void Clear()
        => _lastAccess.Value = null;

    /// <summary>
    /// Represents a recorded property access.
    /// </summary>
    public readonly record struct PropertyAccess(string Alias, Guid ContentKey);
}

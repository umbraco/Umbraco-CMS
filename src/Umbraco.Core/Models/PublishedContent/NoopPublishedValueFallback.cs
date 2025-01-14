namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Provides a noop implementation for <see cref="IPublishedValueFallback" />.
/// </summary>
/// <remarks>
///     <para>This is for tests etc - does not implement fallback at all.</para>
/// </remarks>
public class NoopPublishedValueFallback : IPublishedValueFallback
{
    /// <inheritdoc />
    public bool TryGetValue(IPublishedProperty property, string? culture, string? segment, Fallback fallback, object? defaultValue, out object? value)
    {
        value = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetValue<T>(IPublishedProperty property, string? culture, string? segment, Fallback fallback, T? defaultValue, out T? value)
    {
        value = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetValue(IPublishedElement content, string alias, string? culture, string? segment, Fallback fallback, object? defaultValue, out object? value)
    {
        value = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetValue<T>(IPublishedElement content, string alias, string? culture, string? segment, Fallback fallback, T? defaultValue, out T? value)
    {
        value = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetValue(IPublishedContent content, string alias, string? culture, string? segment, Fallback fallback, object? defaultValue, out object? value, out IPublishedProperty? noValueProperty)
    {
        value = default;
        noValueProperty = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetValue<T>(IPublishedContent content, string alias, string? culture, string? segment, Fallback fallback, T defaultValue, out T? value, out IPublishedProperty? noValueProperty)
    {
        value = default;
        noValueProperty = default;
        return false;
    }
}

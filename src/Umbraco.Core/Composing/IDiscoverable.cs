namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Marker interface for types that should be discovered during type scanning.
/// </summary>
/// <remarks>
/// Types implementing this interface are cached during discovery operations,
/// making subsequent lookups faster. Use this interface for plugin types that
/// need to be discovered at runtime.
/// </remarks>
public interface IDiscoverable
{
}

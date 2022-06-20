namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used to generate paths to media items for a specified property editor alias
/// </summary>
public interface IMediaUrlGenerator
{
    /// <summary>
    ///     Tries to get a media path for a given property editor alias
    /// </summary>
    /// <param name="propertyEditorAlias">The property editor alias</param>
    /// <param name="value">The value of the property</param>
    /// <returns>
    ///     True if a media path was returned
    /// </returns>
    bool TryGetMediaPath(string? propertyEditorAlias, object? value, out string? mediaPath);
}

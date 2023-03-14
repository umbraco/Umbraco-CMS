namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Defines an entity that tracks property changes and can be dirty, and remembers
///     which properties were dirty when the changes were committed.
/// </summary>
public interface IRememberBeingDirty : ICanBeDirty
{
    /// <summary>
    ///     Determines whether the current entity is dirty.
    /// </summary>
    /// <remarks>A property was dirty if it had been changed and the changes were committed.</remarks>
    bool WasDirty();

    /// <summary>
    ///     Determines whether a specific property was dirty.
    /// </summary>
    /// <remarks>A property was dirty if it had been changed and the changes were committed.</remarks>
    bool WasPropertyDirty(string propertyName);

    /// <summary>
    ///     Resets properties that were dirty.
    /// </summary>
    void ResetWereDirtyProperties();

    /// <summary>
    ///     Resets dirty properties.
    /// </summary>
    /// <param name="rememberDirty">A value indicating whether to remember dirty properties.</param>
    /// <remarks>
    ///     When <paramref name="rememberDirty" /> is true, dirty properties are saved so they can be checked with
    ///     WasDirty.
    /// </remarks>
    void ResetDirtyProperties(bool rememberDirty);

    /// <summary>
    ///     Gets properties that were dirty.
    /// </summary>
    IEnumerable<string> GetWereDirtyProperties();
}

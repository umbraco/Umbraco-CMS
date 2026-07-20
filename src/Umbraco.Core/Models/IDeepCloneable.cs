namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Provides a mean to deep-clone an object.
/// </summary>
public interface IDeepCloneable
{
    /// <summary>
    ///     Creates a deep clone of the current object.
    /// </summary>
    /// <returns>A deep clone of the current object.</returns>
    object DeepClone();
}

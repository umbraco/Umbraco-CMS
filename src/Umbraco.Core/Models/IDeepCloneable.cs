namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Provides a mean to deep-clone an object.
/// </summary>
public interface IDeepCloneable
{
    object DeepClone();
}

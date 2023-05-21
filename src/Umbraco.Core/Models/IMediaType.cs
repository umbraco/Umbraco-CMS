namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines a ContentType, which Media is based on
/// </summary>
public interface IMediaType : IContentTypeComposition
{
    /// <summary>
    ///     Creates a deep clone of the current entity with its identity/alias and it's property identities reset
    /// </summary>
    /// <param name="newAlias"></param>
    /// <returns></returns>
    IMediaType DeepCloneWithResetIdentities(string newAlias);
}

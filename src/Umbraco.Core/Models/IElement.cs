namespace Umbraco.Cms.Core.Models;

public interface IElement : IPublishableContentBase
{
    /// <summary>
    ///     Creates a deep clone of the current entity with its identity/alias and it's property identities reset
    /// </summary>
    /// <returns></returns>
    IElement DeepCloneWithResetIdentities();
}

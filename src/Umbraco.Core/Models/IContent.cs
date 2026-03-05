namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a document.
/// </summary>
/// <remarks>
///     <para>A document can be published, rendered by a template.</para>
/// </remarks>
public interface IContent : IPublishableContentBase, ITemplatedContent
{
    /// <summary>
    /// <summary>
    ///     Gets or sets the published state of the content.
    /// </summary>
    ///     Gets a value indicating whether the content item is a blueprint.
    /// </summary>
    bool Blueprint { get; set; }

    /// <summary>
    ///     Creates a deep clone of the current entity with its identity/alias and it's property identities reset
    /// </summary>
    /// <returns></returns>
    IContent DeepCloneWithResetIdentities();
}

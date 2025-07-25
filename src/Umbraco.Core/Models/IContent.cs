namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a document.
/// </summary>
/// <remarks>
///     <para>A document can be published, rendered by a template.</para>
/// </remarks>
public interface IContent : IPublishableContentBase
{
    /// <summary>
    ///     Gets or sets the template id used to render the content.
    /// </summary>
    int? TemplateId { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the content item is a blueprint.
    /// </summary>
    bool Blueprint { get; set; }

    /// <summary>
    ///     Gets the template id used to render the published version of the content.
    /// </summary>
    /// <remarks>When editing the content, the template can change, but this will not until the content is published.</remarks>
    int? PublishTemplateId { get; set; }

    /// <summary>
    ///     Creates a deep clone of the current entity with its identity/alias and it's property identities reset
    /// </summary>
    /// <returns></returns>
    IContent DeepCloneWithResetIdentities();
}

using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines a content type that contains a history cleanup policy.
/// </summary>
[Obsolete("This will be merged into IContentType in Umbraco 10.")]
public interface IContentTypeWithHistoryCleanup : IContentType
{
    /// <summary>
    ///     Gets or sets the history cleanup configuration.
    /// </summary>
    /// <value>The history cleanup configuration.</value>
    HistoryCleanup? HistoryCleanup { get; set; }
}

/// <summary>
///     Defines a ContentType, which Content is based on
/// </summary>
public interface IContentType : IContentTypeComposition
{
    /// <summary>
    ///     Internal property to store the Id of the default template
    /// </summary>
    int DefaultTemplateId { get; set; }

    /// <summary>
    ///     Gets the default Template of the ContentType
    /// </summary>
    ITemplate? DefaultTemplate { get; }

    /// <summary>
    ///     Gets or Sets a list of Templates which are allowed for the ContentType
    /// </summary>
    IEnumerable<ITemplate>? AllowedTemplates { get; set; }

    /// <summary>
    ///     Determines if AllowedTemplates contains templateId
    /// </summary>
    /// <param name="templateId">The template id to check</param>
    /// <returns>True if AllowedTemplates contains the templateId else False</returns>
    bool IsAllowedTemplate(int templateId);

    /// <summary>
    ///     Determines if AllowedTemplates contains templateId
    /// </summary>
    /// <param name="templateAlias">The template alias to check</param>
    /// <returns>True if AllowedTemplates contains the templateAlias else False</returns>
    bool IsAllowedTemplate(string templateAlias);

    /// <summary>
    ///     Sets the default template for the ContentType
    /// </summary>
    /// <param name="template">Default <see cref="ITemplate" /></param>
    void SetDefaultTemplate(ITemplate? template);

    /// <summary>
    ///     Removes a template from the list of allowed templates
    /// </summary>
    /// <param name="template"><see cref="ITemplate" /> to remove</param>
    /// <returns>True if template was removed, otherwise False</returns>
    bool RemoveTemplate(ITemplate template);

    /// <summary>
    ///     Creates a deep clone of the current entity with its identity/alias and it's property identities reset
    /// </summary>
    /// <param name="newAlias"></param>
    /// <returns></returns>
    IContentType DeepCloneWithResetIdentities(string newAlias);
}

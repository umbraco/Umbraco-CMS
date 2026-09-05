namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents content that supports templated rendering.
/// </summary>
public interface ITemplatedContent
{
    /// <summary>
    ///     Gets or sets the template id used to render the content.
    /// </summary>
    int? TemplateId { get; set; }

    /// <summary>
    ///     Gets the template id used to render the published version of the content.
    /// </summary>
    /// <remarks>When editing the content, the template can change, but this will not until the content is published.</remarks>
    int? PublishTemplateId { get; set; }
}

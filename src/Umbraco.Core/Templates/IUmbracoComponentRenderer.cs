using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     Methods used to render umbraco components as HTML in templates
/// </summary>
public interface IUmbracoComponentRenderer
{
    /// <summary>
    ///     Renders the template for the specified pageId and an optional altTemplateId
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <param name="altTemplateId">If not specified, will use the template assigned to the node</param>
    Task<IHtmlEncodedString> RenderTemplateAsync(int contentId, int? altTemplateId = null);
}

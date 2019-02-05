using System.Collections.Generic;
using System.Web;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    /// <summary>
    /// Methods used to render umbraco components as HTML in templates
    /// </summary>
    public interface IUmbracoComponentRenderer
    {
        /// <summary>
        /// Renders the template for the specified pageId and an optional altTemplateId
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="altTemplateId">If not specified, will use the template assigned to the node</param>
        /// <returns></returns>
        IHtmlString RenderTemplate(int contentId, int? altTemplateId = null);

        /// <summary>
        /// Renders the macro with the specified alias.
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        IHtmlString RenderMacro(int contentId, string alias);

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IHtmlString RenderMacro(int contentId, string alias, object parameters);

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IHtmlString RenderMacro(int contentId, string alias, IDictionary<string, object> parameters);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Templates
{
    /// <summary>
    /// Methods used to render umbraco components as HTML in templates
    /// </summary>
    public interface IUmbracoComponentRenderer
    {
        /// <summary>
        /// Renders the template for the specified pageId and an optional altTemplateId
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="altTemplateId">If not specified, will use the template assigned to the node</param>
        Task<IHtmlEncodedString> RenderTemplateAsync(int contentId, int? altTemplateId = null);

        /// <summary>
        /// Renders the macro with the specified alias.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="alias">The alias.</param>
        IHtmlEncodedString RenderMacro(int contentId, string alias);

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        IHtmlEncodedString RenderMacro(int contentId, string alias, object parameters);

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        IHtmlEncodedString RenderMacro(int contentId, string alias, IDictionary<string, object> parameters);

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="content">An IPublishedContent to use for the context for the macro rendering</param>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A raw HTML string of the macro output</returns>
        /// <remarks>
        /// Currently only used when the node is unpublished and unable to get the contentId item from the
        /// content cache as its unpublished. This deals with taking in a preview/draft version of the content node
        /// </remarks>
        IHtmlEncodedString RenderMacroForContent(IPublishedContent content, string alias, IDictionary<string, object> parameters);

    }
}
